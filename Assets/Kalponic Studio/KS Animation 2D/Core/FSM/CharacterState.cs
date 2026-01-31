using UnityEngine;

namespace KalponicStudio
{
    public abstract class CharacterState
    {
        protected CharacterStateMachine machine;
        protected IAnimationPlayer animationPlayer;

        public CharacterState(CharacterStateMachine machine, IAnimationPlayer animationPlayer)
        {
            this.machine = machine;
            this.animationPlayer = animationPlayer;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
    }
}
