using UnityEngine;
using UnityEditor;
using KalponicGames.KS_SnapStudio;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Handles frame information display overlay
    /// Single Responsibility: Display frame metadata and information
    /// </summary>
    public class FrameInfoDisplay : IPreviewComponent, IToggleable, IFrameInfoDisplay
    {
        private bool showFrameInfo = true;
        private GUIStyle infoStyle;

        // Constants for consistent UI
        private const int INFO_FONT_SIZE = 10;
        private const float INFO_RECT_HEIGHT = 20f;

        public string ComponentName => "FrameInfoDisplay";
        public bool IsEnabled { get => showFrameInfo; set => showFrameInfo = value; }
        public bool ShowFrameInfo { get => showFrameInfo; set => showFrameInfo = value; }

        public void Initialize()
        {
            infoStyle = new GUIStyle(EditorStyles.label);
            infoStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            infoStyle.fontSize = INFO_FONT_SIZE;

            Logger.LogSuccess(ComponentName, "Initialized");
        }

        public void OnGUI()
        {
            DrawInfoToggle();
        }

        public void Cleanup()
        {
            // Cleanup resources if needed
            Logger.LogInfo(ComponentName, "Cleaned up");
        }

        public void DrawInfoToggle()
        {
            showFrameInfo = EditorGUILayout.Toggle("Frame Info", showFrameInfo, GUILayout.Width(80));
        }

        public void DrawFrameInfo(Rect displayRect, int currentFrameIndex, int totalFrames, int originalFrameIndex, Texture2D texture)
        {
            if (!showFrameInfo || texture == null) return;

            // Validate parameters using ValidationUtils
            var validation = ValidationUtils.ValidatePreviewDisplay(currentFrameIndex, totalFrames, texture, displayRect);
            if (!validation.IsValid)
            {
                Logger.LogWarning(ComponentName, $"Invalid parameters: {validation.GetErrorMessage()}");
                return;
            }

            string infoText = $"Frame {currentFrameIndex + 1}/{totalFrames} (Original: {originalFrameIndex}) | {texture.width}x{texture.height}";
            Rect infoRect = new Rect(displayRect.x, displayRect.y + displayRect.height + 5, displayRect.width, INFO_RECT_HEIGHT);

            EditorGUI.LabelField(infoRect, infoText, infoStyle);
        }

        public void DrawDetailedInfo(Rect rect, Frame frame, Texture2D texture)
        {
            if (!showFrameInfo) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Frame Details", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Index: {frame.Index}");
                EditorGUILayout.LabelField($"Time: {frame.Time:F3}s");

                if (texture != null)
                {
                    EditorGUILayout.LabelField($"Resolution: {texture.width}x{texture.height}");
                    EditorGUILayout.LabelField($"Format: {texture.format}");
                }
            }
        }
    }
}
