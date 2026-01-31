using UnityEngine;
using KalponicStudio.Health;
using KalponicStudio.Health.UI;

namespace KalponicStudio.Health.Extensions.UI
{
    public class BossHealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private HealthText healthText;

        private void Awake()
        {
            if (healthSystem == null)
            {
                healthSystem = GetComponentInParent<HealthSystem>();
            }

            if (healthBar == null)
            {
                healthBar = GetComponentInChildren<HealthBar>();
            }

            if (healthText == null)
            {
                healthText = GetComponentInChildren<HealthText>();
            }
        }

        private void OnEnable()
        {
            if (healthSystem != null)
            {
                healthSystem.HealthChanged += OnHealthChanged;
            }

            RefreshUI();
        }

        private void OnDisable()
        {
            if (healthSystem != null)
            {
                healthSystem.HealthChanged -= OnHealthChanged;
            }
        }

        public void SetBoss(HealthSystem newBoss)
        {
            if (healthSystem != null)
            {
                healthSystem.HealthChanged -= OnHealthChanged;
            }

            healthSystem = newBoss;

            if (healthSystem != null)
            {
                healthSystem.HealthChanged += OnHealthChanged;
            }

            RefreshUI();
        }

        private void OnHealthChanged(int current, int max)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (healthSystem == null) return;

            if (healthBar != null)
            {
                healthBar.UpdateHealth(healthSystem.CurrentHealth, healthSystem.MaxHealth);
            }

            if (healthText != null)
            {
                healthText.UpdateHealth(healthSystem.CurrentHealth, healthSystem.MaxHealth);
            }
        }
    }
}
