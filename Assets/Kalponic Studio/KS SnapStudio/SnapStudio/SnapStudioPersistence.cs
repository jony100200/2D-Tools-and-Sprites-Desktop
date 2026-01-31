// SnapStudioPersistence.cs
// Persistence logic for SnapStudio settings
// Following SOLID principles - Single Responsibility for data persistence

using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

namespace KalponicGames.KS_SnapStudio
{
    public static class SnapStudioPersistence
    {
        private const string ANIMATOR_CONTROLLER_KEY = "KS.SnapStudio.AnimatorController";
        private const string SELECTED_ANIMATION_KEY = "KS.SnapStudio.SelectedAnimation";

        /// <summary>
        /// Loads all persisted SnapStudio values into the UI
        /// </summary>
        public static void LoadPersistedValues(VisualElement root)
        {
            try
            {
                var widthField = root.Q<IntegerField>("ks-snapstudio-settings-width");
                var heightField = root.Q<IntegerField>("ks-snapstudio-settings-height");
                var fpsField = root.Q<IntegerField>("ks-snapstudio-settings-fps");
                var maxFramesField = root.Q<IntegerField>("ks-snapstudio-settings-max-frames");
                var trimSpritesToggle = root.Q<Toggle>("ks-snapstudio-settings-trim-sprites");
                var characterFillField = root.Q<Slider>("ks-snapstudio-settings-character-fill");
                var mirrorSpritesToggle = root.Q<Toggle>("ks-snapstudio-settings-mirror-sprites");
                var hdrToggle = root.Q<Toggle>("ks-snapstudio-settings-hdr");
                var pixelSizeField = root.Q<IntegerField>("ks-snapstudio-settings-pixel-size");
                var captureAllToggle = root.Q<Toggle>("ks-snapstudio-settings-capture-all");
                var outputPathField = root.Q<TextField>("ks-snapstudio-output-path");

                if (widthField != null) widthField.value = EditorPrefs.GetInt("KS.SnapStudio.Width", 1024);
                if (heightField != null) heightField.value = EditorPrefs.GetInt("KS.SnapStudio.Height", 1024);
                if (fpsField != null) fpsField.value = EditorPrefs.GetInt("KS.SnapStudio.Fps", 24);
                if (maxFramesField != null) maxFramesField.value = EditorPrefs.GetInt("KS.SnapStudio.MaxFrames", 24);
                if (trimSpritesToggle != null) trimSpritesToggle.value = EditorPrefs.GetBool("KS.SnapStudio.TrimSprites", true);
                if (characterFillField != null) characterFillField.value = EditorPrefs.GetFloat("KS.SnapStudio.CharacterFill", 75f);
                if (mirrorSpritesToggle != null) mirrorSpritesToggle.value = EditorPrefs.GetBool("KS.SnapStudio.MirrorSprites", true);
                if (hdrToggle != null) hdrToggle.value = EditorPrefs.GetBool("KS.SnapStudio.HDR", false);
                if (pixelSizeField != null) pixelSizeField.value = EditorPrefs.GetInt("KS.SnapStudio.PixelSize", 16);
                if (captureAllToggle != null) captureAllToggle.value = EditorPrefs.GetBool("KS.SnapStudio.CaptureAll", false);
                if (outputPathField != null) outputPathField.value = EditorPrefs.GetString("KS.SnapStudio.OutputPath", "KS_SnapStudio_Renders");

                // Load selected animation clip
                string selectedAnimationName = EditorPrefs.GetString(SELECTED_ANIMATION_KEY, "");
                if (!string.IsNullOrEmpty(selectedAnimationName))
                {
                    var animationClipField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-settings-animation-select");
                    if (animationClipField != null)
                    {
                        string[] guids = AssetDatabase.FindAssets($"t:AnimationClip {selectedAnimationName}");
                        if (guids.Length > 0)
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                            if (clip != null && clip.name == selectedAnimationName)
                            {
                                animationClipField.SetValueWithoutNotify(clip);
                            }
                        }
                    }
                }

                // Restore target object reference
                RestoreTargetReference(root);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading SnapStudio persisted values: {e.Message}");
            }
        }

        /// <summary>
        /// Saves a SnapStudio setting value
        /// </summary>
        public static void SaveValue(string key, object value)
        {
            string fullKey = "KS.SnapStudio." + key;
            if (value is int intValue) EditorPrefs.SetInt(fullKey, intValue);
            else if (value is bool boolValue) EditorPrefs.SetBool(fullKey, boolValue);
            else if (value is string stringValue) EditorPrefs.SetString(fullKey, stringValue);
            else if (value is float floatValue) EditorPrefs.SetFloat(fullKey, floatValue);
        }

        /// <summary>
        /// Saves the target GameObject reference
        /// </summary>
        public static void SaveTargetReference(GameObject target)
        {
            if (target != null)
            {
                EditorPrefs.SetString("KS.SnapStudio.TargetName", target.name);
            }
            else
            {
                EditorPrefs.SetString("KS.SnapStudio.TargetName", "");
            }
        }

        /// <summary>
        /// Restores the target GameObject reference from persistence
        /// </summary>
        public static void RestoreTargetReference(VisualElement root)
        {
            // If caller passed null, try to locate the root VisualElement from the open SnapStudio window
            if (root == null)
            {
#if UNITY_EDITOR
                // Prefer ServiceLocator's reference if available (set in KSSnapStudioWindow.OnEnable)
                try
                {
                    var main = KalponicGames.KS_SnapStudio.SnapStudio.ServiceLocator.MainWindow;
                    if (main != null)
                    {
                        root = main.rootVisualElement;
                    }
                }
                catch
                {
                    // Ignore resolution errors and fall back to EditorWindow lookup
                }

                if (root == null)
                {
                    // Try to find an open KSSnapStudioWindow without creating a new one
                    if (EditorWindow.HasOpenInstances<KSSnapStudioWindow>())
                    {
                        var win = EditorWindow.GetWindow<KSSnapStudioWindow>();
                        if (win != null)
                        {
                            root = win.rootVisualElement;
                        }
                    }
                }
#endif
            }

            if (root == null) return;

            var targetField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-input-target");
            if (targetField == null) return;

            string targetName = EditorPrefs.GetString("KS.SnapStudio.TargetName", "");
            if (!string.IsNullOrEmpty(targetName))
            {
                GameObject targetObject = GameObject.Find(targetName);
                if (targetObject != null)
                {
                    targetField.value = targetObject;
                }
            }
        }

        /// <summary>
        /// Loads the persisted AnimatorController
        /// </summary>
        public static void LoadPersistedAnimatorController(VisualElement root)
        {
            if (root == null) return;

            var animatorControllerField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-settings-animator-controller");
            if (animatorControllerField == null) return;

            var controllerPath = EditorPrefs.GetString(ANIMATOR_CONTROLLER_KEY, "");
            if (!string.IsNullOrEmpty(controllerPath))
            {
                var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                if (controller != null)
                {
                    animatorControllerField.SetValueWithoutNotify(controller);
                    Debug.Log($"Loaded persisted AnimatorController: {controller.name}");
                }
            }
        }

        /// <summary>
        /// Handles AnimatorController changes and persistence
        /// </summary>
        public static void OnAnimatorControllerChanged(RuntimeAnimatorController controller)
        {
            if (controller != null)
            {
                EditorPrefs.SetString(ANIMATOR_CONTROLLER_KEY, AssetDatabase.GetAssetPath(controller));
            }
            else
            {
                EditorPrefs.DeleteKey(ANIMATOR_CONTROLLER_KEY);
            }
        }
    }
}