using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Advanced character animation configuration with themes and variants
    /// Supports multiple characters sharing animation logic but with different sprites
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterAnimationConfig", menuName = "KS Animation 2D/Character Animation Config")]
    public class CharacterAnimationConfig : ScriptableObject
    {
        [System.Serializable]
        public class AnimationStateConfig
        {
            public string stateName; // Logical state name (Idle, Walk, Jump, etc.)
            public AnimationStatePriority priority = AnimationStatePriority.Medium;
            public float transitionDuration = 0.1f;
            public bool canBeInterrupted = true;

            // Optional: custom logic for this state
            public System.Func<bool> customCondition;
            public System.Action onStateEntered;
            public System.Action onStateExited;
        }

        [System.Serializable]
        public class AnimationVariant
        {
            public string variantName; // "Chris", "SkeletonArcher", "Hero", etc.
            public Sprite[] idleSprites;
            public Sprite[] walkSprites;
            public Sprite[] runSprites;
            public Sprite[] jumpSprites;
            public Sprite[] fallSprites;
            public Sprite[] attackSprites;
            public Sprite[] hurtSprites;
            public Sprite[] deathSprites;
            // Add more as needed...
        }

        [Header("Animation Logic (Shared)")]
        public List<AnimationStateConfig> states = new List<AnimationStateConfig>();

        [Header("Character Variants (Sprites)")]
        public List<AnimationVariant> variants = new List<AnimationVariant>();

        [Header("Default States")]
        public string idleState = "Idle";
        public string walkState = "Walk";
        public string runState = "Run";
        public string jumpState = "Jump";
        public string fallState = "Fall";
        public string attackState = "Attack";
        public string hurtState = "Hurt";
        public string deathState = "Death";

        // Quick access properties
        public AnimationStateConfig Idle => GetStateConfig(idleState);
        public AnimationStateConfig Walk => GetStateConfig(walkState);
        public AnimationStateConfig Run => GetStateConfig(runState);
        public AnimationStateConfig Jump => GetStateConfig(jumpState);
        public AnimationStateConfig Fall => GetStateConfig(fallState);
        public AnimationStateConfig Attack => GetStateConfig(attackState);
        public AnimationStateConfig Hurt => GetStateConfig(hurtState);
        public AnimationStateConfig Death => GetStateConfig(deathState);

        public AnimationStateConfig GetStateConfig(string stateName)
        {
            return states.Find(s => s.stateName == stateName);
        }

        public bool HasState(string stateName)
        {
            return states.Exists(s => s.stateName == stateName);
        }

        public void AddState(string stateName, AnimationStatePriority priority = AnimationStatePriority.Medium, float transitionDuration = 0.1f)
        {
            if (!HasState(stateName))
            {
                states.Add(new AnimationStateConfig
                {
                    stateName = stateName,
                    priority = priority,
                    transitionDuration = transitionDuration
                });
            }
        }

        public void RemoveState(string stateName)
        {
            states.RemoveAll(s => s.stateName == stateName);
        }

        // Variant management
        public AnimationVariant GetVariant(string variantName)
        {
            return variants.Find(v => v.variantName == variantName);
        }

        public Sprite[] GetSpritesForState(string variantName, string stateName)
        {
            var variant = GetVariant(variantName);
            if (variant == null) return null;

            // Return appropriate sprite array based on state
            switch (stateName)
            {
                case "Idle": return variant.idleSprites;
                case "Walk": return variant.walkSprites;
                case "Run": return variant.runSprites;
                case "Jump": return variant.jumpSprites;
                case "Fall": return variant.fallSprites;
                case "Attack": return variant.attackSprites;
                case "Hurt": return variant.hurtSprites;
                case "Death": return variant.deathSprites;
                default: return null;
            }
        }
    }

    /// <summary>
    /// Constants class to eliminate magic strings throughout the codebase
    /// </summary>
    public static class AnimationStateNames
    {
        // Common states
        public const string Idle = "Idle";
        public const string Walk = "Walk";
        public const string Run = "Run";
        public const string Jump = "Jump";
        public const string Fall = "Fall";
        public const string Attack = "Attack";
        public const string Hurt = "Hurt";
        public const string Death = "Death";

        // Utility states
        public const string Any = "Any"; // Special state for transitions from any state
        public const string None = "";  // No state
    }
}
