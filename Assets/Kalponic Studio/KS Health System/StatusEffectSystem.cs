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

            if (effect.effectName == "Poison")
            {
                healthSystem.TakeDamage(effect.amountPerTick);
            }
            else if (effect.effectName == "Regeneration")
            {
                healthSystem.Heal(effect.amountPerTick);
            }
        }

        /// <summary>
        /// Apply a status effect
        /// </summary>
        public void ApplyEffect(StatusEffect effect)
        {
            // Remove existing effect of same name
            RemoveEffect(effect.effectName);

            // Add new effect
            activeEffects.Add(effect);

            ApplyInstantEffect(effect, true);
            RaiseEffectApplied(effect.effectName);
        }

        public void RemoveEffect(string effectName)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].effectName == effectName)
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
            StatusEffect poison = new StatusEffect("Poison", duration, false, Mathf.Max(1, damagePerSecond), 1f);
            ApplyEffect(poison);
        }

        public void ApplyRegeneration(float duration = 8f, int healPerSecond = 4)
        {
            StatusEffect regen = new StatusEffect("Regeneration", duration, true, Mathf.Max(1, healPerSecond), 1f);
            ApplyEffect(regen);
        }

        public void ApplySpeedBoost(float duration = 10f, float multiplier = 1.5f)
        {
            float clampedMultiplier = Mathf.Max(0.1f, multiplier);
            StatusEffect speedBoost = new StatusEffect("Speed Boost", duration, true, 0, 0f, clampedMultiplier);
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
            if (effect.effectName == "Speed Boost" && speedModifier != null)
            {
                if (apply)
                {
                    speedModifier.ApplySpeedMultiplier(effect.speedMultiplier);
                }
                else
                {
                    speedModifier.ResetSpeedMultiplier();
                }
            }
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
