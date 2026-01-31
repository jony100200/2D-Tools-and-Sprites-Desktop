using System;
using UnityEngine;
using UnityEngine.Events;

namespace KalponicStudio.Health
{
    /// <summary>
    /// Reusable health system component for 2D and 3D games
    /// Handles health management, damage, healing, and death events
    /// Can be attached to any GameObject that needs health functionality
    /// Now uses event channels for better decoupling and modularity
    /// </summary>
    public class HealthSystem : MonoBehaviour, IHealthComponent
    {
        [Header("Event Channels")]
        [SerializeField] private HealthEventChannelSO healthEvents;
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private bool regenerateHealth = false;
        [SerializeField] private float regenerationRate = 1f; // Health per second
        [SerializeField] private float regenerationDelay = 2f; // Delay before regeneration starts

        [Header("Invulnerability")]
        [SerializeField] private bool enableInvulnerability = true;
        [SerializeField] private float invulnerabilityDuration = 1f;
        [SerializeField] private bool flashDuringInvulnerability = true;
        [SerializeField] private float flashInterval = 0.1f;

        [Header("Events")]
        public UnityEvent<int, int> onHealthChanged = new UnityEvent<int, int>(); // (currentHealth, maxHealth)
        public UnityEvent<int> onDamageTaken = new UnityEvent<int>(); // damage amount
        public UnityEvent<int> onHealed = new UnityEvent<int>(); // heal amount
        public UnityEvent onDeath = new UnityEvent();
        public UnityEvent onInvulnerabilityStart = new UnityEvent();
        public UnityEvent onInvulnerabilityEnd = new UnityEvent();

        // Public properties
        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        public bool IsAlive => currentHealth > 0;
        public bool IsInvulnerable => invulnerabilityTimer > 0;

        // Private fields
        private float regenerationTimer = 0f;
        private float invulnerabilityTimer = 0f;
        private float lastDamageTime = 0f;
        private SpriteRenderer spriteRenderer;
        private bool originalSpriteEnabled = true;

        private void Awake()
        {
            // Cache components
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Ensure current health doesn't exceed max
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // Trigger initial health changed event
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Update()
        {
            HandleInvulnerability();
            HandleRegeneration();
        }

        private void HandleInvulnerability()
        {
            if (invulnerabilityTimer > 0)
            {
                invulnerabilityTimer -= Time.deltaTime;

                // Handle flashing effect
                if (flashDuringInvulnerability && spriteRenderer != null)
                {
                    float flashTime = Time.time - (lastDamageTime + invulnerabilityDuration - invulnerabilityTimer);
                    bool shouldShow = Mathf.FloorToInt(flashTime / flashInterval) % 2 == 0;
                    spriteRenderer.enabled = shouldShow;
                }

                // Check if invulnerability ended
                if (invulnerabilityTimer <= 0)
                {
                    invulnerabilityTimer = 0;
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = originalSpriteEnabled;
                    }
                    onInvulnerabilityEnd?.Invoke();
                }
            }
        }

        private void HandleRegeneration()
        {
            if (!regenerateHealth || !IsAlive || IsInvulnerable) return;

            // Check if enough time has passed since last damage
            if (Time.time - lastDamageTime < regenerationDelay) return;

            // Regenerate health
            if (currentHealth < maxHealth)
            {
                int oldHealth = currentHealth;
                currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.RoundToInt(regenerationRate * Time.deltaTime));

                if (currentHealth != oldHealth)
                {
                    onHealthChanged?.Invoke(currentHealth, maxHealth);
                }
            }
        }

        /// <summary>
        /// Take damage from any source
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (!IsAlive || (IsInvulnerable && damage > 0)) return;

            int oldHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - damage);

            // Trigger events
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            if (damage > 0)
            {
                if (healthEvents != null)
                {
                    healthEvents.RaiseDamageTaken(damage);
                }
                else
                {
                    onDamageTaken?.Invoke(damage);
                }
            }

            // Check for death
            if (currentHealth <= 0 && oldHealth > 0)
            {
                onDeath?.Invoke();
            }
        }

        /// <summary>
        /// Heal the entity
        /// </summary>
        public void Heal(int healAmount)
        {
            if (!IsAlive || healAmount <= 0) return;

            int oldHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);

            if (currentHealth != oldHealth)
            {
                onHealthChanged?.Invoke(currentHealth, maxHealth);
                if (healthEvents != null)
                {
                    healthEvents.RaiseHealed(healAmount);
                }
                else
                {
                    onHealed?.Invoke(healAmount);
                }
            }
        }

        /// <summary>
        /// Set health to specific value
        /// </summary>
        public void SetHealth(int newHealth)
        {
            int oldHealth = currentHealth;
            currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

            if (currentHealth != oldHealth)
            {
                onHealthChanged?.Invoke(currentHealth, maxHealth);

                // Check for death
                if (currentHealth <= 0 && oldHealth > 0)
                {
                    if (healthEvents != null)
                    {
                        healthEvents.RaiseDeath();
                    }
                    else
                    {
                        onDeath?.Invoke();
                    }
                }
            }
        }

        public void SetMaxHealth(int newMaxHealth)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Fully restore health
        /// </summary>
        public void RestoreFullHealth()
        {
            SetHealth(maxHealth);
        }

        /// <summary>
        /// Kill the entity instantly
        /// </summary>
        public void Kill()
        {
            TakeDamage(currentHealth);
        }

        public void StartInvulnerability()
        {
            if (!enableInvulnerability) return;

            invulnerabilityTimer = invulnerabilityDuration;
            onInvulnerabilityStart?.Invoke();

            // Store original sprite state
            if (spriteRenderer != null)
            {
                originalSpriteEnabled = spriteRenderer.enabled;
            }
        }

        public void EndInvulnerability()
        {
            invulnerabilityTimer = 0;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = originalSpriteEnabled;
            }
            onInvulnerabilityEnd?.Invoke();
        }

        /// <summary>
        /// Check if entity can take damage
        /// </summary>
        public bool CanTakeDamage()
        {
            return IsAlive && !IsInvulnerable;
        }

        // Unity event helpers for easy inspector setup
        public void TakeDamageFromInt(int damage) => TakeDamage(damage);
        public void HealFromInt(int healAmount) => Heal(healAmount);
        public void SetHealthFromInt(int health) => SetHealth(health);
        public void SetMaxHealthFromInt(int maxHealth) => SetMaxHealth(maxHealth);
    }
}
