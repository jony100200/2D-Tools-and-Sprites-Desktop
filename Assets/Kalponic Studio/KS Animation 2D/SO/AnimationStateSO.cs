using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// ScriptableObject for individual animation states
    /// Replaces hardcoded state definitions with modular, reusable assets
    /// </summary>
    [CreateAssetMenu(menuName = "KS Animation 2D/Animation State", fileName = "NewAnimationState")]
    public class AnimationStateSO : ScriptableObject
    {
        [System.Serializable]
        public struct AnimationEventData
        {
            public float normalizedTime;
            public string functionName;
            public string stringParameter;
        }

        [System.Serializable]
        public class HitboxFrameData
        {
            public HitboxType type = HitboxType.Hit;
            public Vector2 offset = Vector2.zero;
            public Vector2 size = Vector2.one;
            [Header("Attack Metadata")]
            public float damage = 0f;
            public Vector2 knockback = Vector2.zero;
            public float hitstun = 0f;
            [Header("FX / Projectile")]
            public Vector2 projectileOrigin = Vector2.zero;
            public string fxId = "";
        }

        [System.Serializable]
        public class AttackDefaults
        {
            public float damage = 0f;
            public float poiseDamage = 0f;
            public Vector2 knockback = Vector2.zero;
            public float hitstun = 0f;
            public string fxId = "";
        }

        [System.Serializable]
        public class FrameHitboxSet
        {
            public List<HitboxFrameData> hitboxes = new List<HitboxFrameData>();
        }

        [Header("State Identity")]
        public string stateName = "Idle";
        public AnimationStatePriority priority = AnimationStatePriority.Medium;
        public float transitionDuration = 0.1f;
        public bool canBeInterrupted = true;

        [Header("Animation Data")]
        public Sprite[] sprites; // Array of sprites for frame-by-frame animation
        public float frameRate = 12f; // Frames per second
        public bool loop = true; // Whether animation loops
        // Removed AnimationClip - use direct sprite arrays for 2D animation
        public float defaultFadeDuration = 0.1f; // Default cross-fade time
        [Tooltip("Optional per-frame durations (seconds). If empty or counts mismatch, falls back to frameRate.")]
        public List<float> frameDurations = new List<float>();
        [Tooltip("Optional UnityEvents fired when a specific frame becomes active. Size should match sprites.")]
        public List<UnityEngine.Events.UnityEvent> frameEvents = new List<UnityEngine.Events.UnityEvent>();
        [Tooltip("Optional hitboxes per frame (position/size/type in local units). Size matches sprites.")]
        public List<FrameHitboxSet> frameHitboxes = new List<FrameHitboxSet>();
        [Header("Attack Defaults (used if hitbox metadata is empty)")]
        public AttackDefaults attackDefaults = new AttackDefaults();

        [Header("State Logic")]
        public System.Func<bool> customCondition;
        public UnityEngine.Events.UnityEvent onStateEntered;
        public UnityEngine.Events.UnityEvent onStateExited;
        [Tooltip("Fired the first time this state loops (if looping).")]
        public UnityEngine.Events.UnityEvent onLoopOnce;
        [Tooltip("When enabled, onLoopOnce fires on the first loop iteration.")]
        public bool invokeOnFirstLoop = true;
        [Header("Events")]
        public List<AnimationEventData> events = new List<AnimationEventData>();

        [Header("Metadata")]
        [TextArea] public string description = "Animation state description";

        // Quick validation
        public bool IsValid => !string.IsNullOrEmpty(stateName) && (sprites != null && sprites.Length > 0);
    }
}
