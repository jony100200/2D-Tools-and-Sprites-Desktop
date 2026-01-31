using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using KalponicGames.KS_SnapStudio.Editor;
using KalponicGames.KS_SnapStudio.SnapStudio;

namespace KalponicGames.KS_SnapStudio
{
    using Systems;
    /// <summary>
    /// BatchCaptureComponent - Component for managing batch capture operations.
    /// Handles multiple clips, objects, and export configurations.
    /// </summary>
    public class BatchCaptureComponent : MonoBehaviour
    {
        [Header("Capture Targets")]
        [Tooltip("Objects to capture")]
        public List<GameObject> captureTargets = new();

        [Tooltip("Animation clips to capture (one per target)")]
        public List<AnimationClip> animationClips = new();

        [Header("Capture Settings")]
        [Tooltip("Capture keyframes only")]
        public bool keyframesOnly = true;

        [Tooltip("Number of frames to capture (if not keyframes)")]
        public int frameCount = 8;

        [Tooltip("Directions to capture from")]
        public List<string> captureDirections = new();

        [Header("Output Settings")]
        [Tooltip("Base name for output files")]
        public string outputBaseName = "Capture";

        [Tooltip("Output folder")]
        public string outputFolder = "Assets/Captures";

        [Tooltip("Create sprite sheets")]
        public bool createSpriteSheets = true;

        [Tooltip("Resolution width")]
        public int resolutionWidth = 1024;

        [Tooltip("Resolution height")]
        public int resolutionHeight = 1024;

        // Progress tracking
        public float Progress { get; private set; }
        public string Status { get; private set; }

        // Capture results
        public Dictionary<string, List<Texture2D>> CapturedFrames { get; private set; }

        void OnValidate()
        {
            // Ensure we have enough clips for targets
            while (animationClips.Count < captureTargets.Count)
                animationClips.Add(null);

            while (animationClips.Count > captureTargets.Count)
                animationClips.RemoveAt(animationClips.Count - 1);

            // Remove null targets
            captureTargets = captureTargets.Where(t => t != null).ToList();
        }

        /// <summary>
        /// Execute batch capture operation
        /// </summary>
        public async Task ExecuteBatchCapture(System.IProgress<(float, string)> progress = null)
        {
            if (captureTargets.Count == 0)
            {
                Status = "No capture targets specified";
                return;
            }

            Progress = 0f;
            Status = "Initializing capture...";
            progress?.Report((0f, Status));

            // Setup capture service settings
            CaptureService.ResolutionWidth = resolutionWidth;
            CaptureService.ResolutionHeight = resolutionHeight;
            CaptureService.OutputFolder = outputFolder;

            // Initialize scene
            SceneBuilder.InitializeCaptureScene();

            try
            {
                // Add targets to scene
                SceneBuilder.AddSnappables(captureTargets);

                CapturedFrames = new Dictionary<string, List<Texture2D>>();
                var totalOperations = captureTargets.Count * (captureDirections.Count > 0 ? captureDirections.Count : 1);
                var completedOperations = 0;

                // Capture each target
                for (int i = 0; i < captureTargets.Count; i++)
                {
                    var target = captureTargets[i];
                    var clip = animationClips.Count > i ? animationClips[i] : null;

                    if (target == null) continue;

                    var targetName = target.name;

                    // Use legacy directions if available, otherwise use new direction rigs
                    var directionsToUse = legacyDirections?.Length > 0 ? legacyDirections : 
                        (captureDirections.Count > 0 ? captureDirections.ToArray() : new[] { "default" });

                    foreach (var direction in directionsToUse)
                    {
                        Status = $"Capturing {targetName} from {direction}...";
                        progress?.Report((completedOperations / (float)totalOperations, Status));

                        // Apply direction using legacy system or new rig system
                        if (legacyDirections?.Length > 0)
                        {
                            // Use legacy DirectionRig static method
                            DirectionRig.PoseCamera(
                                SceneBuilder.GetCaptureCamera(),
                                target,
                                direction,
                                legacyPixelSize,
                                legacyGameType,
                                legacyFacingAxis,
                                legacyBaseYawOffset,
                                legacyRotateAxis,
                                legacyCustomOrthoSize
                            );
                        }
                        else if (captureDirections.Count > 0)
                        {
                            // For now, skip new direction rig system since DirectionRig is static
                            // This would need to be redesigned if we want to support custom rigs
                        }

                        var frames = await CaptureTarget(target, clip);
                        var key = $"{targetName}_{direction}";
                        CapturedFrames[key] = frames;

                        completedOperations++;
                    }
                }

                // Export results
                Status = "Exporting results...";
                progress?.Report((0.9f, Status));

                ExportResults();

                Progress = 1f;
                Status = "Batch capture complete!";
                progress?.Report((1f, Status));
            }
            finally
            {
                // Cleanup
                SceneBuilder.CleanupCaptureScene();
            }
        }

        /// <summary>
        /// Capture a single target
        /// </summary>
        private async Task<List<Texture2D>> CaptureTarget(GameObject target, AnimationClip clip)
        {
            if (clip == null)
            {
                // Static capture
                return new List<Texture2D> { CaptureService.CaptureFrame() };
            }
            else
            {
                // Animation capture
                if (keyframesOnly)
                {
                    return await CaptureService.CaptureKeyframes(clip, target);
                }
                else
                {
                    return await CaptureService.CaptureIntervals(clip, target, frameCount);
                }
            }
        }

        /// <summary>
        /// Export captured results
        /// </summary>
        private void ExportResults()
        {
            foreach (var kvp in CapturedFrames)
            {
                var name = kvp.Key;
                var frames = kvp.Value;

                if (frames.Count == 0) continue;

                // Export individual frames
                CaptureService.ExportFrames(frames, name, outputFolder);

                // Create sprite sheet if requested
                if (createSpriteSheets && frames.Count > 1)
                {
                    var spriteSheet = CaptureService.CreateSpriteSheet(frames);
                    CaptureService.ExportSpriteSheet(spriteSheet, $"{name}_sheet", outputFolder);
                }
            }
        }

        /// <summary>
        /// Add a capture target
        /// </summary>
        public void AddTarget(GameObject target, AnimationClip clip = null)
        {
            if (target == null || captureTargets.Contains(target)) return;

            captureTargets.Add(target);
            animationClips.Add(clip);
        }

        /// <summary>
        /// Remove a capture target
        /// </summary>
        public void RemoveTarget(GameObject target)
        {
            var index = captureTargets.IndexOf(target);
            if (index >= 0)
            {
                captureTargets.RemoveAt(index);
                if (index < animationClips.Count)
                    animationClips.RemoveAt(index);
            }
        }

        /// <summary>
        /// Clear all targets
        /// </summary>
        public void ClearTargets()
        {
            captureTargets.Clear();
            animationClips.Clear();
        }

        /// <summary>
        /// Get capture summary
        /// </summary>
        public string GetSummary()
        {
            var totalFrames = CapturedFrames?.Sum(kvp => kvp.Value.Count) ?? 0;
            return $"Captured {CapturedFrames?.Count ?? 0} items, {totalFrames} total frames";
        }

        /// <summary>
        /// Legacy method for compatibility - starts batch capture with detailed parameters
        /// </summary>
        public void StartBatchCapture(
            AnimationClip[] clips,
            Animator animator,
            Animation animation,
            KSClipRunner clipRunner,
            Camera camera,
            int width,
            int height,
            int fps,
            int maxFrames,
            string outputRoot,
            string[] directions,
            bool dropLastFrame,
            int pixelSize,
            bool trimSprites,
            bool mirrorSprites,
            GameType gameType,
            FacingAxis facingAxis,
            float baseYawOffset,
            DirectionMask directionMask,
            bool disableRootMotion,
            RotateAxis rotateAxis,
            bool generateClips,
            float? customOrthoSize = null,
            MemoryManager memoryManager = null,
            TexturePool texturePool = null,
            AsyncFrameProcessor asyncFrameProcessor = null)
        {
            // Store legacy parameters for use during capture
            this.legacyDirections = directions ?? new string[0];
            this.legacyGameType = gameType;
            this.legacyFacingAxis = facingAxis;
            this.legacyBaseYawOffset = baseYawOffset;
            this.legacyDirectionMask = directionMask;
            this.legacyRotateAxis = rotateAxis;
            this.legacyPixelSize = pixelSize;
            this.legacyCustomOrthoSize = customOrthoSize;

            // Convert legacy parameters to new format
            captureTargets.Clear();
            animationClips.Clear();
            captureDirections.Clear(); // We'll handle directions differently

            if (animator != null && animator.gameObject != null)
            {
                captureTargets.Add(animator.gameObject);
                if (clips != null && clips.Length > 0)
                    animationClips.AddRange(clips);
            }

            // Set capture settings
            resolutionWidth = width;
            resolutionHeight = height;
            outputFolder = outputRoot;
            keyframesOnly = !generateClips;
            frameCount = maxFrames;

            // Start the batch capture coroutine directly with the provided camera
            StartCoroutine(BatchCaptureCoroutine(clips, animator, animation, clipRunner, camera, width, height, fps, maxFrames, outputRoot, directions, dropLastFrame, pixelSize, trimSprites, mirrorSprites, gameType, facingAxis, baseYawOffset, directionMask, disableRootMotion, rotateAxis, generateClips, customOrthoSize, memoryManager, texturePool, asyncFrameProcessor));
        }

        private System.Collections.IEnumerator BatchCaptureCoroutine(AnimationClip[] clips, Animator animator, Animation animation, KSClipRunner clipRunner, Camera camera, int width, int height, int fps, int maxFrames, string outputRoot, string[] directions, bool dropLastFrame, int pixelSize, bool trimSprites, bool mirrorSprites, GameType gameType, FacingAxis facingAxis, float baseYawOffset, DirectionMask directionMask, bool disableRootMotion, RotateAxis rotateAxis, bool generateClips, float? customOrthoSize = null, MemoryManager memoryManager = null, TexturePool texturePool = null, AsyncFrameProcessor asyncFrameProcessor = null)
        {
            // Disable root motion if requested (only for Animator)
            bool originalApplyRootMotion = false;
            if (animator != null && disableRootMotion)
            {
                originalApplyRootMotion = animator.applyRootMotion;
                animator.applyRootMotion = false;
            }

            try
            {
                foreach (string direction in directions)
                {
                    // Pose camera for direction (without custom ortho size initially)
                    DirectionRig.PoseCamera(camera, gameObject, direction, pixelSize, gameType, facingAxis, baseYawOffset, rotateAxis, null);

                    // For auto-fit mode (non-trimmed), calculate orthographic size after camera positioning
                    if (!trimSprites && customOrthoSize == null)
                    {
                        // Calculate bounds of the character in world space
                        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
                        Bounds characterBounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(gameObject.transform.position, Vector3.one);
                        for (int i = 1; i < renderers.Length; i++)
                        {
                            characterBounds.Encapsulate(renderers[i].bounds);
                        }

                        // Calculate orthographic size to fit the character bounds with fill percentage
                        float aspectRatio = (float)width / height;
                        float boundsWidth = characterBounds.size.x;
                        float boundsHeight = characterBounds.size.y;

                        // Apply fill percentage (convert percentage to decimal and invert for scaling)
                        float fillScale = 100f / 75f; // Default 75% fill for auto-fit

                        // Calculate orthographic size needed to fit width and height
                        float orthoSizeForWidth = (boundsWidth * fillScale) / (2f * aspectRatio);
                        float orthoSizeForHeight = (boundsHeight * fillScale) / 2f;
                        float requiredOrthoSize = Mathf.Max(orthoSizeForWidth, orthoSizeForHeight);

                        // CRITICAL FIX: For smaller resolutions, ensure we don't zoom in too much
                        float referenceResolution = 1024f;
                        float resolutionScale = referenceResolution / Mathf.Max(width, height);
                        requiredOrthoSize *= Mathf.Max(1f, resolutionScale * 0.8f);

                        // Ensure minimum size to prevent extreme zoom
                        requiredOrthoSize = Mathf.Max(requiredOrthoSize, 0.1f);

                        camera.orthographicSize = requiredOrthoSize;
                        Debug.Log($"📏 Auto-fit orthographic size calculated after positioning: {requiredOrthoSize} (direction: {direction})");
                    }
                    else if (customOrthoSize.HasValue)
                    {
                        // Use the provided custom orthographic size
                        camera.orthographicSize = customOrthoSize.Value;
                    }

                    foreach (AnimationClip clip in clips)
                    {
                        yield return StartCoroutine(CaptureClipCoroutine(clip, animator, animation, clipRunner, camera, width, height, fps, maxFrames, outputRoot, direction, dropLastFrame, trimSprites, mirrorSprites, memoryManager, texturePool, asyncFrameProcessor));
                    }
                }

                Debug.Log("KS SnapStudio: Batch capture completed.");

                // Cleanup
                Object.Destroy(camera.gameObject);
                // Don't destroy the character object as it's from the scene
                Object.Destroy(this);
            }
            finally
            {
                // Restore original root motion setting (only for Animator)
                if (animator != null)
                {
                    animator.applyRootMotion = originalApplyRootMotion;
                }
            }
        }

        private System.Collections.IEnumerator CaptureClipCoroutine(AnimationClip clip, Animator animator, Animation animation, KSClipRunner clipRunner, Camera camera, int width, int height, int fps, int maxFrames, string outputRoot, string direction, bool dropLastFrame, bool trimSprites, bool mirrorSprites, MemoryManager memoryManager = null, TexturePool texturePool = null, AsyncFrameProcessor asyncFrameProcessor = null)
        {
            // Get character name from the game object
            string characterName = (animator != null ? animator.gameObject.name : animation.gameObject.name).Replace(" ", "_").Replace("/", "_").Replace("\\", "_");

            // Prepare output directory - organize by character name, then animation clip name
            string sanitizedClipName = clip.name.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
            string characterDir = System.IO.Path.Combine(outputRoot, characterName);
            string outputDir = System.IO.Path.Combine(characterDir, sanitizedClipName);

            // Create both character and animation directories
            System.IO.Directory.CreateDirectory(characterDir);
            System.IO.Directory.CreateDirectory(outputDir);

            Debug.Log($"📁 Creating character folder: {characterDir}");
            Debug.Log($"📁 Creating animation folder: {outputDir} (for character: {characterName}, clip: {clip.name})");

            if (!System.IO.Directory.Exists(characterDir))
            {
                Debug.LogError($"❌ Failed to create character directory: {characterDir}");
            }
            else if (!System.IO.Directory.Exists(outputDir))
            {
                Debug.LogError($"❌ Failed to create animation directory: {outputDir}");
            }
            else
            {
                Debug.Log($"✅ Successfully created directories: {characterDir} -> {outputDir}");
            }

            // Calculate frame count (use full animation length for smooth playback, but cap at reasonable maximum)
            int totalFrames = Mathf.Min(maxFrames * 2, (int)(clip.length * fps)); // Allow up to 2x maxFrames for longer animations
            if (dropLastFrame)
                totalFrames = Mathf.Max(0, totalFrames - 1);

            // OPTIMIZATION: Calculate dynamic bounds to prevent frame cropping during animation
            Bounds dynamicBounds = CalculateDynamicBounds(animator != null ? animator.gameObject : animation.gameObject, clip, totalFrames, fps);
            float requiredOrthoSize = CalculateOrthoSizeForBounds(dynamicBounds, (float)width / height);
            camera.orthographicSize = Mathf.Max(requiredOrthoSize, camera.orthographicSize); // Don't shrink if already larger
            Debug.Log($"📏 Dynamic bounds calculated for {clip.name}: orthoSize={requiredOrthoSize}, bounds={dynamicBounds.size}");

            // OPTIMIZATION: Initialize active memory monitoring and texture pool optimization
            if (memoryManager != null)
            {
                // Start memory monitoring by triggering initial cleanup
                memoryManager.ForceCleanup();
                Debug.Log("🧠 Memory monitoring started for capture session");
            }

            if (texturePool != null)
            {
                // Clear existing pool and let it rebuild optimally
                texturePool.ClearPool();
                Debug.Log($"🏊 Texture pool cleared for optimal performance");
            }

            // Disable animator and clip runner during capture to prevent interference with manual sampling
            bool wasAnimatorEnabled = false;
            bool wasClipRunnerEnabled = false;
            if (animator != null)
            {
                wasAnimatorEnabled = animator.enabled;
                animator.enabled = false;
            }
            if (clipRunner != null)
            {
                wasClipRunnerEnabled = clipRunner.enabled;
                clipRunner.enabled = false;
            }

            try
            {
                for (int frame = 0; frame < totalFrames; frame++)
                {
                    // Sample animation at evenly distributed time points for smoother playback
                    float normalizedTime = frame / (float)(totalFrames - (dropLastFrame ? 1 : 0));
                    float time = Mathf.Lerp(0f, clip.length, normalizedTime);
                    clip.SampleAnimation(animator != null ? animator.gameObject : animation.gameObject, time);

                    // Wait for the pose to be applied
                    yield return null;

                    // OPTIMIZATION: Active memory monitoring during capture
                    if (memoryManager != null && frame % 10 == 0) // Check every 10 frames
                    {
                        memoryManager.LogMemoryUsage($"Frame {frame}/{totalFrames} captured");
                        // Check if memory usage is getting high and trigger cleanup
                        if (memoryManager.IsMemoryLow)
                        {
                            yield return StartCoroutine(PerformMemoryCleanup());
                        }
                    }

                    // Capture frame with enhanced optimizations
                    CaptureFrame(camera, width, height, outputDir, frame, trimSprites, mirrorSprites, direction, clip.name, sanitizedClipName, memoryManager, texturePool, asyncFrameProcessor);
                }

                // OPTIMIZATION: Final memory cleanup and reporting
                if (memoryManager != null)
                {
                    memoryManager.ForceCleanup();
                    Debug.Log("🧠 Memory monitoring completed - final cleanup done");
                }
            }
            finally
            {
                // Re-enable animator and clip runner
                if (animator != null)
                {
                    animator.enabled = wasAnimatorEnabled;
                }
                if (clipRunner != null)
                {
                    clipRunner.enabled = wasClipRunnerEnabled;
                }
            }
        }

        private void CaptureFrame(Camera camera, int width, int height, string outputDir, int frame, bool trimSprites, bool mirrorSprites, string direction, string clipName, string sanitizedClipName, MemoryManager memoryManager = null, TexturePool texturePool = null, AsyncFrameProcessor asyncFrameProcessor = null)
        {
            // Try optimized capture first if components are available
            if (asyncFrameProcessor != null && texturePool != null)
            {
                try
                {
                    // Create render texture
                    var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                    camera.targetTexture = rt;

                    // Set camera background to clear for transparent backgrounds
                    Color originalBackground = camera.backgroundColor;
                    camera.backgroundColor = Color.clear;

                    // Render
                    camera.Render();
                    var diffuseTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                    RenderTexture.active = rt;
                    diffuseTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    diffuseTex.Apply();

                    // Trim if enabled
                    if (trimSprites)
                    {
                        diffuseTex = CaptureService.TrimTexture(diffuseTex);
                    }

                    // Queue frame for async processing
                    string fileName = $"{sanitizedClipName}_{direction}_{frame:0000}.png";
                    string diffusePath = System.IO.Path.Combine(outputDir, fileName);

                    asyncFrameProcessor.QueueFrame(diffuseTex, diffusePath,
                        () => {
                            Debug.Log($"📁 Async saved frame to: {diffusePath} (Clip: {clipName})");
                        },
                        (processedTexture) => {
                            // Texture processed, return to pool if available
                            if (texturePool != null)
                            {
                                texturePool.ReturnTexture(processedTexture);
                            }
                        });

                    // Mirror for left direction if enabled and this is a right-facing capture
                    if (mirrorSprites && direction.ToLower().Contains("right"))
                    {
                        Texture2D mirroredTex = FlipTextureHorizontally(diffuseTex);
                        string mirroredFileName = $"{sanitizedClipName}_left_{frame:0000}.png";
                        string mirroredPath = System.IO.Path.Combine(outputDir, mirroredFileName);
                        System.IO.File.WriteAllBytes(mirroredPath, mirroredTex.EncodeToPNG());
                        Object.DestroyImmediate(mirroredTex);
                    }

                    // Restore original camera background color
                    camera.backgroundColor = originalBackground;

                    // Cleanup
                    RenderTexture.active = null;
                    camera.targetTexture = null;
                    Object.Destroy(rt);

                    // Log memory usage if available
                    if (memoryManager != null)
                    {
                        memoryManager.LogMemoryUsage($"Frame {frame} captured");
                    }

                    return;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Optimized capture failed, falling back to original method: {ex.Message}");
                }
            }

            // Fallback to original capture method
            CaptureFrameOriginal(camera, width, height, outputDir, frame, trimSprites, mirrorSprites, direction, clipName, sanitizedClipName);
        }

        private void CaptureFrameOriginal(Camera camera, int width, int height, string outputDir, int frame, bool trimSprites, bool mirrorSprites, string direction, string clipName, string sanitizedClipName)
        {
            // Create render texture
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = rt;

            // Set camera background to clear for transparent backgrounds
            Color originalBackground = camera.backgroundColor;
            camera.backgroundColor = Color.clear;

            // Render
            camera.Render();
            var diffuseTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture.active = rt;
            diffuseTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            diffuseTex.Apply();

            if (trimSprites)
            {
                diffuseTex = CaptureService.TrimTexture(diffuseTex);
            }

            // Save frame
            string fileName = $"{sanitizedClipName}_{direction}_{frame:0000}.png";
            string diffusePath = System.IO.Path.Combine(outputDir, fileName);
            System.IO.File.WriteAllBytes(diffusePath, diffuseTex.EncodeToPNG());

            Debug.Log($"📁 Saved frame to: {diffusePath} (Clip: {clipName})");

            // Mirror for left direction if enabled and this is a right-facing capture
            if (mirrorSprites && direction.ToLower().Contains("right"))
            {
                Texture2D mirroredTex = FlipTextureHorizontally(diffuseTex);
                string mirroredFileName = $"{sanitizedClipName}_left_{frame:0000}.png";
                string mirroredPath = System.IO.Path.Combine(outputDir, mirroredFileName);
                System.IO.File.WriteAllBytes(mirroredPath, mirroredTex.EncodeToPNG());
                Object.DestroyImmediate(mirroredTex);
            }

            // Restore original camera background color
            camera.backgroundColor = originalBackground;

            // Cleanup
            RenderTexture.active = null;
            camera.targetTexture = null;
            Object.Destroy(rt);
            Object.Destroy(diffuseTex);
        }

        private Texture2D FlipTextureHorizontally(Texture2D original)
        {
            Texture2D flipped = new Texture2D(original.width, original.height);
            for (int y = 0; y < original.height; y++)
            {
                for (int x = 0; x < original.width; x++)
                {
                    flipped.SetPixel(x, y, original.GetPixel(original.width - 1 - x, y));
                }
            }
            flipped.Apply();
            return flipped;
        }

        /// <summary>
        /// Calculate dynamic bounds for a character throughout an animation to prevent frame cropping
        /// </summary>
        private Bounds CalculateDynamicBounds(GameObject character, AnimationClip clip, int totalFrames, int fps)
        {
            Bounds totalBounds = new Bounds(character.transform.position, Vector3.zero);
            bool firstFrame = true;

            // Sample animation at key points to find maximum bounds
            int samplePoints = Mathf.Min(totalFrames, 10); // Sample up to 10 points for performance
            for (int i = 0; i < samplePoints; i++)
            {
                float normalizedTime = i / (float)(samplePoints - 1);
                float time = Mathf.Lerp(0f, clip.length, normalizedTime);

                // Sample animation
                clip.SampleAnimation(character, time);

                // Calculate bounds of all renderers
                Renderer[] renderers = character.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds frameBounds = renderers[0].bounds;
                    for (int r = 1; r < renderers.Length; r++)
                    {
                        frameBounds.Encapsulate(renderers[r].bounds);
                    }

                    if (firstFrame)
                    {
                        totalBounds = frameBounds;
                        firstFrame = false;
                    }
                    else
                    {
                        totalBounds.Encapsulate(frameBounds);
                    }
                }
            }

            // Add small padding to prevent edge clipping
            totalBounds.size *= 1.05f;
            return totalBounds;
        }

        /// <summary>
        /// Calculate orthographic size needed to fit bounds with proper aspect ratio
        /// </summary>
        private float CalculateOrthoSizeForBounds(Bounds bounds, float aspectRatio)
        {
            float boundsWidth = bounds.size.x;
            float boundsHeight = bounds.size.y;

            // Calculate orthographic size needed for both dimensions
            float orthoSizeForWidth = (boundsWidth * 0.5f) / aspectRatio;
            float orthoSizeForHeight = boundsHeight * 0.5f;

            return Mathf.Max(orthoSizeForWidth, orthoSizeForHeight);
        }

        /// <summary>
        /// Perform memory cleanup during capture to prevent OOM
        /// </summary>
        private System.Collections.IEnumerator PerformMemoryCleanup()
        {
            Debug.Log("🧹 Performing memory cleanup during capture...");

            // Force garbage collection
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            // Wait a frame for cleanup to complete
            yield return null;

            Debug.Log("✅ Memory cleanup completed");
        }

        /// <summary>
        /// Categorize animation clips into types based on naming patterns
        /// </summary>
        private string GetAnimationType(string clipName)
        {
            string lowerName = clipName.ToLower();

            // Idle animations
            if (lowerName.Contains("idle") || lowerName.Contains("stand") || lowerName.Contains("wait"))
                return "Idle";

            // Walking animations
            if (lowerName.Contains("walk") || lowerName.Contains("stroll"))
                return "Walking";

            // Running animations
            if (lowerName.Contains("run") || lowerName.Contains("sprint") || lowerName.Contains("dash"))
                return "Running";

            // Jumping animations
            if (lowerName.Contains("jump") || lowerName.Contains("leap") || lowerName.Contains("hop"))
                return "Jumping";

            // Combat/Attack animations
            if (lowerName.Contains("attack") || lowerName.Contains("fight") || lowerName.Contains("strike") || lowerName.Contains("punch") || lowerName.Contains("kick"))
                return "Combat";

            // Death/Dying animations
            if (lowerName.Contains("death") || lowerName.Contains("die") || lowerName.Contains("dead") || lowerName.Contains("dying"))
                return "Death";

            // Special/Custom animations - try to extract meaningful category
            if (lowerName.Contains("cast") || lowerName.Contains("spell") || lowerName.Contains("magic"))
                return "Magic";

            if (lowerName.Contains("dance") || lowerName.Contains("celebrat"))
                return "Dance";

            if (lowerName.Contains("sleep") || lowerName.Contains("rest"))
                return "Rest";

            // Default fallback - use the first word or "Other"
            string[] parts = clipName.Split('_', ' ');
            if (parts.Length > 0 && parts[0].Length > 0)
            {
                // Capitalize first letter
                string category = parts[0].Substring(0, 1).ToUpper() + parts[0].Substring(1).ToLower();
                return category;
            }

            return "Other";
        }

        // Legacy parameter storage
        private string[] legacyDirections;
        private GameType legacyGameType;
        private FacingAxis legacyFacingAxis;
        private float legacyBaseYawOffset;
        private DirectionMask legacyDirectionMask;
        private RotateAxis legacyRotateAxis;
        private int legacyPixelSize;
        private float? legacyCustomOrthoSize;
    }
}
