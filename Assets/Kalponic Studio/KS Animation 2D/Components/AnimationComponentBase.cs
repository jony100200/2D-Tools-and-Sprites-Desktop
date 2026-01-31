using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Base class for modular animation parts (body/weapon/effect). Wraps an IAnimator to provide a consistent API.
    /// </summary>
    public abstract class AnimationComponentBase : MonoBehaviour
    {
        [Tooltip("Animator implementation to drive this component. If empty, will search on this GameObject.")]
        [SerializeField] private MonoBehaviour animatorBehaviour;

        private IAnimator cachedAnimator;

        protected IAnimator Animator
        {
            get
            {
                if (cachedAnimator != null) return cachedAnimator;
                if (animatorBehaviour is IAnimator anim)
                {
                    cachedAnimator = anim;
                }
                else
                {
                    cachedAnimator = GetComponent<IAnimator>();
                }
                return cachedAnimator;
            }
        }

        public virtual void PlayAnimation(AnimationType type, float speed = 1f, bool loop = true)
        {
            Animator?.Play(type, speed, loop);
        }

        public virtual void PauseAnimation() => Animator?.Pause();
        public virtual void ResumeAnimation() => Animator?.Resume();
        public virtual void StopAnimation() => Animator?.Stop();
    }
}
