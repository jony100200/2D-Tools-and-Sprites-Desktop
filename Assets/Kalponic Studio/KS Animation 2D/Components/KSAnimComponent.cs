using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Convenience component that resolves an <see cref="IAnimationPlayer"/> for other systems.
    /// Acts as a single entry point regardless of whether animations are driven by Animator,
    /// Playables, or a custom implementation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class KSAnimComponent : MonoBehaviour, IAnimationPlayer
    {
        [Tooltip("Optional direct reference to an IAnimationPlayer. If left empty the component will search locally.")]
        [SerializeField] private MonoBehaviour animationPlayerBehaviour;
        [Tooltip("Search child objects for an IAnimationPlayer if none is found on this GameObject.")]
        [SerializeField] private bool searchChildren = true;

        private IAnimationPlayer resolvedPlayer;
        public event System.Action<AnimationId> AnimationStarted;
        public event System.Action<AnimationId> AnimationCompleted;
        public event System.Action<AnimationId> AnimationLooped;

        private void Awake()
        {
            ResolvePlayer();
        }

        public void ResolvePlayer()
        {
            if (resolvedPlayer != null)
            {
                return;
            }

            if (animationPlayerBehaviour is IAnimationPlayer explicitPlayer)
            {
                resolvedPlayer = explicitPlayer;
                SubscribeTo(resolvedPlayer);
                return;
            }

            if (TryGetComponent(out IAnimationPlayer local) && local != (IAnimationPlayer)this)
            {
                resolvedPlayer = local;
                SubscribeTo(resolvedPlayer);
                return;
            }

            if (searchChildren)
            {
                var child = GetComponentInChildren<IAnimationPlayer>(true);
                if (child != null && child != (IAnimationPlayer)this)
                {
                    resolvedPlayer = child;
                    SubscribeTo(resolvedPlayer);
                    return;
                }
            }

            throw new MissingComponentException($"{nameof(KSAnimComponent)} could not find an IAnimationPlayer. Assign one explicitly or add AnimatorAnimationPlayer / PlayableAnimatorComponent.");
        }

        public IAnimationPlayer Player
        {
            get
            {
                if (resolvedPlayer == null)
                {
                    ResolvePlayer();
                }
                return resolvedPlayer;
            }
        }

        public void Play(AnimationId id)
        {
            Player?.Play(id);
        }

        public void Play(AnimationId id, float fadeOverride)
        {
            Player?.Play(id, fadeOverride);
        }

        public void Play(AnimationType type)
        {
            Player?.Play(type);
        }

        public void Play(AnimationType type, float fadeOverride)
        {
            Player?.Play(type, fadeOverride);
        }

        private void SubscribeTo(IAnimationPlayer player)
        {
            if (player == null) return;
            player.AnimationStarted += ForwardStarted;
            player.AnimationCompleted += ForwardCompleted;
            player.AnimationLooped += ForwardLooped;
        }

        private void ForwardStarted(AnimationId id) => AnimationStarted?.Invoke(id);
        private void ForwardCompleted(AnimationId id) => AnimationCompleted?.Invoke(id);
        private void ForwardLooped(AnimationId id) => AnimationLooped?.Invoke(id);

        private void OnDestroy()
        {
            if (resolvedPlayer != null)
            {
                resolvedPlayer.AnimationStarted -= ForwardStarted;
                resolvedPlayer.AnimationCompleted -= ForwardCompleted;
                resolvedPlayer.AnimationLooped -= ForwardLooped;
            }
        }
    }
}
