using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// ScriptableObject-based dictionary for storing key-value pairs
    /// </summary>
    /// <typeparam name="TKey">The type of keys</typeparam>
    /// <typeparam name="TValue">The type of values</typeparam>
    public abstract class ScriptableDictionary<TKey, TValue> : ScriptableVariableBase
    {
        [Header("Dictionary Configuration")]
        [BoxGroup("Dictionary Settings")]
        [SerializeField] protected List<TKey> _keys = new List<TKey>();

        [BoxGroup("Dictionary Settings")]
        [SerializeField] protected List<TValue> _values = new List<TValue>();

        [BoxGroup("Dictionary Settings")]
        [ReadOnly]
        [SerializeField] protected int _count = 0;

        [BoxGroup("Dictionary Settings")]
        [SerializeField] protected bool _allowDuplicateKeys = false;

        [BoxGroup("Dictionary Settings")]
        [SerializeField] protected bool _allowNullValues = true;

        protected Dictionary<TKey, TValue> _runtimeDictionary = new Dictionary<TKey, TValue>();

        /// <summary>
        /// The number of key-value pairs
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Get all keys
        /// </summary>
        public List<TKey> Keys => new List<TKey>(_keys);

        /// <summary>
        /// Get all values
        /// </summary>
        public List<TValue> Values => new List<TValue>(_values);

        /// <summary>
        /// Access dictionary by key
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                if (_runtimeDictionary.TryGetValue(key, out TValue value))
                {
                    return value;
                }
                throw new KeyNotFoundException($"Key '{key}' not found in dictionary");
            }
            set
            {
                SetValue(key, value);
            }
        }

        /// <summary>
        /// Check if key exists
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return _runtimeDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Try to get value by key
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _runtimeDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Add a key-value pair
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            if (!_allowDuplicateKeys && ContainsKey(key))
            {
                throw new ArgumentException($"Key '{key}' already exists");
            }

            if (!_allowNullValues && value == null)
            {
                throw new ArgumentNullException("Value cannot be null");
            }

            _keys.Add(key);
            _values.Add(value);
            _runtimeDictionary[key] = value;
            _count = _keys.Count;

            OnDictionaryChanged();
        }

        /// <summary>
        /// Remove a key-value pair
        /// </summary>
        public bool Remove(TKey key)
        {
            if (_runtimeDictionary.TryGetValue(key, out TValue value))
            {
                int index = _keys.IndexOf(key);
                if (index >= 0)
                {
                    _keys.RemoveAt(index);
                    _values.RemoveAt(index);
                }

                _runtimeDictionary.Remove(key);
                _count = _keys.Count;

                OnDictionaryChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clear all entries
        /// </summary>
        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
            _runtimeDictionary.Clear();
            _count = 0;

            OnDictionaryChanged();
        }

        /// <summary>
        /// Set a value for a key (adds if key doesn't exist)
        /// </summary>
        public void SetValue(TKey key, TValue value)
        {
            if (!_allowNullValues && value == null)
            {
                throw new ArgumentNullException("Value cannot be null");
            }

            if (_runtimeDictionary.ContainsKey(key))
            {
                int index = _keys.IndexOf(key);
                if (index >= 0)
                {
                    _values[index] = value;
                }
                _runtimeDictionary[key] = value;
            }
            else
            {
                Add(key, value);
            }

            OnDictionaryChanged();
        }

        /// <summary>
        /// Called when the dictionary changes
        /// </summary>
        protected virtual void OnDictionaryChanged()
        {
            OnRepaintRequest();
        }

        /// <summary>
        /// Initialize runtime dictionary from serialized data
        /// </summary>
        protected virtual void InitializeRuntimeDictionary()
        {
            _runtimeDictionary.Clear();
            for (int i = 0; i < _keys.Count && i < _values.Count; i++)
            {
                if (_keys[i] != null || typeof(TKey).IsValueType)
                {
                    _runtimeDictionary[_keys[i]] = _values[i];
                }
            }
            _count = _runtimeDictionary.Count;
        }

        /// <summary>
        /// Called when the object is enabled
        /// </summary>
        protected virtual void OnEnable()
        {
            InitializeRuntimeDictionary();
        }

        /// <summary>
        /// Called when the object is validated in the editor
        /// </summary>
        protected virtual void OnValidate()
        {
            // Ensure keys and values lists are synchronized
            while (_values.Count < _keys.Count)
            {
                _values.Add(default(TValue));
            }
            while (_values.Count > _keys.Count)
            {
                _values.RemoveAt(_values.Count - 1);
            }

            InitializeRuntimeDictionary();
        }

        [Button("Clear Dictionary")]
        private void ClearDictionary()
        {
            Clear();
        }

        [Button("Log Contents")]
        private void LogContents()
        {
            Debug.Log($"Dictionary contents ({Count} entries):");
            foreach (var kvp in _runtimeDictionary)
            {
                Debug.Log($"  {kvp.Key} -> {kvp.Value}");
            }
        }
    }

    /// <summary>
    /// String-to-string dictionary
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Dictionaries/String Dictionary", fileName = "StringDictionary")]
    public class StringDictionary : ScriptableDictionary<string, string>
    {
        [Button("Add Entry")]
        private void AddEntry()
        {
            Add($"Key_{Count}", $"Value_{Count}");
        }

        [Button("Get Value by Key")]
        private void GetValueByKey()
        {
            if (Count > 0)
            {
                string firstKey = Keys[0];
                Debug.Log($"Value for '{firstKey}': {this[firstKey]}");
            }
        }
    }

    /// <summary>
    /// String-to-int dictionary
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Dictionaries/String Int Dictionary", fileName = "StringIntDictionary")]
    public class StringIntDictionary : ScriptableDictionary<string, int>
    {
        [Button("Increment Value")]
        private void IncrementValue()
        {
            if (Count > 0)
            {
                string firstKey = Keys[0];
                this[firstKey] = this[firstKey] + 1;
            }
        }

        [Button("Sum All Values")]
        private void SumAllValues()
        {
            int sum = 0;
            foreach (var value in Values)
            {
                sum += value;
            }
            Debug.Log($"Sum of all values: {sum}");
        }
    }

    /// <summary>
    /// String-to-GameObject dictionary
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Dictionaries/Prefab Dictionary", fileName = "PrefabDictionary")]
    public class PrefabDictionary : ScriptableDictionary<string, GameObject>
    {
        [Button("Instantiate Random")]
        private void InstantiateRandom()
        {
            if (Count > 0)
            {
                var randomPrefab = Values[UnityEngine.Random.Range(0, Count)];
                if (randomPrefab != null)
                {
                    Instantiate(randomPrefab);
                }
            }
        }

        [Button("Instantiate by Key")]
        private void InstantiateByKey()
        {
            if (Count > 0)
            {
                string firstKey = Keys[0];
                var prefab = this[firstKey];
                if (prefab != null)
                {
                    Instantiate(prefab);
                }
            }
        }
    }

    /// <summary>
    /// ScriptableObject-based enum system
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Enums/Scriptable Enum", fileName = "ScriptableEnum")]
    public class ScriptableEnum : ScriptableObject
    {
        [Header("Enum Configuration")]
        [BoxGroup("Enum Settings")]
        [SerializeField] protected string _enumName = "NewEnum";

        [BoxGroup("Enum Settings")]
        [SerializeField] protected List<string> _enumValues = new List<string>();

        [BoxGroup("Enum Settings")]
        [ReadOnly]
        [SerializeField] protected int _valueCount = 0;

        /// <summary>
        /// The name of the enum
        /// </summary>
        public string EnumName => _enumName;

        /// <summary>
        /// The enum values
        /// </summary>
        public List<string> Values => new List<string>(_enumValues);

        /// <summary>
        /// The number of values
        /// </summary>
        public int Count => _valueCount;

        /// <summary>
        /// Check if a value exists
        /// </summary>
        public bool Contains(string value)
        {
            return _enumValues.Contains(value);
        }

        /// <summary>
        /// Get the index of a value
        /// </summary>
        public int IndexOf(string value)
        {
            return _enumValues.IndexOf(value);
        }

        /// <summary>
        /// Get value by index
        /// </summary>
        public string GetValue(int index)
        {
            if (index >= 0 && index < _enumValues.Count)
            {
                return _enumValues[index];
            }
            return null;
        }

        /// <summary>
        /// Add a new value
        /// </summary>
        public void AddValue(string value)
        {
            if (!string.IsNullOrEmpty(value) && !_enumValues.Contains(value))
            {
                _enumValues.Add(value);
                _valueCount = _enumValues.Count;
                OnRepaintRequest();
            }
        }

        /// <summary>
        /// Remove a value
        /// </summary>
        public bool RemoveValue(string value)
        {
            if (_enumValues.Remove(value))
            {
                _valueCount = _enumValues.Count;
                OnRepaintRequest();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clear all values
        /// </summary>
        public void Clear()
        {
            _enumValues.Clear();
            _valueCount = 0;
            OnRepaintRequest();
        }

        /// <summary>
        /// Generate C# enum code
        /// </summary>
        public string GenerateEnumCode()
        {
            string code = $"public enum {_enumName}\n{{\n";
            for (int i = 0; i < _enumValues.Count; i++)
            {
                code += $"    {_enumValues[i]} = {i}";
                if (i < _enumValues.Count - 1)
                {
                    code += ",";
                }
                code += "\n";
            }
            code += "}\n";
            return code;
        }

        /// <summary>
        /// Called when the object is validated in the editor
        /// </summary>
        protected virtual void OnValidate()
        {
            _valueCount = _enumValues.Count;
        }

        [Button("Add Value")]
        private void AddNewValue()
        {
            AddValue($"Value_{Count}");
        }

        [Button("Remove Last")]
        private void RemoveLast()
        {
            if (Count > 0)
            {
                RemoveValue(_enumValues[Count - 1]);
            }
        }

        [Button("Generate Code")]
        private void GenerateCode()
        {
            string code = GenerateEnumCode();
            Debug.Log($"Generated enum code:\n{code}");
            GUIUtility.systemCopyBuffer = code;
        }

        [Button("Clear All")]
        private void ClearAll()
        {
            Clear();
        }

        /// <summary>
        /// Request a repaint of the inspector
        /// </summary>
        protected virtual void OnRepaintRequest()
        {
#if UNITY_EDITOR
            // Use the event system instead of direct API call
            // This allows custom editors to subscribe to repaint requests
#endif
        }
    }

    /// <summary>
    /// Scriptable enum with associated data
    /// </summary>
    /// <typeparam name="T">The type of associated data</typeparam>
    public abstract class ScriptableEnum<T> : ScriptableEnum
    {
        [BoxGroup("Enum Data")]
        [SerializeField] protected List<T> _enumData = new List<T>();

        /// <summary>
        /// Get data associated with a value
        /// </summary>
        public T GetData(string value)
        {
            int index = IndexOf(value);
            if (index >= 0 && index < _enumData.Count)
            {
                return _enumData[index];
            }
            return default(T);
        }

        /// <summary>
        /// Get data by index
        /// </summary>
        public T GetData(int index)
        {
            if (index >= 0 && index < _enumData.Count)
            {
                return _enumData[index];
            }
            return default(T);
        }

        /// <summary>
        /// Set data for a value
        /// </summary>
        public void SetData(string value, T data)
        {
            int index = IndexOf(value);
            if (index >= 0)
            {
                while (_enumData.Count <= index)
                {
                    _enumData.Add(default(T));
                }
                _enumData[index] = data;
                OnRepaintRequest();
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            // Ensure data list matches values list
            while (_enumData.Count < _enumValues.Count)
            {
                _enumData.Add(default(T));
            }
            while (_enumData.Count > _enumValues.Count)
            {
                _enumData.RemoveAt(_enumData.Count - 1);
            }
        }
    }

    /// <summary>
    /// Item rarity enum with colors
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Enums/Item Rarity Enum", fileName = "ItemRarityEnum")]
    public class ItemRarityEnum : ScriptableEnum<Color>
    {
        [Button("Add Common Rarity")]
        private void AddCommon()
        {
            AddValue("Common");
            SetData("Common", Color.gray);
        }

        [Button("Add Rare Rarity")]
        private void AddRare()
        {
            AddValue("Rare");
            SetData("Rare", Color.blue);
        }

        [Button("Add Epic Rarity")]
        private void AddEpic()
        {
            AddValue("Epic");
            SetData("Epic", new Color(0.5f, 0f, 0.5f)); // Purple
        }

        [Button("Add Legendary Rarity")]
        private void AddLegendary()
        {
            AddValue("Legendary");
            SetData("Legendary", new Color(1f, 0.5f, 0f)); // Orange
        }
    }

    /// <summary>
    /// Game state enum
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Enums/Game State Enum", fileName = "GameStateEnum")]
    public class GameStateEnum : ScriptableEnum
    {
        [Button("Add Standard States")]
        private void AddStandardStates()
        {
            AddValue("Menu");
            AddValue("Playing");
            AddValue("Paused");
            AddValue("GameOver");
            AddValue("Loading");
        }
    }
}
