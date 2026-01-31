using System;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Base class for class-driven animation states that run against an <see cref="IAnimator"/>.
    /// </summary>
    public abstract class AnimationState
    {
        public string Name { get; }
        public AnimationStatePriority Priority { get; private set; }

        protected AnimationState(string name, AnimationStatePriority priority = AnimationStatePriority.Medium)
        {
            Name = name ?? string.Empty;
            Priority = priority;
        }

        public virtual void Enter(IAnimator animator) { }
        public virtual void Update(IAnimator animator, float deltaTime) { }
        public virtual void Exit(IAnimator animator) { }

        public void SetPriority(AnimationStatePriority priority) => Priority = priority;
    }

    /// <summary>
    /// Adapter that plays a legacy <see cref="AnimationStateSO"/> through the class-based FSM.
    /// </summary>
    internal sealed class ScriptableAnimationState : AnimationState
    {
        private readonly AnimationStateSO stateAsset;

        public ScriptableAnimationState(AnimationStateSO stateAsset)
            : base(stateAsset?.stateName ?? string.Empty, stateAsset != null ? stateAsset.priority : AnimationStatePriority.Medium)
        {
            this.stateAsset = stateAsset;
        }

        public override void Enter(IAnimator animator)
        {
            if (animator == null || stateAsset == null) return;
            animator.Play(stateAsset.stateName, 1f, stateAsset.loop);
            stateAsset.onStateEntered?.Invoke();
        }

        public override void Exit(IAnimator animator)
        {
            if (stateAsset == null) return;
            stateAsset.onStateExited?.Invoke();
        }
    }

    /// <summary>
    /// Lightweight, code-first FSM that drives animations via <see cref="IAnimator"/>.
    /// Supports both class-based states and ScriptableObject adapter states.
    /// </summary>
    public sealed class AnimationStateMachine : IAnimationStateMachine
    {
        private readonly Dictionary<string, AnimationState> states = new Dictionary<string, AnimationState>(StringComparer.Ordinal);
        private readonly List<Transition> transitions = new List<Transition>();
        private AnimationState currentState;
        private IAnimator animator;

        private struct Transition
        {
            public string fromState;
            public string toState;
            public Func<bool> condition;
            public AnimationStatePriority priority;
        }

        public AnimationStateMachine(IAnimator animator)
        {
            this.animator = animator;
        }

        public string CurrentState => currentState?.Name;
        public bool IsTransitioning => false; // Instant transitions for now
        public AnimationStatePriority CurrentPriority => currentState?.Priority ?? AnimationStatePriority.Low;

        public void Update()
        {
            if (currentState != null)
            {
                currentState.Update(animator, Time.deltaTime);
            }

            // Evaluate transitions in order-added
            for (int i = 0; i < transitions.Count; i++)
            {
                var t = transitions[i];
                if (!string.IsNullOrEmpty(t.fromState) && !string.Equals(t.fromState, CurrentState, StringComparison.Ordinal))
                {
                    continue;
                }

                if (t.condition == null || !t.condition())
                {
                    continue;
                }

                if (!CanInterruptCurrentState(t.priority))
                {
                    continue;
                }

                SwitchState(t.toState);
                break;
            }
        }

        public void AddState(string stateName, AnimationStateSO stateSO)
        {
            if (stateSO == null || string.IsNullOrEmpty(stateName)) return;
            states[stateName] = new ScriptableAnimationState(stateSO);
        }

        /// <summary>
        /// Register a class-based state directly.
        /// </summary>
        public void AddState(AnimationState state)
        {
            if (state == null || string.IsNullOrEmpty(state.Name)) return;
            states[state.Name] = state;
        }

        public void AddTransition(string fromState, string toState, Func<bool> condition, float duration = 0.1f)
        {
            transitions.Add(new Transition
            {
                fromState = fromState ?? string.Empty,
                toState = toState,
                condition = condition,
                priority = AnimationStatePriority.Low // Default priority gate
            });
        }

        public void AddTransitionWithPriority(string fromState, string toState, Func<bool> condition, AnimationStatePriority priority, float duration = 0.1f)
        {
            transitions.Add(new Transition
            {
                fromState = fromState ?? string.Empty,
                toState = toState,
                condition = condition,
                priority = priority
            });
        }

        public void SetStatePriority(string stateName, AnimationStatePriority priority)
        {
            if (string.IsNullOrEmpty(stateName)) return;
            if (states.TryGetValue(stateName, out var state))
            {
                state.SetPriority(priority);
            }
        }

        public void ForceTransition(string stateName, float transitionDuration = 0.1f)
        {
            SwitchState(stateName);
        }

        public bool CanInterruptCurrentState(AnimationStatePriority newPriority)
        {
            return newPriority >= CurrentPriority;
        }

        public void SetAnimator(IAnimator newAnimator)
        {
            animator = newAnimator;
        }

        private void SwitchState(string targetState)
        {
            if (string.IsNullOrEmpty(targetState))
            {
                return;
            }

            if (!states.TryGetValue(targetState, out var nextState))
            {
                Debug.LogWarning($"AnimationStateMachine: State '{targetState}' not found.");
                return;
            }

            currentState?.Exit(animator);
            currentState = nextState;
            currentState?.Enter(animator);
        }
    }
}
