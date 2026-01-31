using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// ClipSampler - Samples animation clips at specific times for thumbnail generation.
    /// Handles frame extraction, pose sampling, and keyframe detection.
    /// </summary>
    public static class ClipSampler
    {
        /// <summary>
        /// Sample a clip at specific normalized time (0-1)
        /// </summary>
        public static void SampleClipAtTime(AnimationClip clip, float normalizedTime, GameObject target)
        {
            if (clip == null || target == null) return;

            var time = normalizedTime * clip.length;
            SampleClipAtAbsoluteTime(clip, time, target);
        }

        /// <summary>
        /// Sample a clip at absolute time
        /// </summary>
        public static void SampleClipAtAbsoluteTime(AnimationClip clip, float time, GameObject target)
        {
            if (clip == null || target == null) return;

            // Clamp time to valid range
            time = Mathf.Clamp(time, 0f, clip.length);

            // Sample the animation
            clip.SampleAnimation(target, time);
        }

        /// <summary>
        /// Get keyframe times from an animation clip
        /// </summary>
        public static float[] GetKeyframeTimes(AnimationClip clip)
        {
            if (clip == null) return new float[0];

            var times = new HashSet<float>();

            // Get keyframes from all curves
            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                foreach (var keyframe in curve.keys)
                {
                    times.Add(keyframe.time);
                }
            }

            return times.OrderBy(t => t).ToArray();
        }

        /// <summary>
        /// Sample clip at all keyframes
        /// </summary>
        public static PoseData[] SampleAtKeyframes(AnimationClip clip, GameObject target)
        {
            var keyframeTimes = GetKeyframeTimes(clip);
            var poses = new List<PoseData>();

            foreach (var time in keyframeTimes)
            {
                SampleClipAtAbsoluteTime(clip, time, target);
                poses.Add(CapturePose(target, time));
            }

            return poses.ToArray();
        }

        /// <summary>
        /// Sample clip at regular intervals
        /// </summary>
        public static PoseData[] SampleAtIntervals(AnimationClip clip, GameObject target, int frameCount)
        {
            if (frameCount <= 1) frameCount = 2;

            var poses = new List<PoseData>();
            var interval = clip.length / (frameCount - 1);

            for (int i = 0; i < frameCount; i++)
            {
                var time = i * interval;
                SampleClipAtAbsoluteTime(clip, time, target);
                poses.Add(CapturePose(target, time));
            }

            return poses.ToArray();
        }

        /// <summary>
        /// Find the most interesting pose in a clip (heuristic based on movement)
        /// </summary>
        public static float FindInterestingTime(AnimationClip clip, GameObject target)
        {
            if (clip == null || target == null) return 0f;

            // Sample at multiple points and find the one with most movement
            const int sampleCount = 10;
            var maxMovement = 0f;
            var bestTime = 0f;

            var originalPosition = target.transform.position;
            var originalRotation = target.transform.rotation;

            for (int i = 0; i < sampleCount; i++)
            {
                var time = (i / (float)(sampleCount - 1)) * clip.length;
                SampleClipAtAbsoluteTime(clip, time, target);

                // Calculate movement heuristic (position + rotation change)
                var posDelta = Vector3.Distance(target.transform.position, originalPosition);
                var rotDelta = Quaternion.Angle(target.transform.rotation, originalRotation);

                var movement = posDelta + rotDelta * 0.1f; // Weight rotation less

                if (movement > maxMovement)
                {
                    maxMovement = movement;
                    bestTime = time;
                }
            }

            // Restore original pose
            target.transform.position = originalPosition;
            target.transform.rotation = originalRotation;

            return bestTime;
        }

        /// <summary>
        /// Capture current pose data from a GameObject
        /// </summary>
        public static PoseData CapturePose(GameObject target, float time = 0f)
        {
            var pose = new PoseData
            {
                time = time,
                rootPosition = target.transform.position,
                rootRotation = target.transform.rotation,
                bonePositions = new Dictionary<string, Vector3>(),
                boneRotations = new Dictionary<string, Quaternion>()
            };

            // Capture all bone transforms
            var animators = target.GetComponentsInChildren<Animator>();
            foreach (var animator in animators)
            {
                CaptureBonesRecursive(animator.transform, pose);
            }

            return pose;
        }

        /// <summary>
        /// Recursively capture bone transforms
        /// </summary>
        private static void CaptureBonesRecursive(Transform bone, PoseData pose)
        {
            var path = GetBonePath(bone);
            pose.bonePositions[path] = bone.localPosition;
            pose.boneRotations[path] = bone.localRotation;

            foreach (Transform child in bone)
            {
                CaptureBonesRecursive(child, pose);
            }
        }

        /// <summary>
        /// Get the path to a bone from the root
        /// </summary>
        private static string GetBonePath(Transform bone)
        {
            var path = bone.name;
            var current = bone.parent;

            while (current != null && current.GetComponent<Animator>() == null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        /// <summary>
        /// Apply pose data to a GameObject
        /// </summary>
        public static void ApplyPose(GameObject target, PoseData pose)
        {
            target.transform.position = pose.rootPosition;
            target.transform.rotation = pose.rootRotation;

            // Apply bone transforms
            var animators = target.GetComponentsInChildren<Animator>();
            foreach (var animator in animators)
            {
                ApplyPoseRecursive(animator.transform, pose);
            }
        }

        /// <summary>
        /// Recursively apply pose to bones
        /// </summary>
        private static void ApplyPoseRecursive(Transform bone, PoseData pose)
        {
            var path = GetBonePath(bone);

            if (pose.bonePositions.ContainsKey(path))
                bone.localPosition = pose.bonePositions[path];

            if (pose.boneRotations.ContainsKey(path))
                bone.localRotation = pose.boneRotations[path];

            foreach (Transform child in bone)
            {
                ApplyPoseRecursive(child, pose);
            }
        }

        /// <summary>
        /// Get all animation clips from a GameObject and its children
        /// </summary>
        public static AnimationClip[] GetClips(GameObject target)
        {
            if (target == null) return new AnimationClip[0];

            var clips = new HashSet<AnimationClip>();

            // Get clips from KSClipRunner components
            var clipRunners = target.GetComponentsInChildren<KSClipRunner>();
            foreach (var runner in clipRunners)
            {
                clips.UnionWith(runner.clips.Where(c => c != null));
            }

            // Get clips from Animator components
            var animators = target.GetComponentsInChildren<Animator>();
            foreach (var animator in animators)
            {
                if (animator.runtimeAnimatorController != null)
                {
                    // Use the improved method that recursively searches through BlendTrees
                    var controllerClips = GetClipsFromController(animator.runtimeAnimatorController);
                    clips.UnionWith(controllerClips.Where(c => c != null));
                }
            }

            return clips.ToArray();
        }

        /// <summary>
        /// Get clips from an AnimatorController
        /// </summary>
        public static AnimationClip[] GetClipsFromController(RuntimeAnimatorController controller)
        {
            if (controller == null) return new AnimationClip[0];

            var clips = new HashSet<AnimationClip>();
            var animatorController = controller as UnityEditor.Animations.AnimatorController;

            if (animatorController != null)
            {
                // Recursively search through all layers and their state machines
                foreach (var layer in animatorController.layers)
                {
                    ExtractClipsFromStateMachine(layer.stateMachine, clips);
                }
            }

            return clips.ToArray();
        }

        /// <summary>
        /// Recursively extract AnimationClips from a state machine and its children
        /// </summary>
        private static void ExtractClipsFromStateMachine(UnityEditor.Animations.AnimatorStateMachine stateMachine, HashSet<AnimationClip> clips)
        {
            // Check all states in this state machine
            foreach (var state in stateMachine.states)
            {
                ExtractClipsFromMotion(state.state.motion, clips);
            }

            // Recursively check child state machines
            foreach (var childStateMachine in stateMachine.stateMachines)
            {
                ExtractClipsFromStateMachine(childStateMachine.stateMachine, clips);
            }
        }

        /// <summary>
        /// Extract AnimationClips from a motion (AnimationClip, BlendTree, etc.)
        /// </summary>
        private static void ExtractClipsFromMotion(Motion motion, HashSet<AnimationClip> clips)
        {
            if (motion == null) return;

            // Direct AnimationClip
            if (motion is AnimationClip clip)
            {
                clips.Add(clip);
                return;
            }

            // BlendTree - recursively extract child motions
            if (motion is UnityEditor.Animations.BlendTree blendTree)
            {
                foreach (var childMotion in blendTree.children)
                {
                    ExtractClipsFromMotion(childMotion.motion, clips);
                }
            }
        }
        /// <summary>
        /// Pose data structure
        /// </summary>
        public class PoseData
        {
            public float time;
            public Vector3 rootPosition;
            public Quaternion rootRotation;
            public Dictionary<string, Vector3> bonePositions;
            public Dictionary<string, Quaternion> boneRotations;

            public PoseData()
            {
                bonePositions = new Dictionary<string, Vector3>();
                boneRotations = new Dictionary<string, Quaternion>();
            }
        }
    }
}
