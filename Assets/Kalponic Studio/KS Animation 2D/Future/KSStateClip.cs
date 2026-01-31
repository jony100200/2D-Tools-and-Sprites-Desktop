using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace KalponicStudio
{
    /// <summary>
    /// Timeline clip for KS Animation 2D states
    /// Allows designers to place animation states directly on Timeline tracks
    /// </summary>
    [System.Serializable]
    public class KSStateClip : PlayableAsset, ITimelineClipAsset
    {
        [Header("Animation State")]
        public string stateName = "Idle";
        public float speed = 1f;
        public bool loop = true;

        [Header("Transition")]
        public float fadeInDuration = 0.1f;
        public float fadeOutDuration = 0.1f;

        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.ClipIn | ClipCaps.SpeedMultiplier;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<KSStateBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            behaviour.StateName = stateName;
            behaviour.Speed = speed;
            behaviour.Loop = loop;
            behaviour.FadeInDuration = fadeInDuration;
            behaviour.FadeOutDuration = fadeOutDuration;

            // Try to find animator on the owner
            var animator = owner.GetComponent<IAnimator>();
            if (animator != null)
            {
                behaviour.Animator = animator;
            }

            return playable;
        }
    }

    /// <summary>
    /// Behaviour for KS State Timeline clips
    /// </summary>
    public class KSStateBehaviour : PlayableBehaviour
    {
        public string StateName;
        public float Speed = 1f;
        public bool Loop = true;
        public float FadeInDuration = 0.1f;
        public float FadeOutDuration = 0.1f;

        public IAnimator Animator;

        private bool isPlaying;
        private double startTime;
        private double endTime;

        public override void OnPlayableCreate(Playable playable)
        {
            startTime = playable.GetTime();
            endTime = startTime + playable.GetDuration();
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (Animator != null && !string.IsNullOrEmpty(StateName))
            {
                if (FadeInDuration > 0)
                {
                    // For Timeline, we use Play instead of CrossFade since we're not transitioning between states
                    Animator.Play(StateName, Speed, Loop);
                }
                else
                {
                    Animator.Play(StateName, Speed, Loop);
                }
                isPlaying = true;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // Optional: could fade out here
            isPlaying = false;
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            // Could handle speed changes or other dynamic adjustments here
            if (isPlaying && Animator != null)
            {
                float currentSpeed = (float)playable.GetSpeed() * Speed;
                if (Mathf.Abs(Animator.CurrentSpeed - currentSpeed) > 0.01f)
                {
                    Animator.CurrentSpeed = currentSpeed;
                }
            }
        }
    }

    /// <summary>
    /// Timeline track for KS Animation 2D
    /// </summary>
    [TrackColor(0.2f, 0.8f, 0.2f)]
    [TrackClipType(typeof(KSStateClip))]
    [TrackBindingType(typeof(IAnimator))]
    public class KSStateTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<KSStateMixerBehaviour>.Create(graph, inputCount);
            var behaviour = mixer.GetBehaviour();

            // Get the bound animator
            var animator = go.GetComponent<IAnimator>();
            behaviour.Animator = animator;

            return mixer;
        }
    }

    /// <summary>
    /// Mixer behaviour for KS State tracks
    /// Handles blending between multiple animation clips
    /// </summary>
    public class KSStateMixerBehaviour : PlayableBehaviour
    {
        public IAnimator Animator;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (Animator == null) return;

            int inputCount = playable.GetInputCount();

            // Find the clip with the highest weight
            int maxWeightIndex = -1;
            float maxWeight = 0f;

            for (int i = 0; i < inputCount; i++)
            {
                float weight = playable.GetInputWeight(i);
                if (weight > maxWeight)
                {
                    maxWeight = weight;
                    maxWeightIndex = i;
                }
            }

            // If we have a dominant clip, ensure it's playing
            if (maxWeightIndex >= 0 && maxWeight > 0.5f)
            {
                var inputPlayable = (ScriptPlayable<KSStateBehaviour>)playable.GetInput(maxWeightIndex);
                var behaviour = inputPlayable.GetBehaviour();

                if (!Animator.IsPlaying(behaviour.StateName))
                {
                    if (behaviour.FadeInDuration > 0)
                    {
                        // For Timeline, we use Play instead of CrossFade since we're not transitioning between states
                        Animator.Play(behaviour.StateName, behaviour.Speed, behaviour.Loop);
                    }
                    else
                    {
                        Animator.Play(behaviour.StateName, behaviour.Speed, behaviour.Loop);
                    }
                }
            }
        }
    }
}
