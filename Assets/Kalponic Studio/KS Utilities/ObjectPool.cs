using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Generic object pool for performance optimization
    /// Reusable for any Unity object that needs pooling
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly Queue<T> availableObjects = new Queue<T>();
        private readonly HashSet<T> activeObjects = new HashSet<T>();
        private readonly T prefab;
        private readonly Transform parent;
        private readonly int initialSize;

        public ObjectPool(T prefab, int initialSize = 10, Transform parent = null)
        {
            this.prefab = prefab;
            this.initialSize = initialSize;
            this.parent = parent;

            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private T CreateNewObject()
        {
            T newObject = Object.Instantiate(prefab, parent);
            newObject.gameObject.SetActive(false);
            availableObjects.Enqueue(newObject);
            return newObject;
        }

        public T Get()
        {
            T obj;
            if (availableObjects.Count > 0)
            {
                obj = availableObjects.Dequeue();
            }
            else
            {
                obj = CreateNewObject();
            }

            activeObjects.Add(obj);
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            if (activeObjects.Contains(obj))
            {
                activeObjects.Remove(obj);
                obj.gameObject.SetActive(false);
                availableObjects.Enqueue(obj);
            }
        }

        public void ReturnAll()
        {
            foreach (var obj in new List<T>(activeObjects))
            {
                Return(obj);
            }
        }

        public void Clear()
        {
            foreach (var obj in availableObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }

            foreach (var obj in activeObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }

            availableObjects.Clear();
            activeObjects.Clear();
        }

        public int AvailableCount => availableObjects.Count;
        public int ActiveCount => activeObjects.Count;
        public int TotalCount => AvailableCount + ActiveCount;
    }

    /// <summary>
    /// MonoBehaviour-based object pool for automatic lifecycle management
    /// </summary>
    public class MonoObjectPool<T> : MonoBehaviour where T : Component
    {
        [Header("Pool Settings")]
        [Tooltip("The prefab to instantiate for the pool.")]
        [SerializeField] private T prefab;
        [Tooltip("Initial number of objects to create in the pool.")]
        [SerializeField] private int initialSize = 10;
        [Tooltip("Whether the pool should expand when all objects are in use.")]
        [SerializeField] private bool autoExpand = true;

        private ObjectPool<T> pool;

        private void Awake()
        {
            pool = new ObjectPool<T>(prefab, initialSize, transform);
        }

        public T Get()
        {
            return pool.Get();
        }

        public void Return(T obj)
        {
            pool.Return(obj);
        }

        public void ReturnAll()
        {
            pool.ReturnAll();
        }

        private void OnDestroy()
        {
            pool?.Clear();
        }

        // Public properties for inspection
        public int AvailableCount => pool?.AvailableCount ?? 0;
        public int ActiveCount => pool?.ActiveCount ?? 0;
        public int TotalCount => pool?.TotalCount ?? 0;
    }
}
