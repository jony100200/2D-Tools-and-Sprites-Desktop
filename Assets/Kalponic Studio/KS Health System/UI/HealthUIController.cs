using UnityEngine;
using UnityEngine.UI;
using KalponicStudio.Health;

namespace KalponicStudio.Health.UI
{
    /// <summary>
    /// Main controller for health UI components.
    /// Manages multiple health UI elements and coordinates their updates.
    /// </summary>
    public class HealthUIController : MonoBehaviour
    {
        [Header("Health System Reference")]
        [SerializeField] private HealthSystem healthSystem;

        [Header("UI Components")]
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private HealthText healthText;
        [SerializeField] private HealthIcon healthIcon;

        [Header("Settings")]
        [SerializeField] private bool autoFindComponents = true;
        [SerializeField] private bool updateInRealTime = true;

        private void Awake()
        {
            if (autoFindComponents)
            {
                FindUIComponents();
            }
        }

        private void OnEnable()
        {
            if (healthSystem != null)
            {
                healthSystem.HealthChanged += OnHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (healthSystem != null)
            {
                healthSystem.HealthChanged -= OnHealthChanged;
            }
        }

        private void Start()
        {
            UpdateAllUI();
        }

        private void Update()
        {
            if (updateInRealTime && healthSystem != null)
            {
                UpdateAllUI();
            }
        }

        private void FindUIComponents()
        {
            if (healthBar == null)
                healthBar = GetComponentInChildren<HealthBar>();

            if (healthText == null)
                healthText = GetComponentInChildren<HealthText>();

            if (healthIcon == null)
                healthIcon = GetComponentInChildren<HealthIcon>();
        }

        private void OnHealthChanged(int current, int max)
        {
            UpdateAllUI();
        }

        private void UpdateAllUI()
        {
            if (healthSystem == null) return;

            if (healthBar != null)
                healthBar.UpdateHealth(healthSystem.CurrentHealth, healthSystem.MaxHealth);

            if (healthText != null)
                healthText.UpdateHealth(healthSystem.CurrentHealth, healthSystem.MaxHealth);

            if (healthIcon != null)
                healthIcon.UpdateHealthState(healthSystem.HealthPercent);
        }

        // Public API for external control
        public void SetHealthSystem(HealthSystem newHealthSystem)
        {
            // Unsubscribe from old system
            if (healthSystem != null)
            {
                healthSystem.HealthChanged -= OnHealthChanged;
            }

            healthSystem = newHealthSystem;

            // Subscribe to new system
            if (healthSystem != null)
            {
                healthSystem.HealthChanged += OnHealthChanged;
                UpdateAllUI();
            }
        }

        public void ForceUpdate()
        {
            UpdateAllUI();
        }
    }
}
