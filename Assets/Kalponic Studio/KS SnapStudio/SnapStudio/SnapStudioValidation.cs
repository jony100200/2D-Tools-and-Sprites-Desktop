// SnapStudioValidation.cs
// Validation logic for SnapStudio operations
// Following SOLID principles - Single Responsibility for validation

using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using KalponicGames.KS_SnapStudio.Systems;

namespace KalponicGames.KS_SnapStudio
{
    public static class SnapStudioValidation
    {
        /// <summary>
        /// Validates the target GameObject for animation capture
        /// </summary>
        public static void ValidateTarget(GameObject target, VisualElement root)
        {
            var targetField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-input-target");
            var validationIndicator = root.Q<VisualElement>("ks-snapstudio-target-validation-indicator");
            var startCaptureBtn = root.Q<Button>("ks-snapstudio-controls-start-capture");
            var statusText = root.Q<Label>("status-text");

            if (target == null)
            {
                if (targetField != null) targetField.style.borderBottomColor = UnityEngine.Color.red;
                if (validationIndicator != null) validationIndicator.AddToClassList("visible");
                if (statusText != null) statusText.text = "Invalid target: No object selected.";
                if (startCaptureBtn != null) startCaptureBtn.SetEnabled(false);
                return;
            }

            var animator = target.GetComponentInChildren<Animator>();
            var animation = target.GetComponentInChildren<Animation>();

            if (animator == null && animation == null)
            {
                if (targetField != null) targetField.style.borderBottomColor = UnityEngine.Color.red;
                if (validationIndicator != null) validationIndicator.AddToClassList("visible");
                if (statusText != null) statusText.text = "Invalid target: Must have Animator or Animation component.";
                if (startCaptureBtn != null) startCaptureBtn.SetEnabled(false);
            }
            else
            {
                if (targetField != null) targetField.style.borderBottomColor = UnityEngine.Color.clear;
                if (validationIndicator != null) validationIndicator.RemoveFromClassList("visible");
                if (statusText != null) statusText.text = "Ready";
                if (startCaptureBtn != null) startCaptureBtn.SetEnabled(true);
            }
        }

        /// <summary>
        /// Calculates estimated sprite count for capture configuration
        /// </summary>
        public static string CalculateSpriteEstimate(VisualElement root, GameObject target)
        {
            if (target == null) return "Configuration valid";

            var fpsField = root.Q<IntegerField>("ks-snapstudio-settings-fps");
            var maxFramesField = root.Q<IntegerField>("ks-snapstudio-settings-max-frames");
            var captureAllToggle = root.Q<Toggle>("ks-snapstudio-settings-capture-all");

            int fps = fpsField != null ? fpsField.value : 24;
            int maxFrames = maxFramesField != null ? maxFramesField.value : 24;
            bool captureAll = captureAllToggle != null ? captureAllToggle.value : false;

            if (captureAll)
            {
                // For batch capture, estimate based on all animations
                var clips = ClipSampler.GetClips(target);
                if (clips != null && clips.Length > 0)
                {
                    int totalSprites = 0;
                    foreach (var clip in clips)
                    {
                        int clipFrames = Mathf.Min(maxFrames * 2, (int)(clip.length * fps));
                        totalSprites += clipFrames;
                    }
                    return $"~{totalSprites} sprites ({clips.Length} animations)";
                }
                return "Configuration valid";
            }
            else
            {
                // For single animation capture
                AnimationClip selectedClip = null;

                // Check if user selected a specific clip
                var animationClipField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-settings-animation-select");
                if (animationClipField != null && animationClipField.value != null)
                {
                    selectedClip = animationClipField.value as AnimationClip;
                }

                // If no selection, try to find a clip
                if (selectedClip == null)
                {
                    var clips = ClipSampler.GetClips(target);
                    if (clips != null && clips.Length > 0)
                    {
                        selectedClip = clips[0];
                    }
                }

                if (selectedClip != null)
                {
                    int estimatedFrames = Mathf.Min(maxFrames, Mathf.CeilToInt(selectedClip.length * fps));
                    return $"{estimatedFrames} sprites ({selectedClip.name})";
                }

                return "Configuration valid - select animation to see estimate";
            }
        }

        /// <summary>
        /// Tests the current configuration for validity
        /// </summary>
        public static bool TestConfiguration(VisualElement root, out string message)
        {
            var targetField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-input-target");
            var outputPathField = root.Q<TextField>("ks-snapstudio-output-path");

            var target = targetField?.value as GameObject;
            var outputPath = outputPathField?.value;

            if (target == null || (target.GetComponentInChildren<Animator>() == null && target.GetComponentInChildren<Animation>() == null))
            {
                message = "Test failed: Invalid target.";
                return false;
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                message = "Test failed: Output path required.";
                return false;
            }

            // Calculate estimated sprite count
            string spriteEstimate = CalculateSpriteEstimate(root, target);
            message = $"Test passed: {spriteEstimate}";
            return true;
        }
    }
}