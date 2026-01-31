using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using KalponicGames.KS_SnapStudio.Editor;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// CaptureService - High-level capture orchestration service.
    /// Manages batch captures, progress tracking, and export operations.
    /// </summary>
    public static class CaptureService
    {
        // Capture settings
        public static int ResolutionWidth = 1024;
        public static int ResolutionHeight = 1024;
        public static bool TransparentBackground = true;
        public static string OutputFolder;

        static CaptureService()
        {
            OutputFolder = PluginAssetLocator.DefaultCapturesFolder;
        }

        // Progress tracking
        public static float CurrentProgress { get; private set; }
        public static string CurrentStatus { get; private set; }

        /// <summary>
        /// Capture a single frame from the current scene
        /// </summary>
        public static Texture2D CaptureFrame(Camera camera = null)
        {
            if (camera == null)
                camera = SceneBuilder.GetCaptureCamera();

            if (camera == null) return null;

            // Create render texture
            var rt = RenderTexture.GetTemporary(ResolutionWidth, ResolutionHeight, 24, RenderTextureFormat.ARGB32);
            var prevRT = camera.targetTexture;
            camera.targetTexture = rt;
            camera.Render();

            // Read pixels
            var tex = new Texture2D(ResolutionWidth, ResolutionHeight, TextureFormat.ARGB32, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, ResolutionWidth, ResolutionHeight), 0, 0);
            tex.Apply();

            // Cleanup
            camera.targetTexture = prevRT;
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return tex;
        }

        /// <summary>
        /// Capture animation frames at specified times
        /// </summary>
        public static async Task<List<Texture2D>> CaptureAnimationFrames(
            AnimationClip clip,
            GameObject target,
            float[] times,
            System.IProgress<(float progress, string status)> progress = null)
        {
            var frames = new List<Texture2D>();
            var totalFrames = times.Length;

            for (int i = 0; i < totalFrames; i++)
            {
                var time = times[i];
                CurrentProgress = (float)i / totalFrames;
                CurrentStatus = $"Capturing frame {i + 1}/{totalFrames} at {time:F2}s";

                progress?.Report((CurrentProgress, CurrentStatus));

                // Sample animation at this time
                ClipSampler.SampleClipAtAbsoluteTime(clip, time, target);

                // Wait a frame for changes to take effect
                await Task.Yield();

                // Capture frame
                var frame = CaptureFrame();
                if (frame != null)
                    frames.Add(frame);
            }

            CurrentProgress = 1f;
            CurrentStatus = "Capture complete";
            progress?.Report((1f, "Capture complete"));

            return frames;
        }

        /// <summary>
        /// Capture keyframes from an animation clip
        /// </summary>
        public static async Task<List<Texture2D>> CaptureKeyframes(
            AnimationClip clip,
            GameObject target,
            System.IProgress<(float progress, string status)> progress = null)
        {
            var keyframeTimes = ClipSampler.GetKeyframeTimes(clip);
            return await CaptureAnimationFrames(clip, target, keyframeTimes, progress);
        }

        /// <summary>
        /// Capture regular intervals from an animation clip
        /// </summary>
        public static async Task<List<Texture2D>> CaptureIntervals(
            AnimationClip clip,
            GameObject target,
            int frameCount,
            System.IProgress<(float progress, string status)> progress = null)
        {
            var poses = ClipSampler.SampleAtIntervals(clip, target, frameCount);
            var times = poses.Select(p => p.time).ToArray();
            return await CaptureAnimationFrames(clip, target, times, progress);
        }

        /// <summary>
        /// Batch capture multiple clips
        /// </summary>
        public static async Task<Dictionary<string, List<Texture2D>>> BatchCaptureClips(
            Dictionary<string, (AnimationClip clip, GameObject target)> clips,
            System.IProgress<(float progress, string status)> progress = null)
        {
            var results = new Dictionary<string, List<Texture2D>>();
            var totalClips = clips.Count;
            var completedClips = 0;

            foreach (var kvp in clips)
            {
                var clipName = kvp.Key;
                var (clip, target) = kvp.Value;

                CurrentStatus = $"Capturing {clipName}...";
                progress?.Report((completedClips / (float)totalClips, CurrentStatus));

                // Capture keyframes for this clip
                var frames = await CaptureKeyframes(clip, target, progress);
                results[clipName] = frames;

                completedClips++;
            }

            CurrentProgress = 1f;
            CurrentStatus = "Batch capture complete";
            progress?.Report((1f, "Batch capture complete"));

            return results;
        }

        /// <summary>
        /// Export captured frames to PNG files
        /// </summary>
        public static void ExportFrames(List<Texture2D> frames, string baseName, string folder = null)
        {
            if (frames == null || frames.Count == 0) return;

            folder = folder ?? OutputFolder;
            EnsureOutputFolder(folder);

            for (int i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];
                var fileName = $"{baseName}_{i:000}.png";
                var path = System.IO.Path.Combine(folder, fileName);

                var bytes = frame.EncodeToPNG();
                System.IO.File.WriteAllBytes(path, bytes);

                // Import as asset
                AssetDatabase.ImportAsset(path);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Export captured frames as a sprite sheet
        /// </summary>
        public static Texture2D CreateSpriteSheet(List<Texture2D> frames, int columns = 0)
        {
            if (frames == null || frames.Count == 0) return null;

            // Auto-calculate columns if not specified
            if (columns <= 0)
                columns = Mathf.CeilToInt(Mathf.Sqrt(frames.Count));

            var rows = Mathf.CeilToInt((float)frames.Count / columns);
            var frameWidth = frames[0].width;
            var frameHeight = frames[0].height;

            var sheet = new Texture2D(columns * frameWidth, rows * frameHeight, TextureFormat.ARGB32, false);

            for (int i = 0; i < frames.Count; i++)
            {
                var x = (i % columns) * frameWidth;
                var y = (rows - 1 - (i / columns)) * frameHeight; // Flip Y for correct orientation

                var pixels = frames[i].GetPixels();
                sheet.SetPixels(x, y, frameWidth, frameHeight, pixels);
            }

            sheet.Apply();
            return sheet;
        }

        /// <summary>
        /// Export sprite sheet to file
        /// </summary>
        public static void ExportSpriteSheet(Texture2D spriteSheet, string fileName, string folder = null)
        {
            if (spriteSheet == null) return;

            folder = folder ?? OutputFolder;
            EnsureOutputFolder(folder);

            var path = System.IO.Path.Combine(folder, fileName + ".png");
            var bytes = spriteSheet.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);

            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Ensure output folder exists
        /// </summary>
        private static void EnsureOutputFolder(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                var parent = System.IO.Path.GetDirectoryName(folder);
                var name = System.IO.Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        /// <summary>
        /// Trim transparent borders from texture
        /// </summary>
        public static Texture2D TrimTexture(Texture2D texture)
        {
            if (texture == null) return null;

            var pixels = texture.GetPixels();
            var width = texture.width;
            var height = texture.height;

            // Find bounds of non-transparent pixels
            int minX = width, maxX = 0, minY = height, maxY = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = pixels[y * width + x];
                    if (pixel.a > 0.01f) // Consider pixel visible if alpha > 1%
                    {
                        minX = Mathf.Min(minX, x);
                        maxX = Mathf.Max(maxX, x);
                        minY = Mathf.Min(minY, y);
                        maxY = Mathf.Max(maxY, y);
                    }
                }
            }

            // If no visible pixels, return original
            if (minX > maxX || minY > maxY)
                return texture;

            // Add small border
            minX = Mathf.Max(0, minX - 1);
            maxX = Mathf.Min(width - 1, maxX + 1);
            minY = Mathf.Max(0, minY - 1);
            maxY = Mathf.Min(height - 1, maxY + 1);

            var trimWidth = maxX - minX + 1;
            var trimHeight = maxY - minY + 1;

            // Create trimmed texture
            var trimmed = new Texture2D(trimWidth, trimHeight, texture.format, false);
            var trimmedPixels = new Color[trimWidth * trimHeight];

            for (int y = 0; y < trimHeight; y++)
            {
                for (int x = 0; x < trimWidth; x++)
                {
                    trimmedPixels[y * trimWidth + x] = pixels[(minY + y) * width + (minX + x)];
                }
            }

            trimmed.SetPixels(trimmedPixels);
            trimmed.Apply();

            return trimmed;
        }

        /// <summary>
        /// Get capture statistics
        /// </summary>
        public static (int totalFrames, float totalTime) GetCaptureStats(List<Texture2D> frames)
        {
            return (frames?.Count ?? 0, 0f); // Time tracking could be added later
        }
    }
}
