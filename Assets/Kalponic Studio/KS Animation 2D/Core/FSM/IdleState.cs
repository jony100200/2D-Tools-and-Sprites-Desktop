using UnityEngine;

namespace KalponicStudio
{
    public class IdleState : CharacterState
    {
        public IdleState(CharacterStateMachine machine, IAnimationPlayer animationPlayer) : base(machine, animationPlayer) { }

        public override void Enter()
        {
            animationPlayer.Play(AnimationId.Idle);
        }

        public override void Update()
        {
            if (machine.Input.Move != Vector2.zero)
            {
                machine.ChangeState(machine.WalkState);
            }
            else if (machine.Input.JumpPressed)
            {
                machine.ChangeState(machine.JumpState);
            }
        }
    }
}
