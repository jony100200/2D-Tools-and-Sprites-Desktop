using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KalponicStudio.Editor
{
    [CustomEditor(typeof(AnimatorAnimationPlayer))]
    public class AnimatorAnimationPlayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var player = (AnimatorAnimationPlayer)target;
            var profileSet = GetProfileSet(player);
            var profiles = GetProfiles(player, profileSet);

            if (profiles.Count == 0)
            {
                EditorGUILayout.HelpBox("No AnimationProfiles assigned. Add a Profile Set or populate Profiles.", MessageType.Warning);
                return;
            }

            var duplicateIds = FindDuplicates(profiles);
            if (duplicateIds.Count > 0)
            {
                EditorGUILayout.HelpBox("Duplicate AnimationId entries: " + string.Join(", ", duplicateIds), MessageType.Warning);
            }

            var missingClipProfiles = profiles.Where(p => p != null && p.clip == null).ToList();
            if (missingClipProfiles.Count > 0)
            {
                EditorGUILayout.HelpBox("Profiles missing clips: " + string.Join(", ", missingClipProfiles.Select(p => p.name)), MessageType.Warning);
            }

            if (profileSet != null)
            {
                var missingIds = FindMissingIds(profiles);
                if (missingIds.Count > 0)
                {
                    EditorGUILayout.HelpBox("Profile Set is missing AnimationIds: " + string.Join(", ", missingIds), MessageType.Info);
                }
            }
        }

        private AnimationProfileSet GetProfileSet(AnimatorAnimationPlayer player)
        {
            var so = new SerializedObject(player);
            var prop = so.FindProperty("profileSet");
            return prop != null ? prop.objectReferenceValue as AnimationProfileSet : null;
        }

        private List<AnimationProfile> GetProfiles(AnimatorAnimationPlayer player, AnimationProfileSet set)
        {
            var list = new List<AnimationProfile>();
            if (set != null && set.Profiles != null)
            {
                list.AddRange(set.Profiles.Where(p => p != null));
            }

            var so = new SerializedObject(player);
            var profilesProp = so.FindProperty("profiles");
            if (profilesProp != null)
            {
                for (int i = 0; i < profilesProp.arraySize; i++)
                {
                    var element = profilesProp.GetArrayElementAtIndex(i);
                    if (element != null && element.objectReferenceValue is AnimationProfile profile && profile != null)
                    {
                        list.Add(profile);
                    }
                }
            }

            return list;
        }

        private List<AnimationId> FindDuplicates(List<AnimationProfile> profiles)
        {
            return profiles
                .Where(p => p != null)
                .GroupBy(p => p.id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
        }

        private List<AnimationId> FindMissingIds(List<AnimationProfile> profiles)
        {
            var covered = new HashSet<AnimationId>(profiles.Where(p => p != null).Select(p => p.id));
            var missing = new List<AnimationId>();
            foreach (AnimationId id in System.Enum.GetValues(typeof(AnimationId)))
            {
                if (!covered.Contains(id))
                {
                    missing.Add(id);
                }
            }
            return missing;
        }
    }
}
