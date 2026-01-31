using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace KalponicStudio.Health.UI
{
    /// <summary>
    /// Modular health bar component with customizable appearance and animations.
    /// Uses coroutine-based animations for smooth health transitions.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;

        [Header("Appearance")]
        [SerializeField] private Gradient fillGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.red, 0f),
                new GradientColorKey(Color.yellow, 0.5f),
                new GradientColorKey(Color.green, 1f)
            }
        };

        [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Sprite customFillSprite;
        [SerializeField] private Sprite customBackgroundSprite;

        [Header("Animation")]
        [SerializeField] private bool animateChanges = true;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Settings")]
        [SerializeField] private bool showBackground = true;
        [SerializeField] private bool invertFillDirection = false;

        private float currentDisplayedHealth = 1f;
        private Coroutine animationCoroutine;

        private void Awake()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (healthSlider == null)
                healthSlider = GetComponent<Slider>();

            if (healthSlider == null)
            {
                Debug.LogError("HealthBar: No Slider component found!");
                return;
            }

            // Setup slider
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.wholeNumbers = false;
            healthSlider.interactable = false;

            // Get fill image
            if (fillImage == null && healthSlider.fillRect != null)
            {
                fillImage = healthSlider.fillRect.GetComponent<Image>();
            }

            // Get background image
            if (backgroundImage == null && healthSlider.transform.Find("Background") != null)
            {
                backgroundImage = healthSlider.transform.Find("Background").GetComponent<Image>();
            }

            // Apply custom sprites
            if (customFillSprite != null && fillImage != null)
                fillImage.sprite = customFillSprite;

            if (customBackgroundSprite != null && backgroundImage != null)
                backgroundImage.sprite = customBackgroundSprite;

            // Apply colors
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
                backgroundImage.enabled = showBackground;
            }

            // Set initial state
            UpdateVisuals(1f);
        }

        public void UpdateHealth(int currentHealth, int maxHealth)
        {
            if (maxHealth <= 0) return;

            float healthPercent = (float)currentHealth / maxHealth;

            if (animateChanges)
            {
                AnimateToHealth(healthPercent);
            }
            else
            {
                UpdateVisuals(healthPercent);
            }
        }

        private void AnimateToHealth(float targetHealth)
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            animationCoroutine = StartCoroutine(AnimateHealthBar(targetHealth));
        }

        private IEnumerator AnimateHealthBar(float targetHealth)
        {
            float startHealth = currentDisplayedHealth;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsed / animationDuration);
                float currentHealth = Mathf.Lerp(startHealth, targetHealth, t);

                UpdateVisuals(currentHealth);
                yield return null;
            }

            UpdateVisuals(targetHealth);
            animationCoroutine = null;
        }

        private void UpdateVisuals(float healthPercent)
        {
            currentDisplayedHealth = healthPercent;

            if (healthSlider != null)
            {
                healthSlider.value = invertFillDirection ? (1f - healthPercent) : healthPercent;
            }

            if (fillImage != null)
            {
                fillImage.color = fillGradient.Evaluate(healthPercent);
            }
        }

        // Public API for customization
        public void SetFillGradient(Gradient gradient)
        {
            fillGradient = gradient;
            UpdateVisuals(currentDisplayedHealth);
        }

        public void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            if (backgroundImage != null)
                backgroundImage.color = color;
        }

        public void SetCustomSprites(Sprite fill, Sprite background)
        {
            customFillSprite = fill;
            customBackgroundSprite = background;

            if (fill != null && fillImage != null)
                fillImage.sprite = fill;

            if (background != null && backgroundImage != null)
                backgroundImage.sprite = background;
        }

        public void SetAnimationSettings(bool animate, float duration, AnimationCurve curve)
        {
            animateChanges = animate;
            animationDuration = duration;
            animationCurve = curve;
        }
    }
}
