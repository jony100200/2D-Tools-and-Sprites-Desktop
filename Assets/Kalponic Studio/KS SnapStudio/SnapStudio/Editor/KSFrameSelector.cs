using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using KalponicGames.KS_SnapStudio;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    public class KSFrameSelector : EditorWindow
    {
        private KSPlayModeCapture captureComponent = null;
        private Vector2 scrollPosition = Vector2.zero;
        private const int THUMBNAIL_SIZE = 64;
        private const int PADDING = 4;

        public static void ShowWindow(KSPlayModeCapture capture)
        {
            var window = GetWindow<KSFrameSelector>("Frame Selector");
            window.captureComponent = capture;
            window.minSize = new Vector2(400, 300);
        }

        void OnGUI()
        {
            if (captureComponent == null || captureComponent.Sampling == null)
            {
                EditorGUILayout.HelpBox("No sampling data available. Please sample an animation first.", MessageType.Warning);
                return;
            }

            var sampling = captureComponent.Sampling;
            var selectedFrames = sampling.SelectedFrames;
            var trimMainTextures = sampling.TrimMainTextures;

            if (trimMainTextures == null || trimMainTextures.Length == 0)
            {
                EditorGUILayout.HelpBox("No textures available. Please sample an animation first.", MessageType.Info);
                return;
            }

            // Selection controls
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select All"))
                {
                    selectedFrames.Clear();
                    for (int i = 0; i < trimMainTextures.Length; i++)
                    {
                        selectedFrames.Add(new Frame(i, 0f));
                    }
                }

                if (GUILayout.Button("Clear All"))
                {
                    selectedFrames.Clear();
                }

                EditorGUILayout.LabelField($"Selected: {selectedFrames.Count}/{trimMainTextures.Length}", EditorStyles.boldLabel);
            }

            EditorGUILayout.Space();

            // Frame grid
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int columns = Mathf.FloorToInt((position.width - 20) / (THUMBNAIL_SIZE + PADDING));
            columns = Mathf.Max(1, columns);

            int currentColumn = 0;

            for (int i = 0; i < trimMainTextures.Length; i++)
            {
                if (currentColumn == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }

                var texture = trimMainTextures[i];
                if (texture != null)
                {
                    // Check if frame is selected
                    bool isSelected = selectedFrames.Exists(f => f.Index == i);

                    // Frame button
                    var buttonStyle = new GUIStyle(GUI.skin.button);
                    buttonStyle.normal.background = isSelected ? Texture2D.whiteTexture : Texture2D.grayTexture;

                    if (GUILayout.Button("", buttonStyle, GUILayout.Width(THUMBNAIL_SIZE), GUILayout.Height(THUMBNAIL_SIZE)))
                    {
                        if (isSelected)
                        {
                            selectedFrames.RemoveAll(f => f.Index == i);
                        }
                        else
                        {
                            selectedFrames.Add(new Frame(i, 0f));
                        }
                    }

                    // Draw texture
                    var lastRect = GUILayoutUtility.GetLastRect();
                    EditorGUI.DrawTextureTransparent(lastRect, texture);

                    // Draw selection border
                    if (isSelected)
                    {
                        DrawingHelper.StrokeRect(lastRect, Color.green, 2f);
                    }

                    // Frame number label
                    var labelRect = new Rect(lastRect.x, lastRect.y + lastRect.height - 15, lastRect.width, 15);
                    EditorGUI.LabelField(labelRect, i.ToString(), EditorStyles.miniLabel);
                }

                currentColumn++;
                if (currentColumn >= columns)
                {
                    EditorGUILayout.EndHorizontal();
                    currentColumn = 0;
                }
            }

            if (currentColumn > 0)
            {
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    // Helper class for drawing rectangles.
    public static class DrawingHelper
    {
        public static void StrokeRect(Rect rect, Color color, float width)
        {
            var oldColor = Handles.color;
            Handles.color = color;

            // Top
            Handles.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin));
            // Bottom
            Handles.DrawLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMax, rect.yMax));
            // Left
            Handles.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMin, rect.yMax));
            // Right
            Handles.DrawLine(new Vector3(rect.xMax, rect.yMin), new Vector3(rect.xMax, rect.yMax));

            Handles.color = oldColor;
        }
    }
}
