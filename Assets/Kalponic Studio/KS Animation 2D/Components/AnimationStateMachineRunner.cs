using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// MonoBehaviour helper that ticks an <see cref="AnimationStateMachine"/> against a resolved <see cref="IAnimator"/>.
    /// Keeps FSM concerns inside the animation system (no gameplay controller coupling).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnimationStateMachineRunner : MonoBehaviour
    {
        [Tooltip("Optional explicit animator reference. If empty the runner searches on this GameObject.")]
        [SerializeField] private MonoBehaviour animatorBehaviour;

        public AnimationStateMachine StateMachine { get; private set; }

        private IAnimator animator;

        private void Awake()
        {
            animator = animatorBehaviour as IAnimator ?? GetComponent<IAnimator>();
            if (animator != null)
            {
                StateMachine = new AnimationStateMachine(animator);
            }
            else
            {
                Debug.LogWarning($"{nameof(AnimationStateMachineRunner)}: No IAnimator found. FSM will not tick.", this);
            }
        }

        private void Update()
        {
            StateMachine?.Update();
        }

        /// <summary>
        /// Allows external configuration when constructing the FSM from code.
        /// </summary>
        public void SetStateMachine(AnimationStateMachine machine, IAnimator targetAnimator = null)
        {
            StateMachine = machine;
            animator = targetAnimator ?? animator;
            StateMachine?.SetAnimator(animator);
        }
    }
}
