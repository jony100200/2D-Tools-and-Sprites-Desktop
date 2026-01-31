using UnityEngine;

namespace KalponicStudio
{
    public class WalkState : CharacterState
    {
        public WalkState(CharacterStateMachine machine, IAnimationPlayer animationPlayer) : base(machine, animationPlayer) { }

        public override void Enter()
        {
            animationPlayer.Play(AnimationId.Walk);
        }

        public override void Update()
        {
            if (machine.Input.Move == Vector2.zero)
            {
                machine.ChangeState(machine.IdleState);
            }
            else if (machine.Input.JumpPressed)
            {
                machine.ChangeState(machine.JumpState);
            }
        }
    }
}
