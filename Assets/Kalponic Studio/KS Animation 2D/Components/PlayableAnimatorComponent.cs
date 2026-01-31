using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// MonoBehaviour wrapper for <see cref="PlayableAnimator"/> so teams can drive sprite animations via
    /// Playables without using Animator controllers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayableAnimatorComponent : MonoBehaviour, IAnimator, IAnimationPlayer, IAnimationDiagnostics
    {
        [Header("Playable Setup")]
        [Tooltip("SpriteRenderer to drive. If left empty, the component searches locally.")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Optional animator target for 3D rigs (not commonly used for sprites).")]
        [SerializeField] private Animator targetAnimator;
        [Tooltip("Character data that defines sprite animations.")]
        [SerializeField] private CharacterTypeSO characterType;

        [Header("Animation Mapping")]
        [Tooltip("Optional profile set that maps AnimationIds to state names.")]
        [SerializeField] private AnimationProfileSet profileSet;
        [Tooltip("Optional overrides for individual AnimationIds.")]
        [SerializeField] private AnimationProfile[] profiles;
        [Tooltip("Optional transition overrides (fade/speed/start) per AnimationId.")]
        [SerializeField] private AnimationTransitionAsset[] transitions;
        [Header("Events")]
        [SerializeField] private AnimationUnityEvents unityEvents = new AnimationUnityEvents();

        private PlayableAnimator playableAnimator;
        private Dictionary<AnimationId, AnimationProfile> profileMap;
        private readonly Dictionary<string, AnimationId> stateLookup = new Dictionary<string, AnimationId>();
        private Dictionary<AnimationId, AnimationTransitionAsset> transitionMap = new Dictionary<AnimationId, AnimationTransitionAsset>();
        private readonly Queue<string> transitionHistory = new Queue<string>();
        private const int TransitionHistoryLimit = 12;
        private AnimationId? activeId;

        public event System.Action<AnimationId> AnimationStarted;
        public event System.Action<AnimationId> AnimationCompleted;
        public event System.Action<AnimationId> AnimationLooped;

        public bool IsPaused => playableAnimator?.IsPaused ?? false;
        public bool IsPlayingProperty => playableAnimator?.IsPlayingProperty ?? false;
        public float CurrentSpeed
        {
            get => playableAnimator?.CurrentSpeed ?? 1f;
            set { if (playableAnimator != null) playableAnimator.CurrentSpeed = value; }
        }
        public string CurrentState => playableAnimator?.CurrentState ?? string.Empty;
        public AnimationClip Clip => playableAnimator?.Clip;
        public string ClipName => playableAnimator?.ClipName ?? string.Empty;
        public float NormalizedTime
        {
            get => playableAnimator?.NormalizedTime ?? 0f;
            set { if (playableAnimator != null) playableAnimator.NormalizedTime = value; }
        }
        public CharacterTypeSO CharacterType => characterType;
        public float Time
        {
            get => playableAnimator?.Time ?? 0f;
            set { if (playableAnimator != null) playableAnimator.Time = value; }
        }

        private void Awake()
        {
            CacheRenderer();
            InitializeProfileMap();
            InitializePlayableAnimator();
        }

        private void OnDestroy()
        {
            if (playableAnimator != null)
            {
                playableAnimator.onAnimationStart -= HandleAnimationStart;
                playableAnimator.onAnimationComplete -= HandleAnimationComplete;
                playableAnimator.onAnimationLoop -= HandleAnimationLoop;
                playableAnimator.Dispose();
            }
        }

        private void CacheRenderer()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void InitializeProfileMap()
        {
            profileMap = new Dictionary<AnimationId, AnimationProfile>();
            stateLookup.Clear();
            transitionMap.Clear();

            if (profileSet != null && profileSet.Profiles != null)
            {
                foreach (var profile in profileSet.Profiles)
                {
                    AddProfile(profile);
                }
            }

            if (profiles != null)
            {
                foreach (var profile in profiles)
                {
                    AddProfile(profile);
                }
            }

            if (transitions != null)
            {
                foreach (var transition in transitions)
                {
                    if (transition == null) continue;
                    transitionMap[transition.id] = transition;
                }
            }
        }

        private void AddProfile(AnimationProfile profile)
        {
            if (profile == null) return;
            profileMap[profile.id] = profile;
            var state = profile.ResolveStateName();
            if (!string.IsNullOrEmpty(state))
            {
                stateLookup[state] = profile.id;
            }
        }

        private void InitializePlayableAnimator()
        {
            if (spriteRenderer == null && targetAnimator == null)
            {
                KSAnimLog.Error($"{nameof(PlayableAnimatorComponent)}: No SpriteRenderer or Animator assigned.", "Playback", this);
                return;
            }

            playableAnimator = spriteRenderer != null
                ? new PlayableAnimator(spriteRenderer, characterType)
                : new PlayableAnimator(targetAnimator);

            if (characterType != null)
            {
                playableAnimator.SetCharacterType(characterType);
            }

            playableAnimator.onAnimationStart += HandleAnimationStart;
            playableAnimator.onAnimationComplete += HandleAnimationComplete;
            playableAnimator.onAnimationLoop += HandleAnimationLoop;
        }

        public void SetCharacterType(CharacterTypeSO newType)
        {
            characterType = newType;
            playableAnimator?.SetCharacterType(characterType);
        }

        public void Stop()
        {
            activeId = null;
            playableAnimator?.Stop();
        }

        public void Pause()
        {
            playableAnimator?.Pause();
        }

        public void Resume()
        {
            playableAnimator?.Resume();
        }

        public void SavePlaybackState()
        {
            playableAnimator?.SavePlaybackState();
        }

        public void RestorePlaybackState()
        {
            playableAnimator?.RestorePlaybackState();
        }

        public void PlayWithEvents(string stateName, Dictionary<int, System.Action> frameEvents)
        {
            playableAnimator?.PlayWithEvents(stateName, frameEvents);
        }

        public void PlayWithEvents(string stateName, Dictionary<float, System.Action> normalizedEvents)
        {
            playableAnimator?.PlayWithEvents(stateName, normalizedEvents);
        }

        public void Play(AnimationId id)
        {
            Play(id, -1f);
        }

        public void Play(AnimationType type)
        {
            Play(type, -1f);
        }

        void IAnimator.Play(AnimationType type, float speed, bool loop)
        {
            playableAnimator?.Play(type.ToStateName(), speed, loop);
        }

        public void Play(AnimationType type, float fadeOverride)
        {
            if (type.TryToAnimationId(out var id))
            {
                Play(id, fadeOverride);
            }
            else
            {
                KSAnimLog.Warn($"{nameof(PlayableAnimatorComponent)}: AnimationType '{type}' has no mapped AnimationId.", "Playback", this);
            }
        }

        public bool IsPlaying(string stateName)
        {
            if (stateLookup.TryGetValue(stateName, out var id))
            {
                return activeId == id;
            }
            return playableAnimator?.IsPlaying(stateName) ?? false;
        }

        public bool IsPlaying(AnimationType type)
        {
            return IsPlaying(type.ToStateName());
        }

        public void Play(string stateName)
        {
            if (stateLookup.TryGetValue(stateName, out var id))
            {
                Play(id);
            }
            else
            {
                RecordTransition($"Play {stateName}");
                playableAnimator?.Play(stateName);
            }
        }

        void IAnimator.Play(string stateName, float speed, bool loop)
        {
            playableAnimator?.Play(stateName, speed, loop);
        }

        void IAnimator.Play(string stateName, float speed, bool loop, bool pingPong)
        {
            playableAnimator?.Play(stateName, speed, loop);
        }

        public void Play(AnimationId id, float fadeOverride)
        {
            if (playableAnimator == null)
            {
                KSAnimLog.Warn($"{nameof(PlayableAnimatorComponent)}: PlayableAnimator not initialized.", "Playback", this);
                return;
            }

            if (!profileMap.TryGetValue(id, out var profile) || profile == null)
            {
                KSAnimLog.Warn($"{nameof(PlayableAnimatorComponent)}: No AnimationProfile for id '{id}'.", "Playback", this);
                return;
            }

            var stateName = profile.ResolveStateName();
            if (string.IsNullOrEmpty(stateName))
            {
                KSAnimLog.Warn($"{nameof(PlayableAnimatorComponent)}: AnimationProfile '{profile.name}' has no state name.", "Playback", this);
                return;
            }

            activeId = id;
            RecordTransition($"Play {id} ({stateName})");

            float fadeTime = ResolveFade(profile, fadeOverride);
            float speed = ResolveSpeed(profile.id);
            float startTime = ResolveStartTime(profile.id, profile.clip);

            if (fadeTime > 0f)
            {
                playableAnimator.CrossFade("", stateName, fadeTime);
            }
            else
            {
                playableAnimator.Play(stateName, speed, true);
            }

            if (startTime > 0f)
            {
                playableAnimator.SetTime(stateName, startTime);
            }
        }

        public void CrossFade(string fromState, string toState, float duration)
        {
            playableAnimator?.CrossFade(fromState, toState, duration);
        }

        public void Blend(string baseState, string blendState, float weight)
        {
            playableAnimator?.Blend(baseState, blendState, weight);
        }

        public IAnimationSequence Sequence()
        {
            return playableAnimator?.Sequence();
        }

        public void PlaySequence(IAnimationSequence sequence)
        {
            playableAnimator?.PlaySequence(sequence);
        }

        public void SetStateMachine(IAnimationStateMachine stateMachine)
        {
            playableAnimator?.SetStateMachine(stateMachine);
        }

        public void TransitionToState(string stateName)
        {
            playableAnimator?.TransitionToState(stateName);
        }

        public void TransitionToState(AnimationType type)
        {
            playableAnimator?.TransitionToState(type.ToStateName());
        }

        public Vector3 GetNodePosition(int nodeId, bool ignorePivot = false)
        {
            return playableAnimator?.GetNodePosition(nodeId, ignorePivot) ?? transform.position;
        }

        public float GetNodeAngle(int nodeId)
        {
            return playableAnimator?.GetNodeAngle(nodeId) ?? transform.eulerAngles.z;
        }

        public void SetUpdateFrequency(float updatesPerSecond)
        {
            playableAnimator?.SetUpdateFrequency(updatesPerSecond);
        }

        public void EnableLOD(bool enable, float maxDistance = 50f)
        {
            playableAnimator?.EnableLOD(enable, maxDistance);
        }

        private float ResolveFade(AnimationProfile profile, float fadeOverride)
        {
            if (fadeOverride >= 0f)
            {
                return fadeOverride;
            }

            if (profile != null && profile.fadeTime >= 0f)
            {
                return profile.fadeTime;
            }

            if (profileSet != null)
            {
                return profileSet.DefaultFade;
            }

            return 0.1f;
        }

        private float ResolveSpeed(AnimationId id)
        {
            if (transitionMap.TryGetValue(id, out var t) && t != null && t.speed >= 0f)
            {
                return t.speed;
            }
            return 1f;
        }

        private float ResolveStartTime(AnimationId id, AnimationClip clip)
        {
            if (!transitionMap.TryGetValue(id, out var t) || t == null) return 0f;
            if (t.normalizedStartTime < 0f) return 0f;
            if (clip == null) return 0f;
            return Mathf.Clamp01(t.normalizedStartTime) * clip.length;
        }

        private void HandleAnimationStart(string stateName)
        {
            if (TryGetId(stateName, out var id))
            {
                activeId = id;
                RecordTransition($"Start {id}");
                AnimationStarted?.Invoke(id);
                unityEvents?.onAnimationStart?.Invoke();
            }
        }

        private void HandleAnimationComplete(string stateName)
        {
            if (TryGetId(stateName, out var id))
            {
                RecordTransition($"Complete {id}");
                AnimationCompleted?.Invoke(id);
                unityEvents?.onAnimationEnd?.Invoke();
            }
        }

        private void HandleAnimationLoop(string stateName)
        {
            if (TryGetId(stateName, out var id))
            {
                RecordTransition($"Loop {id}");
                AnimationLooped?.Invoke(id);
                unityEvents?.onAnimationLoop?.Invoke();
            }
        }

        private bool TryGetId(string stateName, out AnimationId id)
        {
            return stateLookup.TryGetValue(stateName, out id);
        }

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
            if (playableAnimator is IAnimationDiagnostics diagnostics)
            {
                var snapshot = diagnostics.CaptureDebugSnapshot();
                return new AnimationDebugSnapshot(
                    activeId,
                    snapshot.StateName,
                    snapshot.ClipName,
                    snapshot.ClipTime,
                    snapshot.ClipLength,
                    snapshot.NormalizedTime,
                    snapshot.Speed,
                    snapshot.IsPlaying,
                    transitionHistory.ToArray(),
                    gameObject.name);
            }

            return AnimationDebugSnapshot.Inactive(gameObject.name, transitionHistory.ToArray());
        }
    }
}
