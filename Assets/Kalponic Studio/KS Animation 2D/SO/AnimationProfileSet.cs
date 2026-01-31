using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Bundles multiple <see cref="AnimationProfile"/> assets so characters can be configured with a single reference.
    /// </summary>
    [CreateAssetMenu(menuName = "KS Animation 2D/Animation Profile Set")]
    public class AnimationProfileSet : ScriptableObject
    {
        [SerializeField] private AnimationProfile[] profiles = { };
        [SerializeField, Tooltip("Fallback fade duration used when a profile leaves fadeTime at -1.")]
        private float defaultFade = 0.1f;
        [SerializeField, Tooltip("Validate that every AnimationId has a profile and warn about duplicates.")]
        private bool validateAllIds = true;

        private Dictionary<AnimationId, AnimationProfile> lookup;

        public IReadOnlyList<AnimationProfile> Profiles => profiles;
        public float DefaultFade => defaultFade;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void OnValidate()
        {
            BuildLookup();
            if (validateAllIds)
            {
                ValidateProfiles();
            }
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<AnimationId, AnimationProfile>();
            if (profiles == null) return;

            foreach (var profile in profiles)
            {
                if (profile == null) continue;
                if (lookup.ContainsKey(profile.id))
                {
                    KSAnimLog.Warn($"AnimationProfileSet '{name}' contains duplicate id '{profile.id}'. Using the last occurrence.", "Playback", this);
                }
                lookup[profile.id] = profile;
            }
        }

        public bool TryGetProfile(AnimationId id, out AnimationProfile profile)
        {
            if (lookup == null)
            {
                BuildLookup();
            }
            return lookup.TryGetValue(id, out profile);
        }

        public void ValidateProfiles()
        {
            if (lookup == null)
            {
                BuildLookup();
            }

            if (lookup == null) return;

            var missing = new List<AnimationId>();
            foreach (AnimationId id in System.Enum.GetValues(typeof(AnimationId)))
            {
                if (!lookup.ContainsKey(id))
                {
                    missing.Add(id);
                }
            }

            if (missing.Count > 0)
            {
                KSAnimLog.Warn($"AnimationProfileSet '{name}' is missing profiles for: {string.Join(", ", missing)}", "Playback", this);
            }

            foreach (var pair in lookup)
            {
                var profile = pair.Value;
                if (profile == null) continue;
                if (profile.clip == null)
                {
                    KSAnimLog.Warn($"AnimationProfileSet '{name}' profile '{profile.name}' for id '{pair.Key}' has no clip assigned.", "Playback", profile);
                }
            }
        }
    }
}
