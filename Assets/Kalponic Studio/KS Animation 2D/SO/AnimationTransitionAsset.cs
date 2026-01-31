using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Optional transition overrides per AnimationId (fade, speed, start time).
    /// </summary>
    [CreateAssetMenu(menuName = "KS Animation 2D/Animation Transition")]
    public class AnimationTransitionAsset : ScriptableObject
    {
        public AnimationId id;
        [Tooltip("Override fade duration (seconds). Set negative to ignore.")]
        public float fade = -1f;
        [Tooltip("Override playback speed. Set negative to ignore.")]
        public float speed = -1f;
        [Tooltip("Optional normalized start time (0-1). Set negative to ignore.")]
        [Range(-1f, 1f)] public float normalizedStartTime = -1f;
    }
}
