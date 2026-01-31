using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace KalponicStudio.Editor
{
    /// <summary>
    /// Generates AnimationProfile assets and a ProfileSet from an AnimatorController's states.
    /// Assumes state names match AnimationId values.
    /// </summary>
    public class KSAnimationProfileWizard : EditorWindow
    {
        private AnimatorController controller;
        private string outputFolder = "Assets/_Project/ScriptableObjects/AnimationProfiles/Generated";
        private DefaultAsset outputFolderAsset;
        private const string PREF_PROFILE_WIZARD_OUTPUT = "KSAP_ProfileWizard_Output";
        private Vector2 scrollPos;
        private string profileSetName = "GeneratedProfileSet";
        private float defaultFade = 0.1f;

        [MenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Profile Wizard")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<KSAnimationProfileWizard>();
            wnd.titleContent = new GUIContent("KS Profile Wizard");
            wnd.minSize = new Vector2(350, 200);
        }

        private void OnGUI()
        {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                EditorGUILayout.LabelField("Generate Animation Profiles from Animator", EditorStyles.boldLabel);
                controller = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", controller, typeof(AnimatorController), false);

                outputFolderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder (drag folder)", outputFolderAsset, typeof(DefaultAsset), false);
                if (outputFolderAsset != null)
                {
                    outputFolder = AssetDatabase.GetAssetPath(outputFolderAsset);
                    EditorPrefs.SetString(PREF_PROFILE_WIZARD_OUTPUT, outputFolder);
                }
                outputFolder = EditorGUILayout.TextField("Output Folder (path)", outputFolder);
            profileSetName = EditorGUILayout.TextField("Profile Set Name", profileSetName);
            defaultFade = EditorGUILayout.FloatField("Default Fade", defaultFade);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(controller == null))
            {
                if (GUILayout.Button("Generate Profiles + Set"))
                {
                    Generate();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void OnEnable()
        {
            string pref = EditorPrefs.GetString(PREF_PROFILE_WIZARD_OUTPUT, string.Empty);
            if (!string.IsNullOrEmpty(pref))
            {
                outputFolder = pref;
                outputFolderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(pref);
            }
        }

        private void Generate()
        {
            if (controller == null)
            {
                Debug.LogWarning("No AnimatorController assigned.");
                return;
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
                AssetDatabase.Refresh();
            }

            var profiles = new List<AnimationProfile>();

            foreach (var layer in controller.layers)
            {
                CollectStates(layer.stateMachine, layer.defaultWeight, profiles);
            }

            if (profiles.Count == 0)
            {
                Debug.LogWarning("No states found to generate profiles.");
                return;
            }

            // Save profiles
            foreach (var profile in profiles)
            {
                var assetPath = Path.Combine(outputFolder, $"{profile.name}.asset");
                AssetDatabase.CreateAsset(profile, assetPath);
            }

            // Create set
            var set = ScriptableObject.CreateInstance<AnimationProfileSet>();
            SetProfileSetDefaults(set, profiles.ToArray(), defaultFade);
            var setPath = Path.Combine(outputFolder, $"{profileSetName}.asset");
            AssetDatabase.CreateAsset(set, setPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Generated {profiles.Count} AnimationProfiles and AnimationProfileSet at {outputFolder}");
        }

        private void CollectStates(AnimatorStateMachine sm, float layerWeight, List<AnimationProfile> profiles)
        {
            foreach (var childState in sm.states)
            {
                var state = childState.state;
                var profile = ScriptableObject.CreateInstance<AnimationProfile>();
                profile.name = state.name;
                profile.stateName = state.name;
                profile.fadeTime = -1f; // use set default
                profile.layer = 0;
                profile.clip = state.motion as AnimationClip;

                if (Enum.TryParse(state.name, out AnimationId id))
                {
                    profile.id = id;
                    profiles.Add(profile);
                }
                else
                {
                    Debug.LogWarning($"State '{state.name}' does not match any AnimationId. Skipping profile generation for this state.");
                }
            }

            foreach (var childSM in sm.stateMachines)
            {
                CollectStates(childSM.stateMachine, layerWeight, profiles);
            }
        }

        private void SetProfileSetDefaults(AnimationProfileSet set, AnimationProfile[] profiles, float fade)
        {
            var so = new SerializedObject(set);
            so.FindProperty("profiles").arraySize = profiles.Length;
            for (int i = 0; i < profiles.Length; i++)
            {
                so.FindProperty("profiles").GetArrayElementAtIndex(i).objectReferenceValue = profiles[i];
            }
            so.FindProperty("defaultFade").floatValue = fade;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
