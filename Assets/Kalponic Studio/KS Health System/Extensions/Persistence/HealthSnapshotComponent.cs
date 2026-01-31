using UnityEngine;
using KalponicStudio.Health;

namespace KalponicStudio.Health.Extensions.Persistence
{
    public class HealthSnapshotComponent : MonoBehaviour, IHealthSerializable
    {
        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private ShieldSystem shieldSystem;
        [SerializeField] private StatusEffectSystem statusEffectSystem;

        private void Awake()
        {
            if (healthSystem == null)
            {
                healthSystem = GetComponent<HealthSystem>();
            }

            if (shieldSystem == null)
            {
                shieldSystem = GetComponent<ShieldSystem>();
            }

            if (statusEffectSystem == null)
            {
                statusEffectSystem = GetComponent<StatusEffectSystem>();
            }
        }

        public HealthSnapshot CaptureSnapshot()
        {
            HealthSnapshot snapshot = new HealthSnapshot();

            if (healthSystem != null)
            {
                snapshot.MaxHealth = healthSystem.MaxHealth;
                snapshot.CurrentHealth = healthSystem.CurrentHealth;
                snapshot.IsDowned = healthSystem.IsDowned;
                snapshot.IsDead = healthSystem.IsDead;
                snapshot.DownedTimeRemaining = healthSystem.DownedTimeRemaining;
            }

            if (shieldSystem != null)
            {
                snapshot.MaxShield = shieldSystem.MaxShield;
                snapshot.CurrentShield = shieldSystem.CurrentShield;
            }

            if (statusEffectSystem != null)
            {
                snapshot.StatusEffects = statusEffectSystem.GetActiveEffects();
            }

            return snapshot;
        }

        public void RestoreSnapshot(HealthSnapshot snapshot)
        {
            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(snapshot.MaxHealth > 0 ? snapshot.MaxHealth : healthSystem.MaxHealth);
                healthSystem.SetHealth(snapshot.CurrentHealth);

                if (snapshot.IsDead)
                {
                    healthSystem.ForceDead();
                }
                else if (snapshot.IsDowned)
                {
                    healthSystem.ForceDowned(snapshot.DownedTimeRemaining);
                }
            }

            if (shieldSystem != null)
            {
                if (snapshot.MaxShield > 0)
                {
                    shieldSystem.SetMaxShield(snapshot.MaxShield);
                }

                shieldSystem.SetShield(snapshot.CurrentShield);
            }

            if (statusEffectSystem != null && snapshot.StatusEffects != null)
            {
                statusEffectSystem.RestoreEffects(snapshot.StatusEffects);
            }
        }
    }
}
