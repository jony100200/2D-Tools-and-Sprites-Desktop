using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio.Health
{
    /// <summary>
    /// Simplified health controller that orchestrates health systems
    /// Uses the simplified interfaces for maximum simplicity and reusability
    /// </summary>
    public class ModularHealthController : MonoBehaviour, IHealthEventHandler
    {
        [Header("Event Channels")]
        [SerializeField] private HealthEventChannelSO healthEvents;

        [Header("Health Systems")]
        [SerializeField] private MonoBehaviour healthSystem;
        [SerializeField] private MonoBehaviour shieldSystem;
        [SerializeField] private MonoBehaviour statusEffectSystem;

        [Header("Event Handlers")]
        [SerializeField] private List<MonoBehaviour> healthEventHandlers = new List<MonoBehaviour>();

        // Interface references (cached for performance)
        private IHealthComponent _healthSystem;
        private IShieldComponent _shieldSystem;
        private IStatusEffectComponent _statusEffectSystem;

        private HealthSystem _healthSystemBehaviour;
        private ShieldSystem _shieldSystemBehaviour;
        private StatusEffectSystem _statusEffectSystemBehaviour;

        private List<IHealthEventHandler> _healthEventHandlers = new List<IHealthEventHandler>();

        private void Awake()
        {
            InitializeSystems();
            InitializeEventHandlers();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeSystems()
        {
            // Cache interface references
            _healthSystem = healthSystem as IHealthComponent;
            _shieldSystem = shieldSystem as IShieldComponent;
            _statusEffectSystem = statusEffectSystem as IStatusEffectComponent;

            _healthSystemBehaviour = healthSystem as HealthSystem;
            _shieldSystemBehaviour = shieldSystem as ShieldSystem;
            _statusEffectSystemBehaviour = statusEffectSystem as StatusEffectSystem;

            // Validate systems
            if (healthSystem != null && _healthSystem == null)
                Debug.LogError("Health System does not implement IHealthComponent interface!");

            if (shieldSystem != null && _shieldSystem == null)
                Debug.LogError("Shield System does not implement IShieldComponent interface!");

            if (statusEffectSystem != null && _statusEffectSystem == null)
                Debug.LogError("Status Effect System does not implement IStatusEffectComponent interface!");
        }

        private void InitializeEventHandlers()
        {
            // Convert MonoBehaviour lists to interface lists
            foreach (var handler in healthEventHandlers)
            {
                if (handler is IHealthEventHandler healthHandler)
                    _healthEventHandlers.Add(healthHandler);
            }
        }

        private void SubscribeToEvents()
        {
            if (healthEvents != null)
            {
                // Subscribe to event channel
                healthEvents.DamageTaken += OnDamageTaken;
                healthEvents.Healed += OnHealed;
                healthEvents.Death += OnDeath;

                healthEvents.ShieldAbsorbed += OnShieldAbsorbed;
                healthEvents.ShieldDepleted += OnShieldDepleted;

                healthEvents.EffectApplied += OnEffectApplied;
                healthEvents.EffectExpired += OnEffectExpired;
                return;
            }

            if (_healthSystemBehaviour != null)
            {
                _healthSystemBehaviour.DamageTaken += OnDamageTaken;
                _healthSystemBehaviour.Healed += OnHealed;
                _healthSystemBehaviour.Death += OnDeath;
            }

            if (_shieldSystemBehaviour != null)
            {
                _shieldSystemBehaviour.ShieldAbsorbed += OnShieldAbsorbed;
                _shieldSystemBehaviour.ShieldDepleted += OnShieldDepleted;
            }

            if (_statusEffectSystemBehaviour != null)
            {
                _statusEffectSystemBehaviour.EffectApplied += OnEffectApplied;
                _statusEffectSystemBehaviour.EffectExpired += OnEffectExpired;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (healthEvents != null)
            {
                // Unsubscribe from event channel
                healthEvents.DamageTaken -= OnDamageTaken;
                healthEvents.Healed -= OnHealed;
                healthEvents.Death -= OnDeath;

                healthEvents.ShieldAbsorbed -= OnShieldAbsorbed;
                healthEvents.ShieldDepleted -= OnShieldDepleted;

                healthEvents.EffectApplied -= OnEffectApplied;
                healthEvents.EffectExpired -= OnEffectExpired;
                return;
            }

            if (_healthSystemBehaviour != null)
            {
                _healthSystemBehaviour.DamageTaken -= OnDamageTaken;
                _healthSystemBehaviour.Healed -= OnHealed;
                _healthSystemBehaviour.Death -= OnDeath;
            }

            if (_shieldSystemBehaviour != null)
            {
                _shieldSystemBehaviour.ShieldAbsorbed -= OnShieldAbsorbed;
                _shieldSystemBehaviour.ShieldDepleted -= OnShieldDepleted;
            }

            if (_statusEffectSystemBehaviour != null)
            {
                _statusEffectSystemBehaviour.EffectApplied -= OnEffectApplied;
                _statusEffectSystemBehaviour.EffectExpired -= OnEffectExpired;
            }
        }

        // Public API - Delegate to systems through interfaces
        public void TakeDamage(int damage)
        {
            _healthSystem?.TakeDamage(damage);
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            _healthSystem?.TakeDamage(damageInfo);
        }

        public void Heal(int amount)
        {
            _healthSystem?.Heal(amount);
        }

        public void Kill()
        {
            _healthSystem?.Kill();
        }

        public void DamageShield(int damage)
        {
            _shieldSystem?.DamageShield(damage);
        }

        public void RestoreShield(int amount)
        {
            _shieldSystem?.RestoreShield(amount);
        }

        public void ApplyPoison(float duration = 5f, int damagePerSecond = 3)
        {
            _statusEffectSystem?.ApplyPoison(duration, damagePerSecond);
        }

        public void ApplyRegeneration(float duration = 8f, int healPerSecond = 4)
        {
            _statusEffectSystem?.ApplyRegeneration(duration, healPerSecond);
        }

        public void ApplySpeedBoost(float duration = 10f, float multiplier = 1.5f)
        {
            _statusEffectSystem?.ApplySpeedBoost(duration, multiplier);
        }

        public void ClearEffects()
        {
            _statusEffectSystem?.ClearEffects();
        }

        // Properties - Delegate to systems
        public int MaxHealth => _healthSystem?.MaxHealth ?? 0;
        public int CurrentHealth => _healthSystem?.CurrentHealth ?? 0;
        public float HealthPercent => _healthSystem?.HealthPercent ?? 0f;
        public bool IsAlive => _healthSystem?.IsAlive ?? false;

        public int MaxShield => _shieldSystem?.MaxShield ?? 0;
        public int CurrentShield => _shieldSystem?.CurrentShield ?? 0;
        public bool HasShield => _shieldSystem?.HasShield ?? false;

        // Event handler implementations - Forward to all registered handlers
        public void OnDamageTaken(int damage)
        {
            foreach (var handler in _healthEventHandlers)
            {
                handler.OnDamageTaken(damage);
            }
        }

        public void OnHealed(int healAmount)
        {
            foreach (var handler in _healthEventHandlers)
            {
                handler.OnHealed(healAmount);
            }
        }

        public void OnDeath()
        {
            foreach (var handler in _healthEventHandlers)
            {
                handler.OnDeath();
            }
        }

        // Shield event handlers - simplified
        private void OnShieldAbsorbed(int damage)
        {
            // Forward to health event handlers if they want shield info
            foreach (var handler in _healthEventHandlers)
            {
                // Could add shield-specific handling here if needed
            }
        }

        private void OnShieldDepleted()
        {
            // Forward to health event handlers if they want shield info
            foreach (var handler in _healthEventHandlers)
            {
                // Could add shield-specific handling here if needed
            }
        }

        // Status effect event handlers - simplified
        private void OnEffectApplied(string effectName)
        {
            // Forward to health event handlers if they want effect info
            foreach (var handler in _healthEventHandlers)
            {
                // Could add effect-specific handling here if needed
            }
        }

        private void OnEffectExpired(string effectName)
        {
            // Forward to health event handlers if they want effect info
            foreach (var handler in _healthEventHandlers)
            {
                // Could add effect-specific handling here if needed
            }
        }

        // Runtime handler management
        public void AddHealthEventHandler(IHealthEventHandler handler)
        {
            if (!_healthEventHandlers.Contains(handler))
                _healthEventHandlers.Add(handler);
        }

        public void RemoveHealthEventHandler(IHealthEventHandler handler)
        {
            _healthEventHandlers.Remove(handler);
        }
    }
}
