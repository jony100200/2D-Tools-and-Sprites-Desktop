using UnityEngine;
using NaughtyAttributes;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// Integer range variable with min/max clamping and validation
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Int Range Variable", fileName = "IntRangeVariable")]
    public class IntRangeVariable : ScriptableVariable<int>
    {
        [BoxGroup("Range Constraints")]
        [MinValue(0)]
        [Tooltip("Minimum allowed value for the integer range.")]
        [SerializeField] private int _minValue = 0;

        [BoxGroup("Range Constraints")]
        [MinValue(0)]
        [ValidateInput("IsMaxGreaterThanMin", "Max value must be greater than min value")]
        [Tooltip("Maximum allowed value for the integer range.")]
        [SerializeField] private int _maxValue = 100;

        public int MinValue => _minValue;
        public int MaxValue => _maxValue;

        public override int Value
        {
            get => base.Value;
            set => base.Value = Mathf.Clamp(value, _minValue, _maxValue);
        }

        private bool IsMaxGreaterThanMin()
        {
            return _maxValue > _minValue;
        }

        [Button("Set to Min Value")]
        private void SetToMin()
        {
            Value = _minValue;
        }

        [Button("Set to Max Value")]
        private void SetToMax()
        {
            Value = _maxValue;
        }

        [Button("Set to Mid Value")]
        private void SetToMid()
        {
            Value = (_minValue + _maxValue) / 2;
        }
    }

    /// <summary>
    /// Float range variable with min/max clamping and validation
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Float Range Variable", fileName = "FloatRangeVariable")]
    public class FloatRangeVariable : ScriptableVariable<float>
    {
        [BoxGroup("Range Constraints")]
        [Tooltip("Minimum allowed value for the float range.")]
        [SerializeField] private float _minValue = 0f;

        [BoxGroup("Range Constraints")]
        [ValidateInput("IsMaxGreaterThanMin", "Max value must be greater than min value")]
        [Tooltip("Maximum allowed value for the float range.")]
        [SerializeField] private float _maxValue = 1f;

        public float MinValue => _minValue;
        public float MaxValue => _maxValue;

        public override float Value
        {
            get => base.Value;
            set => base.Value = Mathf.Clamp(value, _minValue, _maxValue);
        }

        private bool IsMaxGreaterThanMin()
        {
            return _maxValue > _minValue;
        }

        [Button("Set to Min Value")]
        private void SetToMin()
        {
            Value = _minValue;
        }

        [Button("Set to Max Value")]
        private void SetToMax()
        {
            Value = _maxValue;
        }

        [Button("Set to Mid Value")]
        private void SetToMid()
        {
            Value = (_minValue + _maxValue) / 2f;
        }
    }

    /// <summary>
    /// Enhanced string variable with length constraints and utilities
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Enhanced String Variable", fileName = "EnhancedStringVariable")]
    public class EnhancedStringVariable : ScriptableVariable<string>
    {
        [BoxGroup("String Constraints")]
        [MinValue(0)]
        [Tooltip("Maximum allowed length for the string.")]
        [SerializeField] private int _maxLength = 100;

        [BoxGroup("String Constraints")]
        [Tooltip("Whether empty strings are allowed.")]
        [SerializeField] private bool _allowEmpty = true;

        [BoxGroup("String Constraints")]
        [Tooltip("Automatically trim whitespace from the string.")]
        [SerializeField] private bool _autoTrim = true;

        public int MaxLength => _maxLength;
        public bool AllowEmpty => _allowEmpty;
        public bool AutoTrim => _autoTrim;

        public override string Value
        {
            get => base.Value;
            set
            {
                if (_autoTrim && value != null)
                {
                    value = value.Trim();
                }

                if (value != null && value.Length > _maxLength)
                {
                    value = value.Substring(0, _maxLength);
                }

                if (!_allowEmpty && string.IsNullOrEmpty(value))
                {
                    value = "Default";
                }

                base.Value = value;
            }
        }

        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                Value = PlayerPrefs.GetString(GetSaveKey(), InitialValue ?? "");
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                PlayerPrefs.SetString(GetSaveKey(), Value ?? "");
                PlayerPrefs.Save();
            }
        }

        [Button("Clear String")]
        private void Clear()
        {
            Value = _allowEmpty ? "" : "Default";
        }

        [Button("Set to Uppercase")]
        private void ToUpper()
        {
            if (!string.IsNullOrEmpty(Value))
            {
                Value = Value.ToUpper();
            }
        }

        [Button("Set to Lowercase")]
        private void ToLower()
        {
            if (!string.IsNullOrEmpty(Value))
            {
                Value = Value.ToLower();
            }
        }

        [Button("Reverse String")]
        private void Reverse()
        {
            if (!string.IsNullOrEmpty(Value))
            {
                char[] charArray = Value.ToCharArray();
                System.Array.Reverse(charArray);
                Value = new string(charArray);
            }
        }
    }

    /// <summary>
    /// Color variable with HDR support and presets
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Color Variable", fileName = "ColorVariable")]
    public class ColorVariable : ScriptableVariable<Color>
    {
        [BoxGroup("Color Settings")]
        [Tooltip("Enable HDR color support.")]
        [SerializeField] private bool _hdr = false;

        [BoxGroup("Color Settings")]
        [Tooltip("Include alpha channel in the color.")]
        [SerializeField] private bool _alpha = true;

        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                string json = PlayerPrefs.GetString(GetSaveKey(), "");
                if (!string.IsNullOrEmpty(json))
                {
                    ColorData data = JsonUtility.FromJson<ColorData>(json);
                    Value = new Color(data.r, data.g, data.b, data.a);
                }
                else
                {
                    Value = InitialValue;
                }
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                ColorData data = new ColorData
                {
                    r = Value.r,
                    g = Value.g,
                    b = Value.b,
                    a = Value.a
                };
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(GetSaveKey(), json);
                PlayerPrefs.Save();
            }
        }

        [System.Serializable]
        private struct ColorData
        {
            public float r, g, b, a;
        }

        [Button("Set to Red")]
        private void SetRed()
        {
            Value = Color.red;
        }

        [Button("Set to Green")]
        private void SetGreen()
        {
            Value = Color.green;
        }

        [Button("Set to Blue")]
        private void SetBlue()
        {
            Value = Color.blue;
        }

        [Button("Set to White")]
        private void SetWhite()
        {
            Value = Color.white;
        }

        [Button("Set to Black")]
        private void SetBlack()
        {
            Value = Color.black;
        }

        [Button("Set to Random")]
        private void SetRandom()
        {
            Value = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, _alpha ? UnityEngine.Random.value : 1f);
        }

        [Button("Invert Color")]
        private void Invert()
        {
            Value = new Color(1f - Value.r, 1f - Value.g, 1f - Value.b, Value.a);
        }

        [Button("Grayscale")]
        private void ToGrayscale()
        {
            float gray = Value.r * 0.299f + Value.g * 0.587f + Value.b * 0.114f;
            Value = new Color(gray, gray, gray, Value.a);
        }
    }

    /// <summary>
    /// Vector3 variable with normalization and presets
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Vector3 Variable", fileName = "Vector3Variable")]
    public class Vector3Variable : ScriptableVariable<Vector3>
    {
        [BoxGroup("Vector Constraints")]
        [Tooltip("Normalize the vector when set.")]
        [SerializeField] private bool _normalizeOnSet = false;

        [BoxGroup("Vector Constraints")]
        [ShowIf("_normalizeOnSet")]
        [MinValue(0f)]
        [Tooltip("Magnitude to set when normalizing.")]
        [SerializeField] private float _magnitude = 1f;

        [BoxGroup("Vector Info")]
        [ReadOnly]
        [ShowIf("_normalizeOnSet")]
        [Tooltip("Current magnitude of the vector.")]
        [SerializeField] private float _currentMagnitude = 0f;

        public override Vector3 Value
        {
            get => base.Value;
            set
            {
                if (_normalizeOnSet)
                {
                    value = value.normalized * _magnitude;
                    _currentMagnitude = value.magnitude;
                }
                base.Value = value;
            }
        }

        [Button("Set to Zero")]
        private void SetZero()
        {
            Value = Vector3.zero;
        }

        [Button("Set to One")]
        private void SetOne()
        {
            Value = Vector3.one;
        }

        [Button("Set to Up")]
        private void SetUp()
        {
            Value = Vector3.up;
        }

        [Button("Set to Forward")]
        private void SetForward()
        {
            Value = Vector3.forward;
        }

        [Button("Set to Right")]
        private void SetRight()
        {
            Value = Vector3.right;
        }

        [Button("Normalize")]
        private void Normalize()
        {
            Value = Value.normalized;
        }

        [Button("Random Unit Vector")]
        private void RandomUnit()
        {
            Value = UnityEngine.Random.onUnitSphere;
        }

        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                string json = PlayerPrefs.GetString(GetSaveKey(), "");
                if (!string.IsNullOrEmpty(json))
                {
                    Vector3Data data = JsonUtility.FromJson<Vector3Data>(json);
                    Value = new Vector3(data.x, data.y, data.z);
                }
                else
                {
                    Value = InitialValue;
                }
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                Vector3Data data = new Vector3Data
                {
                    x = Value.x,
                    y = Value.y,
                    z = Value.z
                };
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(GetSaveKey(), json);
                PlayerPrefs.Save();
            }
        }

        [System.Serializable]
        private struct Vector3Data
        {
            public float x, y, z;
        }
    }

    /// <summary>
    /// LayerMask variable with preset buttons
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/LayerMask Variable", fileName = "LayerMaskVariable")]
    public class LayerMaskVariable : ScriptableVariable<LayerMask>
    {
        [Button("Everything")]
        private void SetEverything()
        {
            Value = ~0; // All layers
        }

        [Button("Nothing")]
        private void SetNothing()
        {
            Value = 0;
        }

        [Button("Default Layer")]
        private void SetDefault()
        {
            Value = 1 << 0; // Default layer
        }

        [Button("UI Layer")]
        private void SetUI()
        {
            Value = 1 << 5; // UI layer
        }

        [Button("Water Layer")]
        private void SetWater()
        {
            Value = 1 << 4; // Water layer
        }

        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                Value = PlayerPrefs.GetInt(GetSaveKey(), InitialValue.value);
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                PlayerPrefs.SetInt(GetSaveKey(), Value.value);
                PlayerPrefs.Save();
            }
        }
    }

    /// <summary>
    /// Vector2 variable with normalization and presets
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Vector2 Variable", fileName = "Vector2Variable")]
    public class Vector2Variable : ScriptableVariable<Vector2>
    {
        [BoxGroup("Vector Constraints")]
        [Tooltip("Normalize the vector when set.")]
        [SerializeField] private bool _normalizeOnSet = false;

        [BoxGroup("Vector Constraints")]
        [ShowIf("_normalizeOnSet")]
        [MinValue(0f)]
        [Tooltip("Magnitude to set when normalizing.")]
        [SerializeField] private float _magnitude = 1f;

        [BoxGroup("Vector Info")]
        [ReadOnly]
        [ShowIf("_normalizeOnSet")]
        [Tooltip("Current magnitude of the vector.")]
        [SerializeField] private float _currentMagnitude = 0f;

        public override Vector2 Value
        {
            get => base.Value;
            set
            {
                if (_normalizeOnSet)
                {
                    value = value.normalized * _magnitude;
                    _currentMagnitude = value.magnitude;
                }
                base.Value = value;
            }
        }

        [Button("Set to Zero")]
        private void SetZero()
        {
            Value = Vector2.zero;
        }

        [Button("Set to One")]
        private void SetOne()
        {
            Value = Vector2.one;
        }

        [Button("Set to Up")]
        private void SetUp()
        {
            Value = Vector2.up;
        }

        [Button("Set to Right")]
        private void SetRight()
        {
            Value = Vector2.right;
        }

        [Button("Normalize")]
        private void Normalize()
        {
            Value = Value.normalized;
        }

        [Button("Random Unit Vector")]
        private void RandomUnit()
        {
            Value = UnityEngine.Random.insideUnitCircle.normalized;
        }

        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                string json = PlayerPrefs.GetString(GetSaveKey(), "");
                if (!string.IsNullOrEmpty(json))
                {
                    Vector2Data data = JsonUtility.FromJson<Vector2Data>(json);
                    Value = new Vector2(data.x, data.y);
                }
                else
                {
                    Value = InitialValue;
                }
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                Vector2Data data = new Vector2Data
                {
                    x = Value.x,
                    y = Value.y
                };
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(GetSaveKey(), json);
                PlayerPrefs.Save();
            }
        }

        [System.Serializable]
        private struct Vector2Data
        {
            public float x, y;
        }
    }

    /// <summary>
    /// Vector2Int variable with grid-based operations
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Vector2Int Variable", fileName = "Vector2IntVariable")]
    public class Vector2IntVariable : ScriptableVariable<Vector2Int>
    {
        [BoxGroup("Grid Constraints")]
        [MinValue(0)]
        [Tooltip("Maximum X value for the grid.")]
        [SerializeField] private int _maxX = 100;

        [BoxGroup("Grid Constraints")]
        [MinValue(0)]
        [Tooltip("Maximum Y value for the grid.")]
        [SerializeField] private int _maxY = 100;

        [BoxGroup("Grid Constraints")]
        [Tooltip("Clamp values to the grid bounds.")]
        [SerializeField] private bool _clampToBounds = true;

        public int MaxX => _maxX;
        public int MaxY => _maxY;
        public bool ClampToBounds => _clampToBounds;

        public override Vector2Int Value
        {
            get => base.Value;
            set
            {
                if (_clampToBounds)
                {
                    value.x = Mathf.Clamp(value.x, 0, _maxX);
                    value.y = Mathf.Clamp(value.y, 0, _maxY);
                }
                base.Value = value;
            }
        }

        [Button("Set to Zero")]
        private void SetZero()
        {
            Value = Vector2Int.zero;
        }

        [Button("Set to One")]
        private void SetOne()
        {
            Value = Vector2Int.one;
        }

        [Button("Set to Up")]
        private void SetUp()
        {
            Value = Vector2Int.up;
        }

        [Button("Set to Right")]
        private void SetRight()
        {
            Value = Vector2Int.right;
        }

        [Button("Move Up")]
        private void MoveUp()
        {
            Value += Vector2Int.up;
        }

        [Button("Move Down")]
        private void MoveDown()
        {
            Value += Vector2Int.down;
        }

        [Button("Move Left")]
        private void MoveLeft()
        {
            Value += Vector2Int.left;
        }

        [Button("Move Right")]
        private void MoveRight()
        {
            Value += Vector2Int.right;
        }

        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                string json = PlayerPrefs.GetString(GetSaveKey(), "");
                if (!string.IsNullOrEmpty(json))
                {
                    Vector2IntData data = JsonUtility.FromJson<Vector2IntData>(json);
                    Value = new Vector2Int(data.x, data.y);
                }
                else
                {
                    Value = InitialValue;
                }
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                Vector2IntData data = new Vector2IntData
                {
                    x = Value.x,
                    y = Value.y
                };
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(GetSaveKey(), json);
                PlayerPrefs.Save();
            }
        }

        [System.Serializable]
        private struct Vector2IntData
        {
            public int x, y;
        }
    }

    /// <summary>
    /// Quaternion variable with rotation utilities
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Quaternion Variable", fileName = "QuaternionVariable")]
    public class QuaternionVariable : ScriptableVariable<Quaternion>
    {
        [BoxGroup("Rotation Info")]
        [ReadOnly]
        [Tooltip("Euler angles representation of the rotation.")]
        [SerializeField] private Vector3 _eulerAngles;

        [BoxGroup("Rotation Info")]
        [ReadOnly]
        [Tooltip("Angle of rotation in degrees.")]
        [SerializeField] private float _angle;

        [BoxGroup("Rotation Info")]
        [ReadOnly]
        [Tooltip("Axis of rotation.")]
        [SerializeField] private Vector3 _axis;

        public override Quaternion Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
                UpdateRotationInfo();
            }
        }

        private void UpdateRotationInfo()
        {
            _eulerAngles = Value.eulerAngles;
            Value.ToAngleAxis(out _angle, out _axis);
        }

        [Button("Set Identity")]
        private void SetIdentity()
        {
            Value = Quaternion.identity;
        }

        [Button("Set from Euler")]
        private void SetFromEuler()
        {
            // This would need additional UI, for now just reset
            Value = Quaternion.Euler(_eulerAngles);
        }

        [Button("Look at Forward")]
        private void LookAtForward()
        {
            Value = Quaternion.LookRotation(Vector3.forward);
        }

        [Button("Look at Up")]
        private void LookAtUp()
        {
            Value = Quaternion.LookRotation(Vector3.up);
        }

        [Button("Random Rotation")]
        private void RandomRotation()
        {
            Value = UnityEngine.Random.rotation;
        }

        [Button("Inverse")]
        private void Inverse()
        {
            Value = Quaternion.Inverse(Value);
        }

        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                string json = PlayerPrefs.GetString(GetSaveKey(), "");
                if (!string.IsNullOrEmpty(json))
                {
                    QuaternionData data = JsonUtility.FromJson<QuaternionData>(json);
                    Value = new Quaternion(data.x, data.y, data.z, data.w);
                }
                else
                {
                    Value = InitialValue;
                }
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                QuaternionData data = new QuaternionData
                {
                    x = Value.x,
                    y = Value.y,
                    z = Value.z,
                    w = Value.w
                };
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(GetSaveKey(), json);
                PlayerPrefs.Save();
            }
        }

        [System.Serializable]
        private struct QuaternionData
        {
            public float x, y, z, w;
        }
    }

    /// <summary>
    /// Component reference variable for Unity components
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Component Variable", fileName = "ComponentVariable")]
    public class ComponentVariable : ScriptableVariable<Component>
    {
        [BoxGroup("Component Info")]
        [ReadOnly]
        [Tooltip("Type of the referenced component.")]
        [SerializeField] private string _componentType = "";

        [BoxGroup("Component Info")]
        [ReadOnly]
        [Tooltip("Name of the GameObject the component is attached to.")]
        [SerializeField] private string _gameObjectName = "";

        [BoxGroup("Component Info")]
        [ReadOnly]
        [Tooltip("Whether the component reference is valid.")]
        [SerializeField] private bool _isValid = false;

        public override Component Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
                UpdateComponentInfo();
            }
        }

        private void UpdateComponentInfo()
        {
            if (Value != null)
            {
                _componentType = Value.GetType().Name;
                _gameObjectName = Value.gameObject.name;
                _isValid = true;
            }
            else
            {
                _componentType = "";
                _gameObjectName = "";
                _isValid = false;
            }
        }

        [Button("Clear Reference")]
        private void ClearReference()
        {
            Value = null;
        }

        [Button("Log Component Info")]
        private void LogInfo()
        {
            if (Value != null)
            {
                Debug.Log($"Component: {_componentType} on GameObject: {_gameObjectName}");
            }
            else
            {
                Debug.Log("No component assigned");
            }
        }

        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                string json = PlayerPrefs.GetString(GetSaveKey(), "");
                if (!string.IsNullOrEmpty(json))
                {
                    ComponentReferenceData data = JsonUtility.FromJson<ComponentReferenceData>(json);
                    if (!string.IsNullOrEmpty(data.gameObjectPath) && !string.IsNullOrEmpty(data.componentType))
                    {
                        // Try to find the GameObject by path
                        GameObject obj = GameObject.Find(data.gameObjectPath);
                        if (obj != null)
                        {
                            // Try to get the component
                            System.Type componentType = System.Type.GetType(data.componentType);
                            if (componentType != null)
                            {
                                Component component = obj.GetComponent(componentType);
                                if (component != null)
                                {
                                    Value = component;
                                    return;
                                }
                            }
                        }
                        Debug.LogWarning($"Could not restore component reference: {data.gameObjectPath} -> {data.componentType}");
                    }
                }
                // If we can't restore, set to initial value (usually null)
                Value = InitialValue;
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                ComponentReferenceData data = new ComponentReferenceData();

                if (Value != null && Value.gameObject != null)
                {
                    // Store the GameObject path (this is fragile but works for simple cases)
                    data.gameObjectPath = Value.gameObject.name; // Could be enhanced to use full hierarchy path
                    data.componentType = Value.GetType().AssemblyQualifiedName;
                }

                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(GetSaveKey(), json);
                PlayerPrefs.Save();
            }
        }

        [System.Serializable]
        private struct ComponentReferenceData
        {
            public string gameObjectPath;
            public string componentType;
        }
    }
}
