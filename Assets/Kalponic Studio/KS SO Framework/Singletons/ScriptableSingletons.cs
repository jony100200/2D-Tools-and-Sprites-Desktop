using System;
using UnityEngine;
using NaughtyAttributes;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// Base class for ScriptableObject singletons that provide global access
    /// </summary>
    public abstract class ScriptableSingleton<T> : ScriptableVariableBase where T : ScriptableSingleton<T>
    {
        private static T _instance;

        [Header("Singleton Configuration")]
        [BoxGroup("Singleton Settings")]
        [ReadOnly]
        [SerializeField] private bool _isInitialized = false;

        [BoxGroup("Singleton Settings")]
        [SerializeField] private bool _dontDestroyOnLoad = true;

        [BoxGroup("Singleton Settings")]
        [SerializeField] private bool _hideInHierarchy = true;

        /// <summary>
        /// Get the singleton instance
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to find existing instance
                    _instance = Resources.FindObjectsOfTypeAll<T>()[0] as T;

                    if (_instance == null)
                    {
                        // Create new instance
                        _instance = CreateInstance<T>();
                        _instance.name = typeof(T).Name;

                        // Initialize the singleton
                        _instance.InitializeSingleton();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Check if the singleton exists
        /// </summary>
        public static bool Exists => _instance != null;

        /// <summary>
        /// Initialize the singleton
        /// </summary>
        protected virtual void InitializeSingleton()
        {
            if (_dontDestroyOnLoad)
            {
                // Note: ScriptableObjects don't persist across scenes like MonoBehaviours
                // This is just for configuration
            }

            if (_hideInHierarchy)
            {
                // ScriptableObjects are assets, not scene objects, so this doesn't apply
                // But we can set a flag for editor tools
            }

            _isInitialized = true;
            OnSingletonInitialized();
        }

        /// <summary>
        /// Called when the singleton is initialized
        /// </summary>
        protected virtual void OnSingletonInitialized()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Reset the singleton instance
        /// </summary>
        [Button("Reset Singleton")]
        public static void ResetInstance()
        {
            if (_instance != null)
            {
                _instance.OnSingletonReset();
                _instance = null;
            }
        }

        /// <summary>
        /// Called when the singleton is reset
        /// </summary>
        protected virtual void OnSingletonReset()
        {
            _isInitialized = false;
        }

        /// <summary>
        /// Called when the object is enabled
        /// </summary>
        protected virtual void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this as T;
                InitializeSingleton();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"Multiple instances of {typeof(T).Name} found. Destroying duplicate.");
#if UNITY_EDITOR
                DestroyImmediate(this);
#else
                Destroy(this);
#endif
            }
        }

        /// <summary>
        /// Called when the object is destroyed
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                OnSingletonReset();
                _instance = null;
            }
        }
    }

    /// <summary>
    /// Game settings singleton
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Singletons/Game Settings", fileName = "GameSettings")]
    public class GameSettings : ScriptableSingleton<GameSettings>
    {
        [Header("Audio Settings")]
        [BoxGroup("Audio")]
        [Range(0f, 1f)]
        [SerializeField] private float _masterVolume = 1f;

        [BoxGroup("Audio")]
        [Range(0f, 1f)]
        [SerializeField] private float _musicVolume = 0.7f;

        [BoxGroup("Audio")]
        [Range(0f, 1f)]
        [SerializeField] private float _sfxVolume = 0.8f;

        [Header("Graphics Settings")]
        [BoxGroup("Graphics")]
        [SerializeField] private bool _fullscreen = true;

        [BoxGroup("Graphics")]
        [SerializeField] private int _resolutionWidth = 1920;

        [BoxGroup("Graphics")]
        [SerializeField] private int _resolutionHeight = 1080;

        [BoxGroup("Graphics")]
        [Range(0.5f, 2f)]
        [SerializeField] private float _renderScale = 1f;

        [Header("Gameplay Settings")]
        [BoxGroup("Gameplay")]
        [SerializeField] private bool _autoSave = true;

        [BoxGroup("Gameplay")]
        [Range(30, 300)]
        [SerializeField] private int _autoSaveInterval = 60;

        [BoxGroup("Gameplay")]
        [SerializeField] private string _language = "en";

        // Properties
        public float MasterVolume => _masterVolume;
        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;
        public bool Fullscreen => _fullscreen;
        public int ResolutionWidth => _resolutionWidth;
        public int ResolutionHeight => _resolutionHeight;
        public float RenderScale => _renderScale;
        public bool AutoSave => _autoSave;
        public int AutoSaveInterval => _autoSaveInterval;
        public string Language => _language;

        // Methods to modify settings
        public void SetMasterVolume(float volume) => _masterVolume = Mathf.Clamp01(volume);
        public void SetMusicVolume(float volume) => _musicVolume = Mathf.Clamp01(volume);
        public void SetSfxVolume(float volume) => _sfxVolume = Mathf.Clamp01(volume);
        public void SetFullscreen(bool fullscreen) => _fullscreen = fullscreen;
        public void SetResolution(int width, int height)
        {
            _resolutionWidth = Mathf.Max(640, width);
            _resolutionHeight = Mathf.Max(480, height);
        }
        public void SetRenderScale(float scale) => _renderScale = Mathf.Clamp(scale, 0.5f, 2f);
        public void SetAutoSave(bool autoSave) => _autoSave = autoSave;
        public void SetAutoSaveInterval(int interval) => _autoSaveInterval = Mathf.Clamp(interval, 30, 300);
        public void SetLanguage(string language) => _language = language ?? "en";

        [Button("Apply Graphics Settings")]
        private void ApplyGraphicsSettings()
        {
            Screen.fullScreen = _fullscreen;
            Screen.SetResolution(_resolutionWidth, _resolutionHeight, _fullscreen);
            // Additional graphics settings would be applied here
        }

        [Button("Reset to Defaults")]
        private void ResetToDefaults()
        {
            _masterVolume = 1f;
            _musicVolume = 0.7f;
            _sfxVolume = 0.8f;
            _fullscreen = true;
            _resolutionWidth = 1920;
            _resolutionHeight = 1080;
            _renderScale = 1f;
            _autoSave = true;
            _autoSaveInterval = 60;
            _language = "en";
        }
    }

    /// <summary>
    /// Game state singleton
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Singletons/Game State", fileName = "GameState")]
    public class GameState : ScriptableSingleton<GameState>
    {
        [Header("Game Progress")]
        [BoxGroup("Progress")]
        [ReadOnly]
        [SerializeField] private int _currentLevel = 1;

        [BoxGroup("Progress")]
        [ReadOnly]
        [SerializeField] private int _experience = 0;

        [BoxGroup("Progress")]
        [ReadOnly]
        [SerializeField] private int _playerLevel = 1;

        [Header("Resources")]
        [BoxGroup("Resources")]
        [ReadOnly]
        [SerializeField] private int _gold = 0;

        [BoxGroup("Resources")]
        [ReadOnly]
        [SerializeField] private int _gems = 0;

        [BoxGroup("Resources")]
        [ReadOnly]
        [SerializeField] private int _keys = 0;

        [Header("Session Data")]
        [BoxGroup("Session")]
        [ReadOnly]
        [SerializeField] private float _playTime = 0f;

        [BoxGroup("Session")]
        [ReadOnly]
        [SerializeField] private int _enemiesKilled = 0;

        [BoxGroup("Session")]
        [ReadOnly]
        [SerializeField] private int _itemsCollected = 0;

        // Properties
        public int CurrentLevel => _currentLevel;
        public int Experience => _experience;
        public int PlayerLevel => _playerLevel;
        public int Gold => _gold;
        public int Gems => _gems;
        public int Keys => _keys;
        public float PlayTime => _playTime;
        public int EnemiesKilled => _enemiesKilled;
        public int ItemsCollected => _itemsCollected;

        // Methods to modify state
        public void SetCurrentLevel(int level) => _currentLevel = Mathf.Max(1, level);
        public void AddExperience(int amount) => _experience = Mathf.Max(0, _experience + amount);
        public void SetPlayerLevel(int level) => _playerLevel = Mathf.Max(1, level);
        public void AddGold(int amount) => _gold = Mathf.Max(0, _gold + amount);
        public void AddGems(int amount) => _gems = Mathf.Max(0, _gems + amount);
        public void AddKeys(int amount) => _keys = Mathf.Max(0, _keys + amount);
        public void AddPlayTime(float time) => _playTime += Mathf.Max(0, time);
        public void AddEnemiesKilled(int count) => _enemiesKilled += Mathf.Max(0, count);
        public void AddItemsCollected(int count) => _itemsCollected += Mathf.Max(0, count);

        // Utility methods
        public void ResetSessionData()
        {
            _playTime = 0f;
            _enemiesKilled = 0;
            _itemsCollected = 0;
        }

        public void ResetAllProgress()
        {
            _currentLevel = 1;
            _experience = 0;
            _playerLevel = 1;
            _gold = 0;
            _gems = 0;
            _keys = 0;
            ResetSessionData();
        }

        [Button("Level Up")]
        private void LevelUp()
        {
            _playerLevel++;
        }

        [Button("Add Test Gold")]
        private void AddTestGold()
        {
            AddGold(100);
        }

        [Button("Reset Progress")]
        private void ResetProgress()
        {
            ResetAllProgress();
        }
    }

    /// <summary>
    /// Input settings singleton
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Singletons/Input Settings", fileName = "InputSettings")]
    public class InputSettings : ScriptableSingleton<InputSettings>
    {
        [Header("Mouse Settings")]
        [BoxGroup("Mouse")]
        [Range(0.1f, 3f)]
        [SerializeField] private float _mouseSensitivity = 1f;

        [BoxGroup("Mouse")]
        [SerializeField] private bool _invertMouseY = false;

        [BoxGroup("Mouse")]
        [Range(0.1f, 5f)]
        [SerializeField] private float _mouseSmoothing = 1f;

        [Header("Controller Settings")]
        [BoxGroup("Controller")]
        [Range(0.1f, 3f)]
        [SerializeField] private float _controllerSensitivity = 1f;

        [BoxGroup("Controller")]
        [SerializeField] private bool _rumbleEnabled = true;

        [BoxGroup("Controller")]
        [Range(0f, 1f)]
        [SerializeField] private float _rumbleStrength = 0.5f;

        [Header("Key Bindings")]
        [BoxGroup("Keys")]
        [SerializeField] private KeyCode _interactKey = KeyCode.E;

        [BoxGroup("Keys")]
        [SerializeField] private KeyCode _inventoryKey = KeyCode.I;

        [BoxGroup("Keys")]
        [SerializeField] private KeyCode _pauseKey = KeyCode.Escape;

        // Properties
        public float MouseSensitivity => _mouseSensitivity;
        public bool InvertMouseY => _invertMouseY;
        public float MouseSmoothing => _mouseSmoothing;
        public float ControllerSensitivity => _controllerSensitivity;
        public bool RumbleEnabled => _rumbleEnabled;
        public float RumbleStrength => _rumbleStrength;
        public KeyCode InteractKey => _interactKey;
        public KeyCode InventoryKey => _inventoryKey;
        public KeyCode PauseKey => _pauseKey;

        // Methods to modify settings
        public void SetMouseSensitivity(float sensitivity) => _mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 3f);
        public void SetInvertMouseY(bool invert) => _invertMouseY = invert;
        public void SetMouseSmoothing(float smoothing) => _mouseSmoothing = Mathf.Clamp(smoothing, 0.1f, 5f);
        public void SetControllerSensitivity(float sensitivity) => _controllerSensitivity = Mathf.Clamp(sensitivity, 0.1f, 3f);
        public void SetRumbleEnabled(bool enabled) => _rumbleEnabled = enabled;
        public void SetRumbleStrength(float strength) => _rumbleStrength = Mathf.Clamp01(strength);
        public void SetInteractKey(KeyCode key) => _interactKey = key;
        public void SetInventoryKey(KeyCode key) => _inventoryKey = key;
        public void SetPauseKey(KeyCode key) => _pauseKey = key;

        [Button("Reset to Defaults")]
        private void ResetToDefaults()
        {
            _mouseSensitivity = 1f;
            _invertMouseY = false;
            _mouseSmoothing = 1f;
            _controllerSensitivity = 1f;
            _rumbleEnabled = true;
            _rumbleStrength = 0.5f;
            _interactKey = KeyCode.E;
            _inventoryKey = KeyCode.I;
            _pauseKey = KeyCode.Escape;
        }
    }
}
