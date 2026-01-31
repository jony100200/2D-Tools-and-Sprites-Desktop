using UnityEditor;
using UnityEngine;

namespace KalponicStudio.Editor
{
    /// <summary>
    /// Simple editor window for editing a HitboxCollisionMatrix asset in a grid view.
    /// </summary>
    public sealed class HitboxCollisionMatrixWindow : EditorWindow
    {
        private HitboxCollisionMatrix matrix;
        private SerializedObject serializedMatrix;
        private SerializedProperty rulesProp;
        private Vector2 scrollPos;

        [MenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Hitbox Collision Matrix")]
        public static void Open()
        {
            var window = GetWindow<HitboxCollisionMatrixWindow>();
            window.titleContent = new GUIContent("Hitbox Matrix");
            window.minSize = new Vector2(420, 200);
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.Space();
            matrix = (HitboxCollisionMatrix)EditorGUILayout.ObjectField("Matrix Asset", matrix, typeof(HitboxCollisionMatrix), false);

            if (matrix == null)
            {
                EditorGUILayout.HelpBox("Assign a HitboxCollisionMatrix asset (Create via Hub or Assets/Create).", MessageType.Info);
                return;
            }

            if (serializedMatrix == null || serializedMatrix.targetObject != matrix)
            {
                serializedMatrix = new SerializedObject(matrix);
                rulesProp = serializedMatrix.FindProperty("rules");
            }

            serializedMatrix.Update();

            var types = System.Enum.GetValues(typeof(HitboxType));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Allowed Interactions", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("", GUILayout.Width(90));
                foreach (HitboxType t in types)
                {
                    EditorGUILayout.LabelField(t.ToString(), GUILayout.Width(75));
                }
            }

            foreach (HitboxType row in types)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(row.ToString(), GUILayout.Width(90));
                    foreach (HitboxType col in types)
                    {
                        bool allow = matrix.Allows(row, col);
                        bool newAllow = GUILayout.Toggle(allow, GUIContent.none, GUILayout.Width(75));
                        if (newAllow != allow)
                        {
                            SetRule(row, col, newAllow);
                        }
                    }
                }
            }

            serializedMatrix.ApplyModifiedProperties();

            EditorGUILayout.EndScrollView();
        }

        private void SetRule(HitboxType src, HitboxType dst, bool allow)
        {
            // Try to find existing
            for (int i = 0; i < rulesProp.arraySize; i++)
            {
                var elem = rulesProp.GetArrayElementAtIndex(i);
                var s = elem.FindPropertyRelative("source");
                var t = elem.FindPropertyRelative("target");
                if ((HitboxType)s.enumValueIndex == src && (HitboxType)t.enumValueIndex == dst)
                {
                    elem.FindPropertyRelative("allow").boolValue = allow;
                    return;
                }
            }

            rulesProp.arraySize++;
            var newElem = rulesProp.GetArrayElementAtIndex(rulesProp.arraySize - 1);
            newElem.FindPropertyRelative("source").enumValueIndex = (int)src;
            newElem.FindPropertyRelative("target").enumValueIndex = (int)dst;
            newElem.FindPropertyRelative("allow").boolValue = allow;
        }
    }
}
