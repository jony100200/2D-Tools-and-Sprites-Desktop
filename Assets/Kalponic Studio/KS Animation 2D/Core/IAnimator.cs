using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Interface for 2D animation system - decouples gameplay from animation implementation
    /// Follows coding rules: interface for loose coupling
    /// Enhanced for complex 2D games with blending, sequencing, and state machines
    /// Version: 2.0 - Added advanced features for complex animations
    /// </summary>
    public interface IAnimator
    {
        // Basic playback
        void Play(string stateName, float speed = 1f, bool loop = true);
        void Play(AnimationType type, float speed = 1f, bool loop = true);
        void Stop();
        void Pause();
        void Resume();
        bool IsPlaying(string stateName);
        bool IsPlaying(AnimationType type);
        bool IsPlayingProperty { get; }
        float CurrentSpeed { get; set; }
        string CurrentState { get; }

        // Advanced playback for complex games
        void Play(string stateName, float speed, bool loop, bool pingPong);
        void CrossFade(string fromState, string toState, float duration);
        void Blend(string baseState, string blendState, float weight);

        // Time control (Phase 1)
        float Time { get; set; }
        float NormalizedTime { get; set; }

        // State queries (Phase 1)
        bool IsPaused { get; }
        AnimationClip Clip { get; }
        string ClipName { get; }

        // Sequencing for chained animations
        IAnimationSequence Sequence();
        void PlaySequence(IAnimationSequence sequence);

        // State machines for complex behavior management
        void SetStateMachine(IAnimationStateMachine stateMachine);
        void TransitionToState(string stateName);
        void TransitionToState(AnimationType type);

        // Frame-perfect events for precise timing (Phase 1)
        void PlayWithEvents(string stateName, Dictionary<int, System.Action> frameEvents);
        void PlayWithEvents(string stateName, Dictionary<float, System.Action> normalizedEvents);

        // Animation nodes for attachments (Phase 2)
        Vector3 GetNodePosition(int nodeId, bool ignorePivot = false);
        float GetNodeAngle(int nodeId);

        // Performance controls for large-scale games
        void SetUpdateFrequency(float updatesPerSecond);
        void EnableLOD(bool enable, float maxDistance = 50f);
    }

    /// <summary>
    /// Interface for animation sequences (chaining animations)
    /// </summary>
    public interface IAnimationSequence
    {
        IAnimationSequence Play(string stateName, float speed = 1f);
        IAnimationSequence Then(string stateName, float speed = 1f);
        IAnimationSequence Delay(float seconds);
        IAnimationSequence OnComplete(System.Action callback);
        IAnimationSequence AtFrame(int frame, System.Action callback);
        IAnimationSequence AtNormalizedTime(float normalizedTime, System.Action callback);
        void Execute(IAnimator animator);
    }

    /// <summary>
    /// Interface for animation state machines
    /// Enhanced with priority system for robust state interruptions
    /// </summary>
    public interface IAnimationStateMachine
    {
        void AddState(string stateName, AnimationStateSO stateSO);
        void AddTransition(string fromState, string toState, System.Func<bool> condition, float duration = 0.1f);
        void AddTransitionWithPriority(string fromState, string toState, System.Func<bool> condition, AnimationStatePriority priority, float duration = 0.1f);
        void SetStatePriority(string stateName, AnimationStatePriority priority);
        void Update();
        string CurrentState { get; }
        bool IsTransitioning { get; }
        AnimationStatePriority CurrentPriority { get; }
        void ForceTransition(string stateName, float transitionDuration = 0.1f);
        bool CanInterruptCurrentState(AnimationStatePriority newPriority);
    }
}
