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
        [SerializeField] private float flatDamageReduction = 0f;
        [SerializeField, Range(0f, 1f)] private float percentDamageReduction = 0f;
        [SerializeField] private DamageResistance[] damageResistances = new DamageResistance[0];

        [Header("Downed State")]
        [SerializeField] private bool enableDownedState = false;
        [SerializeField] private float downedDuration = 5f;
        [SerializeField] private bool allowRevive = true;

        [Header("Invulnerability")]
        [SerializeField] private bool enableInvulnerability = true;
        [SerializeField] private float invulnerabilityDuration = 1f;
        [SerializeField] private bool flashDuringInvulnerability = true;
        [SerializeField] private float flashInterval = 0.1f;

        [Header("Events")]
        [SerializeField] private UnityEvent<int, int> onHealthChanged = new UnityEvent<int, int>(); // (currentHealth, maxHealth)
        [SerializeField] private UnityEvent<int> onDamageTaken = new UnityEvent<int>(); // damage amount
        [SerializeField] private UnityEvent<int> onHealed = new UnityEvent<int>(); // heal amount
        [SerializeField] private UnityEvent onDeath = new UnityEvent();
        [SerializeField] private UnityEvent onDowned = new UnityEvent();
        [SerializeField] private UnityEvent onRevived = new UnityEvent();
        [SerializeField] private UnityEvent onDownedExpired = new UnityEvent();
        [SerializeField] private UnityEvent onInvulnerabilityStart = new UnityEvent();
        [SerializeField] private UnityEvent onInvulnerabilityEnd = new UnityEvent();

        public event Action<int, int> HealthChanged;
        public event Action<int> DamageTaken;
        public event Action<int> Healed;
        public event Action Death;
        public event Action Downed;
        public event Action Revived;
        public event Action DownedExpired;
        public event Action InvulnerabilityStarted;
        public event Action InvulnerabilityEnded;

        // Public properties
        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        public bool IsAlive => currentHealth > 0 && !isDead;
        public bool IsInvulnerable => invulnerabilityTimer > 0;
        public bool IsDowned => isDowned;
        public bool IsDead => isDead;
        public float DownedTimeRemaining => downedTimer;

        // Private fields
        private float regenerationTimer = 0f;
        private float invulnerabilityTimer = 0f;
        private float lastDamageTime = 0f;
        private SpriteRenderer spriteRenderer;
        private bool originalSpriteEnabled = true;
        private IShieldAbsorber shieldAbsorber;
        private bool isDowned = false;
        private bool isDead = false;
        private float downedTimer = 0f;

        private void Awake()
        {
            // Cache components
            spriteRenderer = GetComponent<SpriteRenderer>();
            shieldAbsorber = GetComponent<IShieldAbsorber>();

            // Ensure current health doesn't exceed max
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            isDowned = false;
            isDead = false;

            // Trigger initial health changed event
            RaiseHealthChanged();
        }

        private void Update()
        {
            HandleInvulnerability();
            HandleRegeneration();
            HandleDownedState();
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
            if (!regenerateHealth || !IsAlive || IsInvulnerable || isDowned) return;

            // Check if enough time has passed since last damage
            if (Time.time - lastDamageTime < regenerationDelay) return;

            // Regenerate health
            if (currentHealth < maxHealth)
            {
                int oldHealth = currentHealth;
                currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.RoundToInt(regenerationRate * Time.deltaTime));

                if (currentHealth != oldHealth)
                {
                    RaiseHealthChanged();
                }
            }
        }

        private void HandleDownedState()
        {
            if (!isDowned) return;

            if (downedTimer > 0f)
            {
                downedTimer -= Time.deltaTime;
                if (downedTimer <= 0f)
                {
                    downedTimer = 0f;
                    RaiseDownedExpired();
                    Die();
                }
            }
        }

        /// <summary>
        /// Take damage from any source
        /// </summary>
        public void TakeDamage(int damage)
        {
            TakeDamage(DamageInfo.FromAmount(damage));
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (isDead) return;
            if (damageInfo.Amount <= 0) return;
            if (IsInvulnerable && !damageInfo.IgnoreMitigation) return;
            if (isDowned)
            {
                Die();
                return;
            }

            int remainingDamage = damageInfo.Amount;
            if (shieldAbsorber == null)
            {
                shieldAbsorber = GetComponent<IShieldAbsorber>();
            }

            if (shieldAbsorber != null && !damageInfo.BypassShield && damageInfo.Type != DamageType.True)
            {
                remainingDamage = shieldAbsorber.AbsorbDamage(remainingDamage);
            }

            if (remainingDamage <= 0) return;

            if (!damageInfo.IgnoreMitigation && damageInfo.Type != DamageType.True)
            {
                remainingDamage = ApplyMitigation(remainingDamage, damageInfo.Type);
            }

            if (remainingDamage <= 0) return;

            int oldHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - remainingDamage);
            lastDamageTime = Time.time;

            // Trigger events
            RaiseHealthChanged();
            RaiseDamageTaken(remainingDamage);

            if (enableInvulnerability)
            {
                StartInvulnerability();
            }

            // Check for death
            if (currentHealth <= 0 && oldHealth > 0)
            {
                if (enableDownedState && allowRevive)
                {
                    EnterDownedState();
                }
                else
                {
                    Die();
                }
            }
        }

        /// <summary>
        /// Heal the entity
        /// </summary>
        public void Heal(int healAmount)
        {
            if (isDead || healAmount <= 0) return;

            int oldHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);

            if (currentHealth != oldHealth)
            {
                if (isDowned)
                {
                    Revive(currentHealth);
                    return;
                }

                RaiseHealthChanged();
                RaiseHealed(healAmount);
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
                if (isDowned && currentHealth > 0)
                {
                    Revive(currentHealth);
                    return;
                }

                RaiseHealthChanged();

                // Check for death
                if (currentHealth <= 0 && oldHealth > 0)
                {
                    if (enableDownedState && allowRevive)
                    {
                        EnterDownedState();
                    }
                    else
                    {
                        Die();
                    }
                }
            }
        }

        public void SetMaxHealth(int newMaxHealth)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            RaiseHealthChanged();
        }

        public void ConfigureRegeneration(bool enabled, float rate, float delay)
        {
            regenerateHealth = enabled;
            regenerationRate = Mathf.Max(0f, rate);
            regenerationDelay = Mathf.Max(0f, delay);
        }

        public void ConfigureMitigation(float flatReduction, float percentReduction, DamageResistance[] resistances)
        {
            flatDamageReduction = Mathf.Max(0f, flatReduction);
            percentDamageReduction = Mathf.Clamp01(percentReduction);
            damageResistances = resistances ?? new DamageResistance[0];
        }

        public void ConfigureDownedState(bool enabled, float duration, bool canRevive)
        {
            enableDownedState = enabled;
            downedDuration = Mathf.Max(0.1f, duration);
            allowRevive = canRevive;
        }

        public void ConfigureInvulnerability(bool enabled, float duration, bool flashDuring, float interval)
        {
            enableInvulnerability = enabled;
            invulnerabilityDuration = Mathf.Max(0f, duration);
            flashDuringInvulnerability = flashDuring;
            flashInterval = Mathf.Max(0.01f, interval);
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
            if (isDead) return;
            currentHealth = 0;
            RaiseHealthChanged();
            Die();
        }

        public void Revive(int healthAmount)
        {
            if (!isDowned || !allowRevive) return;

            isDowned = false;
            downedTimer = 0f;
            currentHealth = Mathf.Clamp(healthAmount, 1, maxHealth);
            RaiseHealthChanged();
            RaiseRevived();
        }

        public void ForceDowned(float timeRemaining)
        {
            if (isDead) return;

            isDowned = true;
            downedTimer = Mathf.Max(0.1f, timeRemaining);
            currentHealth = 0;
            RaiseHealthChanged();
            RaiseDowned();
        }

        public void ForceDead()
        {
            if (isDead) return;

            currentHealth = 0;
            isDowned = false;
            downedTimer = 0f;
            Die();
        }

        public void StartInvulnerability()
        {
            if (!enableInvulnerability) return;

            invulnerabilityTimer = invulnerabilityDuration;
            onInvulnerabilityStart?.Invoke();
            InvulnerabilityStarted?.Invoke();

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
            InvulnerabilityEnded?.Invoke();
        }

        /// <summary>
        /// Check if entity can take damage
        /// </summary>
        public bool CanTakeDamage()
        {
            return IsAlive && !IsInvulnerable && !isDowned;
        }

        // Unity event helpers for easy inspector setup
        public void TakeDamageFromInt(int damage) => TakeDamage(damage);
        public void HealFromInt(int healAmount) => Heal(healAmount);
        public void SetHealthFromInt(int health) => SetHealth(health);
        public void SetMaxHealthFromInt(int maxHealth) => SetMaxHealth(maxHealth);

        private void RaiseHealthChanged()
        {
            HealthChanged?.Invoke(currentHealth, maxHealth);
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            if (healthEvents != null)
            {
                healthEvents.RaiseHealthChanged(currentHealth, maxHealth);
            }
        }

        private void RaiseDamageTaken(int damage)
        {
            DamageTaken?.Invoke(damage);
            onDamageTaken?.Invoke(damage);
            if (healthEvents != null)
            {
                healthEvents.RaiseDamageTaken(damage);
            }
        }

        private void RaiseHealed(int amount)
        {
            Healed?.Invoke(amount);
            onHealed?.Invoke(amount);
            if (healthEvents != null)
            {
                healthEvents.RaiseHealed(amount);
            }
        }

        private void RaiseDeath()
        {
            Death?.Invoke();
            onDeath?.Invoke();
            if (healthEvents != null)
            {
                healthEvents.RaiseDeath();
            }
        }

        private void RaiseDowned()
        {
            Downed?.Invoke();
            onDowned?.Invoke();
            if (healthEvents != null)
            {
                healthEvents.RaiseDowned();
            }
        }

        private void RaiseRevived()
        {
            Revived?.Invoke();
            onRevived?.Invoke();
            if (healthEvents != null)
            {
                healthEvents.RaiseRevived();
            }
        }

        private void RaiseDownedExpired()
        {
            DownedExpired?.Invoke();
            onDownedExpired?.Invoke();
            if (healthEvents != null)
            {
                healthEvents.RaiseDownedExpired();
            }
        }

        private int ApplyMitigation(int damage, DamageType damageType)
        {
            float multiplier = GetResistanceMultiplier(damageType);
            float mitigated = damage * multiplier;

            mitigated -= flatDamageReduction;
            mitigated *= 1f - percentDamageReduction;

            return Mathf.Max(0, Mathf.RoundToInt(mitigated));
        }

        private float GetResistanceMultiplier(DamageType damageType)
        {
            if (damageResistances == null || damageResistances.Length == 0) return 1f;

            for (int i = 0; i < damageResistances.Length; i++)
            {
                if (damageResistances[i].type == damageType)
                {
                    return Mathf.Clamp(damageResistances[i].multiplier, 0f, 2f);
                }
            }

            return 1f;
        }

        private void EnterDownedState()
        {
            if (isDowned || isDead) return;
            isDowned = true;
            downedTimer = Mathf.Max(0.1f, downedDuration);
            RaiseDowned();
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;
            isDowned = false;
            downedTimer = 0f;
            RaiseDeath();
        }
    }
}
