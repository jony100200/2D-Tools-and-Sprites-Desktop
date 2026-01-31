using System.Collections.Generic;

namespace KalponicStudio.Health
{
    /// <summary>
    /// Simplified, practical health interfaces - no over-engineering
    /// </summary>

    // Core health interface - just the essentials
    public interface IHealthComponent
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        float HealthPercent { get; }
        bool IsAlive { get; }

        void TakeDamage(int damage);
        void Heal(int amount);
        void Kill();
    }

    // Optional shield interface - only if needed
    public interface IShieldComponent
    {
        int CurrentShield { get; }
        int MaxShield { get; }
        bool HasShield { get; }

        void DamageShield(int damage);
        void RestoreShield(int amount);
    }

    // Optional status effects - only if needed
    public interface IStatusEffectComponent
    {
        void ApplyPoison(float duration = 5f, int damagePerSecond = 3);
        void ApplyRegeneration(float duration = 8f, int healPerSecond = 4);
        void ApplySpeedBoost(float duration = 10f, float multiplier = 1.5f);
        void ClearEffects();
    }

    // Simple event handler - just the essentials
    public interface IHealthEventHandler
    {
        void OnDamageTaken(int damage);
        void OnHealed(int amount);
        void OnDeath();
    }

    // Status effect data - simplified
    [System.Serializable]
    public class StatusEffect
    {
        public string effectName;
        public float duration;
        public float remainingTime;
        public bool isBuff; // true for buffs, false for debuffs

        public StatusEffect(string name, float time, bool buff = false)
        {
            effectName = name;
            duration = time;
            remainingTime = time;
            isBuff = buff;
        }
    }
}
