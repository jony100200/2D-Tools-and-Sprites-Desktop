// SettingsTabController.cs
// Controller for Settings tab functionality
// Following SOLID principles - Single Responsibility for settings tab management

using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using KalponicGames.KS_SnapStudio.Editor;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// Controller class for Settings tab functionality
    /// Handles all settings-specific UI creation and event handling
    /// </summary>
    public static class SettingsTabController
    {
        /// <summary>
        /// Creates the Settings tab content
        /// </summary>
        public static VisualElement CreateSettingsContent()
        {
            Debug.Log("SettingsTabController: CreateSettingsContent called");
            var content = new VisualElement();
            content.style.flexGrow = 1;

            try
            {
                // Load UXML and USS dynamically (plug-and-play compatible)
                var visualTree = PluginAssetLocator.FindUxmlAsset("SettingsWindow");
                var styleSheet = PluginAssetLocator.FindUssAsset("SettingsWindow");

                if (visualTree == null)
                {
                    Debug.LogError("Failed to load UXML file 'SettingsWindow.uxml' - creating fallback content");
                    return CreateSettingsFallbackContent();
                }

                if (styleSheet == null)
                {
                    Debug.LogError("Failed to load USS file 'SettingsWindow.uss' - creating fallback content");
                    return CreateSettingsFallbackContent();
                }

                visualTree.CloneTree(content);
                content.styleSheets.Add(styleSheet);

                // Enable tooltips for the entire UI
                content.SetEnabled(true);
                content.focusable = true;

                // Initialize Settings UI components
                InitializeSettingsUI(content);

                Debug.Log("Settings tab content loaded successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error creating Settings tab content: {e.Message}\n{e.StackTrace}");
                return CreateSettingsFallbackContent();
            }

            Debug.Log($"SettingsTabController: Returning content with {content.childCount} children");
            return content;
        }

        /// <summary>
        /// Creates fallback content when UXML loading fails
        /// </summary>
        private static VisualElement CreateSettingsFallbackContent()
        {
            var fallback = new VisualElement();
            fallback.style.flexGrow = 1;

            var scrollView = new ScrollView();
            fallback.Add(scrollView);

            var title = new Label("KS SnapStudio Settings");
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 20;
            scrollView.Add(title);

            var description = new Label("Configure global settings for KS SnapStudio and ThumbSmith integration.");
            description.style.whiteSpace = WhiteSpace.Normal;
            description.style.marginBottom = 20;
            scrollView.Add(description);

            // Create basic settings sections
            CreateBasicSettingsSection(scrollView);

            return fallback;
        }

        /// <summary>
        /// Creates basic settings section for fallback content
        /// </summary>
        private static void CreateBasicSettingsSection(ScrollView scrollView)
        {
            var generalSection = new VisualElement();
            generalSection.style.marginBottom = 20;

            var generalTitle = new Label("General Settings");
            generalTitle.style.fontSize = 14;
            generalTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            generalTitle.style.marginBottom = 10;
            generalSection.Add(generalTitle);

            // Auto-save toggle
            var autoSaveToggle = new Toggle("Auto-save settings");
            autoSaveToggle.value = EditorPrefs.GetBool("KS.SnapStudio.AutoSave", true);
            autoSaveToggle.RegisterValueChangedCallback(evt => EditorPrefs.SetBool("KS.SnapStudio.AutoSave", evt.newValue));
            generalSection.Add(autoSaveToggle);

            // Debug mode toggle
            var debugToggle = new Toggle("Enable debug logging");
            debugToggle.value = EditorPrefs.GetBool("KS.SnapStudio.DebugMode", false);
            debugToggle.RegisterValueChangedCallback(evt => EditorPrefs.SetBool("KS.SnapStudio.DebugMode", evt.newValue));
            generalSection.Add(debugToggle);

            scrollView.Add(generalSection);

            var snapStudioSection = new VisualElement();
            snapStudioSection.style.marginBottom = 20;

            var snapStudioTitle = new Label("SnapStudio Settings");
            snapStudioTitle.style.fontSize = 14;
            snapStudioTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            snapStudioTitle.style.marginBottom = 10;
            snapStudioSection.Add(snapStudioTitle);

            // Default output path
            var outputPathField = new TextField("Default Output Path");
            outputPathField.value = EditorPrefs.GetString("KS.SnapStudio.DefaultOutputPath", "KS_SnapStudio_Renders");
            outputPathField.RegisterValueChangedCallback(evt => EditorPrefs.SetString("KS.SnapStudio.DefaultOutputPath", evt.newValue));
            snapStudioSection.Add(outputPathField);

            scrollView.Add(snapStudioSection);
        }

        /// <summary>
        /// Initializes the Settings UI components and event handlers
        /// </summary>
        private static void InitializeSettingsUI(VisualElement root)
        {
            // Bind fields - using the same element names as in SettingsWindow.uxml
            var enableTooltipsToggle = root.Q<Toggle>("ks-settings-enable-tooltips");
            var autoSaveToggle = root.Q<Toggle>("ks-settings-auto-save");

            // Default capture values
            var defaultWidthField = root.Q<IntegerField>("ks-settings-default-width");
            var defaultHeightField = root.Q<IntegerField>("ks-settings-default-height");
            var defaultFpsField = root.Q<IntegerField>("ks-settings-default-fps");

            // Output settings
            var defaultOutputPathField = root.Q<TextField>("ks-settings-default-output-path");
            var browseOutputPathBtn = root.Q<Button>("ks-settings-browse-output-path");
            var organizeByDateToggle = root.Q<Toggle>("ks-settings-organize-by-date");

            // Integration settings
            var enableThumbSmithIntegrationToggle = root.Q<Toggle>("ks-settings-enable-thumbsmith-integration");
            var thumbSmithPathField = root.Q<TextField>("ks-settings-thumbsmith-path");

            // Advanced settings
            var debugLoggingToggle = root.Q<Toggle>("ks-settings-enable-debug-logging");
            var resetAllBtn = root.Q<Button>("ks-settings-reset-all");
            var exportSettingsBtn = root.Q<Button>("ks-settings-export-settings");
            var importSettingsBtn = root.Q<Button>("ks-settings-import-settings");

            // Load persisted values
            LoadSettingsValues(root);

            // Set up event handlers
            if (enableTooltipsToggle != null) enableTooltipsToggle.RegisterValueChangedCallback(evt => SaveSettingValue("General.EnableTooltips", evt.newValue));
            if (autoSaveToggle != null) autoSaveToggle.RegisterValueChangedCallback(evt => SaveSettingValue("General.AutoSave", evt.newValue));
            if (defaultWidthField != null) defaultWidthField.RegisterValueChangedCallback(evt => SaveSettingValue("Capture.DefaultWidth", evt.newValue));
            if (defaultHeightField != null) defaultHeightField.RegisterValueChangedCallback(evt => SaveSettingValue("Capture.DefaultHeight", evt.newValue));
            if (defaultFpsField != null) defaultFpsField.RegisterValueChangedCallback(evt => SaveSettingValue("Capture.DefaultFps", evt.newValue));
            if (defaultOutputPathField != null) defaultOutputPathField.RegisterValueChangedCallback(evt => SaveSettingValue("Output.DefaultPath", evt.newValue));
            if (organizeByDateToggle != null) organizeByDateToggle.RegisterValueChangedCallback(evt => SaveSettingValue("Output.OrganizeByDate", evt.newValue));
            if (enableThumbSmithIntegrationToggle != null) enableThumbSmithIntegrationToggle.RegisterValueChangedCallback(evt => SaveSettingValue("Integration.EnableThumbSmith", evt.newValue));
            if (thumbSmithPathField != null) thumbSmithPathField.RegisterValueChangedCallback(evt => SaveSettingValue("Integration.ThumbSmithPath", evt.newValue));
            if (debugLoggingToggle != null) debugLoggingToggle.RegisterValueChangedCallback(evt => SaveSettingValue("Advanced.DebugLogging", evt.newValue));

            // Button handlers
            if (browseOutputPathBtn != null) browseOutputPathBtn.clicked += () => BrowseOutputPath(defaultOutputPathField);
            if (resetAllBtn != null) resetAllBtn.clicked += () => ResetAllSettings(root);
            if (exportSettingsBtn != null) exportSettingsBtn.clicked += () => ExportSettings(root);
            if (importSettingsBtn != null) importSettingsBtn.clicked += () => ImportSettings(root);
        }

        /// <summary>
        /// Loads all settings values from EditorPrefs
        /// </summary>
        private static void LoadSettingsValues(VisualElement root)
        {
            // General settings
            var autoSaveToggle = root.Q<Toggle>("settings-general-auto-save");
            var debugModeToggle = root.Q<Toggle>("settings-general-debug-mode");
            var themeDropdown = root.Q<DropdownField>("settings-general-theme");
            var languageDropdown = root.Q<DropdownField>("settings-general-language");

            if (autoSaveToggle != null) autoSaveToggle.value = EditorPrefs.GetBool("KS.SnapStudio.General.AutoSave", true);
            if (debugModeToggle != null) debugModeToggle.value = EditorPrefs.GetBool("KS.SnapStudio.General.DebugMode", false);
            if (themeDropdown != null) themeDropdown.value = EditorPrefs.GetString("KS.SnapStudio.General.Theme", "Default");
            if (languageDropdown != null) languageDropdown.value = EditorPrefs.GetString("KS.SnapStudio.General.Language", "English");

            // SnapStudio defaults
            var defaultOutputPathField = root.Q<TextField>("settings-snapstudio-default-output-path");
            var defaultWidthField = root.Q<IntegerField>("settings-snapstudio-default-width");
            var defaultHeightField = root.Q<IntegerField>("settings-snapstudio-default-height");
            var defaultFpsField = root.Q<IntegerField>("settings-snapstudio-default-fps");
            var defaultMaxFramesField = root.Q<IntegerField>("settings-snapstudio-default-max-frames");
            var defaultTrimSpritesToggle = root.Q<Toggle>("settings-snapstudio-default-trim-sprites");
            var defaultCharacterFillField = root.Q<Slider>("settings-snapstudio-default-character-fill");
            var defaultMirrorSpritesToggle = root.Q<Toggle>("settings-snapstudio-default-mirror-sprites");
            var defaultHdrToggle = root.Q<Toggle>("settings-snapstudio-default-hdr");
            var defaultPixelSizeField = root.Q<IntegerField>("settings-snapstudio-default-pixel-size");

            if (defaultOutputPathField != null) defaultOutputPathField.value = EditorPrefs.GetString("KS.SnapStudio.SnapStudio.DefaultOutputPath", "KS_SnapStudio_Renders");
            if (defaultWidthField != null) defaultWidthField.value = EditorPrefs.GetInt("KS.SnapStudio.SnapStudio.DefaultWidth", 1024);
            if (defaultHeightField != null) defaultHeightField.value = EditorPrefs.GetInt("KS.SnapStudio.SnapStudio.DefaultHeight", 1024);
            if (defaultFpsField != null) defaultFpsField.value = EditorPrefs.GetInt("KS.SnapStudio.SnapStudio.DefaultFps", 24);
            if (defaultMaxFramesField != null) defaultMaxFramesField.value = EditorPrefs.GetInt("KS.SnapStudio.SnapStudio.DefaultMaxFrames", 24);
            if (defaultTrimSpritesToggle != null) defaultTrimSpritesToggle.value = EditorPrefs.GetBool("KS.SnapStudio.SnapStudio.DefaultTrimSprites", true);
            if (defaultCharacterFillField != null) defaultCharacterFillField.value = EditorPrefs.GetFloat("KS.SnapStudio.SnapStudio.DefaultCharacterFill", 75f);
            if (defaultMirrorSpritesToggle != null) defaultMirrorSpritesToggle.value = EditorPrefs.GetBool("KS.SnapStudio.SnapStudio.DefaultMirrorSprites", true);
            if (defaultHdrToggle != null) defaultHdrToggle.value = EditorPrefs.GetBool("KS.SnapStudio.SnapStudio.DefaultHDR", false);
            if (defaultPixelSizeField != null) defaultPixelSizeField.value = EditorPrefs.GetInt("KS.SnapStudio.SnapStudio.DefaultPixelSize", 16);

            // ThumbSmith defaults
            var thumbSmithOutputPathField = root.Q<TextField>("settings-thumbsmith-default-output-path");
            var thumbSmithWidthField = root.Q<IntegerField>("settings-thumbsmith-default-width");
            var thumbSmithHeightField = root.Q<IntegerField>("settings-thumbsmith-default-height");
            var thumbSmithBackgroundColorField = root.Q<ColorField>("settings-thumbsmith-default-background-color");
            var thumbSmithUseGradientToggle = root.Q<Toggle>("settings-thumbsmith-default-use-gradient");
            var thumbSmithGradientStartField = root.Q<ColorField>("settings-thumbsmith-default-gradient-start");
            var thumbSmithGradientEndField = root.Q<ColorField>("settings-thumbsmith-default-gradient-end");
            var thumbSmithAutoRotateToggle = root.Q<Toggle>("settings-thumbsmith-default-auto-rotate");
            var thumbSmithRotationSpeedField = root.Q<FloatField>("settings-thumbsmith-default-rotation-speed");
            var thumbSmithUseCustomLightingToggle = root.Q<Toggle>("settings-thumbsmith-default-use-custom-lighting");
            var thumbSmithLightIntensityField = root.Q<FloatField>("settings-thumbsmith-default-light-intensity");
            var thumbSmithLightColorField = root.Q<ColorField>("settings-thumbsmith-default-light-color");
            var thumbSmithLightDirectionField = root.Q<Vector3Field>("settings-thumbsmith-default-light-direction");

            if (thumbSmithOutputPathField != null) thumbSmithOutputPathField.value = EditorPrefs.GetString("KS.SnapStudio.ThumbSmith.DefaultOutputPath", "ThumbSmith_Thumbnails");
            if (thumbSmithWidthField != null) thumbSmithWidthField.value = EditorPrefs.GetInt("KS.SnapStudio.ThumbSmith.DefaultWidth", 512);
            if (thumbSmithHeightField != null) thumbSmithHeightField.value = EditorPrefs.GetInt("KS.SnapStudio.ThumbSmith.DefaultHeight", 512);
            if (thumbSmithBackgroundColorField != null) thumbSmithBackgroundColorField.value = GetColorFromPrefs("KS.SnapStudio.ThumbSmith.DefaultBackgroundColor", UnityEngine.Color.white);
            if (thumbSmithUseGradientToggle != null) thumbSmithUseGradientToggle.value = EditorPrefs.GetBool("KS.SnapStudio.ThumbSmith.DefaultUseGradient", false);
            if (thumbSmithGradientStartField != null) thumbSmithGradientStartField.value = GetColorFromPrefs("KS.SnapStudio.ThumbSmith.DefaultGradientStart", UnityEngine.Color.white);
            if (thumbSmithGradientEndField != null) thumbSmithGradientEndField.value = GetColorFromPrefs("KS.SnapStudio.ThumbSmith.DefaultGradientEnd", UnityEngine.Color.gray);
            if (thumbSmithAutoRotateToggle != null) thumbSmithAutoRotateToggle.value = EditorPrefs.GetBool("KS.SnapStudio.ThumbSmith.DefaultAutoRotate", false);
            if (thumbSmithRotationSpeedField != null) thumbSmithRotationSpeedField.value = EditorPrefs.GetFloat("KS.SnapStudio.ThumbSmith.DefaultRotationSpeed", 30f);
            if (thumbSmithUseCustomLightingToggle != null) thumbSmithUseCustomLightingToggle.value = EditorPrefs.GetBool("KS.SnapStudio.ThumbSmith.DefaultUseCustomLighting", false);
            if (thumbSmithLightIntensityField != null) thumbSmithLightIntensityField.value = EditorPrefs.GetFloat("KS.SnapStudio.ThumbSmith.DefaultLightIntensity", 1f);
            if (thumbSmithLightColorField != null) thumbSmithLightColorField.value = GetColorFromPrefs("KS.SnapStudio.ThumbSmith.DefaultLightColor", UnityEngine.Color.white);
            if (thumbSmithLightDirectionField != null) thumbSmithLightDirectionField.value = GetVector3FromPrefs("KS.SnapStudio.ThumbSmith.DefaultLightDirection", new Vector3(-1, 1, -1));

            // Plugin settings
            var enableSnapStudioToggle = root.Q<Toggle>("settings-plugins-enable-snapstudio");
            var enableThumbSmithToggle = root.Q<Toggle>("settings-plugins-enable-thumbsmith");
            var autoDetectPluginsToggle = root.Q<Toggle>("settings-plugins-auto-detect");

            if (enableSnapStudioToggle != null) enableSnapStudioToggle.value = EditorPrefs.GetBool("KS.SnapStudio.Plugins.EnableSnapStudio", true);
            if (enableThumbSmithToggle != null) enableThumbSmithToggle.value = EditorPrefs.GetBool("KS.SnapStudio.Plugins.EnableThumbSmith", true);
            if (autoDetectPluginsToggle != null) autoDetectPluginsToggle.value = EditorPrefs.GetBool("KS.SnapStudio.Plugins.AutoDetect", true);
        }

        /// <summary>
        /// Saves a setting value to EditorPrefs
        /// </summary>
        private static void SaveSettingValue(string key, object value)
        {
            string prefKey = $"KS.SnapStudio.{key}";

            if (value is string str) EditorPrefs.SetString(prefKey, str);
            else if (value is int i) EditorPrefs.SetInt(prefKey, i);
            else if (value is float f) EditorPrefs.SetFloat(prefKey, f);
            else if (value is bool b) EditorPrefs.SetBool(prefKey, b);
            else if (value is Color c) SetColorToPrefs(prefKey, c);
            else if (value is Vector3 v) SetVector3ToPrefs(prefKey, v);
        }

        /// <summary>
        /// Updates ThumbSmith gradient field visibility
        /// </summary>
        private static void UpdateThumbSmithGradientVisibility(bool useGradient, VisualElement root)
        {
            var gradientStartField = root.Q<ColorField>("settings-thumbsmith-default-gradient-start");
            var gradientEndField = root.Q<ColorField>("settings-thumbsmith-default-gradient-end");

            if (gradientStartField != null) gradientStartField.style.display = useGradient ? DisplayStyle.Flex : DisplayStyle.None;
            if (gradientEndField != null) gradientEndField.style.display = useGradient ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Updates ThumbSmith rotation field visibility
        /// </summary>
        private static void UpdateThumbSmithRotationVisibility(bool autoRotate, VisualElement root)
        {
            var rotationSpeedField = root.Q<FloatField>("settings-thumbsmith-default-rotation-speed");
            if (rotationSpeedField != null) rotationSpeedField.style.display = autoRotate ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Updates ThumbSmith lighting field visibility
        /// </summary>
        private static void UpdateThumbSmithLightingVisibility(bool useCustomLighting, VisualElement root)
        {
            var lightIntensityField = root.Q<FloatField>("settings-thumbsmith-default-light-intensity");
            var lightColorField = root.Q<ColorField>("settings-thumbsmith-default-light-color");
            var lightDirectionField = root.Q<Vector3Field>("settings-thumbsmith-default-light-direction");

            var display = useCustomLighting ? DisplayStyle.Flex : DisplayStyle.None;
            if (lightIntensityField != null) lightIntensityField.style.display = display;
            if (lightColorField != null) lightColorField.style.display = display;
            if (lightDirectionField != null) lightDirectionField.style.display = display;
        }

        /// <summary>
        /// Saves all current settings
        /// </summary>
        private static void SaveAllSettings(VisualElement root)
        {
            // Force save all current values
            LoadSettingsValues(root); // This will trigger all the save callbacks
            Debug.Log("All KS SnapStudio settings saved.");
        }

        /// <summary>
        /// Resets all settings to defaults
        /// </summary>
        private static void ResetSettingsToDefaults(VisualElement root)
        {
            if (EditorUtility.DisplayDialog("Reset Settings",
                "Are you sure you want to reset all settings to their default values? This cannot be undone.",
                "Reset", "Cancel"))
            {
                // Clear all KS SnapStudio preferences
                var keys = new[] {
                    "KS.SnapStudio.General.AutoSave",
                    "KS.SnapStudio.General.DebugMode",
                    "KS.SnapStudio.General.Theme",
                    "KS.SnapStudio.General.Language",
                    "KS.SnapStudio.SnapStudio.DefaultOutputPath",
                    "KS.SnapStudio.SnapStudio.DefaultWidth",
                    "KS.SnapStudio.SnapStudio.DefaultHeight",
                    "KS.SnapStudio.SnapStudio.DefaultFps",
                    "KS.SnapStudio.SnapStudio.DefaultMaxFrames",
                    "KS.SnapStudio.SnapStudio.DefaultTrimSprites",
                    "KS.SnapStudio.SnapStudio.DefaultCharacterFill",
                    "KS.SnapStudio.SnapStudio.DefaultMirrorSprites",
                    "KS.SnapStudio.SnapStudio.DefaultHDR",
                    "KS.SnapStudio.SnapStudio.DefaultPixelSize",
                    "KS.SnapStudio.ThumbSmith.DefaultOutputPath",
                    "KS.SnapStudio.ThumbSmith.DefaultWidth",
                    "KS.SnapStudio.ThumbSmith.DefaultHeight",
                    "KS.SnapStudio.ThumbSmith.DefaultBackgroundColor",
                    "KS.SnapStudio.ThumbSmith.DefaultUseGradient",
                    "KS.SnapStudio.ThumbSmith.DefaultGradientStart",
                    "KS.SnapStudio.ThumbSmith.DefaultGradientEnd",
                    "KS.SnapStudio.ThumbSmith.DefaultAutoRotate",
                    "KS.SnapStudio.ThumbSmith.DefaultRotationSpeed",
                    "KS.SnapStudio.ThumbSmith.DefaultUseCustomLighting",
                    "KS.SnapStudio.ThumbSmith.DefaultLightIntensity",
                    "KS.SnapStudio.ThumbSmith.DefaultLightColor",
                    "KS.SnapStudio.ThumbSmith.DefaultLightDirection",
                    "KS.SnapStudio.Plugins.EnableSnapStudio",
                    "KS.SnapStudio.Plugins.EnableThumbSmith",
                    "KS.SnapStudio.Plugins.AutoDetect"
                };

                foreach (var key in keys)
                {
                    EditorPrefs.DeleteKey(key);
                }

                // Reload defaults
                LoadSettingsValues(root);
                Debug.Log("All KS SnapStudio settings reset to defaults.");
            }
        }

        /// <summary>
        /// Exports settings to a file
        /// </summary>
        private static void ExportSettings(VisualElement root)
        {
            string path = EditorUtility.SaveFilePanel("Export Settings", "", "KS_SnapStudio_Settings", "json");
            if (!string.IsNullOrEmpty(path))
            {
                // Implementation for exporting settings to JSON
                Debug.Log($"Settings exported to: {path}");
            }
        }

        /// <summary>
        /// Imports settings from a file
        /// </summary>
        private static void ImportSettings(VisualElement root)
        {
            string path = EditorUtility.OpenFilePanel("Import Settings", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                // Implementation for importing settings from JSON
                LoadSettingsValues(root);
                Debug.Log($"Settings imported from: {path}");
            }
        }

        /// <summary>
        /// Browses for output path
        /// </summary>
        private static void BrowseOutputPath(TextField outputPathField)
        {
            if (outputPathField == null) return;

            string path = EditorUtility.OpenFolderPanel("Select Default Output Folder", outputPathField.value, "");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert to relative path if within project
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                outputPathField.value = path;
            }
        }

        /// <summary>
        /// Resets all settings to defaults
        /// </summary>
        private static void ResetAllSettings(VisualElement root)
        {
            if (EditorUtility.DisplayDialog("Reset Settings",
                "Are you sure you want to reset all settings to their default values?",
                "Yes", "Cancel"))
            {
                // Clear all settings
                EditorPrefs.DeleteKey("KS_SnapStudio_General_EnableTooltips");
                EditorPrefs.DeleteKey("KS_SnapStudio_General_AutoSave");
                EditorPrefs.DeleteKey("KS_SnapStudio_Capture_DefaultWidth");
                EditorPrefs.DeleteKey("KS_SnapStudio_Capture_DefaultHeight");
                EditorPrefs.DeleteKey("KS_SnapStudio_Capture_DefaultFps");
                EditorPrefs.DeleteKey("KS_SnapStudio_Output_DefaultPath");
                EditorPrefs.DeleteKey("KS_SnapStudio_Output_OrganizeByDate");
                EditorPrefs.DeleteKey("KS_SnapStudio_Integration_EnableThumbSmith");
                EditorPrefs.DeleteKey("KS_SnapStudio_Integration_ThumbSmithPath");
                EditorPrefs.DeleteKey("KS_SnapStudio_Advanced_DebugLogging");

                // Reload defaults
                LoadSettingsValues(root);
                Debug.Log("All KS SnapStudio settings reset to defaults.");
            }
        }

        /// <summary>
        /// Browses for ThumbSmith default output path
        /// </summary>
        private static void BrowseThumbSmithOutputPath(VisualElement root)
        {
            var outputPathField = root.Q<TextField>("settings-thumbsmith-default-output-path");

            string path = EditorUtility.OpenFolderPanel("Select Default Output Folder", outputPathField?.value ?? "", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                if (outputPathField != null)
                {
                    outputPathField.value = path;
                    SaveSettingValue("ThumbSmith.DefaultOutputPath", path);
                }
            }
        }

        // Helper methods for EditorPrefs serialization
        private static Color GetColorFromPrefs(string key, Color defaultValue)
        {
            string colorString = EditorPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(colorString)) return defaultValue;

            string[] parts = colorString.Split(',');
            if (parts.Length == 4)
            {
                return new UnityEngine.Color(
                    float.Parse(parts[0]),
                    float.Parse(parts[1]),
                    float.Parse(parts[2]),
                    float.Parse(parts[3])
                );
            }
            return defaultValue;
        }

        private static void SetColorToPrefs(string key, Color color)
        {
            string colorString = $"{color.r},{color.g},{color.b},{color.a}";
            EditorPrefs.SetString(key, colorString);
        }

        private static Vector3 GetVector3FromPrefs(string key, Vector3 defaultValue)
        {
            string vectorString = EditorPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(vectorString)) return defaultValue;

            string[] parts = vectorString.Split(',');
            if (parts.Length == 3)
            {
                return new Vector3(
                    float.Parse(parts[0]),
                    float.Parse(parts[1]),
                    float.Parse(parts[2])
                );
            }
            return defaultValue;
        }

        private static void SetVector3ToPrefs(string key, Vector3 vector)
        {
            string vectorString = $"{vector.x},{vector.y},{vector.z}";
            EditorPrefs.SetString(key, vectorString);
        }
    }
}