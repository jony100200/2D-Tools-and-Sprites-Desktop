using UnityEngine;
using System.Collections.Generic;
using KalponicStudio.Utilities;

namespace KalponicStudio
{
    /// <summary>
    /// Performance optimization system using KS Utilities ObjectPool
    /// Manages pooling for animation objects, sprites, and effects
    /// </summary>
    public class AnimationPerformanceSystem : MonoBehaviour
    {
        #region Singleton
        private static AnimationPerformanceSystem _instance;
        public static AnimationPerformanceSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AnimationPerformanceSystem");
                    _instance = go.AddComponent<AnimationPerformanceSystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        #endregion

        #region Fields
        [Header("Pool Settings")]
        [SerializeField] private int spriteRendererPoolSize = 50;
        [SerializeField] private int animationObjectPoolSize = 20;
        [SerializeField] private int effectPoolSize = 30;

        [Header("Memory Management")]
        [SerializeField] private float cleanupInterval = 30f;
        [SerializeField] private int maxPoolSize = 100;

        private ObjectPool<SpriteRenderer> spriteRendererPool;
        private ObjectPool<PooledAnimationObject> animationObjectPool;
        private ObjectPool<EffectInstance> effectObjectPool;

        private Transform poolContainer;
        private float lastCleanupTime;
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

            InitializePools();
            StartCoroutine(CleanupCoroutine());
        }

        private void Update()
        {
            // Periodic cleanup check
            if (Time.time - lastCleanupTime > cleanupInterval)
            {
                CleanupUnusedPools();
                lastCleanupTime = Time.time;
            }
        }

        private void OnDestroy()
        {
            spriteRendererPool?.Clear();
            animationObjectPool?.Clear();
            effectObjectPool?.Clear();
        }
        #endregion

        #region Pool Initialization
        private void InitializePools()
        {
            poolContainer = new GameObject("PoolContainer").transform;
            poolContainer.SetParent(transform);

            // Initialize SpriteRenderer pool
            var spriteRendererPrefab = CreateSpriteRendererPrefab();
            spriteRendererPool = new ObjectPool<SpriteRenderer>(spriteRendererPrefab, spriteRendererPoolSize, poolContainer);

            // Initialize Animation Object pool
            var animationObjectPrefab = CreateAnimationObjectPrefab();
            animationObjectPool = new ObjectPool<PooledAnimationObject>(animationObjectPrefab, animationObjectPoolSize, poolContainer);

            // Initialize Effect Object pool
            var effectObjectPrefab = CreateEffectObjectPrefab();
            effectObjectPool = new ObjectPool<EffectInstance>(effectObjectPrefab, effectPoolSize, poolContainer);
        }

        private SpriteRenderer CreateSpriteRendererPrefab()
        {
            var go = new GameObject("PooledSpriteRenderer");
            go.SetActive(false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 0;
            return renderer;
        }

        private PooledAnimationObject CreateAnimationObjectPrefab()
        {
            var go = new GameObject("PooledAnimationObject");
            go.SetActive(false);
            go.AddComponent<SpriteRenderer>();
            var pooledObj = go.AddComponent<PooledAnimationObject>();
            // Add a basic animator component
            go.AddComponent<PlayableAnimatorComponent>();
            return pooledObj;
        }

        private EffectInstance CreateEffectObjectPrefab()
        {
            var go = new GameObject("PooledEffectObject");
            go.SetActive(false);
            go.AddComponent<SpriteRenderer>();
            var effect = go.AddComponent<EffectInstance>();
            return effect;
        }
        #endregion

        #region Public API - SpriteRenderer Pooling
        /// <summary>
        /// Get a pooled SpriteRenderer
        /// </summary>
        public static SpriteRenderer GetSpriteRenderer()
        {
            return Instance.spriteRendererPool.Get();
        }

        /// <summary>
        /// Return a SpriteRenderer to the pool
        /// </summary>
        public static void ReturnSpriteRenderer(SpriteRenderer renderer)
        {
            if (renderer != null)
            {
                renderer.gameObject.SetActive(false);
                Instance.spriteRendererPool.Return(renderer);
            }
        }
        #endregion

        #region Public API - Animation Object Pooling
        /// <summary>
        /// Get a pooled animation object with KSAnimationController
        /// </summary>
        public static GameObject GetAnimationObject()
        {
            var pooledObj = Instance.animationObjectPool.Get();
            return pooledObj != null ? pooledObj.gameObject : null;
        }

        /// <summary>
        /// Return an animation object to the pool
        /// </summary>
        public static void ReturnAnimationObject(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                // Reset animation controller
                var pooledObj = obj.GetComponent<PooledAnimationObject>();
                if (pooledObj != null)
                {
                    pooledObj.ResetAnimation();
                    Instance.animationObjectPool.Return(pooledObj);
                }
            }
        }
        #endregion

        #region Public API - Effect Object Pooling
        /// <summary>
        /// Get a pooled effect object
        /// </summary>
        public static GameObject GetEffectObject()
        {
            var effect = Instance.effectObjectPool.Get();
            return effect != null ? effect.gameObject : null;
        }

        /// <summary>
        /// Return an effect object to the pool
        /// </summary>
        public static void ReturnEffectObject(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                var effect = obj.GetComponent<EffectInstance>();
                if (effect != null)
                {
                    Instance.effectObjectPool.Return(effect);
                }
            }
        }
        #endregion

        #region Memory Management
        private System.Collections.IEnumerator CleanupCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(cleanupInterval);
                CleanupUnusedPools();
            }
        }

        private void CleanupUnusedPools()
        {
            // Trim pools if they exceed max size
            TrimPoolIfNeeded(spriteRendererPool, maxPoolSize);
            TrimPoolIfNeeded(animationObjectPool, maxPoolSize);
            TrimPoolIfNeeded(effectObjectPool, maxPoolSize);

            // Force garbage collection if needed
            System.GC.Collect();
        }

        private void TrimPoolIfNeeded<T>(ObjectPool<T> pool, int maxSize) where T : Component
        {
            if (pool.TotalCount > maxSize)
            {
                int excess = pool.TotalCount - maxSize;
                for (int i = 0; i < excess; i++)
                {
                    var item = pool.Get(); // This will create if needed, but we want to remove
                    if (item != null)
                    {
                        Destroy(item.gameObject);
                    }
                }
            }
        }
        #endregion

        #region Performance Monitoring
        /// <summary>
        /// Get current pool statistics
        /// </summary>
        public static PoolStats GetPoolStats()
        {
            return new PoolStats
            {
                SpriteRendererPoolSize = Instance.spriteRendererPool.TotalCount,
                AnimationObjectPoolSize = Instance.animationObjectPool.TotalCount,
                EffectObjectPoolSize = Instance.effectObjectPool.TotalCount,
                TotalMemoryUsage = CalculateMemoryUsage()
            };
        }

        private static long CalculateMemoryUsage()
        {
            // Rough estimation - each pooled object uses some memory
            long baseMemoryPerObject = 1024; // 1KB per object estimate
            return (Instance.spriteRendererPool.TotalCount +
                   Instance.animationObjectPool.TotalCount +
                   Instance.effectObjectPool.TotalCount) * baseMemoryPerObject;
        }
        #endregion
    }

    /// <summary>
    /// Pool statistics for monitoring
    /// </summary>
    public struct PoolStats
    {
        public int SpriteRendererPoolSize;
        public int AnimationObjectPoolSize;
        public int EffectObjectPoolSize;
        public long TotalMemoryUsage;
    }

    /// <summary>
    /// Extension methods for easy pooling
    /// </summary>
    public static class AnimationPoolingExtensions
    {
        /// <summary>
        /// Automatically return this object to the pool when destroyed
        /// </summary>
        public static void ReturnToPoolOnDestroy(this GameObject obj)
        {
            var returner = obj.AddComponent<PoolReturner>();
        }

        /// <summary>
        /// Automatically return this SpriteRenderer to the pool when destroyed
        /// </summary>
        public static void ReturnToPoolOnDestroy(this SpriteRenderer renderer)
        {
            var returner = renderer.gameObject.AddComponent<PoolReturner>();
            returner.spriteRenderer = renderer;
        }
    }

    /// <summary>
    /// Component that returns objects to pool on destroy
    /// </summary>
    public class PoolReturner : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;

        private void OnDestroy()
        {
            if (spriteRenderer != null)
            {
                AnimationPerformanceSystem.ReturnSpriteRenderer(spriteRenderer);
            }
            else
            {
                // Try to determine object type and return appropriately
                if (GetComponent<PooledAnimationObject>() != null)
                {
                    AnimationPerformanceSystem.ReturnAnimationObject(gameObject);
                }
                else if (GetComponent<EffectInstance>() != null)
                {
                    AnimationPerformanceSystem.ReturnEffectObject(gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Component attached to pooled animation objects
    /// </summary>
    public class PooledAnimationObject : MonoBehaviour
    {
        [HideInInspector]
        public IAnimator animator;

        private void Awake()
        {
            animator = GetComponent<IAnimator>();
        }

        public void ResetAnimation()
        {
            if (animator != null)
            {
                animator.Stop();
            }
        }
    }
}
