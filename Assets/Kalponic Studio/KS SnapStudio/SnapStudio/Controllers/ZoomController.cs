using UnityEngine;
using KalponicGames.KS_SnapStudio;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Handles zoom controls and zoom calculations
    /// Single Responsibility: Zoom level management and UI
    /// </summary>
    public class ZoomController : IPreviewComponent, IInputHandler, IToggleable, IZoomController
    {
        private float zoomLevel = 1f;
        private const float MIN_ZOOM = 0.1f;
        private const float MAX_ZOOM = 3f;

        // Events
        public event System.Action<float> OnZoomChanged;

        public string ComponentName => "ZoomController";
        public bool IsEnabled { get; set; } = true;

        public float ZoomLevel
        {
            get => zoomLevel;
            set
            {
                float newZoom = Mathf.Clamp(value, MIN_ZOOM, MAX_ZOOM);
                if (newZoom != zoomLevel)
                {
                    zoomLevel = newZoom;
                    OnZoomChanged?.Invoke(zoomLevel);
                    Logger.LogInfo(ComponentName, $"Zoom level changed to {zoomLevel:F2}x");
                }
            }
        }

        public void Initialize()
        {
            zoomLevel = 1.0f;
            Logger.LogSuccess(ComponentName, "Initialized");
        }

        public void OnGUI()
        {
            if (!IsEnabled) return;
            DrawZoomControls();
        }

        public void Cleanup()
        {
            Logger.LogInfo(ComponentName, "Cleaned up");
        }

        public void HandleInput(Event e, Rect activeRect)
        {
            if (!IsEnabled) return;

            HandleZoomInput(e, activeRect);
        }

        public void DrawZoomControls()
        {
            using (new UnityEngine.GUILayout.HorizontalScope())
            {
                UnityEngine.GUILayout.Label("Zoom:", UnityEngine.GUILayout.Width(40));
                float newZoom = UnityEditor.EditorGUILayout.Slider(zoomLevel, MIN_ZOOM, MAX_ZOOM, UnityEngine.GUILayout.Width(100));
                ZoomLevel = newZoom;

                if (UnityEngine.GUILayout.Button("Fit", UnityEngine.GUILayout.Width(40)))
                {
                    ZoomLevel = 1f;
                    Logger.LogInteraction(ComponentName, "Reset Zoom to Fit");
                }

                UnityEngine.GUILayout.Label($"{zoomLevel:F1}x", UnityEngine.GUILayout.Width(40));
            }
        }

        public Rect CalculateZoomedRect(Rect originalRect, float zoomLevel)
        {
            // Validate zoom level
            var validation = ValidationUtils.ValidateZoomLevel(zoomLevel);
            if (!validation.IsValid)
            {
                Logger.LogWarning(ComponentName, $"Invalid zoom level: {validation.GetErrorMessage()}");
                zoomLevel = Mathf.Clamp(zoomLevel, MIN_ZOOM, MAX_ZOOM);
            }

            float width = originalRect.width * zoomLevel;
            float height = originalRect.height * zoomLevel;

            return new Rect(
                originalRect.x + (originalRect.width - width) / 2,
                originalRect.y + (originalRect.height - height) / 2,
                width,
                height
            );
        }

        public void ZoomIn()
        {
            ZoomLevel *= 1.2f;
            Logger.LogInteraction(ComponentName, "Zoom In");
        }

        public void ZoomOut()
        {
            ZoomLevel /= 1.2f;
            Logger.LogInteraction(ComponentName, "Zoom Out");
        }

        public void ResetZoom()
        {
            ZoomLevel = 1f;
            Logger.LogInteraction(ComponentName, "Reset Zoom");
        }

        public void HandleZoomInput(Event e, Rect displayRect)
        {
            if (displayRect.Contains(e.mousePosition))
            {
                if (e.type == EventType.ScrollWheel)
                {
                    float zoomDelta = -e.delta.y * 0.1f;
                    ZoomLevel += zoomDelta;
                    e.Use();
                    Logger.LogInteraction(ComponentName, $"Mouse wheel zoom: {zoomDelta:F2}");
                }
            }
        }

        public Rect GetZoomedRect(Rect originalRect, Rect containerRect)
        {
            Vector2 zoomedSize = originalRect.size * zoomLevel;
            Vector2 offset = Vector2.zero;

            // Center the zoomed content if smaller than container
            if (zoomedSize.x < containerRect.width)
            {
                offset.x = (containerRect.width - zoomedSize.x) * 0.5f;
            }

            if (zoomedSize.y < containerRect.height)
            {
                offset.y = (containerRect.height - zoomedSize.y) * 0.5f;
            }

            return new Rect(containerRect.x + offset.x, containerRect.y + offset.y, zoomedSize.x, zoomedSize.y);
        }
    }
}
