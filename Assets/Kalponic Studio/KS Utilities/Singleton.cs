using UnityEngine;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Base singleton class for managers and services
    /// Ensures only one instance exists and provides global access
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again.");
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindFirstObjectByType<T>();

                        if (instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).ToString() + " (Singleton)";

                            DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnAwake();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                applicationIsQuitting = true;
                instance = null;
            }
        }

        /// <summary>
        /// Override this method instead of Awake() for initialization
        /// </summary>
        protected virtual void OnAwake() { }

        /// <summary>
        /// Check if singleton instance exists
        /// </summary>
        public static bool Exists => instance != null;
    }

    /// <summary>
    /// Persistent singleton that survives scene changes
    /// </summary>
    public abstract class PersistentSingleton<T> : Singleton<T> where T : PersistentSingleton<T>
    {
        protected override void Awake()
        {
            base.Awake();
            // Additional persistent setup can go here
        }
    }

    /// <summary>
    /// Scene singleton that gets destroyed on scene change
    /// </summary>
    public abstract class SceneSingleton<T> : MonoBehaviour where T : SceneSingleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();

                    if (instance == null)
                    {
                        Debug.LogError($"[SceneSingleton] No instance of {typeof(T)} found in scene!");
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                OnAwake();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// Override this method instead of Awake() for initialization
        /// </summary>
        protected virtual void OnAwake() { }
    }
}
