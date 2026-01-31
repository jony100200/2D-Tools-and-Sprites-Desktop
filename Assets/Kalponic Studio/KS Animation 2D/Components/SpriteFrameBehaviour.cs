using UnityEngine;
using UnityEngine.Playables;

namespace KalponicStudio
{
    /// <summary>
    /// Playable behaviour for frame-by-frame sprite animation
    /// Replaces AnimationClips for 2D sprite-based animation
    /// </summary>
    public class SpriteFrameBehaviour : IPlayableBehaviour
    {
        public string StateName;
        public Sprite[] Frames;
        public float Fps;
        public bool Loop;
        public SpriteRenderer Renderer;

        // Events
        public System.Action<string, int> OnFrameChanged;
        public System.Action OnComplete;
        public System.Action OnLoop;

        private float time;
        private int currentFrame;
        private bool completed;

        public void OnPlayableCreate(Playable playable)
        {
            time = 0f;
            currentFrame = 0;
            completed = false;
        }

        public void OnPlayableDestroy(Playable playable)
        {
            // Cleanup if needed
        }

        public void OnGraphStart(Playable playable)
        {
            Reset();
        }

        public void OnGraphStop(Playable playable)
        {
            // Optional cleanup
        }

        public void OnBehaviourPlay(Playable playable, FrameData info)
        {
            Reset();
        }

        public void OnBehaviourPause(Playable playable, FrameData info)
        {
            // Optional pause handling
        }

        public void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            PrepareFrame(playable, info);
        }

        public void PrepareFrame(Playable playable, FrameData info)
        {
            if (Frames == null || Frames.Length == 0 || Renderer == null)
            {
                KSAnimLog.Warn($"SpriteFrameBehaviour.PrepareFrame: Cannot animate - Frames: {Frames?.Length ?? 0}, Renderer: {Renderer?.name ?? "null"}", "Playback");
                return;
            }

            // Update time based on playable speed and delta time
            time += info.deltaTime * (float)playable.GetSpeed();

            // Calculate current frame
            float frameDuration = 1f / Fps;
            int newFrame = Mathf.FloorToInt(time / frameDuration);

            // Handle looping or completion
            if (newFrame >= Frames.Length)
            {
                if (Loop)
                {
                    newFrame = newFrame % Frames.Length;
                    time = time % (Frames.Length * frameDuration);
                    OnLoop?.Invoke();
                }
                else
                {
                    newFrame = Frames.Length - 1;
                    if (!completed)
                    {
                        completed = true;
                        OnComplete?.Invoke();
                    }
                    return; // Don't update sprite on completion
                }
            }

            // Update sprite if frame changed
            if (newFrame != currentFrame)
            {
                currentFrame = newFrame;
                if (currentFrame < Frames.Length)
                {
                    KSAnimLog.Info($"SpriteFrameBehaviour: Setting sprite {currentFrame}/{Frames.Length} on {Renderer.name}", "Playback");
                    Renderer.sprite = Frames[currentFrame];
                    OnFrameChanged?.Invoke(StateName, currentFrame);
                }
            }
        }

        private void Reset()
        {
            time = 0f;
            currentFrame = 0;
            completed = false;
        }

        // Utility method to set animation data
        public void SetAnimation(Sprite[] frames, float fps, bool loop = true)
        {
            Frames = frames;
            Fps = fps;
            Loop = loop;
            Reset();
        }

        // Get normalized time (0-1)
        public float GetNormalizedTime()
        {
            if (Frames == null || Frames.Length == 0) return 0f;
            return (float)currentFrame / Frames.Length;
        }

        // Get current frame index
        public int GetCurrentFrame()
        {
            return currentFrame;
        }

        // Check if animation is complete
        public bool IsComplete()
        {
            return completed;
        }
    }
}
