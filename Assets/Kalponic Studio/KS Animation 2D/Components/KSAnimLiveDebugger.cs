using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Lightweight runtime overlay to visualize current animation id, normalized time, speed, and transition history.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class KSAnimLiveDebugger : MonoBehaviour
    {
        [Tooltip("Optional explicit reference to an animation diagnostic source. If left empty the component searches locally.")]
        [SerializeField] private MonoBehaviour diagnosticsSource;
        [Tooltip("Draw the overlay when in Play Mode.")]
        [SerializeField] private bool showInPlayMode = true;
        [Tooltip("Offset from the top-left of the screen for the overlay box.")]
        [SerializeField] private Vector2 overlayOffset = new Vector2(12f, 12f);
        [Tooltip("Maximum number of history entries to display.")]
        [SerializeField] private int historyLength = 8;

        private IAnimationDiagnostics diagnostics;
        private IAnimationPlayer playerEvents;
        private readonly Queue<string> transitions = new Queue<string>();

        private void Awake()
        {
            diagnostics = ResolveDiagnostics();
            playerEvents = ResolvePlayer();
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private IAnimationDiagnostics ResolveDiagnostics()
        {
            if (diagnosticsSource is IAnimationDiagnostics explicitDiagnostics)
            {
                return explicitDiagnostics;
            }

            return GetComponentInChildren<IAnimationDiagnostics>(true);
        }

        private IAnimationPlayer ResolvePlayer()
        {
            if (diagnosticsSource is IAnimationPlayer explicitPlayer)
            {
                return explicitPlayer;
            }

            return GetComponentInChildren<IAnimationPlayer>(true);
        }

        private void Subscribe()
        {
            if (playerEvents == null) return;
            playerEvents.AnimationStarted += OnStarted;
            playerEvents.AnimationCompleted += OnCompleted;
            playerEvents.AnimationLooped += OnLooped;
        }

        private void Unsubscribe()
        {
            if (playerEvents == null) return;
            playerEvents.AnimationStarted -= OnStarted;
            playerEvents.AnimationCompleted -= OnCompleted;
            playerEvents.AnimationLooped -= OnLooped;
        }

        private void OnStarted(AnimationId id) => Enqueue($"Start {id}");
        private void OnCompleted(AnimationId id) => Enqueue($"Complete {id}");
        private void OnLooped(AnimationId id) => Enqueue($"Loop {id}");

        private void Enqueue(string text)
        {
            transitions.Enqueue(text);
            while (transitions.Count > historyLength)
            {
                transitions.Dequeue();
            }
        }

        private void OnGUI()
        {
            if (!showInPlayMode || !Application.isPlaying)
            {
                return;
            }

            if (diagnostics == null)
            {
                diagnostics = ResolveDiagnostics();
            }

            if (diagnostics == null)
            {
                return;
            }

            var snapshot = diagnostics.CaptureDebugSnapshot();
            var rect = new Rect(overlayOffset.x, overlayOffset.y, 340f, 180f);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label($"Source: {snapshot.Source}");
            GUILayout.Label($"Playing: {(snapshot.IsPlaying ? "Yes" : "No")}");
            GUILayout.Label($"Animation: {snapshot.Id?.ToString() ?? snapshot.StateName ?? "None"}");
            GUILayout.Label($"Clip: {snapshot.ClipName}");
            GUILayout.Label($"Time: {snapshot.ClipTime:0.00}/{snapshot.ClipLength:0.00}  (norm {snapshot.NormalizedTime:0.00})");
            GUILayout.Label($"Speed: {snapshot.Speed:0.00}");

            if (transitions.Count > 0 || (snapshot.TransitionHistory?.Count ?? 0) > 0)
            {
                GUILayout.Space(6f);
                GUILayout.Label("History:");
                foreach (var entry in transitions)
                {
                    GUILayout.Label("- " + entry);
                }

                if (snapshot.TransitionHistory != null)
                {
                    foreach (var entry in snapshot.TransitionHistory)
                    {
                        GUILayout.Label("- " + entry);
                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}
