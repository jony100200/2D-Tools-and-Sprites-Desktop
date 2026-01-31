using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using KalponicStudio.Health;

namespace KalponicStudio.Health
{
    /// <summary>
    /// Shield system that works with HealthSystem
    /// Provides damage absorption and regeneration mechanics
    /// Now uses event channels for better decoupling
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class ShieldSystem : MonoBehaviour, IShieldComponent
    {
        [Header("Event Channels")]
        [SerializeField] private HealthEventChannelSO healthEvents;
        [Header("Shield Settings")]
        [SerializeField] private int maxShield = 50;
        [SerializeField] private int currentShield = 50;
        [SerializeField] private float shieldRegenerationRate = 5f; // Shield per second
        [SerializeField] private float regenerationDelay = 3f; // Delay before regeneration starts
        [SerializeField] private bool shieldRechargesAfterDamage = true;

        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer shieldRenderer;
        [SerializeField] private Color shieldActiveColor = Color.blue;
        [SerializeField] private Color shieldDepletedColor = Color.gray;

        [Header("Events")]
        public UnityEvent<int, int> onShieldChanged = new UnityEvent<int, int>(); // (currentShield, maxShield)
        public UnityEvent<int> onShieldAbsorbed = new UnityEvent<int>(); // damage absorbed
        public UnityEvent onShieldDepleted = new UnityEvent();
        public UnityEvent onShieldRestored = new UnityEvent();

        // Public properties
        public int MaxShield => maxShield;
        public int CurrentShield => currentShield;
        public bool HasShield => currentShield > 0;

        // Private fields
        private HealthSystem healthSystem;
        private float regenerationTimer = 0f;
        private bool shieldActive = true;

        private void Awake()
        {
            healthSystem = GetComponent<HealthSystem>();
            currentShield = Mathf.Clamp(currentShield, 0, maxShield);

            // Subscribe to health system damage events
            healthSystem.onDamageTaken.AddListener(OnHealthDamageTaken);

            // Setup shield visual
            if (shieldRenderer == null)
            {
                shieldRenderer = GetComponent<SpriteRenderer>();
            }

            UpdateShieldVisual();
            onShieldChanged?.Invoke(currentShield, maxShield);
        }

        private void Update()
        {
            HandleShieldRegeneration();
        }

        private void HandleShieldRegeneration()
        {
            if (!shieldRechargesAfterDamage || !shieldActive || currentShield >= maxShield) return;

            // Check if enough time has passed since last damage
            if (regenerationTimer > 0)
            {
                regenerationTimer -= Time.deltaTime;
                return;
            }

            // Regenerate shield
            if (currentShield < maxShield)
            {
                int oldShield = currentShield;
                currentShield = Mathf.Min(maxShield, currentShield + Mathf.RoundToInt(shieldRegenerationRate * Time.deltaTime));

                if (currentShield != oldShield)
                {
                    onShieldChanged?.Invoke(currentShield, maxShield);
                    UpdateShieldVisual();
                }
            }
        }

        /// <summary>
        /// Called when health system takes damage - shield absorbs first
        /// </summary>
        private void OnHealthDamageTaken(int damage)
        {
            if (!shieldActive || currentShield <= 0) return;

            int damageAbsorbed = Mathf.Min(damage, currentShield);
            int damagePassedThrough = damage - damageAbsorbed;

            // Reduce shield
            int oldShield = currentShield;
            currentShield = Mathf.Max(0, currentShield - damageAbsorbed);

            // Notify listeners
            onShieldChanged?.Invoke(currentShield, maxShield);
            if (healthEvents != null)
            {
                healthEvents.RaiseShieldAbsorbed(damageAbsorbed);
            }
            else
            {
                onShieldAbsorbed?.Invoke(damageAbsorbed);
            }

            // Handle shield depletion
            if (currentShield <= 0 && oldShield > 0)
            {
                if (healthEvents != null)
                {
                    healthEvents.RaiseShieldDepleted();
                }
                else
                {
                    onShieldDepleted?.Invoke();
                }
                regenerationTimer = regenerationDelay; // Start regeneration delay
            }

            UpdateShieldVisual();

            // If there's damage that passed through, we need to handle it
            // This would require modifying HealthSystem to allow damage interception
            if (damagePassedThrough > 0)
            {
                Debug.LogWarning($"Shield absorbed {damageAbsorbed} damage, but {damagePassedThrough} passed through to health!");
                // In a full implementation, you'd need HealthSystem to support damage modification
            }
        }

        /// <summary>
        /// Manually damage the shield
        /// </summary>
        public void DamageShield(int damage)
        {
            if (!shieldActive || damage <= 0) return;

            int oldShield = currentShield;
            currentShield = Mathf.Max(0, currentShield - damage);

            if (healthEvents != null)
            {
                healthEvents.RaiseShieldAbsorbed(damage);
            }
            else
            {
                onShieldChanged?.Invoke(currentShield, maxShield);
                onShieldAbsorbed?.Invoke(damage);
            }

            if (currentShield <= 0 && oldShield > 0)
            {
                if (healthEvents != null)
                {
                    healthEvents.RaiseShieldDepleted();
                }
                else
                {
                    onShieldDepleted?.Invoke();
                }
                regenerationTimer = regenerationDelay;
            }

            UpdateShieldVisual();
        }

        /// <summary>
        /// Restore shield points
        /// </summary>
        public void RestoreShield(int amount)
        {
            if (amount <= 0) return;

            int oldShield = currentShield;
            currentShield = Mathf.Min(maxShield, currentShield + amount);

            if (currentShield != oldShield)
            {
                onShieldChanged?.Invoke(currentShield, maxShield);
                UpdateShieldVisual();

                if (oldShield <= 0 && currentShield > 0)
                {
                    onShieldRestored?.Invoke();
                }
            }
        }

        /// <summary>
        /// Set shield to specific value
        /// </summary>
        public void SetShield(int newShield)
        {
            int oldShield = currentShield;
            currentShield = Mathf.Clamp(newShield, 0, maxShield);

            if (currentShield != oldShield)
            {
                onShieldChanged?.Invoke(currentShield, maxShield);
                UpdateShieldVisual();

                if (oldShield <= 0 && currentShield > 0)
                {
                    onShieldRestored?.Invoke();
                }
                else if (currentShield <= 0 && oldShield > 0)
                {
                    if (healthEvents != null)
                    {
                        healthEvents.RaiseShieldDepleted();
                    }
                    else
                    {
                        onShieldDepleted?.Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// Fully restore shield
        /// </summary>
        public void RestoreFullShield()
        {
            SetShield(maxShield);
        }

        /// <summary>
        /// Deplete shield completely
        /// </summary>
        public void DepleteShield()
        {
            SetShield(0);
        }

        /// <summary>
        /// Enable/disable shield
        /// </summary>
        public void SetShieldActive(bool active)
        {
            shieldActive = active;
            UpdateShieldVisual();
        }

        public void SetMaxShield(int newMaxShield)
        {
            maxShield = Mathf.Max(1, newMaxShield);
            currentShield = Mathf.Min(currentShield, maxShield);
            onShieldChanged?.Invoke(currentShield, maxShield);
            UpdateShieldVisual();
        }

        private void UpdateShieldVisual()
        {
            if (shieldRenderer == null) return;

            shieldRenderer.enabled = shieldActive && currentShield > 0;

            if (shieldRenderer.enabled)
            {
                // Color based on shield strength
                float healthRatio = maxShield > 0 ? (float)currentShield / maxShield : 0f;
                shieldRenderer.color = Color.Lerp(shieldDepletedColor, shieldActiveColor, healthRatio);
            }
        }

        // Unity event helpers for easy inspector setup
        public void DamageShieldFromInt(int damage) => DamageShield(damage);
        public void RestoreShieldFromInt(int amount) => RestoreShield(amount);
        public void SetShieldFromInt(int shield) => SetShield(shield);
        public void SetMaxShieldFromInt(int maxShield) => SetMaxShield(maxShield);
    }
}
