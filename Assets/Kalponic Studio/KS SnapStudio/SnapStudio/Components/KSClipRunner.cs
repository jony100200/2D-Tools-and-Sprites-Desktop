using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// KSClipRunner - Plays AnimationClips directly on an Animator using Unity's Playables API.
    /// No state machines or AnimatorControllers required. Just add clips to the list and play.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class KSClipRunner : MonoBehaviour
    {
        [Tooltip("Clips to capture, in order")]
        public List<AnimationClip> clips = new();

        [Range(0f, 1f)] public float crossFade = 0.15f;
        public bool loopCurrent = false;
        public float speed = 1f;

        // Playables graph components
        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;
        private AnimationPlayableOutput output;
        private int current = -1;

        void OnEnable()
        {
            if (clips.Count == 0) return;

            var animator = GetComponent<Animator>();
            graph = PlayableGraph.Create("KSClipRunner");
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            mixer = AnimationMixerPlayable.Create(graph, 2);
            output = AnimationPlayableOutput.Create(graph, "AnimOut", animator);
            output.SetSourcePlayable(mixer);

            graph.Play();
            PlayIndex(0, immediate: true);
        }

        void OnDisable()
        {
            if (graph.IsValid()) graph.Destroy();
        }

        /// <summary>
        /// Play a specific clip by index
        /// </summary>
        public void PlayIndex(int index, bool immediate = false)
        {
            if (index < 0 || index >= clips.Count) return;

            var nextPlayable = AnimationClipPlayable.Create(graph, clips[index]);
            nextPlayable.SetApplyFootIK(false);
            nextPlayable.SetSpeed(speed);
            nextPlayable.SetDuration(clips[index].length);
            nextPlayable.SetTime(0);

            // slot 0 = current, slot 1 = next
            var currPlayable = mixer.GetInputCount() > 0 ? mixer.GetInput(0) : Playable.Null;

            // ensure 2 inputs
            if (mixer.GetInputCount() < 2) mixer.SetInputCount(2);

            mixer.DisconnectInput(1);
            graph.Connect(nextPlayable, 0, mixer, 1);
            mixer.SetInputWeight(1, immediate ? 1f : 0f);

            if (immediate || !currPlayable.IsValid())
            {
                mixer.SetInputWeight(0, 0f);
                mixer.SetInputWeight(1, 1f);
            }
            else
            {
                // cross-fade
                StartCoroutine(CrossFadeRoutine());
            }

            current = index;
        }

        /// <summary>
        /// Crossfade between current and next clip
        /// </summary>
        private System.Collections.IEnumerator CrossFadeRoutine()
        {
            float t = 0f;
            while (t < crossFade)
            {
                t += Time.deltaTime;
                float w = Mathf.Clamp01(t / crossFade);
                mixer.SetInputWeight(0, 1f - w);
                mixer.SetInputWeight(1, w);
                yield return null;
            }
            mixer.SetInputWeight(0, 0f);
            mixer.SetInputWeight(1, 1f);
            // disconnect old playable to keep graph clean
            var old = mixer.GetInput(0);
            if (old.IsValid()) { mixer.DisconnectInput(0); old.Destroy(); }
            // move new playable to slot 0
            var cur = mixer.GetInput(1);
            mixer.DisconnectInput(1);
            graph.Connect(cur, 0, mixer, 0);
            mixer.SetInputWeight(0, 1f);
        }

        void Update()
        {
            // auto-advance when not looping
            if (!loopCurrent && current >= 0)
            {
                var cur = (AnimationClipPlayable)mixer.GetInput(0);
                if (cur.IsValid() && cur.GetTime() >= cur.GetAnimationClip().length)
                    Next();
            }
        }

        /// <summary>
        /// Play next clip in sequence
        /// </summary>
        public void Next() => PlayIndex((current + 1) % clips.Count);

        /// <summary>
        /// Play previous clip in sequence
        /// </summary>
        public void Prev() => PlayIndex((current - 1 + clips.Count) % clips.Count);

        /// <summary>
        /// Set playback speed
        /// </summary>
        public void SetSpeed(float s)
        {
            speed = s;
            if (mixer.GetInput(0).IsValid())
                ((AnimationClipPlayable)mixer.GetInput(0)).SetSpeed(s);
        }

        /// <summary>
        /// Get current clip being played
        /// </summary>
        public AnimationClip GetCurrentClip()
        {
            if (current >= 0 && current < clips.Count)
                return clips[current];
            return null;
        }

        /// <summary>
        /// Get current playback time
        /// </summary>
        public float GetCurrentTime()
        {
            if (mixer.GetInput(0).IsValid())
                return (float)((AnimationClipPlayable)mixer.GetInput(0)).GetTime();
            return 0f;
        }

        /// <summary>
        /// Check if currently playing
        /// </summary>
        public bool IsPlaying()
        {
            return graph.IsValid() && graph.IsPlaying();
        }

        /// <summary>
        /// Pause playback
        /// </summary>
        public void Pause()
        {
            if (graph.IsValid()) graph.Stop();
        }

        /// <summary>
        /// Resume playback
        /// </summary>
        public void Resume()
        {
            if (graph.IsValid()) graph.Play();
        }
    }
}
