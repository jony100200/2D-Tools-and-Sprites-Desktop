using System;

namespace KalponicStudio
{
    /// <summary>
    /// Unified animation event surface (Action + UnityEvent).
    /// </summary>
    public sealed class AnimationEventHub
    {
        public event Action<AnimationId> Started;
        public event Action<AnimationId> Completed;
        public event Action<AnimationId> Looped;

        public AnimationUnityEvents UnityEvents;

        public AnimationEventHub(AnimationUnityEvents unityEvents = null)
        {
            UnityEvents = unityEvents ?? new AnimationUnityEvents();
        }

        public void RaiseStart(AnimationId id)
        {
            Started?.Invoke(id);
            UnityEvents?.onAnimationStart?.Invoke();
        }

        public void RaiseLoop(AnimationId id)
        {
            Looped?.Invoke(id);
            UnityEvents?.onAnimationLoop?.Invoke();
        }

        public void RaiseComplete(AnimationId id)
        {
            Completed?.Invoke(id);
            UnityEvents?.onAnimationEnd?.Invoke();
        }
    }
}
