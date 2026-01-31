// SnapStudioUIHelpers.cs
// Helper methods for SnapStudio UI operations
// Following SOLID principles - Single Responsibility for UI utilities

using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace KalponicGames.KS_SnapStudio
{
    public static class SnapStudioUIHelpers
    {
        /// <summary>
        /// Updates the visibility of the animation clip field based on capture mode
        /// </summary>
        public static void UpdateAnimationFieldVisibility(bool captureAll, VisualElement root)
        {
            var animationClipField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-settings-animation-select");
            if (animationClipField != null)
            {
                animationClipField.style.display = captureAll ? DisplayStyle.None : DisplayStyle.Flex;
                animationClipField.style.visibility = captureAll ? Visibility.Hidden : Visibility.Visible;
            }
        }

        /// <summary>
        /// Updates the visibility of the character fill slider based on trim sprites setting
        /// </summary>
        public static void UpdateCharacterFillVisibility(bool trimSprites, VisualElement root)
        {
            var characterFillField = root.Q<Slider>("ks-snapstudio-settings-character-fill");
            if (characterFillField != null)
            {
                characterFillField.style.display = trimSprites ? DisplayStyle.None : DisplayStyle.Flex;
                characterFillField.style.visibility = trimSprites ? Visibility.Hidden : Visibility.Visible;
            }
        }

        /// <summary>
        /// Updates the create scene button state based on scene contents
        /// </summary>
        public static void UpdateCreateSceneButtonState(VisualElement root)
        {
            if (root == null) return;

            var createSceneBtn = root.Q<Button>("ks-snapstudio-controls-create-scene");
            if (createSceneBtn != null)
            {
                // Check if capture elements are already present in current scene
                bool hasCaptureElements = HasCaptureElementsInCurrentScene();
                createSceneBtn.SetEnabled(!hasCaptureElements);

                // Update button text to indicate status
                createSceneBtn.text = hasCaptureElements ? "Capture Scene Already Present" : "Create Capture Scene";
            }
        }

        /// <summary>
        /// Check if capture scene elements are already present in the current scene
        /// This prevents accidentally overriding user's existing scene setup
        /// </summary>
        public static bool HasCaptureElementsInCurrentScene()
        {
            // Check for capture lighting rig elements
            bool hasMainLight = GameObject.Find("MainLight") != null;
            bool hasFillLight = GameObject.Find("FillLight") != null;
            bool hasRimLight = GameObject.Find("RimLight") != null;
            bool hasCaptureCameraRig = GameObject.Find("CaptureCameraRig") != null;
            bool hasCaptureCamera = GameObject.Find("CaptureCamera") != null;

            // If any capture elements exist, consider the scene as having capture setup
            return hasMainLight || hasFillLight || hasRimLight || hasCaptureCameraRig || hasCaptureCamera;
        }
    }
}