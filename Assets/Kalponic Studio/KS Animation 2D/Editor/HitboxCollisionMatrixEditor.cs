using UnityEditor;
using UnityEngine;

namespace KalponicStudio.Editor
{
    [CustomEditor(typeof(HitboxCollisionMatrix))]
    public sealed class HitboxCollisionMatrixEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var matrix = (HitboxCollisionMatrix)target;
            var rulesProp = serializedObject.FindProperty("rules");

            var types = System.Enum.GetValues(typeof(HitboxType));
            EditorGUILayout.LabelField("Collision Matrix", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Toggle which hitbox type pairs can interact.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(80));
            foreach (HitboxType t in types)
            {
                EditorGUILayout.LabelField(t.ToString(), GUILayout.Width(70));
            }
            EditorGUILayout.EndHorizontal();

            foreach (HitboxType row in types)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(row.ToString(), GUILayout.Width(80));
                foreach (HitboxType col in types)
                {
                    bool allow = matrix.Allows(row, col);
                    bool newAllow = GUILayout.Toggle(allow, GUIContent.none, GUILayout.Width(70));
                    if (newAllow != allow)
                    {
                        SetRule(rulesProp, row, col, newAllow);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SetRule(SerializedProperty rulesProp, HitboxType src, HitboxType dst, bool allow)
        {
            // Try to find existing rule
            for (int i = 0; i < rulesProp.arraySize; i++)
            {
                var element = rulesProp.GetArrayElementAtIndex(i);
                var sourceProp = element.FindPropertyRelative("source");
                var targetProp = element.FindPropertyRelative("target");
                if ((HitboxType)sourceProp.enumValueIndex == src && (HitboxType)targetProp.enumValueIndex == dst)
                {
                    element.FindPropertyRelative("allow").boolValue = allow;
                    return;
                }
            }

            // Add new
            rulesProp.arraySize++;
            var newElem = rulesProp.GetArrayElementAtIndex(rulesProp.arraySize - 1);
            newElem.FindPropertyRelative("source").enumValueIndex = (int)src;
            newElem.FindPropertyRelative("target").enumValueIndex = (int)dst;
            newElem.FindPropertyRelative("allow").boolValue = allow;
        }
    }
}
