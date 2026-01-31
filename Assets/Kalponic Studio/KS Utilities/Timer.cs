using System.Collections.Generic;
using System;
using UnityEngine;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Handle for controlling individual timers
    /// </summary>
    public class TimerHandle
    {
        private readonly Timer.TimerInstance timer;

        internal TimerHandle(Timer.TimerInstance timer)
        {
            this.timer = timer;
        }

        public void Pause() => timer.IsPaused = true;
        public void Resume() => timer.IsPaused = false;
        public void Stop() => Timer.timersToRemove.Add(timer);
        public void Reset() => timer.Reset();
        public void Restart() => timer.Restart();
        public bool IsPaused => timer.IsPaused;
        public float Progress => 1f - (timer.TimeRemaining / timer.Duration);
        public float TimeRemaining => timer.TimeRemaining;
    }

    /// <summary>
    /// Timer utility for delayed actions - reusable across all projects
    /// </summary>
    public static class Timer
    {
        internal class TimerInstance
        {
            public float Duration { get; private set; }
            public float TimeRemaining { get; private set; }
            public Action OnComplete { get; private set; }
            public Action<float> OnUpdate { get; private set; }
            public bool IsPaused { get; set; }
            public bool UseUnscaledTime { get; private set; }
            public string Name { get; private set; }

            public TimerInstance(float duration, Action onComplete, Action<float> onUpdate,
                               bool useUnscaledTime, string name)
            {
                Duration = duration;
                TimeRemaining = duration;
                OnComplete = onComplete;
                OnUpdate = onUpdate;
                UseUnscaledTime = useUnscaledTime;
                Name = name;
                IsPaused = false;
            }

            public bool Update()
            {
                if (IsPaused) return false;

                float deltaTime = UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                TimeRemaining -= deltaTime;

                // Calculate progress (0 = just started, 1 = complete)
                float progress = 1f - (TimeRemaining / Duration);
                OnUpdate?.Invoke(progress);

                if (TimeRemaining <= 0f)
                {
                    OnComplete?.Invoke();
                    return true; // Timer complete
                }

                return false; // Timer still running
            }

            public void Reset()
            {
                TimeRemaining = Duration;
            }

            public void Restart()
            {
                TimeRemaining = Duration;
                IsPaused = false;
            }
        }

        private static readonly List<TimerInstance> activeTimers = new List<TimerInstance>();
        internal static readonly List<TimerInstance> timersToRemove = new List<TimerInstance>();

        /// <summary>
        /// Create a simple timer that calls an action after a delay
        /// </summary>
        public static TimerHandle Create(float duration, Action onComplete, bool useUnscaledTime = false)
        {
            return Create(duration, onComplete, null, useUnscaledTime, "");
        }

        /// <summary>
        /// Create a timer with progress callback
        /// </summary>
        public static TimerHandle Create(float duration, Action onComplete, Action<float> onUpdate,
                                       bool useUnscaledTime = false, string name = "")
        {
            var timer = new TimerInstance(duration, onComplete, onUpdate, useUnscaledTime, name);
            activeTimers.Add(timer);

            return new TimerHandle(timer);
        }

        /// <summary>
        /// Stop all timers with a specific name
        /// </summary>
        public static void StopAll(string name)
        {
            foreach (var timer in activeTimers)
            {
                if (timer.Name == name)
                {
                    timersToRemove.Add(timer);
                }
            }
        }

        /// <summary>
        /// Stop the first timer with a specific name
        /// </summary>
        public static void StopFirst(string name)
        {
            foreach (var timer in activeTimers)
            {
                if (timer.Name == name)
                {
                    timersToRemove.Add(timer);
                    break;
                }
            }
        }

        /// <summary>
        /// Pause all timers with a specific name
        /// </summary>
        public static void PauseAll(string name)
        {
            foreach (var timer in activeTimers)
            {
                if (timer.Name == name)
                {
                    timer.IsPaused = true;
                }
            }
        }

        /// <summary>
        /// Resume all timers with a specific name
        /// </summary>
        public static void ResumeAll(string name)
        {
            foreach (var timer in activeTimers)
            {
                if (timer.Name == name)
                {
                    timer.IsPaused = false;
                }
            }
        }

        /// <summary>
        /// Update all active timers (call this from a MonoBehaviour's Update)
        /// </summary>
        public static void UpdateAll()
        {
            foreach (var timer in activeTimers)
            {
                if (timer.Update())
                {
                    timersToRemove.Add(timer);
                }
            }

            // Remove completed timers
            foreach (var timer in timersToRemove)
            {
                activeTimers.Remove(timer);
            }
            timersToRemove.Clear();
        }

        /// <summary>
        /// Clear all active timers
        /// </summary>
        public static void ClearAll()
        {
            activeTimers.Clear();
            timersToRemove.Clear();
        }
    }
}
