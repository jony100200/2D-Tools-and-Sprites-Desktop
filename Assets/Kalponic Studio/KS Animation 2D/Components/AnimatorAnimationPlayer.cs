using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace KalponicStudio
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorAnimationPlayer : MonoBehaviour, IAnimationPlayer, IAnimationDiagnostics
    {
        [Header("Animation Setup")]
        [Tooltip("Reference to the Animator component.")]
        [SerializeField] private Animator animator = null;
        [Tooltip("Array of animation profiles mapping AnimationIds to clips.")]
        [SerializeField] private AnimationProfile[] profiles = null;
        [Tooltip("Optional ScriptableObject that bundles multiple profiles.")]
        [SerializeField] private AnimationProfileSet profileSet = null;
        [Tooltip("Optional transition overrides (fade/speed/start time) per AnimationId.")]
        [SerializeField] private AnimationTransitionAsset[] transitions = null;
        [Header("Events")]
        [SerializeField] private AnimationUnityEvents unityEvents = new AnimationUnityEvents();

        private Dictionary<AnimationId, AnimationProfile> profileMap;
        private Dictionary<AnimationId, AnimationTransitionAsset> transitionMap;
        private bool[] layerLocked; 
        private Coroutine[] nextAnimationCoroutines; // For chaining next animations

        // Playables for direct clip playing
        private PlayableGraph playableGraph;
        private AnimationLayerMixerPlayable layerMixer;
        private Dictionary<AnimationId, AnimationClipPlayable> clipPlayables = new Dictionary<AnimationId, AnimationClipPlayable>();
        private Dictionary<AnimationId, int> clipInputs = new Dictionary<AnimationId, int>();
        private int currentInputIndex = 0;

        // Fading support
        private AnimationClipPlayable currentClipPlayable;
        private int currentInput = -1;
        private AnimationClipPlayable fadingOutClip;
        private int fadingOutInput = -1;
        private float fadeDuration;
        private float fadeElapsed;
        private bool hasActiveAnimation;
        private AnimationId activeAnimationId;
        private float activeClipLength;
        private bool activeClipLoops;
        private float activeClipTime;
        private bool isPaused;
        private readonly Queue<string> transitionHistory = new Queue<string>();
        private const int TransitionHistoryLimit = 12;
        private AnimationId savedAnimationId;
        private float savedNormalizedTime;
        private bool hasSavedState;

        public event System.Action<AnimationId> AnimationStarted;
        public event System.Action<AnimationId> AnimationCompleted;
        public event System.Action<AnimationId> AnimationLooped;
        private readonly AnimationEventHub eventHub = new AnimationEventHub();
        private readonly IAnimationMapper mapper = new DefaultAnimationMapper();

        private void Awake()
        {
            CacheAnimator();
            InitializeProfileMap();
            InitializeLayerLocking();
        }

        private void Start()
        {
            InitializePlayables();
        }

        public void Pause()
        {
            if (isPaused || !playableGraph.IsValid()) return;
            isPaused = true;
            playableGraph.Stop();
        }

        public void Resume()
        {
            if (!isPaused || !playableGraph.IsValid()) return;
            isPaused = false;
            playableGraph.Play();
        }

        private void Update()
        {
            if (!playableGraph.IsValid()) return;
            if (isPaused) return;

            if (fadingOutClip.IsValid() && fadeDuration > 0f)
            {
                fadeElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(fadeElapsed / fadeDuration);

                if (fadingOutInput >= 0)
                {
                    layerMixer.SetInputWeight(fadingOutInput, 1f - t);
                }
                if (currentInput >= 0)
                {
                    layerMixer.SetInputWeight(currentInput, t);
                }

                if (t >= 1f)
                {
                    // Fade complete
                    fadingOutClip.Pause();
                    if (fadingOutInput >= 0)
                    {
                        layerMixer.SetInputWeight(fadingOutInput, 0f);
                    }
                    fadingOutClip = default;
                    fadingOutInput = -1;
                    fadeDuration = 0f;
                }
            }
        }

        private void OnValidate()
        {
            CacheAnimator();
        }

        public void SetProfiles(AnimationProfile[] newProfiles)
        {
            profiles = newProfiles;
            InitializeProfileMap();
            InitializePlayables();
        }

        public void SetProfileSet(AnimationProfileSet newSet)
        {
            profileSet = newSet;
            InitializeProfileMap();
            InitializePlayables();
        }

        private void CacheAnimator()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        private void InitializeProfileMap()
        {
            profileMap = new Dictionary<AnimationId, AnimationProfile>();
            transitionMap = new Dictionary<AnimationId, AnimationTransitionAsset>();

            if (profileSet != null && profileSet.Profiles != null)
            {
                foreach (var profile in profileSet.Profiles)
                {
                    AddProfile(profile, "profile set");
                }
            }

            if (profiles != null)
            {
                foreach (var profile in profiles)
                {
                    AddProfile(profile, "array");
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

        private void AddProfile(AnimationProfile profile, string source)
        {
            if (profile == null) return;
            if (profileMap.ContainsKey(profile.id))
            {
                KSAnimLog.Warn($"Duplicate AnimationId '{profile.id}' detected in {source}. Using the latest assignment.", "Playback", this);
            }
            profileMap[profile.id] = profile;
        }

        private void InitializeLayerLocking()
        {
            if (animator == null) return;
            int layerCount = animator.layerCount;
            layerLocked = new bool[layerCount];
            nextAnimationCoroutines = new Coroutine[layerCount];
        }

        private void InitializePlayables()
        {
            if (animator == null || profileMap == null || profileMap.Count == 0) return;

            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
            }

            playableGraph = PlayableGraph.Create("KSAnimationPlayer");
            var output = AnimationPlayableOutput.Create(playableGraph, "Output", animator);
            layerMixer = AnimationLayerMixerPlayable.Create(playableGraph, 1);
            output.SetSourcePlayable(layerMixer);

            foreach (var kvp in profileMap)
            {
                var profile = kvp.Value;
                if (profile.clip != null)
                {
                    var clipPlayable = AnimationClipPlayable.Create(playableGraph, profile.clip);
                    clipPlayables[kvp.Key] = clipPlayable;
                    // Don't connect yet, connect on play
                }
            }

            playableGraph.Play();
        }

        public void Play(AnimationId id)
        {
            Play(id, -1f);
        }

        public void Play(AnimationType type)
        {
            Play(type, -1f);
        }

        public void Play(AnimationType type, float fadeOverride)
        {
            if (!mapper.TryGetId(type, out var id))
            {
                KSAnimLog.Warn($"{nameof(AnimatorAnimationPlayer)}: AnimationType '{type}' has no mapped AnimationId.", "Playback", this);
                return;
            }
            Play(id, fadeOverride);
        }

        public void Play(AnimationId id, float fadeOverride = -1f)
        {
            if (profileMap == null || !profileMap.TryGetValue(id, out var profile))
            {
                KSAnimLog.Warn($"No AnimationProfile mapped for id '{id}'.", "Playback", this);
                return;
            }

            if (isPaused && playableGraph.IsValid())
            {
                isPaused = false;
                playableGraph.Play();
            }

            if (animator == null)
            {
                KSAnimLog.Warn("Animator reference missing.", "Playback", this);
                return;
            }

            if (!animator.enabled)
            {
                KSAnimLog.Warn("Animator is disabled.", "Playback", this);
                return;
            }

            if (layerLocked[profile.layer])
            {
                KSAnimLog.Info($"Layer {profile.layer} is locked, skipping play for '{id}'.", "Playback", this);
                RecordTransition($"Locked layer {profile.layer}, skipped {id}");
                return;
            }

            if (profile.clip == null)
            {
                KSAnimLog.Warn($"AnimationProfile '{profile.name}' has no clip assigned.", "Playback", this);
                return;
            }

            if (!clipPlayables.TryGetValue(id, out var clipPlayable))
            {
                KSAnimLog.Warn($"No playable for id '{id}'.", "Playback", this);
                return;
            }

            CompleteActiveAnimation();

            // Handle fading
            float fadeTime = ResolveFade(profile, fadeOverride, id);
            if (fadeTime > 0f && currentClipPlayable.IsValid())
            {
                // Start fading
                fadingOutClip = currentClipPlayable;
                fadingOutInput = currentInput;
                fadeDuration = fadeTime;
                fadeElapsed = 0f;
            }
            else
            {
                // Instant switch
                if (currentClipPlayable.IsValid())
                {
                    currentClipPlayable.Pause();
                    layerMixer.SetInputWeight(currentInput, 0f);
                }
                if (hasActiveAnimation)
                {
                    AnimationCompleted?.Invoke(activeAnimationId);
                    hasActiveAnimation = false;
                }
            }

            // Set up new clip
            if (currentInput == -1 || !clipInputs.ContainsKey(id))
            {
                // First time or new, connect
                currentInput = currentInputIndex;
                clipInputs[id] = currentInput;
                layerMixer.SetInputCount(currentInput + 1);
                playableGraph.Connect(clipPlayable, 0, layerMixer, currentInput);
                currentInputIndex++;
            }
            else
            {
                currentInput = clipInputs[id];
            }

            currentClipPlayable = clipPlayable;

            // Apply transition overrides: speed and start time
            if (transitionMap.TryGetValue(id, out var transition) && transition != null)
            {
                if (transition.speed >= 0f)
                {
                    clipPlayable.SetSpeed(transition.speed);
                }
                if (transition.normalizedStartTime >= 0f && profile.clip != null)
                {
                    float startTime = Mathf.Clamp01(transition.normalizedStartTime) * profile.clip.length;
                    clipPlayable.SetTime(startTime);
                }
            }
            else
            {
                clipPlayable.SetSpeed(1f);
                clipPlayable.SetTime(0f);
            }

            clipPlayable.Play();

            if (fadeTime <= 0f)
            {
                layerMixer.SetInputWeight(currentInput, 1f);
            }

            KSAnimLog.Playback(id, profile.clip.name, 0f, (float)clipPlayable.GetSpeed(), $"fade={fadeTime:0.00}", this);
            RecordTransition($"Play {id} (fade {fadeTime:0.00})");
            hasActiveAnimation = true;
            activeAnimationId = id;
            activeClipLength = profile.clip.length;
            activeClipLoops = profile.clip.isLooping;
            activeClipTime = 0f;
            AnimationStarted?.Invoke(id);
            unityEvents?.onAnimationStart?.Invoke();
            eventHub.RaiseStart(id);

            // Handle layer locking
            if (profile.lockLayer)
            {
                layerLocked[profile.layer] = true;
                // Unlock after animation finishes
                float unlockDelay = profile.clip.length;
                StartCoroutine(UnlockLayer(profile.layer, unlockDelay));
            }

            // Handle next animation chaining
            if (profile.nextDelay > 0f)
            {
                if (nextAnimationCoroutines[profile.layer] != null)
                {
                    StopCoroutine(nextAnimationCoroutines[profile.layer]);
                }
                nextAnimationCoroutines[profile.layer] = StartCoroutine(PlayNext(profile.nextAnimation, profile.nextDelay, profile.layer));
            }
        }

        public void SavePlaybackState()
        {
            if (!hasActiveAnimation || activeClipLength <= 0f)
            {
                hasSavedState = false;
                return;
            }

            savedAnimationId = activeAnimationId;
            savedNormalizedTime = Mathf.Clamp01(activeClipTime / activeClipLength);
            hasSavedState = true;
        }

        public void RestorePlaybackState()
        {
            if (!hasSavedState)
            {
                return;
            }

            Play(savedAnimationId);
            if (currentClipPlayable.IsValid() && activeClipLength > 0f)
            {
                float time = Mathf.Clamp01(savedNormalizedTime) * activeClipLength;
                currentClipPlayable.SetTime(time);
                activeClipTime = time;
            }
        }

        private IEnumerator UnlockLayer(int layer, float delay)
        {
            yield return new WaitForSeconds(delay);
            layerLocked[layer] = false;
        }

        private IEnumerator PlayNext(AnimationId nextId, float delay, int layer)
        {
            yield return new WaitForSeconds(delay);
            Play(nextId);
        }

        private void RecordTransition(string message)
        {
            transitionHistory.Enqueue(message);
            while (transitionHistory.Count > TransitionHistoryLimit)
            {
                transitionHistory.Dequeue();
            }
        }

        private float ResolveFade(AnimationProfile profile, float fadeOverride, AnimationId id)
        {
            if (fadeOverride >= 0f)
            {
                return fadeOverride;
            }

            if (profile != null && profile.fadeTime >= 0f)
            {
                return profile.fadeTime;
            }

            if (transitionMap.TryGetValue(id, out var transition) && transition != null && transition.fade >= 0f)
            {
                return transition.fade;
            }

            if (profileSet != null)
            {
                return profileSet.DefaultFade;
            }

            return 0.1f;
        }

        private void LateUpdate()
        {
            if (!hasActiveAnimation || !currentClipPlayable.IsValid())
            {
                return;
            }
            if (isPaused)
            {
                return;
            }

            if (activeClipLength <= 0f)
            {
                return;
            }

            float speed = Mathf.Abs((float)currentClipPlayable.GetSpeed());
            if (speed <= Mathf.Epsilon)
            {
                return;
            }

            activeClipTime += Time.deltaTime * speed;

            if (activeClipTime >= activeClipLength)
            {
                if (activeClipLoops)
                {
                    activeClipTime %= activeClipLength;
                    AnimationLooped?.Invoke(activeAnimationId);
                    unityEvents?.onAnimationLoop?.Invoke();
                    eventHub.RaiseLoop(activeAnimationId);
                    RecordTransition($"Loop {activeAnimationId}");
                }
                else
                {
                    AnimationCompleted?.Invoke(activeAnimationId);
                    hasActiveAnimation = false;
                    unityEvents?.onAnimationEnd?.Invoke();
                    eventHub.RaiseComplete(activeAnimationId);
                    RecordTransition($"Complete {activeAnimationId}");
                }
            }
        }

        private void CompleteActiveAnimation()
        {
            if (hasActiveAnimation)
            {
                AnimationCompleted?.Invoke(activeAnimationId);
                unityEvents?.onAnimationEnd?.Invoke();
                hasActiveAnimation = false;
                eventHub.RaiseComplete(activeAnimationId);
                RecordTransition($"Complete {activeAnimationId}");
            }
        }

        // Public methods for manual control
        public void SetLayerLocked(int layer, bool locked)
        {
            if (layer >= 0 && layer < layerLocked.Length)
            {
                layerLocked[layer] = locked;
            }
        }

        public bool IsLayerLocked(int layer)
        {
            return layer >= 0 && layer < layerLocked.Length && layerLocked[layer];
        }

        public AnimationEventHub Events => eventHub;

        public AnimationDebugSnapshot CaptureDebugSnapshot()
        {
            if (!hasActiveAnimation || !currentClipPlayable.IsValid())
            {
                return AnimationDebugSnapshot.Inactive(gameObject.name, transitionHistory.ToArray());
            }

            float clipTime = (float)currentClipPlayable.GetTime();
            float normTime = activeClipLength > 0f ? clipTime / activeClipLength : 0f;
            float speed = (float)currentClipPlayable.GetSpeed();
            string clipName = profileMap != null && profileMap.TryGetValue(activeAnimationId, out var profile) && profile.clip != null
                ? profile.clip.name
                : string.Empty;

            return new AnimationDebugSnapshot(
                activeAnimationId,
                activeAnimationId.ToString(),
                clipName,
                clipTime,
                activeClipLength,
                normTime,
                speed,
                hasActiveAnimation,
                transitionHistory.ToArray(),
                gameObject.name);
        }
    }
}
