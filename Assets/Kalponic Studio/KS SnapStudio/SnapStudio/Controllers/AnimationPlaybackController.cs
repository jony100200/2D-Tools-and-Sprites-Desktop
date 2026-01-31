using UnityEngine;
using UnityEditor;
using KalponicGames.KS_SnapStudio;
using KalponicGames.KS_SnapStudio.SnapStudio;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Handles animation playback logic (play, pause, loop, reset)
    /// Single Responsibility: Manage playback state and timing
    /// </summary>
    public class AnimationPlaybackController : IPreviewComponent, IToggleable, IAnimationController
    {
        private bool isPlaying = false;
        private bool isLooping = true;
        private int currentFrameIndex = 0;
        private double lastFrameTime = 0;
        private int fps = 24;
        private int maxFrames = 0;

        // Events
        public System.Action<int> OnFrameChanged;
        public System.Action<bool> OnPlayStateChanged;

        public string ComponentName => "AnimationPlaybackController";
        public bool IsEnabled { get; set; } = true;

        public bool IsPlaying => isPlaying;
        public bool IsLooping => isLooping;
        public int CurrentFrameIndex
        {
            get => currentFrameIndex;
            set => SetFrame(value, maxFrames);
        }
        public int TotalFrames
        {
            get => maxFrames;
            set => maxFrames = Mathf.Max(0, value);
        }
        public int FPS
        {
            get => fps;
            set => fps = Mathf.Max(1, value);
        }

        public void Initialize()
        {
            Reset();
            Logger.LogSuccess(ComponentName, "Initialized");
        }

        public void OnGUI()
        {
            if (!IsEnabled) return;
            DrawPlaybackControls();
        }

        public void Cleanup()
        {
            Pause(); // Ensure playback is stopped
            Logger.LogInfo(ComponentName, "Cleaned up");
        }

        public void Update(int maxFrames)
        {
            if (!isPlaying || !IsEnabled) return;

            double currentTime = EditorApplication.timeSinceStartup;
            float frameInterval = 1f / fps;

            if (currentTime - lastFrameTime >= frameInterval)
            {
                lastFrameTime = currentTime;
                AdvanceFrame(maxFrames);
            }
        }

        public void Play()
        {
            if (!isPlaying)
            {
                // Validate playback parameters
                var validation = ValidationUtils.ValidateAnimation("Playback", maxFrames, 1f / fps * maxFrames);
                if (!validation.IsValid)
                {
                    Logger.LogError(ComponentName, $"Cannot start playback: {validation.GetErrorMessage()}");
                    return;
                }

                isPlaying = true;
                OnPlayStateChanged?.Invoke(true);
                Logger.LogInfo(ComponentName, "Playback started");
            }
        }

        public void Pause()
        {
            if (isPlaying)
            {
                isPlaying = false;
                OnPlayStateChanged?.Invoke(false);
                Logger.LogInfo(ComponentName, "Playback paused");
            }
        }

        public void TogglePlayPause()
        {
            if (isPlaying)
                Pause();
            else
                Play();
        }

        public void Reset()
        {
            currentFrameIndex = 0;
            lastFrameTime = EditorApplication.timeSinceStartup;
            OnFrameChanged?.Invoke(currentFrameIndex);
            Logger.LogInfo(ComponentName, "Reset to frame 0");
        }

        public void SetLooping(bool looping)
        {
            isLooping = looping;
            Logger.LogInfo(ComponentName, $"Looping set to {looping}");
        }

        public void SetFrame(int frameIndex, int maxFrames)
        {
            // Validate frame index
            var validation = ValidationUtils.ValidateFrameIndex(frameIndex, maxFrames);
            if (!validation.IsValid)
            {
                Logger.LogWarning(ComponentName, $"Invalid frame index: {validation.GetErrorMessage()}");
                return;
            }

            currentFrameIndex = Mathf.Clamp(frameIndex, 0, maxFrames - 1);
            OnFrameChanged?.Invoke(currentFrameIndex);
            Logger.LogInfo(ComponentName, $"Frame set to {currentFrameIndex}");
        }

        public void AdvanceFrame(int maxFrames)
        {
            currentFrameIndex++;
            if (currentFrameIndex >= maxFrames)
            {
                if (isLooping)
                {
                    currentFrameIndex = 0;
                    Logger.LogInfo(ComponentName, "Looped back to frame 0");
                }
                else
                {
                    Pause();
                    currentFrameIndex = maxFrames - 1;
                    Logger.LogInfo(ComponentName, "Playback finished at last frame");
                }
            }
            OnFrameChanged?.Invoke(currentFrameIndex);
        }

        public void DrawPlaybackControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var playButtonText = isPlaying ? "Pause" : "Play";
                if (GUILayout.Button(playButtonText, GUILayout.Width(60)))
                {
                    TogglePlayPause();
                    Logger.LogInteraction(ComponentName, playButtonText);
                }

                isLooping = GUILayout.Toggle(isLooping, "Loop", GUILayout.Width(60));

                if (GUILayout.Button("Reset", GUILayout.Width(60)))
                {
                    Reset();
                    Pause();
                    Logger.LogInteraction(ComponentName, "Reset");
                }
            }
        }
    }
}
