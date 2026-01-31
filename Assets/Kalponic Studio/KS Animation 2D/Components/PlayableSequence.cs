using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Fluent API for animation sequencing with callbacks and interrupts
    /// Replaces Animation Events with deterministic, code-driven timing
    /// </summary>
    public class PlayableSequence : IAnimationSequence
    {
        private readonly List<SequenceStep> steps = new List<SequenceStep>();
        private int currentStepIndex = 0;
        private IAnimator animator;
        private Coroutine sequenceCoroutine;

        public PlayableSequence(IAnimator animator)
        {
            this.animator = animator;
        }

        // IAnimationSequence implementation
        public IAnimationSequence Play(string stateName, float speed = 1f)
        {
            steps.Add(new SequenceStep
            {
                StateName = stateName,
                Speed = speed
            });
            return this;
        }

        public IAnimationSequence Then(string stateName, float speed = 1f)
        {
            return Play(stateName, speed);
        }

        public IAnimationSequence Delay(float seconds)
        {
            steps.Add(new SequenceStep
            {
                IsDelay = true,
                DelayDuration = seconds
            });
            return this;
        }

        public IAnimationSequence OnComplete(System.Action callback)
        {
            if (steps.Count > 0)
            {
                var lastStep = steps[steps.Count - 1];
                lastStep.OnComplete = callback;
            }
            return this;
        }

        public IAnimationSequence AtFrame(int frame, System.Action callback)
        {
            if (steps.Count > 0)
            {
                var lastStep = steps[steps.Count - 1];
                lastStep.Callbacks.Add(new SequenceCallback
                {
                    Frame = frame,
                    Action = callback
                });
            }
            return this;
        }

        public IAnimationSequence AtNormalizedTime(float normalizedTime, System.Action callback)
        {
            if (steps.Count > 0)
            {
                var lastStep = steps[steps.Count - 1];
                lastStep.Callbacks.Add(new SequenceCallback
                {
                    NormalizedTime = normalizedTime,
                    Action = callback
                });
            }
            return this;
        }

        public void Execute(IAnimator targetAnimator)
        {
            animator = targetAnimator;
            if (sequenceCoroutine != null)
            {
                (animator as MonoBehaviour)?.StopCoroutine(sequenceCoroutine);
            }
            sequenceCoroutine = (animator as MonoBehaviour)?.StartCoroutine(RunSequence());
        }

        private IEnumerator RunSequence()
        {
            currentStepIndex = 0;

            while (currentStepIndex < steps.Count)
            {
                var step = steps[currentStepIndex];

                if (step.IsDelay)
                {
                    yield return new WaitForSeconds(step.DelayDuration);
                }
                else
                {
                    // Play the animation
                    animator.Play(step.StateName, step.Speed);

                    // Wait for animation to complete
                    while (animator.IsPlaying(step.StateName))
                    {
                        yield return null;
                    }
                }

                // Execute completion callback
                step.OnComplete?.Invoke();

                currentStepIndex++;
            }

            // Sequence complete
            sequenceCoroutine = null;
        }

        // Allow external interruption
        public void Interrupt()
        {
            if (sequenceCoroutine != null && animator is MonoBehaviour mb)
            {
                mb.StopCoroutine(sequenceCoroutine);
                sequenceCoroutine = null;
            }
        }

        private class SequenceStep
        {
            public string StateName;
            public float Speed = 1f;
            public bool IsDelay = false;
            public float DelayDuration = 0f;
            public System.Action OnComplete;
            public List<SequenceCallback> Callbacks = new List<SequenceCallback>();
        }

        private class SequenceCallback
        {
            public float NormalizedTime = -1f; // -1 means use frame instead
            public int Frame = -1;
            public System.Action Action;
        }
    }

    // Extension methods for fluent API
    public static class AnimationSequenceExtensions
    {
        public static IAnimationSequence Play(this IAnimator animator, string state, float speed = 1f)
        {
            return animator.Sequence().Play(state, speed);
        }

        public static IAnimationSequence Then(this IAnimationSequence sequence, string state, float speed = 1f)
        {
            return ((PlayableSequence)sequence).Then(state, speed);
        }

        public static IAnimationSequence AtNormalized(this IAnimationSequence sequence, float time, System.Action callback)
        {
            return ((PlayableSequence)sequence).AtNormalizedTime(time, callback);
        }

        public static IAnimationSequence AtFrame(this IAnimationSequence sequence, int frame, System.Action callback)
        {
            return ((PlayableSequence)sequence).AtFrame(frame, callback);
        }

        public static IAnimationSequence OnComplete(this IAnimationSequence sequence, System.Action callback)
        {
            return ((PlayableSequence)sequence).OnComplete(callback);
        }
    }
}
