using System;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Base class for grounded states with coyote time and jump buffering helpers.
    /// </summary>
    public abstract class GroundedStateBase : CharacterState
    {
        private readonly Func<bool> groundedCheck;
        private readonly InputBuffer jumpBuffer;
        private readonly float coyoteTime;
        private float lastGroundedTime;
        private readonly float moveDeadZoneSqr;

        protected GroundedStateBase(
            CharacterStateMachine machine,
            IAnimationPlayer animationPlayer,
            Func<bool> groundedCheck,
            float coyoteTimeSeconds = 0.1f,
            float jumpBufferSeconds = 0.1f,
            float moveDeadZone = 0.01f) : base(machine, animationPlayer)
        {
            this.groundedCheck = groundedCheck ?? (() => true);
            this.coyoteTime = Mathf.Max(0f, coyoteTimeSeconds);
            jumpBuffer = new InputBuffer(jumpBufferSeconds);
            moveDeadZoneSqr = moveDeadZone * moveDeadZone;
        }

        public override void Enter()
        {
            base.Enter();
            lastGroundedTime = Time.time;
        }

        public override void Update()
        {
            base.Update();

            if (machine.Input.JumpPressed)
            {
                jumpBuffer.Record();
            }

            if (groundedCheck())
            {
                lastGroundedTime = Time.time;
            }
        }

        /// <summary>
        /// Returns true if movement input is above the configured dead zone.
        /// </summary>
        protected bool HasMoveInput()
        {
            return machine.Input.Move.sqrMagnitude > moveDeadZoneSqr;
        }

        /// <summary>
        /// Returns true if a buffered jump is available (either just pressed or within the buffer window).
        /// </summary>
        protected bool TryConsumeBufferedJump()
        {
            return jumpBuffer.Consume();
        }

        /// <summary>
        /// Returns true if we are within the coyote window after leaving the ground.
        /// </summary>
        protected bool HasCoyoteTime()
        {
            return Time.time - lastGroundedTime <= coyoteTime;
        }
    }

    /// <summary>
    /// Base class for airborne states with landing detection and optional jump buffering.
    /// </summary>
    public abstract class AirborneStateBase : CharacterState
    {
        private readonly Func<bool> groundedCheck;
        private readonly InputBuffer jumpBuffer;

        protected AirborneStateBase(
            CharacterStateMachine machine,
            IAnimationPlayer animationPlayer,
            Func<bool> groundedCheck,
            float bufferedJumpSeconds = 0.1f) : base(machine, animationPlayer)
        {
            this.groundedCheck = groundedCheck ?? (() => false);
            jumpBuffer = new InputBuffer(bufferedJumpSeconds);
        }

        public override void Update()
        {
            base.Update();

            if (machine.Input.JumpPressed)
            {
                jumpBuffer.Record();
            }

            if (groundedCheck())
            {
                OnLanded();
            }
        }

        protected bool TryConsumeBufferedJump()
        {
            return jumpBuffer.Consume();
        }

        /// <summary>
        /// Override to perform landing transitions.
        /// </summary>
        protected abstract void OnLanded();
    }
}
