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
        public UnityEvent<string> onEffectApplied = new UnityEvent<string>();
        public UnityEvent<string> onEffectExpired = new UnityEvent<string>();

        // Component references
        private HealthSystem healthSystem;

        private void Awake()
        {
            healthSystem = GetComponent<HealthSystem>();
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
                    RemoveEffect(effect.effectName);
                }
            }
        }

        private void ApplyContinuousEffect(StatusEffect effect)
        {
            // Simple implementation - just handle poison and regeneration
            if (effect.effectName == "Poison")
            {
                // Apply damage over time (simplified - damage every second)
                if (Time.frameCount % 60 == 0) // Every ~1 second at 60fps
                {
                    healthSystem.TakeDamage(3);
                }
            }
            else if (effect.effectName == "Regeneration")
            {
                // Apply healing over time (simplified - heal every second)
                if (Time.frameCount % 60 == 0) // Every ~1 second at 60fps
                {
                    healthSystem.Heal(4);
                }
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

            if (healthEvents != null)
            {
                healthEvents.RaiseEffectApplied(effect.effectName);
            }
            else
            {
                onEffectApplied?.Invoke(effect.effectName);
            }
        }

        public void RemoveEffect(string effectName)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].effectName == effectName)
                {
                    activeEffects.RemoveAt(i);
                    if (healthEvents != null)
                    {
                        healthEvents.RaiseEffectExpired(effectName);
                    }
                    else
                    {
                        onEffectExpired?.Invoke(effectName);
                    }
                    break;
                }
            }
        }

        public void ClearEffects()
        {
            foreach (var effect in activeEffects)
            {
                if (healthEvents != null)
                {
                    healthEvents.RaiseEffectExpired(effect.effectName);
                }
                else
                {
                    onEffectExpired?.Invoke(effect.effectName);
                }
            }
            activeEffects.Clear();
        }

        // Interface implementations
        public void ApplyPoison(float duration = 5f, int damagePerSecond = 3)
        {
            StatusEffect poison = new StatusEffect("Poison", duration, false);
            ApplyEffect(poison);
        }

        public void ApplyRegeneration(float duration = 8f, int healPerSecond = 4)
        {
            StatusEffect regen = new StatusEffect("Regeneration", duration, true);
            ApplyEffect(regen);
        }

        public void ApplySpeedBoost(float duration = 10f, float multiplier = 1.5f)
        {
            StatusEffect speedBoost = new StatusEffect("Speed Boost", duration, true);
            ApplyEffect(speedBoost);
        }
    }
}
