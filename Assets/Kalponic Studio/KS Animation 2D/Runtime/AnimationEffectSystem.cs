using UnityEngine;
using System.Collections;
using KalponicStudio.SO_Framework;
using KalponicStudio.Utilities;

namespace KalponicStudio
{
    /// <summary>
    /// Effect system for fire-and-forget animations
    /// Uses KS TweenEngine for smooth animations and KS SO Framework for events
    /// </summary>
    public class AnimationEffectSystem : MonoBehaviour
    {
        #region Singleton
        private static AnimationEffectSystem _instance;
        public static AnimationEffectSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AnimationEffectSystem");
                    _instance = go.AddComponent<AnimationEffectSystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        #endregion

        #region Fields
        [Header("Effect Settings")]
        [SerializeField] private int initialPoolSize = 20;
        [SerializeField] private Transform effectContainer;

        private ObjectPool<EffectInstance> effectPool;
        #endregion

        #region Unity Messages
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeEffectPool();
        }

        private void OnDestroy()
        {
            effectPool?.Clear();
        }
        #endregion

        #region Effect Pool Management
        private void InitializeEffectPool()
        {
            if (effectContainer == null)
            {
                effectContainer = new GameObject("EffectContainer").transform;
                effectContainer.SetParent(transform);
            }

            // Create a basic effect prefab for the pool
            var effectPrefab = CreateBasicEffectPrefab();
            effectPool = new ObjectPool<EffectInstance>(effectPrefab, initialPoolSize, effectContainer);
        }

        private EffectInstance CreateBasicEffectPrefab()
        {
            var effectGO = new GameObject("EffectInstance");
            effectGO.SetActive(false);
            var effect = effectGO.AddComponent<EffectInstance>();
            return effect;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Play a fire-and-forget animation effect
        /// </summary>
        public static void PlayEffect(AnimationEffectData effectData, Vector3 position, Quaternion rotation = default)
        {
            Instance.PlayEffectInternal(effectData, position, rotation);
        }

        /// <summary>
        /// Play a fire-and-forget animation effect with callback
        /// </summary>
        public static void PlayEffect(AnimationEffectData effectData, Vector3 position, System.Action onComplete, Quaternion rotation = default)
        {
            Instance.PlayEffectInternal(effectData, position, rotation, onComplete);
        }

        /// <summary>
        /// Chain multiple effects sequentially
        /// </summary>
        public static EffectChain ChainEffects()
        {
            return new EffectChain();
        }
        #endregion

        #region Internal Implementation
        private void PlayEffectInternal(AnimationEffectData effectData, Vector3 position, Quaternion rotation, System.Action onComplete = null)
        {
            var effectInstance = effectPool.Get();
            effectInstance.transform.position = position;
            effectInstance.transform.rotation = rotation;

            effectInstance.PlayEffect(effectData, () =>
            {
                effectPool.Return(effectInstance);
                onComplete?.Invoke();
            });
        }
        #endregion
    }

    /// <summary>
    /// Data container for animation effects
    /// </summary>
    [CreateAssetMenu(menuName = "KS Animation/Effect Data", fileName = "AnimationEffect")]
    public class AnimationEffectData : ScriptableObject
    {
        [Header("Animation Settings")]
        public Sprite[] sprites;
        public float frameRate = 30f;
        public bool loop = false;

        [Header("Transform Effects")]
        public bool useScaleEffect = false;
        public Vector3 startScale = Vector3.one;
        public Vector3 endScale = Vector3.one;
        public float scaleDuration = 0.5f;

        public bool useRotationEffect = false;
        public Vector3 startRotation;
        public Vector3 endRotation;
        public float rotationDuration = 0.5f;

        [Header("Movement")]
        public bool useMovement = false;
        public Vector3 movementOffset;
        public float movementDuration = 1f;

        [Header("Timing")]
        public float effectDuration = 1f;
        public bool destroyOnComplete = true;

        [Header("Events")]
        public VoidEvent onEffectStart;
        public VoidEvent onEffectComplete;
    }

    /// <summary>
    /// Individual effect instance component
    /// </summary>
    public class EffectInstance : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private AnimationEffectData currentEffect;
        private System.Action onCompleteCallback;
        private float elapsedTime;
        private int currentFrame;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        public void PlayEffect(AnimationEffectData effect, System.Action onComplete)
        {
            currentEffect = effect;
            onCompleteCallback = onComplete;
            elapsedTime = 0f;
            currentFrame = 0;

            // Initialize sprite
            if (effect.sprites != null && effect.sprites.Length > 0)
            {
                spriteRenderer.sprite = effect.sprites[0];
            }

            // Initialize transform
            transform.localScale = effect.startScale;
            transform.localEulerAngles = effect.startRotation;

            // Raise start event
            effect.onEffectStart?.Raise();

            // Start effect coroutine
            StartCoroutine(PlayEffectCoroutine());
        }

        private IEnumerator PlayEffectCoroutine()
        {
            var effect = currentEffect;

            while (elapsedTime < effect.effectDuration)
            {
                elapsedTime += Time.deltaTime;

                // Update sprite animation
                if (effect.sprites != null && effect.sprites.Length > 0)
                {
                    float frameTime = 1f / effect.frameRate;
                    int frameIndex = Mathf.FloorToInt(elapsedTime / frameTime) % effect.sprites.Length;
                    spriteRenderer.sprite = effect.sprites[frameIndex];
                }

                // Update scale effect
                if (effect.useScaleEffect)
                {
                    float t = Mathf.Clamp01(elapsedTime / effect.scaleDuration);
                    transform.localScale = Vector3.Lerp(effect.startScale, effect.endScale, t);
                }

                // Update rotation effect
                if (effect.useRotationEffect)
                {
                    float t = Mathf.Clamp01(elapsedTime / effect.rotationDuration);
                    transform.localEulerAngles = Vector3.Lerp(effect.startRotation, effect.endRotation, t);
                }

                // Update movement
                if (effect.useMovement)
                {
                    float t = Mathf.Clamp01(elapsedTime / effect.movementDuration);
                    transform.position += effect.movementOffset * Time.deltaTime / effect.movementDuration;
                }

                yield return null;
            }

            // Raise complete event
            effect.onEffectComplete?.Raise();

            // Call completion callback
            onCompleteCallback?.Invoke();
        }
    }

    /// <summary>
    /// Effect chaining system for sequential effects
    /// </summary>
    public class EffectChain
    {
        private System.Collections.Generic.List<EffectChainItem> effects = new System.Collections.Generic.List<EffectChainItem>();

        public EffectChain AddEffect(AnimationEffectData effect, Vector3 position, Quaternion rotation = default)
        {
            effects.Add(new EffectChainItem { Effect = effect, Position = position, Rotation = rotation });
            return this;
        }

        public EffectChain Delay(float seconds)
        {
            effects.Add(new EffectChainItem { Delay = seconds });
            return this;
        }

        public void Execute(System.Action onComplete = null)
        {
            AnimationEffectSystem.Instance.StartCoroutine(ExecuteChain(onComplete));
        }

        private System.Collections.IEnumerator ExecuteChain(System.Action onComplete)
        {
            foreach (var item in effects)
            {
                if (item.Delay > 0)
                {
                    yield return new WaitForSeconds(item.Delay);
                }
                else if (item.Effect != null)
                {
                    bool effectComplete = false;
                    AnimationEffectSystem.PlayEffect(item.Effect, item.Position, () => effectComplete = true, item.Rotation);

                    // Wait for effect to complete
                    while (!effectComplete)
                    {
                        yield return null;
                    }
                }
            }

            onComplete?.Invoke();
        }

        private class EffectChainItem
        {
            public AnimationEffectData Effect;
            public Vector3 Position;
            public Quaternion Rotation;
            public float Delay;
        }
    }
}
