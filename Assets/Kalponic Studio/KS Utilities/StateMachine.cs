using System;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Simple state machine for managing object states
    /// </summary>
    public class StateMachine<TState> where TState : IState
    {
        public TState CurrentState { get; private set; }
        public bool CanChangeState { get; set; } = true;

        public event Action<TState> OnStateChanged;

        public void Initialize(TState startState)
        {
            CanChangeState = true;
            CurrentState = startState;
            CurrentState.Enter();
        }

        public void ChangeState(TState newState)
        {
            if (!CanChangeState || newState == null) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();

            OnStateChanged?.Invoke(newState);
        }

        public void Update()
        {
            CurrentState?.Update();
        }

        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }

        public void LateUpdate()
        {
            CurrentState?.LateUpdate();
        }

        public void SwitchOffStateMachine() => CanChangeState = false;
        public void SwitchOnStateMachine() => CanChangeState = true;
    }

    /// <summary>
    /// Interface for state objects
    /// </summary>
    public interface IState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void LateUpdate();
        void Exit();
    }

    /// <summary>
    /// Base class for states with empty implementations
    /// </summary>
    public abstract class BaseState : IState
    {
        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void LateUpdate() { }
        public virtual void Exit() { }
    }
}
