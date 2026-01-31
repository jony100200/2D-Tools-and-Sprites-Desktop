using UnityEditor;

namespace ABS
{
    public class ConfigurationWindow : EditorWindow
    {
        public const string GPU_USE_KEY = "GpuUse";

        [MenuItem("Assets/" + EngineConstants.PROJECT_NAME + "/Configuration")]
        public static void Init()
        {
            ConfigurationWindow window = GetWindow<ConfigurationWindow>("Configuration");
            window.Show();
        }

        void OnEnable()
        {
            UpdateGlobalVariables();   
        }

        public static void UpdateGlobalVariables()
        {
            EngineConstants.gpuUse = EditorPrefs.GetBool(GPU_USE_KEY, false);
        }

        void OnGUI()
        {
#if UNITY_6000_0_OR_NEWER
            Studio studio = FindFirstObjectByType<Studio>();
#else
            Studio studio = FindObjectOfType<Studio>();
#endif
            if (studio == null)
            {
                EditorGUILayout.HelpBox("No Studio object", MessageType.Error);
                return;
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EngineConstants.gpuUse = EditorGUILayout.Toggle("Use GPU", EngineConstants.gpuUse);

                if (check.changed)
                    EditorPrefs.SetBool(GPU_USE_KEY, EngineConstants.gpuUse);
            }
        }
    }
}
