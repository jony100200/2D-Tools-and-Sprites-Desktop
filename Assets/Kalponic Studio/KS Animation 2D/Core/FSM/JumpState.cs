using UnityEngine;

namespace KalponicStudio
{
    public class JumpState : CharacterState
    {
        public JumpState(CharacterStateMachine machine, IAnimationPlayer animationPlayer) : base(machine, animationPlayer) { }

        public override void Enter()
        {
            animationPlayer.Play(AnimationId.Jump);
        }

        public override void Update()
        {
            // Add logic for jump to fall transition, e.g., if not grounded
            // For simplicity, transition back to idle after some time or on land
        }
    }
}
