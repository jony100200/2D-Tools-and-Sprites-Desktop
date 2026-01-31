using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using KalponicGames.KS_SnapStudio;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Handles animation export functionality
    /// Single Responsibility: Export frames to various formats
    /// </summary>
    public class AnimationExporter : IAnimationExporter
    {
        private string exportPath = "";
        private string exportName = "Animation";
        private ExportFormat exportFormat = ExportFormat.PNG;
        private bool exportAllFrames = true;
        private int startFrame = 0;
        private int endFrame = 0;

        public enum ExportFormat
        {
            PNG,
            JPG,
            TGA,
            EXR
        }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(exportPath))
            {
                exportPath = Application.dataPath;
            }
        }

        public void DrawExportControls(List<Frame> frames)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Export Settings", EditorStyles.boldLabel);

                exportName = EditorGUILayout.TextField("Export Name", exportName);
                exportFormat = (ExportFormat)EditorGUILayout.EnumPopup("Format", exportFormat);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel("Export Path");
                    if (GUILayout.Button("Browse", GUILayout.Width(60)))
                    {
                        string selectedPath = EditorUtility.OpenFolderPanel("Select Export Folder", exportPath, "");
                        if (!string.IsNullOrEmpty(selectedPath))
                        {
                            exportPath = selectedPath;
                        }
                    }
                    EditorGUILayout.LabelField(exportPath, EditorStyles.textField);
                }

                exportAllFrames = EditorGUILayout.Toggle("Export All Frames", exportAllFrames);

                if (!exportAllFrames)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        startFrame = EditorGUILayout.IntField("Start Frame", startFrame);
                        endFrame = EditorGUILayout.IntField("End Frame", endFrame);
                    }

                    startFrame = Mathf.Clamp(startFrame, 0, frames.Count - 1);
                    endFrame = Mathf.Clamp(endFrame, startFrame, frames.Count - 1);
                }

                if (GUILayout.Button("Export Animation"))
                {
                    ExportFrames(frames);
                }
            }
        }

        private void ExportFrames(List<Frame> frames)
        {
            if (frames == null || frames.Count == 0)
            {
                EditorUtility.DisplayDialog("Export Error", "No frames to export!", "OK");
                return;
            }

            int start = exportAllFrames ? 0 : startFrame;
            int end = exportAllFrames ? frames.Count - 1 : endFrame;

            string basePath = Path.Combine(exportPath, exportName);
            Directory.CreateDirectory(basePath);

            for (int i = start; i <= end; i++)
            {
                var frame = frames[i];
                // Get texture from sampling data using frame index
                var sampling = ServiceLocator.CaptureSystem?.Sampling;
                if (sampling != null && frame.Index < sampling.TrimMainTextures.Length)
                {
                    var texture = sampling.TrimMainTextures[frame.Index];
                    if (texture != null)
                    {
                        string fileName = $"{exportName}_{i:D4}.{GetExtension(exportFormat)}";
                        string filePath = Path.Combine(basePath, fileName);

                        byte[] bytes = GetTextureBytes(texture, exportFormat);
                        File.WriteAllBytes(filePath, bytes);
                    }
                }
            }

            EditorUtility.DisplayDialog("Export Complete", $"Exported {end - start + 1} frames to {basePath}", "OK");
            EditorUtility.RevealInFinder(basePath);
            Debug.Log($"📤 AnimationExporter: Successfully exported {end - start + 1} frames to {basePath}");
        }

        private byte[] GetTextureBytes(Texture2D texture, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.PNG:
                    return texture.EncodeToPNG();
                case ExportFormat.JPG:
                    return texture.EncodeToJPG();
                case ExportFormat.TGA:
                    return texture.EncodeToTGA();
                case ExportFormat.EXR:
                    return texture.EncodeToEXR();
                default:
                    return texture.EncodeToPNG();
            }
        }

        private string GetExtension(ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.PNG:
                    return "png";
                case ExportFormat.JPG:
                    return "jpg";
                case ExportFormat.TGA:
                    return "tga";
                case ExportFormat.EXR:
                    return "exr";
                default:
                    return "png";
            }
        }
    }
}
