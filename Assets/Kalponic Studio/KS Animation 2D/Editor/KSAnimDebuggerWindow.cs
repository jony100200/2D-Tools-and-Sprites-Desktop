using UnityEditor;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Editor window that mirrors the runtime live debugger, useful for inspecting selected objects.
    /// </summary>
    public sealed class KSAnimDebuggerWindow : EditorWindow
    {
        private GameObject inspected;
        private Vector2 scrollPos;
        private IAnimationDiagnostics diagnostics;

        [MenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Live Debugger")]
        private static void Open()
        {
            var window = GetWindow<KSAnimDebuggerWindow>();
            window.titleContent = new GUIContent("KS Anim Debugger");
            window.Show();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.update += Repaint;
            OnSelectionChanged();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.update -= Repaint;
        }

        private void OnSelectionChanged()
        {
            inspected = Selection.activeGameObject;
            diagnostics = inspected != null ? inspected.GetComponentInChildren<IAnimationDiagnostics>() : null;
            Repaint();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            inspected = (GameObject)EditorGUILayout.ObjectField("Inspected GameObject (optional)", inspected, typeof(GameObject), true);
            if (inspected == null)
            {
                inspected = Selection.activeGameObject;
            }

            if (diagnostics == null)
            {
                EditorGUILayout.HelpBox("Select a GameObject with an IAnimationDiagnostics implementer (AnimatorAnimationPlayer, PlayableAnimatorComponent, etc.).", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            var snapshot = diagnostics.CaptureDebugSnapshot();
            EditorGUILayout.LabelField("Source", snapshot.Source);
            EditorGUILayout.LabelField("Playing", snapshot.IsPlaying ? "Yes" : "No");
            EditorGUILayout.LabelField("Animation", snapshot.Id?.ToString() ?? snapshot.StateName ?? "None");
            EditorGUILayout.LabelField("Clip", snapshot.ClipName);
            EditorGUILayout.LabelField("Time", $"{snapshot.ClipTime:0.00}/{snapshot.ClipLength:0.00} (norm {snapshot.NormalizedTime:0.00})");
            EditorGUILayout.LabelField("Speed", snapshot.Speed.ToString("0.00"));

            // Show state event info when possible
            var pac = inspected != null ? inspected.GetComponentInChildren<PlayableAnimatorComponent>() : null;
            if (pac != null && pac.CharacterType != null && !string.IsNullOrEmpty(pac.CurrentState))
            {
                var state = pac.CharacterType.GetState(pac.CurrentState);
                int frameEventCount = state?.frameEvents?.Count ?? 0;
                bool hasLoopOnce = state != null && state.invokeOnFirstLoop && state.onLoopOnce != null;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("State Events", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Per-frame events", frameEventCount.ToString());
                EditorGUILayout.LabelField("Loop-once callback", hasLoopOnce ? "Yes" : "No");
            }

            if (snapshot.TransitionHistory != null && snapshot.TransitionHistory.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Transitions");
                foreach (var entry in snapshot.TransitionHistory)
                {
                    EditorGUILayout.LabelField("- " + entry);
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
