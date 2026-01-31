using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace KalponicStudio.Editor
{
    /// <summary>
    /// Integrated character editor to manage CharacterTypeSO animations, attack defaults, and hitbox summaries.
    /// </summary>
    public sealed class CharacterEditorWindow : EditorWindow
    {
        private CharacterTypeSO character;
        private Vector2 leftScroll;
        private Vector2 rightScroll;
        private int selectedIndex = -1;

        [MenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Character Editor")]
        public static void Open()
        {
            var window = GetWindow<CharacterEditorWindow>();
            window.titleContent = new GUIContent("KS Character Editor");
            window.minSize = new Vector2(700, 480);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            character = (CharacterTypeSO)EditorGUILayout.ObjectField("Character", character, typeof(CharacterTypeSO), false);
            if (character == null)
            {
                EditorGUILayout.HelpBox("Assign a CharacterTypeSO to edit.", MessageType.Info);
                return;
            }

            var states = character.AllStates;
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeft(states);
                DrawRight(states);
            }
        }

        private void DrawLeft(List<AnimationStateSO> states)
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(leftScroll, GUILayout.Width(240)))
            {
                leftScroll = scroll.scrollPosition;
                EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
                for (int i = 0; i < states.Count; i++)
                {
                    var st = states[i];
                    if (st == null) continue;
                    bool selected = i == selectedIndex;
                    if (GUILayout.Toggle(selected, st.stateName, "Button"))
                    {
                        selectedIndex = i;
                    }
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("Open Sprite Animation Editor"))
                {
                    SpriteAnimationEditorWindow.ShowWindow();
                }
                if (GUILayout.Button("Open Hitbox Matrix"))
                {
                    HitboxCollisionMatrixWindow.Open();
                }
            }
        }

        private void DrawRight(List<AnimationStateSO> states)
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(rightScroll, GUILayout.ExpandHeight(true)))
            {
                rightScroll = scroll.scrollPosition;
                if (selectedIndex < 0 || selectedIndex >= states.Count || states[selectedIndex] == null)
                {
                    EditorGUILayout.HelpBox("Select an animation to view details.", MessageType.Info);
                    return;
                }

                var st = states[selectedIndex];
                var so = new SerializedObject(st);

                EditorGUILayout.LabelField($"{st.stateName} Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(so.FindProperty("priority"));
                EditorGUILayout.PropertyField(so.FindProperty("loop"));
                EditorGUILayout.PropertyField(so.FindProperty("frameRate"));
                EditorGUILayout.PropertyField(so.FindProperty("defaultFadeDuration"));

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Attack Defaults", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(so.FindProperty("attackDefaults"), true);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Hitbox Summary", EditorStyles.boldLabel);
                var hitboxesProp = so.FindProperty("frameHitboxes");
                if (hitboxesProp != null && st.sprites != null)
                {
                    int totalFrames = st.sprites.Length;
                    EditorGUILayout.LabelField($"Frames: {totalFrames}");
                    int totalBoxes = 0;
                    for (int i = 0; i < hitboxesProp.arraySize; i++)
                    {
                        var setProp = hitboxesProp.GetArrayElementAtIndex(i).FindPropertyRelative("hitboxes");
                        totalBoxes += setProp != null ? setProp.arraySize : 0;
                    }
                    EditorGUILayout.LabelField($"Total Hitboxes: {totalBoxes}");
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Events Summary", EditorStyles.boldLabel);
                var eventsProp = so.FindProperty("events");
                EditorGUILayout.LabelField($"Per-frame UnityEvents: {(eventsProp != null ? eventsProp.arraySize : 0)}");
                var frameEventsProp = so.FindProperty("frameEvents");
                EditorGUILayout.LabelField($"Frame UnityEvent slots: {(frameEventsProp != null ? frameEventsProp.arraySize : 0)}");

                so.ApplyModifiedProperties();
            }
        }
    }
}
