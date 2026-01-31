using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Utility class for sampling AnimationClips from GameObjects and AnimatorControllers
    /// Supports recursive searching through BlendTrees and sub-state machines
    /// </summary>
    public static class ClipSampler
    {
        /// <summary>
        /// Get all AnimationClips from a GameObject
        /// </summary>
        public static AnimationClip[] GetClips(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("ClipSampler: Target GameObject is null");
                return new AnimationClip[0];
            }

            var clips = new List<AnimationClip>();

            // Try to get clips from AnimatorController first
            var animator = target.GetComponentInChildren<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                var controllerClips = GetClipsFromController(animator.runtimeAnimatorController);
                clips.AddRange(controllerClips);
                Debug.Log($"ClipSampler: Found {controllerClips.Length} clips from AnimatorController '{animator.runtimeAnimatorController.name}'");
            }

            // Also check for Animation component
            var animation = target.GetComponentInChildren<Animation>();
            if (animation != null)
            {
                foreach (AnimationState state in animation)
                {
                    if (state.clip != null && !clips.Contains(state.clip))
                    {
                        clips.Add(state.clip);
                        Debug.Log($"ClipSampler: Found clip '{state.clip.name}' from Animation component");
                    }
                }
            }

            // Remove duplicates
            var uniqueClips = new List<AnimationClip>();
            foreach (var clip in clips)
            {
                if (!uniqueClips.Contains(clip))
                {
                    uniqueClips.Add(clip);
                }
            }

            Debug.Log($"ClipSampler: Total unique clips found: {uniqueClips.Count}");
            return uniqueClips.ToArray();
        }

        /// <summary>
        /// Get all AnimationClips from an AnimatorController, including those in BlendTrees and sub-state machines
        /// </summary>
        public static AnimationClip[] GetClipsFromController(RuntimeAnimatorController controller)
        {
            if (controller == null)
            {
                Debug.LogWarning("ClipSampler: Controller is null");
                return new AnimationClip[0];
            }

            var clips = new List<AnimationClip>();

            if (controller is UnityEditor.Animations.AnimatorController animatorController)
            {
                Debug.Log($"ClipSampler: Processing AnimatorController '{animatorController.name}' with {animatorController.layers.Length} layers");

                foreach (var layer in animatorController.layers)
                {
                    if (layer.stateMachine != null)
                    {
                        ExtractClipsFromStateMachine(layer.stateMachine, clips);
                    }
                }
            }

            // Remove duplicates
            var uniqueClips = new List<AnimationClip>();
            foreach (var clip in clips)
            {
                if (clip != null && !uniqueClips.Contains(clip))
                {
                    uniqueClips.Add(clip);
                }
            }

            Debug.Log($"ClipSampler: Found {uniqueClips.Count} unique clips from controller '{controller.name}'");
            return uniqueClips.ToArray();
        }

        /// <summary>
        /// Recursively extract AnimationClips from a state machine
        /// </summary>
        private static void ExtractClipsFromStateMachine(UnityEditor.Animations.AnimatorStateMachine stateMachine, List<AnimationClip> clips)
        {
            if (stateMachine == null) return;

            Debug.Log($"ClipSampler: Processing state machine '{stateMachine.name}' with {stateMachine.states.Length} states and {stateMachine.stateMachines.Length} sub-state machines");

            // Process all states in this state machine
            foreach (var childState in stateMachine.states)
            {
                var state = childState.state;
                if (state != null && state.motion != null)
                {
                    ExtractClipsFromMotion(state.motion, clips);
                }
            }

            // Process sub-state machines recursively
            foreach (var childStateMachine in stateMachine.stateMachines)
            {
                if (childStateMachine.stateMachine != null)
                {
                    ExtractClipsFromStateMachine(childStateMachine.stateMachine, clips);
                }
            }
        }

        /// <summary>
        /// Extract AnimationClips from a motion (AnimationClip, BlendTree, etc.)
        /// </summary>
        private static void ExtractClipsFromMotion(Motion motion, List<AnimationClip> clips)
        {
            if (motion == null) return;

            if (motion is AnimationClip clip)
            {
                if (!clips.Contains(clip))
                {
                    clips.Add(clip);
                    Debug.Log($"ClipSampler: Found AnimationClip '{clip.name}' (length: {clip.length:F2}s)");
                }
            }
            else if (motion is UnityEditor.Animations.BlendTree blendTree)
            {
                Debug.Log($"ClipSampler: Processing BlendTree '{blendTree.name}' with {blendTree.children.Length} children");

                // Process all children of the blend tree
                foreach (var child in blendTree.children)
                {
                    if (child.motion != null)
                    {
                        ExtractClipsFromMotion(child.motion, clips);
                    }
                }
            }
            else
            {
                Debug.Log($"ClipSampler: Unhandled motion type: {motion.GetType().Name}");
            }
        }
    }
}