using UnityEngine;
using UnityEditor;

namespace ABS
{
    public class ModelEditor : Editor
    {
        protected bool DrawGroundPivotField(Model model, out bool isChanged)
        {
            EditorGUI.BeginChangeCheck();
            bool isGroundPivot = EditorGUILayout.Toggle("Ground Pivot", model.isGroundPivot);
            isChanged = EditorGUI.EndChangeCheck();

            return isGroundPivot;
        }

        protected string DrawModelNameSuffix(Model model, out bool isChanged)
        {
            EditorGUI.BeginChangeCheck();
            string nameSuffix = EditorGUILayout.TextField("Name Suffix", model.nameSuffix);
            isChanged = EditorGUI.EndChangeCheck();

            return nameSuffix;
        }

        protected void AddToModelList(Model model)
        {
#if UNITY_6000_0_OR_NEWER
            Studio studio = FindFirstObjectByType<Studio>();
#else
            Studio studio = FindObjectOfType<Studio>();
#endif
            if (studio == null)
                return;

            studio.AddModel(model);
        }
    }
}
