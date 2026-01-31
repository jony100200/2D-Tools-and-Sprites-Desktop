using System;
using UnityEngine;
using NaughtyAttributes;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// Base class for variable references that can point to other variables
    /// </summary>
    public abstract class VariableReferenceBase : ScriptableVariableBase
    {
        [Header("Reference Configuration")]
        [BoxGroup("Reference Settings")]
        [SerializeField] protected bool _useReference = false;

        [BoxGroup("Reference Settings")]
        [ShowIf("_useReference")]
        [SerializeField] protected ScriptableVariableBase _referencedVariable;

        [BoxGroup("Reference Settings")]
        [ShowIf("_useReference")]
        [ReadOnly]
        [SerializeField] protected string _referenceType = "";

        [BoxGroup("Reference Settings")]
        [ShowIf("_useReference")]
        [ReadOnly]
        [SerializeField] protected bool _referenceValid = false;

        public bool UseReference => _useReference;
        public ScriptableVariableBase ReferencedVariable => _referencedVariable;

        /// <summary>
        /// Check if the reference is valid
        /// </summary>
        protected virtual bool IsReferenceValid()
        {
            return _useReference && _referencedVariable != null;
        }

        /// <summary>
        /// Update reference information
        /// </summary>
        protected virtual void UpdateReferenceInfo()
        {
            if (_useReference && _referencedVariable != null)
            {
                _referenceType = _referencedVariable.GetType().Name;
                _referenceValid = IsReferenceValid();
            }
            else
            {
                _referenceType = "";
                _referenceValid = false;
            }
        }

        /// <summary>
        /// Called when the object is validated in the editor
        /// </summary>
        protected virtual void OnValidate()
        {
            UpdateReferenceInfo();
        }
    }

    /// <summary>
    /// Variable reference that can point to other variables of the same type
    /// </summary>
    /// <typeparam name="T">The type of value being referenced</typeparam>
    public abstract class VariableReference<T> : VariableReferenceBase
    {
        /// <summary>
        /// Get the effective value (from reference or local)
        /// </summary>
        public virtual T EffectiveValue
        {
            get
            {
                if (_useReference && _referencedVariable is ScriptableVariable<T> typedVar)
                {
                    return typedVar.Value;
                }
                return default(T);
            }
        }

        /// <summary>
        /// Set the effective value (to reference or local)
        /// </summary>
        public virtual void SetEffectiveValue(T value)
        {
            if (_useReference && _referencedVariable is ScriptableVariable<T> typedVar)
            {
                typedVar.Value = value;
            }
        }

        protected override bool IsReferenceValid()
        {
            return base.IsReferenceValid() && _referencedVariable is ScriptableVariable<T>;
        }
    }

    /// <summary>
    /// Reference to an int variable
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/References/Int Variable Reference", fileName = "IntVariableReference")]
    public class IntVariableReference : VariableReference<int>
    {
        [Header("Local Fallback")]
        [BoxGroup("Local Value")]
        [HideIf("_useReference")]
        [SerializeField] private int _localValue;

        public override int EffectiveValue
        {
            get
            {
                if (_useReference && _referencedVariable is ScriptableVariable<int> typedVar)
                {
                    return typedVar.Value;
                }
                return _localValue;
            }
        }

        public override void SetEffectiveValue(int value)
        {
            if (_useReference && _referencedVariable is ScriptableVariable<int> typedVar)
            {
                typedVar.Value = value;
            }
            else
            {
                _localValue = value;
            }
        }

        [Button("Increment")]
        private void Increment()
        {
            SetEffectiveValue(EffectiveValue + 1);
        }

        [Button("Decrement")]
        private void Decrement()
        {
            SetEffectiveValue(EffectiveValue - 1);
        }
    }

    /// <summary>
    /// Reference to a float variable
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/References/Float Variable Reference", fileName = "FloatVariableReference")]
    public class FloatVariableReference : VariableReference<float>
    {
        [Header("Local Fallback")]
        [BoxGroup("Local Value")]
        [HideIf("_useReference")]
        [SerializeField] private float _localValue;

        public override float EffectiveValue
        {
            get
            {
                if (_useReference && _referencedVariable is ScriptableVariable<float> typedVar)
                {
                    return typedVar.Value;
                }
                return _localValue;
            }
        }

        public override void SetEffectiveValue(float value)
        {
            if (_useReference && _referencedVariable is ScriptableVariable<float> typedVar)
            {
                typedVar.Value = value;
            }
            else
            {
                _localValue = value;
            }
        }

        [Button("Double")]
        private void Double()
        {
            SetEffectiveValue(EffectiveValue * 2f);
        }

        [Button("Half")]
        private void Half()
        {
            SetEffectiveValue(EffectiveValue * 0.5f);
        }
    }

    /// <summary>
    /// Reference to a bool variable
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/References/Bool Variable Reference", fileName = "BoolVariableReference")]
    public class BoolVariableReference : VariableReference<bool>
    {
        [Header("Local Fallback")]
        [BoxGroup("Local Value")]
        [HideIf("_useReference")]
        [SerializeField] private bool _localValue;

        public override bool EffectiveValue
        {
            get
            {
                if (_useReference && _referencedVariable is ScriptableVariable<bool> typedVar)
                {
                    return typedVar.Value;
                }
                return _localValue;
            }
        }

        public override void SetEffectiveValue(bool value)
        {
            if (_useReference && _referencedVariable is ScriptableVariable<bool> typedVar)
            {
                typedVar.Value = value;
            }
            else
            {
                _localValue = value;
            }
        }

        [Button("Toggle")]
        private void Toggle()
        {
            SetEffectiveValue(!EffectiveValue);
        }
    }

    /// <summary>
    /// Reference to a string variable
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/References/String Variable Reference", fileName = "StringVariableReference")]
    public class StringVariableReference : VariableReference<string>
    {
        [Header("Local Fallback")]
        [BoxGroup("Local Value")]
        [HideIf("_useReference")]
        [SerializeField] private string _localValue = "";

        public override string EffectiveValue
        {
            get
            {
                if (_useReference && _referencedVariable is ScriptableVariable<string> typedVar)
                {
                    return typedVar.Value;
                }
                return _localValue;
            }
        }

        public override void SetEffectiveValue(string value)
        {
            if (_useReference && _referencedVariable is ScriptableVariable<string> typedVar)
            {
                typedVar.Value = value;
            }
            else
            {
                _localValue = value;
            }
        }

        [Button("Clear")]
        private void Clear()
        {
            SetEffectiveValue("");
        }

        [Button("To Upper")]
        private void ToUpper()
        {
            SetEffectiveValue(EffectiveValue?.ToUpper() ?? "");
        }

        [Button("To Lower")]
        private void ToLower()
        {
            SetEffectiveValue(EffectiveValue?.ToLower() ?? "");
        }
    }

    /// <summary>
    /// Reference to a Vector3 variable
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/References/Vector3 Variable Reference", fileName = "Vector3VariableReference")]
    public class Vector3VariableReference : VariableReference<Vector3>
    {
        [Header("Local Fallback")]
        [BoxGroup("Local Value")]
        [HideIf("_useReference")]
        [SerializeField] private Vector3 _localValue = Vector3.zero;

        public override Vector3 EffectiveValue
        {
            get
            {
                if (_useReference && _referencedVariable is ScriptableVariable<Vector3> typedVar)
                {
                    return typedVar.Value;
                }
                return _localValue;
            }
        }

        public override void SetEffectiveValue(Vector3 value)
        {
            if (_useReference && _referencedVariable is ScriptableVariable<Vector3> typedVar)
            {
                typedVar.Value = value;
            }
            else
            {
                _localValue = value;
            }
        }

        [Button("Normalize")]
        private void Normalize()
        {
            SetEffectiveValue(EffectiveValue.normalized);
        }

        [Button("Zero")]
        private void Zero()
        {
            SetEffectiveValue(Vector3.zero);
        }
    }

    /// <summary>
    /// Reference to a Color variable
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/References/Color Variable Reference", fileName = "ColorVariableReference")]
    public class ColorVariableReference : VariableReference<Color>
    {
        [Header("Local Fallback")]
        [BoxGroup("Local Value")]
        [HideIf("_useReference")]
        [SerializeField] private Color _localValue = Color.white;

        public override Color EffectiveValue
        {
            get
            {
                if (_useReference && _referencedVariable is ScriptableVariable<Color> typedVar)
                {
                    return typedVar.Value;
                }
                return _localValue;
            }
        }

        public override void SetEffectiveValue(Color value)
        {
            if (_useReference && _referencedVariable is ScriptableVariable<Color> typedVar)
            {
                typedVar.Value = value;
            }
            else
            {
                _localValue = value;
            }
        }

        [Button("Random Color")]
        private void RandomColor()
        {
            SetEffectiveValue(UnityEngine.Random.ColorHSV());
        }

        [Button("Invert")]
        private void Invert()
        {
            Color current = EffectiveValue;
            SetEffectiveValue(new Color(1f - current.r, 1f - current.g, 1f - current.b, current.a));
        }
    }

    /// <summary>
    /// Dynamic variable reference that can reference any type at runtime
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/References/Dynamic Variable Reference", fileName = "DynamicVariableReference")]
    public class DynamicVariableReference : VariableReferenceBase
    {
        [Header("Dynamic Reference")]
        [BoxGroup("Dynamic Settings")]
        [Dropdown("GetAvailableVariableTypes")]
        [SerializeField] private string _targetType = "System.Int32";

        [BoxGroup("Dynamic Settings")]
        [ReadOnly]
        [SerializeField] private object _currentValue;

        private Type _cachedType;

        /// <summary>
        /// Get the effective value as object
        /// </summary>
        public object EffectiveValue
        {
            get
            {
                if (_useReference && _referencedVariable != null)
                {
                    // Use reflection to get the value
                    var valueProperty = _referencedVariable.GetType().GetProperty("Value");
                    if (valueProperty != null)
                    {
                        return valueProperty.GetValue(_referencedVariable);
                    }
                }
                return _currentValue;
            }
        }

        /// <summary>
        /// Set the effective value
        /// </summary>
        public void SetEffectiveValue(object value)
        {
            if (_useReference && _referencedVariable != null)
            {
                // Use reflection to set the value
                var valueProperty = _referencedVariable.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    valueProperty.SetValue(_referencedVariable, value);
                }
            }
            else
            {
                _currentValue = value;
            }
        }

        /// <summary>
        /// Get the target type
        /// </summary>
        public Type TargetType
        {
            get
            {
                if (_cachedType == null && !string.IsNullOrEmpty(_targetType))
                {
                    _cachedType = Type.GetType(_targetType);
                }
                return _cachedType;
            }
        }

        protected override bool IsReferenceValid()
        {
            if (!base.IsReferenceValid()) return false;

            var targetType = TargetType;
            if (targetType == null) return false;

            // Check if the referenced variable is compatible
            var genericType = _referencedVariable.GetGenericType();
            return genericType == targetType;
        }

        /// <summary>
        /// Get available variable types for dropdown
        /// </summary>
        private string[] GetAvailableVariableTypes()
        {
            return new string[]
            {
                "System.Int32",
                "System.Single",
                "System.Boolean",
                "System.String",
                "UnityEngine.Vector2",
                "UnityEngine.Vector3",
                "UnityEngine.Color",
                "UnityEngine.Quaternion"
            };
        }

        [Button("Log Current Value")]
        private void LogValue()
        {
            Debug.Log($"Dynamic Reference Value: {EffectiveValue}");
        }
    }
}
