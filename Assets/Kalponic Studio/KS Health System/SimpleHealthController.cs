using UnityEngine;

namespace KalponicStudio.Health
{
    /// <summary>
    /// Simplified modular health controller - practical, not over-engineered
    /// Orchestrates health systems via interfaces and event channels
    /// </summary>
    public class SimpleHealthController : MonoBehaviour
    {
        [Header("Event Channel")]
        [SerializeField] private HealthEventChannelSO healthEvents;

        [Header("Health Systems (Optional - Drag & Drop)")]
        [SerializeField] private MonoBehaviour healthSystem;
        [SerializeField] private MonoBehaviour shieldSystem;
        [SerializeField] private MonoBehaviour statusEffectSystem;

        // Interface references - cached for performance
        private IHealthComponent _health;
        private IShieldComponent _shield;
        private IStatusEffectComponent _effects;
        private bool raiseHealthEvents = true;
        private bool raiseShieldEvents = true;
        private bool raiseEffectEvents = true;

        private void Awake()
        {
            // Get interface references - allows any component that implements the interface
            _health = healthSystem as IHealthComponent;
            _shield = shieldSystem as IShieldComponent;
            _effects = statusEffectSystem as IStatusEffectComponent;
            raiseHealthEvents = !(healthSystem is HealthSystem);
            raiseShieldEvents = !(shieldSystem is ShieldSystem);
            raiseEffectEvents = !(statusEffectSystem is StatusEffectSystem);

            // Basic validation
            if (healthSystem != null && _health == null)
                Debug.LogWarning("Health System doesn't implement IHealthComponent!");
            if (shieldSystem != null && _shield == null)
                Debug.LogWarning("Shield System doesn't implement IShieldComponent!");
            if (statusEffectSystem != null && _effects == null)
                Debug.LogWarning("Status Effect System doesn't implement IStatusEffectComponent!");
        }

        // Public API - Simple and clean
        public void TakeDamage(int damage)
        {
            if (_health is HealthSystem)
            {
                _health.TakeDamage(damage);
            }
            else if (_shield != null && _shield.HasShield)
            {
                int remainingDamage = damage - _shield.CurrentShield;
                _shield.DamageShield(damage);

                if (remainingDamage > 0 && _health != null)
                {
                    _health.TakeDamage(remainingDamage);
                }
            }
            else if (_health != null)
            {
                _health.TakeDamage(damage);
            }

            if (raiseHealthEvents)
            {
                healthEvents?.RaiseDamageTaken(damage);
            }
        }

        public void Heal(int amount)
        {
            _health?.Heal(amount);
            if (raiseHealthEvents)
            {
                healthEvents?.RaiseHealed(amount);
            }
        }

        public void Kill()
        {
            _health?.Kill();
            if (raiseHealthEvents)
            {
                healthEvents?.RaiseDeath();
            }
        }

        // Shield methods
        public void DamageShield(int damage)
        {
            _shield?.DamageShield(damage);
            if (raiseShieldEvents)
            {
                healthEvents?.RaiseShieldAbsorbed(damage);
            }
        }

        public void RestoreShield(int amount)
        {
            _shield?.RestoreShield(amount);
        }

        // Status effect methods
        public void ApplyPoison(float duration = 5f, int damagePerSecond = 3)
        {
            _effects?.ApplyPoison(duration, damagePerSecond);
            if (raiseEffectEvents)
            {
                healthEvents?.RaiseEffectApplied("Poison");
            }
        }

        public void ApplyRegeneration(float duration = 8f, int healPerSecond = 4)
        {
            _effects?.ApplyRegeneration(duration, healPerSecond);
            if (raiseEffectEvents)
            {
                healthEvents?.RaiseEffectApplied("Regeneration");
            }
        }

        public void ApplySpeedBoost(float duration = 10f, float multiplier = 1.5f)
        {
            _effects?.ApplySpeedBoost(duration, multiplier);
            if (raiseEffectEvents)
            {
                healthEvents?.RaiseEffectApplied("Speed Boost");
            }
        }

        // Properties - delegate to systems
        public int CurrentHealth => _health?.CurrentHealth ?? 0;
        public int MaxHealth => _health?.MaxHealth ?? 0;
        public float HealthPercent => _health?.HealthPercent ?? 0f;
        public bool IsAlive => _health?.IsAlive ?? false;

        public int CurrentShield => _shield?.CurrentShield ?? 0;
        public int MaxShield => _shield?.MaxShield ?? 0;
        public bool HasShield => _shield?.HasShield ?? false;
    }
}
