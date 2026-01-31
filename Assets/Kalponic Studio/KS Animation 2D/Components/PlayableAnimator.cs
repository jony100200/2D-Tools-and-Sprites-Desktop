using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace KalponicStudio
{
    /// <summary>
    /// Playables-based animator implementation
    /// Provides deterministic, code-driven animation with mixing, layering, and blending
    /// </summary>
    public sealed class PlayableAnimator : IAnimator, IAnimationDiagnostics
    {
        private PlayableGraph graph;
        private AnimationLayerMixerPlayable layerMixer;
        private AnimationMixerPlayable baseMixer;
        private readonly Dictionary<string, int> stateSlots = new Dictionary<string, int>();
        private readonly List<Playable> statePlayables = new List<Playable>();
        private CharacterTypeSO characterType; // Made non-readonly to allow updates
        private readonly Dictionary<string, AnimationStateSO> stateCache = new Dictionary<string, AnimationStateSO>();
        private readonly HashSet<string> loopOnceFired = new HashSet<string>();
        
        // Component references
        private readonly SpriteRenderer spriteRenderer;
        private readonly Animator animator;
        
        // State tracking
        private string currentState = "";
        private float currentSpeed = 1f;
        private bool isPlaying = false;
        private readonly Queue<string> transitionHistory = new Queue<string>();
        private const int TransitionHistoryLimit = 12;
        private string savedState = "";
        private float savedNormalizedTime = 0f;
        private bool hasSavedState;
        private bool isPaused = false;
        
        // Time control
        private float timeOffset = 0f;
        private float currentTime = 0f;
        
        // Events
        private Dictionary<int, System.Action> currentFrameEvents;
        private Dictionary<float, System.Action> currentNormalizedEvents;
        
        // Events
        public System.Action<string> onAnimationStart;
        public System.Action<string> onAnimationComplete;
        public System.Action<string> onAnimationLoop;
        private readonly AnimationEventHub eventHub = new AnimationEventHub();
        private readonly IAnimationMapper mapper = new DefaultAnimationMapper();
        private AnimationId? activeId;

        public PlayableAnimator(SpriteRenderer spriteRenderer, CharacterTypeSO characterType = null)
        {
            this.spriteRenderer = spriteRenderer;
            this.characterType = characterType;
            InitializeGraph();
        }

        public PlayableAnimator(Animator animator)
        {
            this.animator = animator;
            InitializeGraph();
        }

        private void InitializeGraph()
        {
            KSAnimLog.Info($"InitializeGraph: spriteRenderer={spriteRenderer?.name ?? "null"}, animator={animator?.name ?? "null"}", "Diagnostics");

            graph = PlayableGraph.Create("KSAnimation2D");

            // Create layer mixer (base + upper body + additive)
            layerMixer = AnimationLayerMixerPlayable.Create(graph, 3);

            // Create base mixer for state blending
            baseMixer = AnimationMixerPlayable.Create(graph, 8);
            graph.Connect(baseMixer, 0, layerMixer, 0);

            // Create output
            if (spriteRenderer != null)
            {
                // For 2D sprites, we'll use a custom output
                var output = ScriptPlayableOutput.Create(graph, "SpriteOutput");
                output.SetSourcePlayable(layerMixer, 0);
                KSAnimLog.Info("Created ScriptPlayableOutput for sprite animation", "Diagnostics");
                // Note: We'll handle sprite rendering in PrepareFrame of individual playables
            }
            else if (animator != null)
            {
                var output = AnimationPlayableOutput.Create(graph, "AnimationOutput", animator);
                output.SetSourcePlayable(layerMixer, 0);
                KSAnimLog.Info("Created AnimationPlayableOutput for 3D animation", "Diagnostics");
            }

            graph.Play();
            KSAnimLog.Info("Graph initialized and playing", "Diagnostics");
        }

        private int EnsureSlot(string stateName)
        {
            if (!stateSlots.TryGetValue(stateName, out int slot))
            {
                slot = statePlayables.Count;
                stateSlots[stateName] = slot;

                // Create a ScriptPlayable for sprite animation
                var scriptPlayable = ScriptPlayable<SpriteFrameBehaviour>.Create(graph);
                var behaviour = scriptPlayable.GetBehaviour();
                behaviour.StateName = stateName;
                behaviour.Renderer = spriteRenderer;
                behaviour.OnFrameChanged = OnFrameChanged;
                behaviour.OnComplete = () => HandleComplete(stateName);
                behaviour.OnLoop = () => HandleLoop(stateName);

                statePlayables.Add(scriptPlayable);
                graph.Connect(scriptPlayable, 0, baseMixer, slot);

                KSAnimLog.Info($"Created slot {slot} for {stateName}, connected to baseMixer", "Diagnostics");
            }
            return slot;
        }

        private void OnFrameChanged(string stateName, int frame)
        {
            var state = GetAnimationStateSO(stateName);
            if (state == null) return;

            if (state.frameEvents != null && frame >= 0 && frame < state.frameEvents.Count)
            {
                try
                {
                    state.frameEvents[frame]?.Invoke();
                }
                catch (System.Exception ex)
                {
                    KSAnimLog.Warn($"Frame event exception in state {stateName} frame {frame}: {ex.Message}", "Events");
                }
            }
        }

        private AnimationId? TryMapStateToId(string stateName)
        {
            if (Enum.TryParse(stateName, out AnimationId id))
            {
                return id;
            }
            return null;
        }

        // IAnimator implementation
        public void Play(AnimationType type, float speed = 1f, bool loop = true)
        {
            if (mapper.TryGetId(type, out var id))
            {
                activeId = id;
            }
            else
            {
                activeId = null;
            }
            var state = mapper.GetStateName(type);
            Play(state, speed, loop);
        }

        public void Play(string stateName, float speed = 1f, bool loop = true)
        {
            KSAnimLog.Info($"Play {stateName}, characterType: {characterType?.name ?? "null"}", "Playback");

            int slot = EnsureSlot(stateName);
            var playable = statePlayables[slot];
            var behaviour = ((ScriptPlayable<SpriteFrameBehaviour>)playable).GetBehaviour();

            // Set animation data (would be loaded from AnimationStateSO or similar)
            // For now, this is a placeholder - you'd load the actual sprite data here
            behaviour.Frames = GetFramesForState(stateName);
            behaviour.Fps = GetFrameRateForState(stateName);
            behaviour.Loop = loop;

            KSAnimLog.Info($"{stateName} has {behaviour.Frames?.Length ?? 0} frames, FPS: {behaviour.Fps}", "Playback");

            // Set weights for immediate transition
            for (int i = 0; i < baseMixer.GetInputCount(); i++)
            {
                baseMixer.SetInputWeight(i, i == slot ? 1f : 0f);
            }

            currentState = stateName;
            currentSpeed = speed;
            currentTime = 0f;
            timeOffset = 0f;
            isPlaying = true;
            isPaused = false;
            RecordTransition($"Play {stateName}");
            activeId = TryMapStateToId(stateName);
            loopOnceFired.Remove(stateName);
            
            // Clear events
            currentFrameEvents = null;
            currentNormalizedEvents = null;

            playable.SetSpeed(speed);
            playable.SetTime(0f);

            onAnimationStart?.Invoke(stateName);
            if (activeId.HasValue)
            {
                eventHub.RaiseStart(activeId.Value);
            }
        }

        private void HandleComplete(string stateName)
        {
            onAnimationComplete?.Invoke(stateName);
            if (activeId.HasValue && currentState == stateName) eventHub.RaiseComplete(activeId.Value);
            RecordTransition($"Complete {stateName}");
        }

        private void HandleLoop(string stateName)
        {
            var state = GetAnimationStateSO(stateName);
            if (state != null && state.invokeOnFirstLoop && !loopOnceFired.Contains(stateName))
            {
                try
                {
                    state.onLoopOnce?.Invoke();
                }
                catch (System.Exception ex)
                {
                    KSAnimLog.Warn($"Loop-once event exception in state {stateName}: {ex.Message}", "Events");
                }
                loopOnceFired.Add(stateName);
            }

            onAnimationLoop?.Invoke(stateName);
            if (activeId.HasValue && currentState == stateName) eventHub.RaiseLoop(activeId.Value);
            RecordTransition($"Loop {stateName}");
        }

        public void CrossFade(string fromState, string toState, float duration)
        {
            // For now, ignore fromState and duration, just play toState
            Play(toState);
        }

        public void Stop()
        {
            isPlaying = false;
            // Zero all weights
            for (int i = 0; i < baseMixer.GetInputCount(); i++)
            {
                baseMixer.SetInputWeight(i, 0f);
            }
        }

        public void Pause()
        {
            isPaused = true;
            graph.Stop();
        }

        public void Resume()
        {
            isPaused = false;
            graph.Play();
        }

        public bool IsPlaying(string stateName)
        {
            return isPlaying && !isPaused && currentState == stateName;
        }

        public bool IsPlaying(AnimationType type)
        {
            return IsPlaying(type.ToStateName());
        }

        public bool IsPlayingProperty => isPlaying && !isPaused;

        public float CurrentSpeed
        {
            get => currentSpeed;
            set
            {
                currentSpeed = Mathf.Max(0f, value);
                if (!string.IsNullOrEmpty(currentState) && stateSlots.TryGetValue(currentState, out int slot))
                {
                    statePlayables[slot].SetSpeed(currentSpeed);
                }
            }
        }

        public string CurrentState => currentState;

        // Time control properties (Phase 1)
        public float Time 
        { 
            get => currentTime + timeOffset;
            set 
            {
                timeOffset = value - currentTime;
                if (!string.IsNullOrEmpty(currentState) && stateSlots.TryGetValue(currentState, out int slot))
                {
                    statePlayables[slot].SetTime(Mathf.Max(0f, value));
                }
            }
        }

        public float NormalizedTime 
        { 
            get 
            {
                if (string.IsNullOrEmpty(currentState) || !stateSlots.TryGetValue(currentState, out int slot))
                    return 0f;
                var playable = statePlayables[slot];
                var behaviour = ((ScriptPlayable<SpriteFrameBehaviour>)playable).GetBehaviour();
                if (behaviour.Frames == null || behaviour.Frames.Length == 0) return 0f;
                return ((currentTime + timeOffset) / (behaviour.Frames.Length / behaviour.Fps)) % 1f;
            }
            set 
            {
                if (string.IsNullOrEmpty(currentState) || !stateSlots.TryGetValue(currentState, out int slot))
                    return;
                var playable = statePlayables[slot];
                var behaviour = ((ScriptPlayable<SpriteFrameBehaviour>)playable).GetBehaviour();
                if (behaviour.Frames == null || behaviour.Frames.Length == 0) return;
                float totalDuration = behaviour.Frames.Length / behaviour.Fps;
                float targetTime = (value % 1f) * totalDuration;
                timeOffset = targetTime - currentTime;
                playable.SetTime(targetTime);
            }
        }

        // State queries (Phase 1)
        public bool IsPaused => isPaused;

        public AnimationClip Clip => null; // Playables-based, no single AnimationClip

        public string ClipName => currentState;

        // IAnimator implementation - Advanced methods
        public void Play(string stateName, float speed, bool loop, bool pingPong)
        {
            // For now, ignore pingPong and use basic play
            Play(stateName, speed, loop);
        }

        public void SetStateMachine(IAnimationStateMachine stateMachine)
        {
            // Store reference for state machine integration
            // Implementation would depend on how the state machine calls back
        }

        public void TransitionToState(string stateName)
        {
            Play(stateName);
        }

        public void TransitionToState(AnimationType type)
        {
            Play(type.ToStateName());
        }

        public void SetTime(string stateName, float timeSeconds)
        {
            if (stateSlots.TryGetValue(stateName, out int slot) && slot < statePlayables.Count)
            {
                statePlayables[slot].SetTime(timeSeconds);
                currentTime = timeSeconds;
                timeOffset = 0f;
            }
        }

        public void SavePlaybackState()
        {
            savedState = currentState;
            savedNormalizedTime = NormalizedTime;
            hasSavedState = !string.IsNullOrEmpty(savedState);
        }

        public void RestorePlaybackState()
        {
            if (!hasSavedState)
            {
                return;
            }

            Play(savedState, currentSpeed, true);
            NormalizedTime = savedNormalizedTime;
        }

        public void PlayWithEvents(string stateName, Dictionary<int, System.Action> frameEvents)
        {
            Play(stateName);
            currentFrameEvents = frameEvents;
            
            // Set up frame callback
            if (!string.IsNullOrEmpty(stateName) && stateSlots.TryGetValue(stateName, out int slot))
            {
                var playable = statePlayables[slot];
                var behaviour = ((ScriptPlayable<SpriteFrameBehaviour>)playable).GetBehaviour();
                behaviour.OnFrameChanged = (name, frame) => 
                {
                    if (currentFrameEvents != null && currentFrameEvents.TryGetValue(frame, out var action))
                    {
                        action?.Invoke();
                    }
                };
            }
        }

        public void PlayWithEvents(string stateName, Dictionary<float, System.Action> normalizedEvents)
        {
            Play(stateName);
            currentNormalizedEvents = normalizedEvents;
            
            // For normalized events, we'll need to check in Update or use a different approach
            // For now, this is a simplified implementation
        }

        // Animation nodes (Phase 2) - stub implementations
        public Vector3 GetNodePosition(int nodeId, bool ignorePivot = false)
        {
            // TODO: Implement when nodes are added
            return spriteRenderer?.transform.position ?? Vector3.zero;
        }

        public float GetNodeAngle(int nodeId)
        {
            // TODO: Implement when nodes are added
            return spriteRenderer?.transform.eulerAngles.z ?? 0f;
        }

        public void SetUpdateFrequency(float updatesPerSecond)
        {
            // Simplified - would adjust update rate for performance
        }

        public void EnableLOD(bool enable, float maxDistance = 50f)
        {
            // Simplified - would disable updates when far from camera
        }

        // Advanced methods
        public void Blend(string baseState, string blendState, float weight)
        {
            int baseSlot = EnsureSlot(baseState);
            int blendSlot = EnsureSlot(blendState);

            // Set blend weights
            for (int i = 0; i < baseMixer.GetInputCount(); i++)
            {
                if (i == baseSlot)
                    baseMixer.SetInputWeight(i, 1f - weight);
                else if (i == blendSlot)
                    baseMixer.SetInputWeight(i, weight);
                else
                    baseMixer.SetInputWeight(i, 0f);
            }
        }

        public IAnimationSequence Sequence()
        {
            return new PlayableSequence(this);
        }

        public void PlaySequence(IAnimationSequence sequence)
        {
            sequence.Execute(this);
        }

        public AnimationEventHub Events => eventHub;

        private void RecordTransition(string message)
        {
            transitionHistory.Enqueue(message);
            while (transitionHistory.Count > TransitionHistoryLimit)
            {
                transitionHistory.Dequeue();
            }
        }

        public AnimationDebugSnapshot CaptureDebugSnapshot()
        {
            if (string.IsNullOrEmpty(currentState))
            {
                return AnimationDebugSnapshot.Inactive("PlayableAnimator", transitionHistory.ToArray());
            }

            float clipTime = 0f;
            float clipLength = 0f;
            float normalizedTime = 0f;
            float speed = currentSpeed;

            if (stateSlots.TryGetValue(currentState, out int slot) && slot < statePlayables.Count)
            {
                var playable = statePlayables[slot];
                var behaviour = ((ScriptPlayable<SpriteFrameBehaviour>)playable).GetBehaviour();
                clipTime = (float)playable.GetTime();
                clipLength = behaviour != null && behaviour.Fps > 0 && behaviour.Frames != null
                    ? behaviour.Frames.Length / behaviour.Fps
                    : 0f;
                normalizedTime = behaviour?.GetNormalizedTime() ?? 0f;
                speed = (float)playable.GetSpeed();
            }

            string source = spriteRenderer != null
                ? spriteRenderer.name
                : animator != null
                    ? animator.name
                    : "PlayableAnimator";

            return new AnimationDebugSnapshot(
                null,
                currentState,
                currentState,
                clipTime,
                clipLength,
                normalizedTime,
                speed,
                isPlaying,
                transitionHistory.ToArray(),
                source);
        }

        // Utility methods
        private Sprite[] GetFramesForState(string stateName)
        {
            var stateSO = GetAnimationStateSO(stateName);
            var frames = stateSO?.sprites ?? new Sprite[0];
            KSAnimLog.Info($"GetFramesForState: {stateName} -> stateSO: {stateSO?.name ?? "null"}, frames: {frames.Length}", "Diagnostics");
            return frames;
        }

        private float GetFrameRateForState(string stateName)
        {
            var stateSO = GetAnimationStateSO(stateName);
            return stateSO?.frameRate ?? 12f;
        }

        private AnimationStateSO GetAnimationStateSO(string stateName)
        {
            if (stateCache.TryGetValue(stateName, out var cached))
            {
                return cached;
            }

            if (characterType != null)
            {
                var state = characterType.GetState(stateName);
                if (state != null)
                {
                    stateCache[stateName] = state;
                    KSAnimLog.Info($"Found {stateName} in characterType {characterType.name}", "Diagnostics");
                    return state;
                }
                else
                {
                    KSAnimLog.Error($"State '{stateName}' not found in characterType {characterType.name}", "Playback");
                }
            }
            else
            {
                KSAnimLog.Error($"Character type is null, cannot find state '{stateName}'", "Playback");
            }

            return null;
        }

        // Public method to update character type
        public void SetCharacterType(CharacterTypeSO newCharacterType)
        {
            if (newCharacterType != characterType)
            {
                characterType = newCharacterType;
                // Clear cache when character type changes
                stateCache.Clear();
                // Note: In a full implementation, you might want to recreate playables
                // For now, we'll just update the reference and clear cache
            }
        }

        private string GetStateName(int slot)
        {
            foreach (var kvp in stateSlots)
            {
                if (kvp.Value == slot) return kvp.Key;
            }
            return "";
        }

        // Cleanup
        public void Dispose()
        {
            if (graph.IsValid())
            {
                graph.Destroy();
            }
        }

        ~PlayableAnimator()
        {
            Dispose();
        }
    }
}
