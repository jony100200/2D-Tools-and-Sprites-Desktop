using UnityEngine;
using TMPro;
using System.Text;
using System.Collections;


namespace KalponicStudio.Health.UI
{
    /// <summary>
    /// Flexible health text display component with customizable formatting.
    /// </summary>
    public class HealthText : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI maxHealthText;

        [Header("Display Format")]
        [SerializeField] private HealthTextFormat format = HealthTextFormat.CurrentAndMax;
        [SerializeField] private string separator = " / ";
        [SerializeField] private bool showPercentage = false;
        [SerializeField] private string percentageFormat = " ({0:0}%)";

        [Header("Custom Formatting")]
        [SerializeField] private string customFormat = "{CURRENT}/{MAX}";
        [SerializeField] private bool useCustomFormat = false;

        [Header("Colors")]
        [SerializeField] private bool useHealthBasedColors = true;
        [SerializeField] private Gradient healthColorGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.red, 0f),
                new GradientColorKey(Color.yellow, 0.3f),
                new GradientColorKey(Color.green, 1f)
            }
        };
        [SerializeField] private Color defaultColor = Color.white;

        [Header("Animation")]
        [SerializeField] private bool animateValueChanges = true;
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private int currentDisplayedHealth = 0;
        private int currentDisplayedMaxHealth = 100;
        private Coroutine animationCoroutine;

        public enum HealthTextFormat
        {
            CurrentOnly,
            MaxOnly,
            CurrentAndMax,
            PercentageOnly,
            CurrentAndPercentage
        }

        private void Awake()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (healthText == null)
                healthText = GetComponent<TextMeshProUGUI>();

            if (healthText == null)
            {
                Debug.LogError("HealthText: No TextMeshProUGUI component found!");
                return;
            }

            // Set initial state
            UpdateText(0, 100);
        }

        public void UpdateHealth(int currentHealth, int maxHealth)
        {
            if (animateValueChanges && currentHealth != currentDisplayedHealth)
            {
                AnimateToHealth(currentHealth, maxHealth);
            }
            else
            {
                UpdateText(currentHealth, maxHealth);
            }
        }

        private void AnimateToHealth(int targetCurrent, int targetMax)
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            animationCoroutine = StartCoroutine(AnimateHealthText(targetCurrent, targetMax));
        }

        private System.Collections.IEnumerator AnimateHealthText(int targetCurrent, int targetMax)
        {
            int startCurrent = currentDisplayedHealth;
            int startMax = currentDisplayedMaxHealth;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsed / animationDuration);

                int currentHealth = Mathf.RoundToInt(Mathf.Lerp(startCurrent, targetCurrent, t));
                int maxHealth = Mathf.RoundToInt(Mathf.Lerp(startMax, targetMax, t));

                UpdateText(currentHealth, maxHealth);
                yield return null;
            }

            UpdateText(targetCurrent, targetMax);
            animationCoroutine = null;
        }

        private void UpdateText(int currentHealth, int maxHealth)
        {
            currentDisplayedHealth = currentHealth;
            currentDisplayedMaxHealth = maxHealth;

            string text = FormatHealthText(currentHealth, maxHealth);

            if (healthText != null)
            {
                healthText.text = text;

                if (useHealthBasedColors)
                {
                    float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
                    healthText.color = healthColorGradient.Evaluate(healthPercent);
                }
                else
                {
                    healthText.color = defaultColor;
                }
            }

            if (maxHealthText != null && format == HealthTextFormat.MaxOnly)
            {
                maxHealthText.text = maxHealth.ToString();
            }
        }

        private string FormatHealthText(int current, int max)
        {
            if (useCustomFormat)
            {
                return customFormat
                    .Replace("{CURRENT}", current.ToString())
                    .Replace("{MAX}", max.ToString())
                    .Replace("{PERCENTAGE}", Mathf.RoundToInt((float)current / max * 100).ToString());
            }

            StringBuilder sb = new StringBuilder();

            switch (format)
            {
                case HealthTextFormat.CurrentOnly:
                    sb.Append(current);
                    break;

                case HealthTextFormat.MaxOnly:
                    sb.Append(max);
                    break;

                case HealthTextFormat.CurrentAndMax:
                    sb.Append(current);
                    sb.Append(separator);
                    sb.Append(max);
                    break;

                case HealthTextFormat.PercentageOnly:
                    sb.Append(Mathf.RoundToInt((float)current / max * 100));
                    sb.Append("%");
                    break;

                case HealthTextFormat.CurrentAndPercentage:
                    sb.Append(current);
                    if (showPercentage)
                    {
                        sb.Append(string.Format(percentageFormat, (float)current / max * 100));
                    }
                    break;
            }

            return sb.ToString();
        }

        // Public API for customization
        public void SetFormat(HealthTextFormat newFormat, string customSep = null)
        {
            format = newFormat;
            if (customSep != null) separator = customSep;
            UpdateText(currentDisplayedHealth, currentDisplayedMaxHealth);
        }

        public void SetCustomFormat(string format)
        {
            customFormat = format;
            useCustomFormat = true;
            UpdateText(currentDisplayedHealth, currentDisplayedMaxHealth);
        }

        public void SetColors(Gradient gradient, Color defaultCol)
        {
            healthColorGradient = gradient;
            defaultColor = defaultCol;
            UpdateText(currentDisplayedHealth, currentDisplayedMaxHealth);
        }

        public void SetAnimationSettings(bool animate, float duration, AnimationCurve curve)
        {
            animateValueChanges = animate;
            animationDuration = duration;
            animationCurve = curve;
        }
    }
}
