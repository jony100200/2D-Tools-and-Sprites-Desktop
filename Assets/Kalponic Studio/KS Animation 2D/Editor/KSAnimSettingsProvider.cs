using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KalponicStudio
{
    /// <summary>
    /// Project Settings entry for KS Animation 2D logging/diagnostics toggles.
    /// </summary>
    public sealed class KSAnimSettingsProvider : SettingsProvider
    {
        private const string PrefEnabled = "KSAnim2D.Logging.Enabled";
        private const string PrefStackTrace = "KSAnim2D.Logging.StackTrace";
        private const string PrefPlayback = "KSAnim2D.Logging.Playback";
        private const string PrefFsm = "KSAnim2D.Logging.FSM";
        private const string PrefDiagnostics = "KSAnim2D.Logging.Diagnostics";

        public KSAnimSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new KSAnimSettingsProvider("Project/KS Animation 2D", SettingsScope.Project);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            KSAnimLog.Enabled = EditorPrefs.GetBool(PrefEnabled, KSAnimLog.Enabled);
            KSAnimLog.IncludeStackTrace = EditorPrefs.GetBool(PrefStackTrace, false);
            KSAnimLog.SetCategoryEnabled("Playback", EditorPrefs.GetBool(PrefPlayback, true));
            KSAnimLog.SetCategoryEnabled("FSM", EditorPrefs.GetBool(PrefFsm, true));
            KSAnimLog.SetCategoryEnabled("Diagnostics", EditorPrefs.GetBool(PrefDiagnostics, true));
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUI.BeginChangeCheck();

            bool enabled = EditorGUILayout.Toggle("Enable Logging", KSAnimLog.Enabled);
            bool stack = EditorGUILayout.Toggle("Include Stack Trace", KSAnimLog.IncludeStackTrace);
            bool playback = EditorGUILayout.Toggle("Playback Logs", KSAnimLog.IsCategoryEnabled("Playback"));
            bool fsm = EditorGUILayout.Toggle("FSM Logs", KSAnimLog.IsCategoryEnabled("FSM"));
            bool diagnostics = EditorGUILayout.Toggle("Diagnostics Logs", KSAnimLog.IsCategoryEnabled("Diagnostics"));

            if (EditorGUI.EndChangeCheck())
            {
                KSAnimLog.Enabled = enabled;
                KSAnimLog.IncludeStackTrace = stack;
                KSAnimLog.SetCategoryEnabled("Playback", playback);
                KSAnimLog.SetCategoryEnabled("FSM", fsm);
                KSAnimLog.SetCategoryEnabled("Diagnostics", diagnostics);

                EditorPrefs.SetBool(PrefEnabled, enabled);
                EditorPrefs.SetBool(PrefStackTrace, stack);
                EditorPrefs.SetBool(PrefPlayback, playback);
                EditorPrefs.SetBool(PrefFsm, fsm);
                EditorPrefs.SetBool(PrefDiagnostics, diagnostics);
            }

            EditorGUILayout.HelpBox("Logging defaults to enabled in Editor builds and disabled in release builds. Use the toggles above to override per-project.", MessageType.Info);
        }
    }
}
