using System;
using UnityEngine;
using UnityEngine.UIElements;
using NaughtyAttributes;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// Base class for UI Toolkit data binding components.
    /// Provides reactive binding between ScriptableVariables and UI elements.
    /// </summary>
    public abstract class UIToolkitDataBinding : MonoBehaviour
    {
        [Header("Binding Configuration")]
        [BoxGroup("Binding Settings")]
        [Required("Data source cannot be empty")]
        [Tooltip("The ScriptableVariable to bind to this UI element.")]
        [SerializeField] protected ScriptableVariableBase _dataSource;

        [BoxGroup("Binding Settings")]
        [Tooltip("Name of the UI element to bind to (leave empty for self).")]
        [SerializeField] protected string _elementName = "";

        [BoxGroup("Binding Settings")]
        [Tooltip("Automatically bind when the component starts.")]
        [SerializeField] protected bool _bindOnStart = true;

        [BoxGroup("Binding Settings")]
        [Tooltip("Enable two-way binding (UI changes update the variable).")]
        [SerializeField] protected bool _twoWayBinding = false;

        [Header("Debug Information")]
        [BoxGroup("Debug Info")]
        [ReadOnly]
        [Tooltip("Indicates if the binding is currently active.")]
        [SerializeField] protected bool _isBound = false;

        [BoxGroup("Debug Info")]
        [ReadOnly]
        [Tooltip("Timestamp of the last update.")]
        [SerializeField] protected string _lastUpdateTime = "Never";

        protected UIDocument _document;
        protected VisualElement _boundElement;

        /// <summary>
        /// Called when the component is enabled
        /// </summary>
        protected virtual void OnEnable()
        {
            if (_bindOnStart)
            {
                Bind();
            }
        }

        /// <summary>
        /// Called when the component is disabled
        /// </summary>
        protected virtual void OnDisable()
        {
            Unbind();
        }

        /// <summary>
        /// Bind the UI element to the data source
        /// </summary>
        [Button("Bind Now")]
        public virtual void Bind()
        {
            if (_dataSource == null)
            {
                Debug.LogWarning($"[{name}] Data source is not assigned");
                return;
            }

            _document = GetComponent<UIDocument>();
            if (_document == null)
            {
                Debug.LogError($"[{name}] UIDocument component not found");
                return;
            }

            if (_document.rootVisualElement == null)
            {
                Debug.LogWarning($"[{name}] UIDocument root visual element is null");
                return;
            }

            // Find the target element
            if (string.IsNullOrEmpty(_elementName))
            {
                _boundElement = _document.rootVisualElement;
            }
            else
            {
                _boundElement = _document.rootVisualElement.Q(_elementName);
                if (_boundElement == null)
                {
                    Debug.LogError($"[{name}] Element '{_elementName}' not found in UIDocument");
                    return;
                }
            }

            // Register for data source changes
            RegisterDataSourceCallbacks();

            // Initial UI update
            UpdateUI();

            _isBound = true;
            _lastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
            OnRepaintRequest();
        }

        /// <summary>
        /// Unbind the UI element from the data source
        /// </summary>
        [Button("Unbind")]
        public virtual void Unbind()
        {
            if (_dataSource != null)
            {
                UnregisterDataSourceCallbacks();
            }

            _boundElement = null;
            _isBound = false;
            OnRepaintRequest();
        }

        /// <summary>
        /// Register callbacks for data source changes
        /// </summary>
        protected abstract void RegisterDataSourceCallbacks();

        /// <summary>
        /// Unregister callbacks for data source changes
        /// </summary>
        protected abstract void UnregisterDataSourceCallbacks();

        /// <summary>
        /// Update the UI element with current data source value
        /// </summary>
        protected abstract void UpdateUI();

        /// <summary>
        /// Handle UI element changes (for two-way binding)
        /// </summary>
        protected virtual void OnUIChanged()
        {
            if (_twoWayBinding)
            {
                UpdateDataSource();
            }
        }

        /// <summary>
        /// Update the data source with current UI element value
        /// </summary>
        protected abstract void UpdateDataSource();

        /// <summary>
        /// Force a UI update
        /// </summary>
        [Button("Update UI")]
        public void ForceUpdateUI()
        {
            if (_isBound)
            {
                UpdateUI();
                _lastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
            }
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
    /// Binds a Label to a string variable
    /// </summary>
    [AddComponentMenu("SO Framework/UI Bindings/Label Binding")]
    public class LabelBinding : UIToolkitDataBinding
    {
        [BoxGroup("Label Settings")]
        [Tooltip("Format string for displaying the value (e.g., 'Score: {0}').")]
        [SerializeField] private string _formatString = "{0}";

        [BoxGroup("Label Settings")]
        [Tooltip("Whether to localize the displayed text.")]
        [SerializeField] private bool _localizeText = false;

        private Label _label;

        protected override void RegisterDataSourceCallbacks()
        {
            if (_dataSource is ScriptableVariable<string> stringVar)
            {
                stringVar.OnValueChanged += OnStringValueChanged;
            }
            else if (_dataSource is ScriptableVariable<int> intVar)
            {
                intVar.OnValueChanged += OnIntValueChanged;
            }
            else if (_dataSource is ScriptableVariable<float> floatVar)
            {
                floatVar.OnValueChanged += OnFloatValueChanged;
            }
            else if (_dataSource is ScriptableVariable<bool> boolVar)
            {
                boolVar.OnValueChanged += OnBoolValueChanged;
            }
        }

        protected override void UnregisterDataSourceCallbacks()
        {
            if (_dataSource is ScriptableVariable<string> stringVar)
            {
                stringVar.OnValueChanged -= OnStringValueChanged;
            }
            else if (_dataSource is ScriptableVariable<int> intVar)
            {
                intVar.OnValueChanged -= OnIntValueChanged;
            }
            else if (_dataSource is ScriptableVariable<float> floatVar)
            {
                floatVar.OnValueChanged -= OnFloatValueChanged;
            }
            else if (_dataSource is ScriptableVariable<bool> boolVar)
            {
                boolVar.OnValueChanged -= OnBoolValueChanged;
            }
        }

        protected override void UpdateUI()
        {
            if (_boundElement is Label label)
            {
                _label = label;
                string displayText = GetFormattedValue();
                _label.text = _localizeText ? LocalizeText(displayText) : displayText;
            }
        }

        protected override void UpdateDataSource()
        {
            // Labels are read-only, no two-way binding
        }

        private string GetFormattedValue()
        {
            if (_dataSource is ScriptableVariable<string> stringVar)
            {
                return string.Format(_formatString, stringVar.Value);
            }
            else if (_dataSource is ScriptableVariable<int> intVar)
            {
                return string.Format(_formatString, intVar.Value);
            }
            else if (_dataSource is ScriptableVariable<float> floatVar)
            {
                return string.Format(_formatString, floatVar.Value);
            }
            else if (_dataSource is ScriptableVariable<bool> boolVar)
            {
                return string.Format(_formatString, boolVar.Value);
            }

            return "Unsupported type";
        }

        private string LocalizeText(string text)
        {
            // TODO: Integrate with localization system
            return text;
        }

        private void OnStringValueChanged(string value) => UpdateUI();
        private void OnIntValueChanged(int value) => UpdateUI();
        private void OnFloatValueChanged(float value) => UpdateUI();
        private void OnBoolValueChanged(bool value) => UpdateUI();
    }

    /// <summary>
    /// Binds a Slider to a numeric variable
    /// </summary>
    [AddComponentMenu("SO Framework/UI Bindings/Slider Binding")]
    public class SliderBinding : UIToolkitDataBinding
    {
        [BoxGroup("Slider Settings")]
        [Tooltip("Use the limits from range variables (IntRangeVariable, FloatRangeVariable).")]
        [SerializeField] private bool _useRangeVariableLimits = true;

        [BoxGroup("Slider Settings")]
        [ShowIf("_useRangeVariableLimits")]
        [ReadOnly]
        [Tooltip("Minimum value (read from range variable).")]
        [SerializeField] private float _minValue = 0f;

        [BoxGroup("Slider Settings")]
        [ShowIf("_useRangeVariableLimits")]
        [ReadOnly]
        [Tooltip("Maximum value (read from range variable).")]
        [SerializeField] private float _maxValue = 1f;

        [BoxGroup("Slider Settings")]
        [HideIf("_useRangeVariableLimits")]
        [Tooltip("Custom minimum value for the slider.")]
        [SerializeField] private float _customMinValue = 0f;

        [BoxGroup("Slider Settings")]
        [HideIf("_useRangeVariableLimits")]
        [Tooltip("Custom maximum value for the slider.")]
        [SerializeField] private float _customMaxValue = 1f;

        private Slider _slider;

        protected override void RegisterDataSourceCallbacks()
        {
            if (_dataSource is ScriptableVariable<int> intVar)
            {
                intVar.OnValueChanged += OnIntValueChanged;
            }
            else if (_dataSource is ScriptableVariable<float> floatVar)
            {
                floatVar.OnValueChanged += OnFloatValueChanged;
            }
        }

        protected override void UnregisterDataSourceCallbacks()
        {
            if (_dataSource is ScriptableVariable<int> intVar)
            {
                intVar.OnValueChanged -= OnIntValueChanged;
            }
            else if (_dataSource is ScriptableVariable<float> floatVar)
            {
                floatVar.OnValueChanged -= OnFloatValueChanged;
            }
        }

        protected override void UpdateUI()
        {
            if (_boundElement is Slider slider)
            {
                _slider = slider;

                // Set slider range
                if (_useRangeVariableLimits)
                {
                    if (_dataSource is IntRangeVariable intRange)
                    {
                        _minValue = intRange.MinValue;
                        _maxValue = intRange.MaxValue;
                        slider.lowValue = _minValue;
                        slider.highValue = _maxValue;
                        slider.value = intRange.Value;
                    }
                    else if (_dataSource is FloatRangeVariable floatRange)
                    {
                        _minValue = floatRange.MinValue;
                        _maxValue = floatRange.MaxValue;
                        slider.lowValue = _minValue;
                        slider.highValue = _maxValue;
                        slider.value = floatRange.Value;
                    }
                }
                else
                {
                    slider.lowValue = _customMinValue;
                    slider.highValue = _customMaxValue;
                }

                // Set current value
                if (_dataSource is ScriptableVariable<int> intVar)
                {
                    slider.value = intVar.Value;
                }
                else if (_dataSource is ScriptableVariable<float> floatVar)
                {
                    slider.value = floatVar.Value;
                }

                // Register for slider changes if two-way binding
                if (_twoWayBinding)
                {
                    slider.RegisterValueChangedCallback(OnSliderValueChanged);
                }
            }
        }

        protected override void UpdateDataSource()
        {
            if (_slider != null)
            {
                if (_dataSource is ScriptableVariable<int> intVar)
                {
                    intVar.Value = Mathf.RoundToInt(_slider.value);
                }
                else if (_dataSource is ScriptableVariable<float> floatVar)
                {
                    floatVar.Value = _slider.value;
                }
            }
        }

        private void OnIntValueChanged(int value) => UpdateUI();
        private void OnFloatValueChanged(float value) => UpdateUI();

        private void OnSliderValueChanged(ChangeEvent<float> evt)
        {
            OnUIChanged();
        }
    }

    /// <summary>
    /// Binds a Toggle to a boolean variable
    /// </summary>
    [AddComponentMenu("SO Framework/UI Bindings/Toggle Binding")]
    public class ToggleBinding : UIToolkitDataBinding
    {
        private Toggle _toggle;

        protected override void RegisterDataSourceCallbacks()
        {
            if (_dataSource is ScriptableVariable<bool> boolVar)
            {
                boolVar.OnValueChanged += OnBoolValueChanged;
            }
        }

        protected override void UnregisterDataSourceCallbacks()
        {
            if (_dataSource is ScriptableVariable<bool> boolVar)
            {
                boolVar.OnValueChanged -= OnBoolValueChanged;
            }
        }

        protected override void UpdateUI()
        {
            if (_boundElement is Toggle toggle)
            {
                _toggle = toggle;

                if (_dataSource is ScriptableVariable<bool> boolVar)
                {
                    toggle.value = boolVar.Value;
                }

                // Register for toggle changes if two-way binding
                if (_twoWayBinding)
                {
                    toggle.RegisterValueChangedCallback(OnToggleValueChanged);
                }
            }
        }

        protected override void UpdateDataSource()
        {
            if (_toggle != null && _dataSource is ScriptableVariable<bool> boolVar)
            {
                boolVar.Value = _toggle.value;
            }
        }

        private void OnBoolValueChanged(bool value) => UpdateUI();

        private void OnToggleValueChanged(ChangeEvent<bool> evt)
        {
            OnUIChanged();
        }
    }

    /// <summary>
    /// Binds an Image fill amount to a numeric variable
    /// </summary>
    [AddComponentMenu("SO Framework/UI Bindings/Image Fill Binding")]
    public class ImageFillBinding : UIToolkitDataBinding
    {
        [BoxGroup("Image Settings")]
        [Tooltip("Method to use for filling the image.")]
        [SerializeField] private FillMethod _fillMethod = FillMethod.Horizontal;

        public enum FillMethod
        {
            Horizontal,
            Vertical,
            Radial
        }

        private UnityEngine.UIElements.Image _image;

        protected override void RegisterDataSourceCallbacks()
        {
            if (_dataSource is ScriptableVariable<float> floatVar)
            {
                floatVar.OnValueChanged += OnFloatValueChanged;
            }
            else if (_dataSource is ScriptableVariable<int> intVar)
            {
                intVar.OnValueChanged += OnIntValueChanged;
            }
        }

        protected override void UnregisterDataSourceCallbacks()
        {
            if (_dataSource is ScriptableVariable<float> floatVar)
            {
                floatVar.OnValueChanged -= OnFloatValueChanged;
            }
            else if (_dataSource is ScriptableVariable<int> intVar)
            {
                intVar.OnValueChanged -= OnIntValueChanged;
            }
        }

        protected override void UpdateUI()
        {
            if (_boundElement is UnityEngine.UIElements.Image image)
            {
                _image = image;
                UpdateFillAmount();
            }
        }

        protected override void UpdateDataSource()
        {
            // Images are read-only for fill amount
        }

        private void UpdateFillAmount()
        {
            if (_image == null) return;

            float fillAmount = 0f;

            if (_dataSource is ScriptableVariable<float> floatVar)
            {
                fillAmount = Mathf.Clamp01(floatVar.Value);
            }
            else if (_dataSource is ScriptableVariable<int> intVar)
            {
                // Assume int represents percentage (0-100)
                fillAmount = Mathf.Clamp01(intVar.Value / 100f);
            }

            // Apply fill based on method
            switch (_fillMethod)
            {
                case FillMethod.Horizontal:
                    // For horizontal fill, we might need to use a custom shader or manipulate UVs
                    // For now, we'll use opacity as a simple representation
                    _image.style.opacity = fillAmount;
                    break;

                case FillMethod.Vertical:
                    _image.style.opacity = fillAmount;
                    break;

                case FillMethod.Radial:
                    _image.style.opacity = fillAmount;
                    break;
            }
        }

        private void OnFloatValueChanged(float value) => UpdateFillAmount();
        private void OnIntValueChanged(int value) => UpdateFillAmount();
    }

    /// <summary>
    /// Binds a TextField to a string variable with two-way binding
    /// </summary>
    [AddComponentMenu("SO Framework/UI Bindings/TextField Binding")]
    public class TextFieldBinding : UIToolkitDataBinding
    {
        [BoxGroup("TextField Settings")]
        [Tooltip("Allow multiline input.")]
        [SerializeField] private bool _multiline = false;

        [BoxGroup("TextField Settings")]
        [Tooltip("Maximum number of characters allowed.")]
        [SerializeField] private int _maxLength = 100;

        private TextField _textField;

        protected override void RegisterDataSourceCallbacks()
        {
            if (_dataSource is ScriptableVariable<string> stringVar)
            {
                stringVar.OnValueChanged += OnStringValueChanged;
            }
        }

        protected override void UnregisterDataSourceCallbacks()
        {
            if (_dataSource is ScriptableVariable<string> stringVar)
            {
                stringVar.OnValueChanged -= OnStringValueChanged;
            }
        }

        protected override void UpdateUI()
        {
            if (_boundElement is TextField textField)
            {
                _textField = textField;

                if (_dataSource is ScriptableVariable<string> stringVar)
                {
                    textField.value = stringVar.Value ?? "";
                    textField.maxLength = _maxLength;
                    textField.multiline = _multiline;
                }

                // Register for text field changes if two-way binding
                if (_twoWayBinding)
                {
                    textField.RegisterValueChangedCallback(OnTextFieldValueChanged);
                }
            }
        }

        protected override void UpdateDataSource()
        {
            if (_textField != null && _dataSource is ScriptableVariable<string> stringVar)
            {
                stringVar.Value = _textField.value;
            }
        }

        private void OnStringValueChanged(string value) => UpdateUI();

        private void OnTextFieldValueChanged(ChangeEvent<string> evt)
        {
            OnUIChanged();
        }
    }
}
