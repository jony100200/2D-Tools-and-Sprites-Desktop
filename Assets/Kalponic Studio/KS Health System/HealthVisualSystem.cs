using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using KalponicStudio.Health;

namespace KalponicStudio.Health
{
    /// <summary>
    /// Handles visual effects for health-related events
    /// Works with HealthSystem to provide feedback for damage, healing, etc.
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class HealthVisualSystem : MonoBehaviour, IHealthEventHandler
    {
        [Header("Event Channels")]
        [SerializeField] private HealthEventChannelSO healthEvents;

        [Header("Screen Effects")]
        [SerializeField] private Image damageFlashImage;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private float damageFlashDuration = 0.2f;

        [SerializeField] private Image healFlashImage;
        [SerializeField] private Color healFlashColor = new Color(0f, 1f, 0f, 0.2f);
        [SerializeField] private float healFlashDuration = 0.3f;

        [Header("Particle Effects")]
        [SerializeField] private ParticleSystem damageParticles;
        [SerializeField] private ParticleSystem healParticles;
        [SerializeField] private ParticleSystem deathParticles;

        [Header("Sprite Effects")]
        [SerializeField] private SpriteRenderer mainRenderer;
        [SerializeField] private Color damageTintColor = Color.red;
        [SerializeField] private float damageTintDuration = 0.15f;
        [SerializeField] private Color healTintColor = Color.green;
        [SerializeField] private float healTintDuration = 0.2f;

        [Header("Camera Effects")]
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private float damageShakeIntensity = 0.3f;
        [SerializeField] private float damageShakeDuration = 0.2f;
        [SerializeField] private float deathShakeIntensity = 0.5f;
        [SerializeField] private float deathShakeDuration = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip damageSound;
        [SerializeField] private AudioClip healSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip lowHealthSound;

        [Header("Low Health Warning")]
        [SerializeField] private float lowHealthThreshold = 0.25f; // 25% health
        [SerializeField] private Image lowHealthOverlay;
        [SerializeField] private Color lowHealthColor = new Color(1f, 0.5f, 0f, 0.3f);
        [SerializeField] private float lowHealthPulseSpeed = 2f;

        // Private fields
        private HealthSystem healthSystem;
        private Color originalSpriteColor;
        private bool isLowHealth = false;
        private bool isAlive = true;

        private void Awake()
        {
            healthSystem = GetComponent<HealthSystem>();

            // Cache original colors
            if (mainRenderer != null)
            {
                originalSpriteColor = mainRenderer.color;
            }

            // Setup screen overlays
            if (damageFlashImage != null)
            {
                damageFlashImage.gameObject.SetActive(false);
            }

            if (healFlashImage != null)
            {
                healFlashImage.gameObject.SetActive(false);
            }

            if (lowHealthOverlay != null)
            {
                lowHealthOverlay.gameObject.SetActive(false);
            }

            if (healthSystem != null)
            {
                OnHealthChanged(healthSystem.CurrentHealth, healthSystem.MaxHealth);
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            UpdateLowHealthEffect();
        }

        public void OnDamageTaken(int damage)
        {
            // Screen flash
            if (damageFlashImage != null)
            {
                StartCoroutine(FlashScreen(damageFlashImage, damageFlashColor, damageFlashDuration));
            }

            // Sprite tint
            if (mainRenderer != null)
            {
                StartCoroutine(TintSprite(damageTintColor, damageTintDuration));
            }

            // Particles
            if (damageParticles != null)
            {
                damageParticles.Play();
            }

            // Camera shake
            if (cameraShake != null)
            {
                cameraShake.Shake(damageShakeIntensity, damageShakeDuration);
            }

            // Audio
            if (audioSource != null && damageSound != null)
            {
                audioSource.PlayOneShot(damageSound);
            }
        }

        public void OnHealed(int healAmount)
        {
            // Screen flash
            if (healFlashImage != null)
            {
                StartCoroutine(FlashScreen(healFlashImage, healFlashColor, healFlashDuration));
            }

            // Sprite tint
            if (mainRenderer != null)
            {
                StartCoroutine(TintSprite(healTintColor, healTintDuration));
            }

            // Particles
            if (healParticles != null)
            {
                healParticles.Play();
            }

            // Audio
            if (audioSource != null && healSound != null)
            {
                audioSource.PlayOneShot(healSound);
            }
        }

        public void OnDeath()
        {
            // Death particles
            if (deathParticles != null)
            {
                deathParticles.Play();
            }

            // Camera shake
            if (cameraShake != null)
            {
                cameraShake.Shake(deathShakeIntensity, deathShakeDuration);
            }

            // Audio
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }

            // Disable low health effect
            if (lowHealthOverlay != null)
            {
                lowHealthOverlay.gameObject.SetActive(false);
            }

            isLowHealth = false;
            isAlive = false;
        }

        private void UpdateLowHealthEffect()
        {
            if (lowHealthOverlay == null) return;

            if (isLowHealth && isAlive)
            {
                lowHealthOverlay.gameObject.SetActive(true);

                // Pulse effect
                float alpha = (Mathf.Sin(Time.time * lowHealthPulseSpeed) + 1f) * 0.5f;
                Color pulseColor = lowHealthColor;
                pulseColor.a = lowHealthColor.a * alpha;
                lowHealthOverlay.color = pulseColor;
            }
            else
            {
                lowHealthOverlay.gameObject.SetActive(false);
            }
        }

        private IEnumerator FlashScreen(Image flashImage, Color flashColor, float duration)
        {
            flashImage.gameObject.SetActive(true);
            flashImage.color = flashColor;

            yield return new WaitForSeconds(duration);

            flashImage.gameObject.SetActive(false);
        }

        private IEnumerator TintSprite(Color tintColor, float duration)
        {
            if (mainRenderer == null) yield break;

            mainRenderer.color = tintColor;
            yield return new WaitForSeconds(duration);
            mainRenderer.color = originalSpriteColor;
        }

        // Public methods for external control
        public void PlayCustomEffect(string effectType)
        {
            switch (effectType.ToLower())
            {
                case "damage":
                    OnDamageTaken(0);
                    break;
                case "heal":
                    OnHealed(0);
                    break;
                case "death":
                    OnDeath();
                    break;
            }
        }

        public void SetDamageFlashColor(Color color)
        {
            damageFlashColor = color;
        }

        public void SetHealFlashColor(Color color)
        {
            healFlashColor = color;
        }

        public void SetLowHealthThreshold(float threshold)
        {
            lowHealthThreshold = Mathf.Clamp01(threshold);
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            isAlive = currentHealth > 0;
            if (maxHealth <= 0)
            {
                isLowHealth = false;
                return;
            }

            float percent = (float)currentHealth / maxHealth;
            isLowHealth = percent <= lowHealthThreshold;
        }

        private void SubscribeToEvents()
        {
            if (healthEvents != null)
            {
                healthEvents.DamageTaken += OnDamageTaken;
                healthEvents.Healed += OnHealed;
                healthEvents.Death += OnDeath;
                healthEvents.HealthChanged += OnHealthChanged;
                return;
            }

            if (healthSystem != null)
            {
                healthSystem.DamageTaken += OnDamageTaken;
                healthSystem.Healed += OnHealed;
                healthSystem.Death += OnDeath;
                healthSystem.HealthChanged += OnHealthChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (healthEvents != null)
            {
                healthEvents.DamageTaken -= OnDamageTaken;
                healthEvents.Healed -= OnHealed;
                healthEvents.Death -= OnDeath;
                healthEvents.HealthChanged -= OnHealthChanged;
                return;
            }

            if (healthSystem != null)
            {
                healthSystem.DamageTaken -= OnDamageTaken;
                healthSystem.Healed -= OnHealed;
                healthSystem.Death -= OnDeath;
                healthSystem.HealthChanged -= OnHealthChanged;
            }
        }
    }

    /// <summary>
    /// Simple camera shake component for health visual effects
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        private Vector3 originalPosition;
        private float shakeTimeRemaining = 0f;
        private float shakeIntensity = 0f;

        private void Update()
        {
            if (shakeTimeRemaining > 0)
            {
                transform.localPosition = originalPosition + Random.insideUnitSphere * shakeIntensity;
                shakeTimeRemaining -= Time.deltaTime;
            }
            else
            {
                transform.localPosition = originalPosition;
            }
        }

        public void Shake(float intensity, float duration)
        {
            if (shakeTimeRemaining <= 0)
            {
                originalPosition = transform.localPosition;
            }

            shakeIntensity = intensity;
            shakeTimeRemaining = duration;
        }
    }
}
