using UnityEditor;
using UnityEngine;

namespace KalponicStudio.Editor
{
    [CustomEditor(typeof(PlayableAnimatorComponent))]
    public sealed class PlayableAnimatorComponentEditor : UnityEditor.Editor
    {
        private SerializedProperty spriteRendererProp;
        private SerializedProperty targetAnimatorProp;
        private SerializedProperty characterTypeProp;
        private SerializedProperty profileSetProp;
        private SerializedProperty profilesProp;
        private SerializedProperty transitionsProp;

        private void OnEnable()
        {
            spriteRendererProp = serializedObject.FindProperty("spriteRenderer");
            targetAnimatorProp = serializedObject.FindProperty("targetAnimator");
            characterTypeProp = serializedObject.FindProperty("characterType");
            profileSetProp = serializedObject.FindProperty("profileSet");
            profilesProp = serializedObject.FindProperty("profiles");
            transitionsProp = serializedObject.FindProperty("transitions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawValidation();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            DrawTestButtons();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidation()
        {
            bool missingRenderer = spriteRendererProp.objectReferenceValue == null && targetAnimatorProp.objectReferenceValue == null;
            if (missingRenderer)
            {
                EditorGUILayout.HelpBox("Assign a SpriteRenderer (2D) or Animator (3D) target.", MessageType.Warning);
            }

            if (characterTypeProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("CharacterTypeSO is not assigned. Profiles cannot resolve sprite states.", MessageType.Info);
            }

            bool hasProfiles = (profileSetProp.objectReferenceValue != null) ||
                               (profilesProp != null && profilesProp.arraySize > 0);
            if (!hasProfiles)
            {
                EditorGUILayout.HelpBox("No AnimationProfileSet or overrides assigned. Playback may warn for missing ids.", MessageType.Warning);
            }
        }

        private void DrawTestButtons()
        {
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Quick Play (Play Mode)", EditorStyles.boldLabel);
                var pac = target as PlayableAnimatorComponent;
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Play Idle"))
                    {
                        pac.Play(AnimationId.Idle);
                    }
                    if (GUILayout.Button("Play Walk"))
                    {
                        pac.Play(AnimationId.Walk);
                    }
                    if (GUILayout.Button("Play Jump"))
                    {
                        pac.Play(AnimationId.Jump);
                    }
                }
            }
        }
    }
}
