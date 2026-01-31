using UnityEngine;
using UnityEditor;
using KalponicGames.KS_SnapStudio;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Handles timeline scrubbing and frame navigation
    /// Single Responsibility: Timeline interaction and frame selection
    /// </summary>
    public class TimelineController : IPreviewComponent, IInputHandler, ITimelineController
    {
        private int maxFrames = 0;
        private float timelineValue = 0f;
        private bool isScrubbing = false;

        // Events
        public event System.Action<int> OnFrameSelected;

        public string ComponentName => "TimelineController";
        public bool IsEnabled { get; set; } = true;

        public int TotalFrames
        {
            get => maxFrames;
            set => maxFrames = Mathf.Max(0, value);
        }

        public void Initialize()
        {
            Logger.LogSuccess(ComponentName, "Initialized");
        }

        public void OnGUI()
        {
            DrawTimeline();
        }

        public void Cleanup()
        {
            Logger.LogInfo(ComponentName, "Cleaned up");
        }

        public void HandleInput(Event currentEvent, Rect activeRect)
        {
            // Handle keyboard shortcuts for timeline navigation
            if (currentEvent.type == EventType.KeyDown)
            {
                switch (currentEvent.keyCode)
                {
                    case KeyCode.LeftArrow:
                        OnFrameSelected?.Invoke(Mathf.Max(0, Mathf.RoundToInt(timelineValue) - 1));
                        Logger.LogInteraction(ComponentName, "Navigate Left");
                        break;
                    case KeyCode.RightArrow:
                        OnFrameSelected?.Invoke(Mathf.Min(maxFrames - 1, Mathf.RoundToInt(timelineValue) + 1));
                        Logger.LogInteraction(ComponentName, "Navigate Right");
                        break;
                    case KeyCode.Home:
                        OnFrameSelected?.Invoke(0);
                        Logger.LogInteraction(ComponentName, "Navigate to Start");
                        break;
                    case KeyCode.End:
                        OnFrameSelected?.Invoke(maxFrames - 1);
                        Logger.LogInteraction(ComponentName, "Navigate to End");
                        break;
                }
            }
        }

        public void DrawTimeline()
        {
            if (maxFrames <= 1) return;

            EditorGUILayout.LabelField("Timeline", EditorStyles.boldLabel);
            float newValue = EditorGUILayout.Slider(timelineValue, 0, maxFrames - 1);

            if (newValue != timelineValue)
            {
                int frameIndex = Mathf.RoundToInt(newValue);

                // Validate frame index
                var validation = ValidationUtils.ValidateFrameIndex(frameIndex, maxFrames);
                if (!validation.IsValid)
                {
                    Logger.LogWarning(ComponentName, $"Invalid frame index: {validation.GetErrorMessage()}");
                    return;
                }

                OnFrameSelected?.Invoke(frameIndex);
                timelineValue = newValue;
                isScrubbing = true;
                Logger.LogInfo(ComponentName, $"Scrubbed to frame {frameIndex}");
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                isScrubbing = false;
            }
        }

        public void DrawFrameNavigation(Rect rect, int currentFrame, int maxFrames)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("◀◀", GUILayout.Width(30)))
                {
                    OnFrameSelected?.Invoke(0);
                }

                if (GUILayout.Button("◀", GUILayout.Width(30)))
                {
                    OnFrameSelected?.Invoke(Mathf.Max(0, currentFrame - 1));
                }

                EditorGUILayout.LabelField($"{currentFrame + 1}/{maxFrames}", GUILayout.Width(60));

                if (GUILayout.Button("▶", GUILayout.Width(30)))
                {
                    OnFrameSelected?.Invoke(Mathf.Min(maxFrames - 1, currentFrame + 1));
                }

                if (GUILayout.Button("▶▶", GUILayout.Width(30)))
                {
                    OnFrameSelected?.Invoke(maxFrames - 1);
                }
            }
        }

        public bool IsScrubbing => isScrubbing;
    }
}
