using System;
using UnityEngine;
using UnityEngine.Events;

namespace KalponicStudio.Health
{
    /// <summary>
    /// Simplified ScriptableObject event channel - just the essentials
    /// Create via: Assets → Create → Kalponic Studio → Health → Event Channel
    /// </summary>
    [CreateAssetMenu(menuName = "Kalponic Studio/Health/Event Channel", fileName = "HealthEventChannel")]
    public class HealthEventChannelSO : ScriptableObject
    {
        // Core health events - simplified
        [SerializeField] private UnityEvent<int, int> onHealthChanged = new UnityEvent<int, int>();
        [SerializeField] private UnityEvent<int> onDamageTaken = new UnityEvent<int>();
        [SerializeField] private UnityEvent<int> onHealed = new UnityEvent<int>();
        [SerializeField] private UnityEvent onDeath = new UnityEvent();
        [SerializeField] private UnityEvent onDowned = new UnityEvent();
        [SerializeField] private UnityEvent onRevived = new UnityEvent();
        [SerializeField] private UnityEvent onDownedExpired = new UnityEvent();

        // Optional shield events
        [SerializeField] private UnityEvent<int> onShieldAbsorbed = new UnityEvent<int>();
        [SerializeField] private UnityEvent onShieldDepleted = new UnityEvent();

        // Optional status effect events
        [SerializeField] private UnityEvent<string> onEffectApplied = new UnityEvent<string>();
        [SerializeField] private UnityEvent<string> onEffectExpired = new UnityEvent<string>();

        public event Action<int, int> HealthChanged;
        public event Action<int> DamageTaken;
        public event Action<int> Healed;
        public event Action Death;
        public event Action Downed;
        public event Action Revived;
        public event Action DownedExpired;
        public event Action<int> ShieldAbsorbed;
        public event Action ShieldDepleted;
        public event Action<string> EffectApplied;
        public event Action<string> EffectExpired;

        // Public API - easy to call from anywhere
        public void RaiseDamageTaken(int damage)
        {
            DamageTaken?.Invoke(damage);
            onDamageTaken.Invoke(damage);
        }

        public void RaiseHealed(int amount)
        {
            Healed?.Invoke(amount);
            onHealed.Invoke(amount);
        }

        public void RaiseDeath()
        {
            Death?.Invoke();
            onDeath.Invoke();
        }

        public void RaiseDowned()
        {
            Downed?.Invoke();
            onDowned.Invoke();
        }

        public void RaiseRevived()
        {
            Revived?.Invoke();
            onRevived.Invoke();
        }

        public void RaiseDownedExpired()
        {
            DownedExpired?.Invoke();
            onDownedExpired.Invoke();
        }

        public void RaiseHealthChanged(int currentHealth, int maxHealth)
        {
            HealthChanged?.Invoke(currentHealth, maxHealth);
            onHealthChanged.Invoke(currentHealth, maxHealth);
        }

        public void RaiseShieldAbsorbed(int damage)
        {
            ShieldAbsorbed?.Invoke(damage);
            onShieldAbsorbed.Invoke(damage);
        }

        public void RaiseShieldDepleted()
        {
            ShieldDepleted?.Invoke();
            onShieldDepleted.Invoke();
        }

        public void RaiseEffectApplied(string effectName)
        {
            EffectApplied?.Invoke(effectName);
            onEffectApplied.Invoke(effectName);
        }

        public void RaiseEffectExpired(string effectName)
        {
            EffectExpired?.Invoke(effectName);
            onEffectExpired.Invoke(effectName);
        }
    }
}
