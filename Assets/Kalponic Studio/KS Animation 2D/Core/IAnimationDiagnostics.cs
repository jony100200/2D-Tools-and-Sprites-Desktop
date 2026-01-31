using System;
using System.Collections.Generic;

namespace KalponicStudio
{
    /// <summary>
    /// Lightweight snapshot of the current animation state for debugging/diagnostic overlays.
    /// </summary>
    public readonly struct AnimationDebugSnapshot
    {
        public readonly AnimationId? Id;
        public readonly string StateName;
        public readonly string ClipName;
        public readonly float ClipTime;
        public readonly float ClipLength;
        public readonly float NormalizedTime;
        public readonly float Speed;
        public readonly bool IsPlaying;
        public readonly IReadOnlyList<string> TransitionHistory;
        public readonly string Source;

        public AnimationDebugSnapshot(
            AnimationId? id,
            string stateName,
            string clipName,
            float clipTime,
            float clipLength,
            float normalizedTime,
            float speed,
            bool isPlaying,
            IReadOnlyList<string> transitionHistory,
            string source)
        {
            Id = id;
            StateName = stateName ?? string.Empty;
            ClipName = clipName ?? string.Empty;
            ClipTime = clipTime;
            ClipLength = clipLength;
            NormalizedTime = normalizedTime;
            Speed = speed;
            IsPlaying = isPlaying;
            TransitionHistory = transitionHistory ?? Array.Empty<string>();
            Source = source ?? string.Empty;
        }

        public static AnimationDebugSnapshot Inactive(string source, IReadOnlyList<string> history = null)
        {
            return new AnimationDebugSnapshot(
                null,
                string.Empty,
                string.Empty,
                0f,
                0f,
                0f,
                0f,
                false,
                history ?? Array.Empty<string>(),
                source);
        }
    }

    /// <summary>
    /// Implemented by animation players that can expose their current playback state for debuggers.
    /// </summary>
    public interface IAnimationDiagnostics
    {
        AnimationDebugSnapshot CaptureDebugSnapshot();
    }
}
