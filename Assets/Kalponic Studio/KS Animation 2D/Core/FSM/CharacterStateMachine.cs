using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Minimal animation-focused state machine for characters.
    /// Drives animation state changes based on an external input snapshot.
    /// </summary>
    public sealed class CharacterStateMachine
    {
        public struct InputSnapshot
        {
            public Vector2 Move;
            public bool JumpPressed;
        }

        public InputSnapshot Input { get; private set; }
        public CharacterState CurrentState { get; private set; }

        // Exposed states for simple movement flows; swap/extend as needed.
        public IdleState IdleState { get; }
        public WalkState WalkState { get; }
        public JumpState JumpState { get; }

        private readonly IAnimationPlayer animationPlayer;

        public CharacterStateMachine(IAnimationPlayer animationPlayer)
        {
            this.animationPlayer = animationPlayer;

            IdleState = new IdleState(this, animationPlayer);
            WalkState = new WalkState(this, animationPlayer);
            JumpState = new JumpState(this, animationPlayer);

            ChangeState(IdleState);
        }

        public void SetInput(InputSnapshot input)
        {
            Input = input;
        }

        public void Update()
        {
            CurrentState?.Update();
        }

        public void ChangeState(CharacterState next)
        {
            if (next == null || next == CurrentState) return;

            CurrentState?.Exit();
            CurrentState = next;
            CurrentState?.Enter();
        }
    }
}
