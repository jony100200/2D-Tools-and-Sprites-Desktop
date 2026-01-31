using UnityEngine;

namespace KalponicStudio.Health
{
    [CreateAssetMenu(menuName = "Kalponic Studio/Health/Health Profile", fileName = "HealthProfile")]
    public class HealthProfileSO : ScriptableObject
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int startingHealth = 100;
        [SerializeField] private bool regenerateHealth = false;
        [SerializeField] private float regenerationRate = 1f;
        [SerializeField] private float regenerationDelay = 2f;
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

        [Header("Shield (Optional)")]
        [SerializeField] private bool enableShield = false;
        [SerializeField] private int maxShield = 50;
        [SerializeField] private int startingShield = 50;
        [SerializeField] private float shieldRegenerationRate = 5f;
        [SerializeField] private float shieldRegenerationDelay = 3f;
        [SerializeField] private bool shieldRechargesAfterDamage = true;

        public void ApplyTo(HealthSystem healthSystem)
        {
            if (healthSystem == null) return;

            healthSystem.SetMaxHealth(maxHealth);
            healthSystem.SetHealth(Mathf.Clamp(startingHealth, 0, maxHealth));
            healthSystem.ConfigureRegeneration(regenerateHealth, regenerationRate, regenerationDelay);
            healthSystem.ConfigureMitigation(flatDamageReduction, percentDamageReduction, damageResistances);
            healthSystem.ConfigureDownedState(enableDownedState, downedDuration, allowRevive);
            healthSystem.ConfigureInvulnerability(enableInvulnerability, invulnerabilityDuration, flashDuringInvulnerability, flashInterval);
        }

        public void ApplyTo(ShieldSystem shieldSystem)
        {
            if (shieldSystem == null || !enableShield) return;

            shieldSystem.SetMaxShield(maxShield);
            shieldSystem.SetShield(Mathf.Clamp(startingShield, 0, maxShield));
            shieldSystem.ConfigureRegeneration(shieldRegenerationRate, shieldRegenerationDelay, shieldRechargesAfterDamage);
        }
    }
}
