using System;
using UnityEngine;
using NaughtyAttributes;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// Generic ScriptableObject variable that can store any type of value.
    /// Provides event system, persistence, validation, and enhanced editor integration.
    /// </summary>
    /// <typeparam name="T">The type of value to store</typeparam>
    public abstract class ScriptableVariable<T> : ScriptableVariableBase
    {
        [Header("Value Configuration")]
        [BoxGroup("Value Settings")]
        [SerializeField] private T _initialValue;

        [BoxGroup("Value Settings")]
        [ReadOnly]
        [SerializeField] private T _runtimeValue;

        [Header("Persistence Settings")]
        [BoxGroup("Persistence")]
        [SerializeField] private bool _saveValue = false;

        [BoxGroup("Persistence")]
        [ShowIf("_saveValue")]
        [ValidateInput("IsValidSaveKey", "Save key cannot be empty when saving is enabled")]
        [SerializeField] private string _saveKey;

        [BoxGroup("Persistence")]
        [ShowIf("_saveValue")]
        [SerializeField] private ResetType _resetType = ResetType.SceneLoaded;

        [Header("Debug Information")]
        [BoxGroup("Debug Info")]
        [ReadOnly]
        [SerializeField] private int _changeCount = 0;

        [BoxGroup("Debug Info")]
        [ReadOnly]
        [SerializeField] private string _lastChangedTime = "Never";

        public enum ResetType
        {
            None,
            SceneLoaded,
            ApplicationStarts
        }

        /// <summary>
        /// Event raised when the value changes
        /// </summary>
        public event Action<T> OnValueChanged;

        /// <summary>
        /// Event raised when the value changes (no parameter)
        /// </summary>
        public event Action OnValueChangedNoParam;

        /// <summary>
        /// The current value of the variable
        /// </summary>
        public virtual T Value
        {
            get => _runtimeValue;
            set
            {
                if (!Equals(_runtimeValue, value))
                {
                    T oldValue = _runtimeValue;
                    _runtimeValue = value;
                    _changeCount++;
                    _lastChangedTime = DateTime.Now.ToString("HH:mm:ss");

                    OnValueChanged?.Invoke(value);
                    OnValueChangedNoParam?.Invoke();

                    if (_saveValue)
                        Save();

                    OnRepaintRequest();
                }
            }
        }

        /// <summary>
        /// The initial/editor value (used for resetting)
        /// </summary>
        public T InitialValue
        {
            get => _initialValue;
            set
            {
                _initialValue = value;
                OnRepaintRequest();
            }
        }

        /// <summary>
        /// Set the value without triggering events (useful for initialization)
        /// </summary>
        public void SetValueWithoutNotify(T value)
        {
            _runtimeValue = value;
            if (_saveValue)
                Save();
            OnRepaintRequest();
        }

        /// <summary>
        /// Reset the value to initial value
        /// </summary>
        [Button("Reset to Initial Value")]
        public override void Reset()
        {
            _runtimeValue = _initialValue;
            _changeCount = 0;
            _lastChangedTime = "Reset at " + DateTime.Now.ToString("HH:mm:ss");
            OnValueChanged?.Invoke(_runtimeValue);
            OnValueChangedNoParam?.Invoke();
        }

        /// <summary>
        /// Load the value from PlayerPrefs
        /// </summary>
        public virtual void Load()
        {
            if (!_saveValue || string.IsNullOrEmpty(_saveKey)) return;

            // Override in derived classes for specific serialization
        }

        /// <summary>
        /// Save the value to PlayerPrefs
        /// </summary>
        public virtual void Save()
        {
            if (!_saveValue || string.IsNullOrEmpty(_saveKey)) return;

            // Override in derived classes for specific serialization
        }

        /// <summary>
        /// Get the save key (auto-generates if not set)
        /// </summary>
        protected string GetSaveKey()
        {
            if (string.IsNullOrEmpty(_saveKey))
            {
                _saveKey = $"{name}_{GetType().Name}";
            }
            return _saveKey;
        }

        /// <summary>
        /// Validation method for save key
        /// </summary>
        private bool IsValidSaveKey()
        {
            return !_saveValue || !string.IsNullOrEmpty(_saveKey);
        }

        /// <summary>
        /// Called when the object is enabled
        /// </summary>
        protected virtual void OnEnable()
        {
            if (_saveValue)
                Load();

            // Handle reset types
            switch (_resetType)
            {
                case ResetType.SceneLoaded:
                    // Unity automatically calls Reset() on scene load for scene objects
                    break;
                case ResetType.ApplicationStarts:
                    // Reset on application start (could be handled by a manager)
                    break;
            }
        }

        public override Type GetGenericType()
        {
            return typeof(T);
        }
    }
}
