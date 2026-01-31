using System.Collections.Generic;
using UnityEngine;

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
        bool IsDowned { get; }
        bool IsDead { get; }

        void TakeDamage(int damage);
        void TakeDamage(DamageInfo damageInfo);
        void Heal(int amount);
        void Kill();
        void Revive(int healthAmount);
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

    // Optional shield damage absorption - health can ask shield to absorb first
    public interface IShieldAbsorber
    {
        /// <summary>
        /// Absorb incoming damage and return remaining damage (if any).
        /// </summary>
        int AbsorbDamage(int damage);
    }

    // Optional status effects - only if needed
    public interface IStatusEffectComponent
    {
        void ApplyPoison(float duration = 5f, int damagePerSecond = 3);
        void ApplyRegeneration(float duration = 8f, int healPerSecond = 4);
        void ApplySpeedBoost(float duration = 10f, float multiplier = 1.5f);
        void ClearEffects();
    }

    // Optional speed modifier hook for status effects
    public interface ISpeedModifier
    {
        void ApplySpeedMultiplier(float multiplier);
        void ResetSpeedMultiplier();
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
        public StatusEffectType effectType;
        public float duration;
        public float remainingTime;
        public bool isBuff; // true for buffs, false for debuffs
        public int amountPerTick;
        public float tickInterval;
        public float tickTimer;
        public float speedMultiplier = 1f;
        public StatusStackingMode stackingMode = StatusStackingMode.Refresh;
        public int maxStacks = 1;
        public int stackCount = 1;

        public StatusEffect(string name, float time, bool buff = false, int amountPerTick = 0, float tickInterval = 1f, float speedMultiplier = 1f)
        {
            effectName = name;
            effectType = StatusEffectType.Custom;
            duration = time;
            remainingTime = time;
            isBuff = buff;
            this.amountPerTick = amountPerTick;
            this.tickInterval = tickInterval;
            this.speedMultiplier = speedMultiplier;
            tickTimer = 0f;
        }

        public StatusEffect(StatusEffectType type, float time, bool buff = false, int amountPerTick = 0, float tickInterval = 1f, float speedMultiplier = 1f)
        {
            effectType = type;
            effectName = type.ToString();
            duration = time;
            remainingTime = time;
            isBuff = buff;
            this.amountPerTick = amountPerTick;
            this.tickInterval = tickInterval;
            this.speedMultiplier = speedMultiplier;
            tickTimer = 0f;
        }
    }

    public enum DamageType
    {
        Generic,
        Physical,
        Fire,
        Ice,
        Poison,
        Electric,
        True
    }

    public enum StatusEffectType
    {
        Custom,
        Poison,
        Regeneration,
        SpeedBoost
    }

    public enum StatusStackingMode
    {
        Refresh,
        Extend,
        Stack
    }

    [System.Serializable]
    public struct DamageInfo
    {
        public int Amount;
        public DamageType Type;
        public bool BypassShield;
        public bool IgnoreMitigation;
        public object Source;
        public string SourceTag;

        public static DamageInfo FromAmount(int amount)
        {
            return new DamageInfo
            {
                Amount = amount,
                Type = DamageType.Generic,
                BypassShield = false,
                IgnoreMitigation = false,
                Source = null,
                SourceTag = string.Empty
            };
        }
    }

    [System.Serializable]
    public struct DamageResistance
    {
        public DamageType type;
        [Range(0f, 2f)] public float multiplier;
    }
}
