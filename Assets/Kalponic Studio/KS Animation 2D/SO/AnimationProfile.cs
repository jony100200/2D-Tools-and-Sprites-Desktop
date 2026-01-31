using UnityEngine;

namespace KalponicStudio
{
    [CreateAssetMenu(menuName = "KS Animation 2D/AnimationProfile")]
    public class AnimationProfile : ScriptableObject
    {
        public AnimationId id;
        public AnimationClip clip; // Direct clip for Playables-based playing
        [Tooltip("Optional string name used by sprite-based or PlayableAnimator workflows.")]
        public string stateName = string.Empty;
        [Tooltip("Fade duration in seconds. Use -1 to fallback to the profile set's default.")]
        public float fadeTime = -1f;
        public int layer = 0;
        public bool lockLayer = false; // Inspired by AnimatorCoder: prevents interruptions
        public AnimationId nextAnimation = AnimationId.Idle; // Optional next animation to chain
        public float nextDelay = 0f; // Delay before next animation (0 = no chaining)

        public string ResolveStateName()
        {
            if (!string.IsNullOrWhiteSpace(stateName))
            {
                return stateName;
            }

            return name;
        }
    }
}
