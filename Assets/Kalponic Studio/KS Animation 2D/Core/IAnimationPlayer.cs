using UnityEngine;

namespace KalponicStudio
{
    public interface IAnimationPlayer
    {
        event System.Action<AnimationId> AnimationStarted;
        event System.Action<AnimationId> AnimationCompleted;
        event System.Action<AnimationId> AnimationLooped;

        void Play(AnimationId id);
        void Play(AnimationId id, float fadeOverride);
        void Play(AnimationType type);
        void Play(AnimationType type, float fadeOverride);
    }
}
