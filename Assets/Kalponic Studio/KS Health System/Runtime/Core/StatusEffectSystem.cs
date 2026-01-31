using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using KalponicStudio.Health;

namespace KalponicStudio.Health
{
    /// <summary>
    /// Simple status effect system - just the essentials
    /// Works with HealthSystem for basic buffs/debuffs
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class StatusEffectSystem : MonoBehaviour, IStatusEffectComponent
    {
        [Header("Event Channels")]
        [SerializeField] private HealthEventChannelSO healthEvents;

        [Header("Active Effects")]
        [SerializeField] private List<StatusEffect> activeEffects = new List<StatusEffect>();

        [Header("Events")]
        [SerializeField] private UnityEvent<string> onEffectApplied = new UnityEvent<string>();
        [SerializeField] private UnityEvent<string> onEffectExpired = new UnityEvent<string>();

        public event Action<string> EffectApplied;
        public event Action<string> EffectExpired;

        // Component references
        private HealthSystem healthSystem;
        private ISpeedModifier speedModifier;

        private void Awake()
        {
            healthSystem = GetComponent<HealthSystem>();
            speedModifier = GetComponent<ISpeedModifier>();
        }

        private void Update()
        {
            UpdateEffects();
        }

        private void UpdateEffects()
        {
            // Update remaining time and remove expired effects
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = activeEffects[i];
                effect.remainingTime -= Time.deltaTime;

                // Apply continuous effects
                ApplyContinuousEffect(effect);

                // Remove expired effects
                if (effect.remainingTime <= 0)
                {
                    ExpireEffect(effect, i);
                }
            }
        }

        private void ApplyContinuousEffect(StatusEffect effect)
        {
            if (effect.tickInterval <= 0f) return;

            effect.tickTimer += Time.deltaTime;
            while (effect.tickTimer >= effect.tickInterval)
            {
                effect.tickTimer -= effect.tickInterval;
                ApplyTick(effect);
            }
        }

        private void ApplyTick(StatusEffect effect)
        {
            if (healthSystem == null) return;

            int tickAmount = Mathf.Max(0, effect.amountPerTick) * Mathf.Max(1, effect.stackCount);

            if (effect.effectType == StatusEffectType.Poison || effect.effectName == "Poison")
            {
                if (tickAmount > 0)
                {
                    healthSystem.TakeDamage(tickAmount);
                }
            }
            else if (effect.effectType == StatusEffectType.Regeneration || effect.effectName == "Regeneration")
            {
                if (tickAmount > 0)
                {
                    healthSystem.Heal(tickAmount);
                }
            }
        }

        /// <summary>
        /// Apply a status effect
        /// </summary>
        public void ApplyEffect(StatusEffect effect)
        {
            int existingIndex = FindEffectIndex(effect);
            if (existingIndex >= 0)
            {
                StatusEffect existing = activeEffects[existingIndex];
                ApplyStacking(existing, effect);
                activeEffects[existingIndex] = existing;
                ApplyInstantEffect(existing, true);
                RaiseEffectApplied(existing.effectName);
                return;
            }

            activeEffects.Add(effect);
            ApplyInstantEffect(effect, true);
            RaiseEffectApplied(effect.effectName);
        }

        public void RemoveEffect(string effectName)
        {
            string normalized = NormalizeEffectName(effectName);
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (NormalizeEffectName(activeEffects[i].effectName) == normalized)
                {
                    ExpireEffect(activeEffects[i], i);
                    break;
                }
            }
        }

        public void ClearEffects()
        {
            foreach (var effect in activeEffects)
            {
                ApplyInstantEffect(effect, false);
                RaiseEffectExpired(effect.effectName);
            }
            activeEffects.Clear();
        }

        // Interface implementations
        public void ApplyPoison(float duration = 5f, int damagePerSecond = 3)
        {
            StatusEffect poison = new StatusEffect(StatusEffectType.Poison, duration, false, Mathf.Max(1, damagePerSecond), 1f);
            poison.stackingMode = StatusStackingMode.Stack;
            poison.maxStacks = 5;
            ApplyEffect(poison);
        }

        public void ApplyRegeneration(float duration = 8f, int healPerSecond = 4)
        {
            StatusEffect regen = new StatusEffect(StatusEffectType.Regeneration, duration, true, Mathf.Max(1, healPerSecond), 1f);
            regen.stackingMode = StatusStackingMode.Refresh;
            regen.maxStacks = 1;
            ApplyEffect(regen);
        }

        public void ApplySpeedBoost(float duration = 10f, float multiplier = 1.5f)
        {
            float clampedMultiplier = Mathf.Max(0.1f, multiplier);
            StatusEffect speedBoost = new StatusEffect(StatusEffectType.SpeedBoost, duration, true, 0, 0f, clampedMultiplier);
            speedBoost.stackingMode = StatusStackingMode.Refresh;
            speedBoost.maxStacks = 1;
            ApplyEffect(speedBoost);
        }

        private void ExpireEffect(StatusEffect effect, int index)
        {
            ApplyInstantEffect(effect, false);
            activeEffects.RemoveAt(index);
            RaiseEffectExpired(effect.effectName);
        }

        private void ApplyInstantEffect(StatusEffect effect, bool apply)
        {
            if ((effect.effectType == StatusEffectType.SpeedBoost || effect.effectName == "Speed Boost") && speedModifier != null)
            {
                if (apply)
                {
                    speedModifier.ResetSpeedMultiplier();
                    speedModifier.ApplySpeedMultiplier(GetEffectiveSpeedMultiplier(effect));
                }
                else
                {
                    speedModifier.ResetSpeedMultiplier();
                }
            }
        }

        private float GetEffectiveSpeedMultiplier(StatusEffect effect)
        {
            if (effect.stackingMode != StatusStackingMode.Stack || effect.stackCount <= 1)
            {
                return effect.speedMultiplier;
            }

            return Mathf.Pow(effect.speedMultiplier, effect.stackCount);
        }

        private int FindEffectIndex(StatusEffect effect)
        {
            string normalized = NormalizeEffectName(effect.effectName);
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (NormalizeEffectName(activeEffects[i].effectName) == normalized)
                {
                    return i;
                }
            }

            return -1;
        }

        private string NormalizeEffectName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? string.Empty : name.Replace(" ", string.Empty).ToLowerInvariant();
        }

        private void ApplyStacking(StatusEffect existing, StatusEffect incoming)
        {
            existing.stackingMode = incoming.stackingMode;
            existing.maxStacks = Mathf.Max(1, incoming.maxStacks);

            switch (incoming.stackingMode)
            {
                case StatusStackingMode.Refresh:
                    existing.remainingTime = incoming.duration;
                    existing.stackCount = 1;
                    break;
                case StatusStackingMode.Extend:
                    existing.remainingTime += incoming.duration;
                    existing.stackCount = 1;
                    break;
                case StatusStackingMode.Stack:
                    existing.stackCount = Mathf.Min(existing.maxStacks, existing.stackCount + 1);
                    existing.remainingTime = Mathf.Max(existing.remainingTime, incoming.duration);
                    break;
            }

            existing.amountPerTick = incoming.amountPerTick;
            existing.tickInterval = incoming.tickInterval;
            existing.speedMultiplier = incoming.speedMultiplier;
        }

        private void RaiseEffectApplied(string effectName)
        {
            EffectApplied?.Invoke(effectName);
            onEffectApplied?.Invoke(effectName);
            if (healthEvents != null)
            {
                healthEvents.RaiseEffectApplied(effectName);
            }
        }

        private void RaiseEffectExpired(string effectName)
        {
            EffectExpired?.Invoke(effectName);
            onEffectExpired?.Invoke(effectName);
            if (healthEvents != null)
            {
                healthEvents.RaiseEffectExpired(effectName);
            }
        }
    }
}
