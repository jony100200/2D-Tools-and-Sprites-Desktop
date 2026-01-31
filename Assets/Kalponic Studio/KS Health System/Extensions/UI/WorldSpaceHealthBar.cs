using UnityEngine;
using KalponicStudio.Health;
using KalponicStudio.Health.UI;

namespace KalponicStudio.Health.Extensions.UI
{
    public class WorldSpaceHealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private Transform target;

        [Header("Positioning")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField] private bool faceCamera = true;
        [SerializeField] private Camera worldCamera;

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

            if (target == null && healthSystem != null)
            {
                target = healthSystem.transform;
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
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

        private void LateUpdate()
        {
            if (target != null)
            {
                transform.position = target.position + worldOffset;
            }

            if (faceCamera && worldCamera != null)
            {
                transform.forward = worldCamera.transform.forward;
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (healthBar != null && healthSystem != null)
            {
                healthBar.UpdateHealth(healthSystem.CurrentHealth, healthSystem.MaxHealth);
            }
        }
    }
}
