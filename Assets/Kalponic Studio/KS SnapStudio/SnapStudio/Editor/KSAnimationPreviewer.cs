using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using KalponicGames.KS_SnapStudio;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    public class KSAnimationPreviewer : EditorWindow
    {
        // Modular components following SOLID principles
        private AnimationPlaybackController playbackController;
        private TimelineController timelineController;
        private ZoomController zoomController;
        private FrameInfoDisplay frameInfoDisplay;
        private AnimationExporter animationExporter;

        private KSPlayModeCapture captureComponent = null;

        private const float MIN_WINDOW_WIDTH = 300f;
        private const float CONTROLS_HEIGHT = 150f; // Increased for modular controls
        private const float FRAME_LABEL_HEIGHT = 20f;

        public static void ShowWindow(KSPlayModeCapture capture)
        {
            var window = GetWindow<KSAnimationPreviewer>("Animation Previewer");
            window.captureComponent = capture;
            window.minSize = new Vector2(MIN_WINDOW_WIDTH, 400f);
            window.InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Initialize all modular components
            playbackController = new AnimationPlaybackController();
            timelineController = new TimelineController();
            zoomController = new ZoomController();
            frameInfoDisplay = new FrameInfoDisplay();
            animationExporter = new AnimationExporter();

            // Wire up event handlers
            playbackController.OnFrameChanged += OnFrameChanged;
            timelineController.OnFrameSelected += OnTimelineFrameSelected;
            zoomController.OnZoomChanged += OnZoomChanged;

            // Initialize components
            playbackController.Initialize();
            timelineController.Initialize();
            zoomController.Initialize();
            frameInfoDisplay.Initialize();
            animationExporter.Initialize();
        }

        void OnEnable()
        {
            if (playbackController == null)
            {
                InitializeComponents();
            }
            EditorApplication.update += UpdateAnimation;
        }

        void OnDisable()
        {
            // Clean up event handlers
            if (playbackController != null)
            {
                playbackController.OnFrameChanged -= OnFrameChanged;
            }
            if (timelineController != null)
            {
                timelineController.OnFrameSelected -= OnTimelineFrameSelected;
            }
            if (zoomController != null)
            {
                zoomController.OnZoomChanged -= OnZoomChanged;
            }
            EditorApplication.update -= UpdateAnimation;
        }

        void UpdateAnimation()
        {
            if (playbackController != null && captureComponent != null && captureComponent.Sampling != null)
            {
                var selectedFrames = captureComponent.Sampling.SelectedFrames;
                playbackController.Update(selectedFrames.Count);
            }
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

            if (selectedFrames.Count == 0)
            {
                EditorGUILayout.HelpBox("No frames selected. Please select frames first.", MessageType.Info);
                return;
            }

            // Update controllers with current data
            playbackController.TotalFrames = selectedFrames.Count;
            timelineController.TotalFrames = selectedFrames.Count;

            // Frame rate controls
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Frame Rate:", GUILayout.Width(80));
                captureComponent.Fps = EditorGUILayout.IntField(captureComponent.Fps, GUILayout.Width(50));
                if (captureComponent.Fps <= 0) captureComponent.Fps = 1;

                EditorGUILayout.LabelField("Interval:", GUILayout.Width(50));
                EditorGUILayout.IntField(1, GUILayout.Width(50));
            }

            // Playback controls (delegated to controller)
            playbackController.DrawPlaybackControls();

            // Zoom controls (delegated to controller)
            zoomController.DrawZoomControls();

            // Frame info toggle (delegated to display)
            frameInfoDisplay.DrawInfoToggle();

            // Frame selector button
            if (GUILayout.Button("Frame Selector", GUILayout.Width(100)))
            {
                KSFrameSelector.ShowWindow(captureComponent);
            }

            EditorGUILayout.Space();

            // Timeline controls (delegated to controller)
            timelineController.DrawTimeline();

            // Animation display area
            var displayRect = new Rect(10, CONTROLS_HEIGHT + 50, position.width - 20, position.height - CONTROLS_HEIGHT - FRAME_LABEL_HEIGHT - 70);

            // Handle zoom input
            Event e = Event.current;
            zoomController.HandleZoomInput(e, displayRect);

            DrawCurrentFrame(displayRect, selectedFrames, sampling);

            // Export controls (delegated to exporter)
            animationExporter.DrawExportControls(selectedFrames);
        }

        private void DrawCurrentFrame(Rect displayRect, List<Frame> selectedFrames, Sampling sampling)
        {
            int currentFrameIndex = playbackController.CurrentFrameIndex;

            if (currentFrameIndex < selectedFrames.Count && currentFrameIndex >= 0)
            {
                var frame = selectedFrames[currentFrameIndex];
                var texture = sampling.TrimMainTextures[frame.Index];

                if (texture != null)
                {
                    // Get zoomed rect from zoom controller
                    Rect textureRect = zoomController.GetZoomedRect(
                        new Rect(0, 0, texture.width, texture.height),
                        displayRect
                    );

                    // Adjust for display area
                    textureRect = new Rect(
                        displayRect.x + textureRect.x,
                        displayRect.y + textureRect.y,
                        textureRect.width,
                        textureRect.height
                    );

                    EditorGUI.DrawTextureTransparent(textureRect, texture);

                    // Draw frame info overlay (delegated to display)
                    frameInfoDisplay.DrawFrameInfo(displayRect, currentFrameIndex, selectedFrames.Count, frame.Index, texture);
                }
            }
        }

        // Event handlers for component communication
        private void OnFrameChanged(int newFrameIndex)
        {
            Repaint();
        }

        private void OnTimelineFrameSelected(int frameIndex)
        {
            playbackController.CurrentFrameIndex = frameIndex;
            Repaint();
        }

        private void OnZoomChanged(float newZoom)
        {
            Repaint();
        }
    }
}
