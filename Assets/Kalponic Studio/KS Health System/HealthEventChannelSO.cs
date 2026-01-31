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
        public UnityEvent<int> onDamageTaken = new UnityEvent<int>();
        public UnityEvent<int> onHealed = new UnityEvent<int>();
        public UnityEvent onDeath = new UnityEvent();

        // Optional shield events
        public UnityEvent<int> onShieldAbsorbed = new UnityEvent<int>();
        public UnityEvent onShieldDepleted = new UnityEvent();

        // Optional status effect events
        public UnityEvent<string> onEffectApplied = new UnityEvent<string>();
        public UnityEvent<string> onEffectExpired = new UnityEvent<string>();

        // Public API - easy to call from anywhere
        public void RaiseDamageTaken(int damage) => onDamageTaken.Invoke(damage);
        public void RaiseHealed(int amount) => onHealed.Invoke(amount);
        public void RaiseDeath() => onDeath.Invoke();
        public void RaiseShieldAbsorbed(int damage) => onShieldAbsorbed.Invoke(damage);
        public void RaiseShieldDepleted() => onShieldDepleted.Invoke();
        public void RaiseEffectApplied(string effectName) => onEffectApplied.Invoke(effectName);
        public void RaiseEffectExpired(string effectName) => onEffectExpired.Invoke(effectName);
    }
}
