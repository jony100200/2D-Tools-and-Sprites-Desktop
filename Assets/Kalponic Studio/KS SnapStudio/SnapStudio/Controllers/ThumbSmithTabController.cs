// ThumbSmithTabController.cs
// Controller for ThumbSmith tab functionality
// Following SOLID principles - Single Responsibility for ThumbSmith tab management

using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using KalponicGames.KS_SnapStudio.Editor;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// Controller class for ThumbSmith tab functionality
    /// Handles all ThumbSmith-specific UI creation and event handling
    /// </summary>
    public static class ThumbSmithTabController
    {
        // ThumbSmith Components (direct references since everything is in the same assembly)
        private static ThumbnailController thumbnailControllerInstance = null;

        /// <summary>
        /// Creates the ThumbSmith tab content
        /// </summary>
        public static VisualElement CreateThumbSmithContent()
        {
            var content = new VisualElement();
            content.style.flexGrow = 1;

            try
            {
                // Create modern UI Toolkit interface for ThumbSmith
                CreateThumbSmithUIToolkitContent(content);
            }
            catch (System.Exception)
            {
                return CreateThumbSmithFallbackContent();
            }

            return content;
        }

        /// <summary>
        /// Creates modern UI Toolkit content for ThumbSmith
        /// </summary>
        private static void CreateThumbSmithUIToolkitContent(VisualElement content)
        {
            // Main container with proper styling
            content.style.paddingTop = 10;
            content.style.paddingBottom = 10;
            content.style.paddingLeft = 15;
            content.style.paddingRight = 15;

            // Header section
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 15;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = new UnityEngine.Color(0.3f, 0.3f, 0.3f, 1f);
            header.style.paddingBottom = 10;

            var title = new Label("ThumbSmith - Thumbnail Generator");
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new UnityEngine.Color(0.9f, 0.9f, 0.9f, 1f);

            var version = new Label("v2.0");
            version.style.fontSize = 12;
            version.style.color = new UnityEngine.Color(0.6f, 0.6f, 0.6f, 1f);
            version.style.marginLeft = 10;

            header.Add(title);
            header.Add(version);
            content.Add(header);

            // Create scrollable content area
            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.style.marginTop = 10;

            // Input section
            var inputSection = CreateInputSection();
            scrollView.Add(inputSection);

            // Settings section
            var settingsSection = CreateSettingsSection();
            scrollView.Add(settingsSection);

            // Output section
            var outputSection = CreateOutputSection();
            scrollView.Add(outputSection);

            // Action buttons
            var actionSection = CreateActionSection();
            scrollView.Add(actionSection);

            content.Add(scrollView);

            // Initialize the UI with current values
            InitializeThumbSmithUI(content);
        }

        /// <summary>
        /// Creates a section container with title
        /// </summary>
        private static VisualElement CreateSectionContainer(string title)
        {
            var section = new VisualElement();
            section.style.marginBottom = 20;
            section.style.paddingTop = 10;
            section.style.paddingBottom = 10;
            section.style.paddingLeft = 10;
            section.style.paddingRight = 10;
                        section.style.backgroundColor = new UnityEngine.Color(0.15f, 0.15f, 0.15f, 1f);

            var titleLabel = new Label(title);
            titleLabel.style.fontSize = 14;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.color = new UnityEngine.Color(0.8f, 0.8f, 0.8f, 1f);
            titleLabel.style.marginBottom = 8;
            section.Add(titleLabel);

            return section;
        }

        /// <summary>
        /// Creates the settings section for thumbnail configuration
        /// </summary>
        private static VisualElement CreateSettingsSection()
        {
            var section = CreateSectionContainer("Thumbnail Settings");

            // Resolution settings
            var resolutionContainer = new VisualElement();
            resolutionContainer.style.flexDirection = FlexDirection.Row;
            resolutionContainer.style.alignItems = Align.Center;
            resolutionContainer.style.marginBottom = 10;

            var widthField = new IntegerField("Width");
            widthField.name = "thumbsmith-settings-width";
            widthField.value = 512;
            widthField.style.flexGrow = 1;
            widthField.style.marginRight = 5;

            var heightField = new IntegerField("Height");
            heightField.name = "thumbsmith-settings-height";
            heightField.value = 512;
            heightField.style.flexGrow = 1;
            heightField.style.marginLeft = 5;

            resolutionContainer.Add(widthField);
            resolutionContainer.Add(heightField);
            section.Add(resolutionContainer);

            // Background settings
            var backgroundColorField = new ColorField("Background Color");
            backgroundColorField.name = "thumbsmith-settings-background-color";
            backgroundColorField.value = UnityEngine.Color.white;
            backgroundColorField.style.marginBottom = 10;
            section.Add(backgroundColorField);

            // Gradient background toggle
            var useGradientToggle = new Toggle("Use Gradient Background");
            useGradientToggle.name = "thumbsmith-settings-use-gradient";
            useGradientToggle.style.marginBottom = 10;
            section.Add(useGradientToggle);

            // Gradient colors (hidden by default)
            var gradientStartField = new ColorField("Gradient Start");
            gradientStartField.name = "thumbsmith-settings-gradient-start";
            gradientStartField.value = UnityEngine.Color.white;
            gradientStartField.style.marginBottom = 5;
            gradientStartField.style.display = DisplayStyle.None;
            section.Add(gradientStartField);

            var gradientEndField = new ColorField("Gradient End");
            gradientEndField.name = "thumbsmith-settings-gradient-end";
            gradientEndField.value = UnityEngine.Color.gray;
            gradientEndField.style.marginBottom = 10;
            gradientEndField.style.display = DisplayStyle.None;
            section.Add(gradientEndField);

            // Animation settings
            var autoRotateToggle = new Toggle("Auto Rotate");
            autoRotateToggle.name = "thumbsmith-settings-auto-rotate";
            autoRotateToggle.style.marginBottom = 10;
            section.Add(autoRotateToggle);

            var rotationSpeedField = new FloatField("Rotation Speed (°/sec)");
            rotationSpeedField.name = "thumbsmith-settings-rotation-speed";
            rotationSpeedField.value = 30f;
            rotationSpeedField.style.display = DisplayStyle.None;
            section.Add(rotationSpeedField);

            // Custom lighting toggle
            var useCustomLightingToggle = new Toggle("Use Custom Lighting");
            useCustomLightingToggle.name = "thumbsmith-settings-use-custom-lighting";
            useCustomLightingToggle.style.marginBottom = 10;
            section.Add(useCustomLightingToggle);

            // Lighting settings (hidden by default)
            var lightIntensityField = new FloatField("Light Intensity");
            lightIntensityField.name = "thumbsmith-settings-light-intensity";
            lightIntensityField.value = 1f;
            lightIntensityField.style.display = DisplayStyle.None;
            section.Add(lightIntensityField);

            var lightColorField = new ColorField("Light Color");
            lightColorField.name = "thumbsmith-settings-light-color";
            lightColorField.value = UnityEngine.Color.white;
            lightColorField.style.display = DisplayStyle.None;
            section.Add(lightColorField);

            var lightDirectionField = new Vector3Field("Light Direction");
            lightDirectionField.name = "thumbsmith-settings-light-direction";
            lightDirectionField.value = new Vector3(-1, 1, -1);
            lightDirectionField.style.display = DisplayStyle.None;
            section.Add(lightDirectionField);

            // Connect visibility toggles
            useGradientToggle.RegisterValueChangedCallback(evt =>
            {
                gradientStartField.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                gradientEndField.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            autoRotateToggle.RegisterValueChangedCallback(evt =>
            {
                rotationSpeedField.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            useCustomLightingToggle.RegisterValueChangedCallback(evt =>
            {
                lightIntensityField.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                lightColorField.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                lightDirectionField.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return section;
        }

        /// <summary>
        /// Creates the output section for specifying where thumbnails are saved
        /// </summary>
        private static VisualElement CreateOutputSection()
        {
            var section = CreateSectionContainer("Output Settings");

            // Output path field
            var outputPathContainer = new VisualElement();
            outputPathContainer.style.flexDirection = FlexDirection.Row;
            outputPathContainer.style.alignItems = Align.FlexStart;
            outputPathContainer.style.marginBottom = 10;

            var outputPathField = new TextField("Output Path");
            outputPathField.name = "thumbsmith-output-path";
            outputPathField.value = "ThumbSmith_Thumbnails";
            outputPathField.style.flexGrow = 1;
            outputPathField.style.marginRight = 5;
            outputPathContainer.Add(outputPathField);

            var browseBtn = new Button(() => BrowseThumbSmithOutputPath(section));
            browseBtn.text = "Browse...";
            browseBtn.name = "thumbsmith-output-browse";
            browseBtn.style.width = 80;
            outputPathContainer.Add(browseBtn);

            section.Add(outputPathContainer);

            // File format selection
            var formatField = new DropdownField("File Format", new System.Collections.Generic.List<string> { "PNG", "JPG", "TGA" }, 0);
            formatField.name = "thumbsmith-output-format";
            formatField.style.marginBottom = 10;
            section.Add(formatField);

            // Quality settings (for JPG)
            var qualityField = new IntegerField("Quality (%)");
            qualityField.name = "thumbsmith-output-quality";
            qualityField.value = 90;
            qualityField.style.marginBottom = 10;
            section.Add(qualityField);

            // Connect format change to quality visibility
            formatField.RegisterValueChangedCallback(evt =>
            {
                qualityField.style.display = evt.newValue == "JPG" ? DisplayStyle.Flex : DisplayStyle.None;
            });

            // Initial visibility
            qualityField.style.display = DisplayStyle.None;

            return section;
        }

        /// <summary>
        /// Creates the action section with control buttons
        /// </summary>
        private static VisualElement CreateActionSection()
        {
            var section = CreateSectionContainer("Actions");

            // Button container
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.SpaceBetween;
            buttonContainer.style.marginBottom = 15;

            var generateBtn = new Button(() => GenerateThumbSmithThumbnail(section));
            generateBtn.text = "Generate Thumbnail";
            generateBtn.name = "thumbsmith-controls-generate";
            generateBtn.style.flexGrow = 1;
            generateBtn.style.height = 30;
            generateBtn.style.marginRight = 5;
            buttonContainer.Add(generateBtn);

            var openThumbnailerBtn = new Button(() => OpenThumbSmithThumbnailer(section));
            openThumbnailerBtn.text = "Open Thumbnailer";
            openThumbnailerBtn.name = "thumbsmith-controls-open-thumbnailer";
            openThumbnailerBtn.style.flexGrow = 1;
            openThumbnailerBtn.style.height = 30;
            openThumbnailerBtn.style.marginLeft = 5;
            buttonContainer.Add(openThumbnailerBtn);

            section.Add(buttonContainer);

            // Status and progress
            var statusText = new Label("Ready to generate thumbnails");
            statusText.name = "thumbsmith-status-text";
            statusText.style.fontSize = 12;
            statusText.style.color = new UnityEngine.Color(0.7f, 0.7f, 0.7f, 1f);
            statusText.style.marginBottom = 5;
            statusText.style.whiteSpace = WhiteSpace.Normal;
            section.Add(statusText);

            var progressBar = new ProgressBar();
            progressBar.name = "thumbsmith-progress-bar";
            progressBar.style.display = DisplayStyle.None;
            progressBar.lowValue = 0;
            progressBar.highValue = 100;
            section.Add(progressBar);

            return section;
        }

        /// <summary>
        /// Creates the input section for selecting objects to thumbnail
        /// </summary>
        private static VisualElement CreateInputSection()
        {
            var section = CreateSectionContainer("Input Selection");

            // Object selection
            var objectField = new ObjectField("Target Object");
            objectField.objectType = typeof(GameObject);
            objectField.tooltip = "Select the GameObject to create a thumbnail for";
            objectField.name = "targetObjectField";
            objectField.style.marginBottom = 10;
            section.Add(objectField);

            // Batch processing toggle
            var batchToggle = new Toggle("Batch Processing");
            batchToggle.tooltip = "Process multiple objects at once";
            batchToggle.name = "batchToggle";
            batchToggle.style.marginBottom = 10;
            section.Add(batchToggle);

            // Batch folder selection (shown when batch is enabled)
            var batchFolder = new ObjectField("Batch Folder");
            batchFolder.objectType = typeof(DefaultAsset);
            batchFolder.tooltip = "Select a folder containing multiple GameObjects";
            batchFolder.name = "batchFolderField";
            batchFolder.style.marginBottom = 10;
            batchFolder.style.display = DisplayStyle.None; // Hidden by default
            section.Add(batchFolder);

            // Connect batch toggle to folder visibility
            batchToggle.RegisterValueChangedCallback(evt =>
            {
                batchFolder.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                objectField.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
            });

            return section;
        }

        /// <summary>
        /// Creates fallback content when UXML loading fails
        /// </summary>
        private static VisualElement CreateThumbSmithFallbackContent()
        {
            var fallback = new VisualElement();
            fallback.style.flexGrow = 1;

            var scrollView = new ScrollView();
            fallback.Add(scrollView);

            var errorLabel = new Label("⚠️ ThumbSmith UI could not be loaded. Plugin may not be properly installed.");
            errorLabel.style.fontSize = 14;
            errorLabel.style.color = UnityEngine.Color.red;
            errorLabel.style.whiteSpace = WhiteSpace.Normal;
            scrollView.Add(errorLabel);

            var placeholder = new Label("ThumbSmith - Thumbnail Generator");
            placeholder.style.fontSize = 18;
            placeholder.style.unityFontStyleAndWeight = FontStyle.Bold;
            placeholder.style.marginBottom = 20;
            scrollView.Add(placeholder);

            var description = new Label("Generate high-quality thumbnails from 3D models with automatic lighting and camera positioning.");
            description.style.whiteSpace = WhiteSpace.Normal;
            description.style.marginBottom = 20;
            scrollView.Add(description);

            return fallback;
        }

        /// <summary>
        /// Initializes the ThumbSmith UI components and event handlers
        /// </summary>
        private static void InitializeThumbSmithUI(VisualElement root)
        {
            // Load ThumbSmith types via reflection
            InitializeThumbSmithComponents();

            // Bind fields using the new programmatic UI element names
            var targetField = root.Q<ObjectField>("targetObjectField");
            var outputPathField = root.Q<TextField>("thumbsmith-output-path");
            var widthField = root.Q<IntegerField>("thumbsmith-settings-width");
            var heightField = root.Q<IntegerField>("thumbsmith-settings-height");
            var backgroundColorField = root.Q<ColorField>("thumbsmith-settings-background-color");
            var useGradientToggle = root.Q<Toggle>("thumbsmith-settings-use-gradient");
            var gradientStartField = root.Q<ColorField>("thumbsmith-settings-gradient-start");
            var gradientEndField = root.Q<ColorField>("thumbsmith-settings-gradient-end");
            var autoRotateToggle = root.Q<Toggle>("thumbsmith-settings-auto-rotate");
            var rotationSpeedField = root.Q<FloatField>("thumbsmith-settings-rotation-speed");
            var useCustomLightingToggle = root.Q<Toggle>("thumbsmith-settings-use-custom-lighting");
            var lightIntensityField = root.Q<FloatField>("thumbsmith-settings-light-intensity");
            var lightColorField = root.Q<ColorField>("thumbsmith-settings-light-color");
            var lightDirectionField = root.Q<Vector3Field>("thumbsmith-settings-light-direction");
            var generateBtn = root.Q<Button>("thumbsmith-controls-generate");
            var openThumbnailerBtn = root.Q<Button>("thumbsmith-controls-open-thumbnailer");
            var statusText = root.Q<Label>("thumbsmith-status-text");
            var progressBar = root.Q<ProgressBar>("thumbsmith-progress-bar");

            // Set up field types and initial values
            if (targetField != null) targetField.objectType = typeof(GameObject);

            // Load persisted values
            LoadThumbSmithPersistedValues(root);

            // Set up event handlers
            if (targetField != null) targetField.RegisterValueChangedCallback(evt => {
                ValidateThumbSmithTarget(evt.newValue as GameObject, root);
                SaveThumbSmithValue("Target", evt.newValue);
            });

            if (outputPathField != null) outputPathField.RegisterValueChangedCallback(evt => SaveThumbSmithValue("OutputPath", evt.newValue));
            if (widthField != null) widthField.RegisterValueChangedCallback(evt => SaveThumbSmithValue("Width", evt.newValue));
            if (heightField != null) heightField.RegisterValueChangedCallback(evt => SaveThumbSmithValue("Height", evt.newValue));
            if (backgroundColorField != null) backgroundColorField.RegisterValueChangedCallback(evt => SaveThumbSmithValue("BackgroundColor", evt.newValue));
            if (useGradientToggle != null) useGradientToggle.RegisterValueChangedCallback(evt => {
                SaveThumbSmithValue("UseGradient", evt.newValue);
                UpdateThumbSmithGradientVisibility(evt.newValue, root);
            });
            if (gradientStartField != null) gradientStartField.RegisterValueChangedCallback(evt => SaveThumbSmithValue("GradientStart", evt.newValue));
            if (gradientEndField != null) gradientEndField.RegisterValueChangedCallback(evt => SaveThumbSmithValue("GradientEnd", evt.newValue));
            if (autoRotateToggle != null) autoRotateToggle.RegisterValueChangedCallback(evt => {
                SaveThumbSmithValue("AutoRotate", evt.newValue);
                UpdateThumbSmithRotationVisibility(evt.newValue, root);
            });
            if (rotationSpeedField != null) rotationSpeedField.RegisterValueChangedCallback(evt => SaveThumbSmithValue("RotationSpeed", evt.newValue));
            if (useCustomLightingToggle != null) useCustomLightingToggle.RegisterValueChangedCallback(evt => {
                SaveThumbSmithValue("UseCustomLighting", evt.newValue);
                UpdateThumbSmithLightingVisibility(evt.newValue, root);
            });
            if (lightIntensityField != null) lightIntensityField.RegisterValueChangedCallback(evt => SaveThumbSmithValue("LightIntensity", evt.newValue));
            if (lightColorField != null) lightColorField.RegisterValueChangedCallback(evt => SaveThumbSmithValue("LightColor", evt.newValue));
            if (lightDirectionField != null) lightDirectionField.RegisterValueChangedCallback(evt => SaveThumbSmithValue("LightDirection", evt.newValue));

            // Button handlers
            if (generateBtn != null) generateBtn.clicked += () => GenerateThumbSmithThumbnail(root);
            if (openThumbnailerBtn != null) openThumbnailerBtn.clicked += () => OpenThumbSmithThumbnailer(root);

            // Browse button handler (already set up in CreateOutputSection)

            // Initial visibility updates
            UpdateThumbSmithGradientVisibility(useGradientToggle != null ? useGradientToggle.value : false, root);
            UpdateThumbSmithRotationVisibility(autoRotateToggle != null ? autoRotateToggle.value : false, root);
            UpdateThumbSmithLightingVisibility(useCustomLightingToggle != null ? useCustomLightingToggle.value : false, root);

            if (targetField != null)
            {
                ValidateThumbSmithTarget(targetField.value as GameObject, root);
            }
        }

        /// <summary>
        /// Initializes ThumbSmith components directly (since everything is in the same assembly)
        /// </summary>
        private static void InitializeThumbSmithComponents()
        {
            try
            {
                if (thumbnailControllerInstance == null)
                {
                    // Create the services needed for ThumbnailController
                    var sceneStager = new SceneStager();
                    var prefabFramer = new PrefabFramer((ISceneStager)sceneStager);
                    var rendererService = new RendererService((ISceneStager)sceneStager);
                    var fileService = new FileService();

                    // Create ThumbnailController instance
                    thumbnailControllerInstance = new ThumbnailController(
                        (ISceneStager)sceneStager,
                        (IPrefabFramer)prefabFramer,
                        (IRendererService)rendererService,
                        (IFileService)fileService
                    );
                }
            }
            catch (System.Exception)
            {
                // Error initializing ThumbSmith components
            }
        }

        /// <summary>
        /// Loads persisted values for ThumbSmith settings
        /// </summary>
        private static void LoadThumbSmithPersistedValues(VisualElement root)
        {
            // Load values from EditorPrefs with ThumbSmith prefix
            var outputPathField = root.Q<TextField>("thumbsmith-output-path");
            var widthField = root.Q<IntegerField>("thumbsmith-settings-width");
            var heightField = root.Q<IntegerField>("thumbsmith-settings-height");
            var backgroundColorField = root.Q<ColorField>("thumbsmith-settings-background-color");
            var useGradientToggle = root.Q<Toggle>("thumbsmith-settings-use-gradient");
            var gradientStartField = root.Q<ColorField>("thumbsmith-settings-gradient-start");
            var gradientEndField = root.Q<ColorField>("thumbsmith-settings-gradient-end");
            var autoRotateToggle = root.Q<Toggle>("thumbsmith-settings-auto-rotate");
            var rotationSpeedField = root.Q<FloatField>("thumbsmith-settings-rotation-speed");
            var useCustomLightingToggle = root.Q<Toggle>("thumbsmith-settings-use-custom-lighting");
            var lightIntensityField = root.Q<FloatField>("thumbsmith-settings-light-intensity");
            var lightColorField = root.Q<ColorField>("thumbsmith-settings-light-color");
            var lightDirectionField = root.Q<Vector3Field>("thumbsmith-settings-light-direction");

            if (outputPathField != null) outputPathField.value = EditorPrefs.GetString("ThumbSmith.OutputPath", "ThumbSmith_Thumbnails");
            if (widthField != null) widthField.value = EditorPrefs.GetInt("ThumbSmith.Width", 512);
            if (heightField != null) heightField.value = EditorPrefs.GetInt("ThumbSmith.Height", 512);
            if (backgroundColorField != null) backgroundColorField.value = GetColorFromPrefs("ThumbSmith.BackgroundColor", UnityEngine.Color.white);
            if (useGradientToggle != null) useGradientToggle.value = EditorPrefs.GetBool("ThumbSmith.UseGradient", false);
            if (gradientStartField != null) gradientStartField.value = GetColorFromPrefs("ThumbSmith.GradientStart", UnityEngine.Color.white);
            if (gradientEndField != null) gradientEndField.value = GetColorFromPrefs("ThumbSmith.GradientEnd", UnityEngine.Color.gray);
            if (autoRotateToggle != null) autoRotateToggle.value = EditorPrefs.GetBool("ThumbSmith.AutoRotate", false);
            if (rotationSpeedField != null) rotationSpeedField.value = EditorPrefs.GetFloat("ThumbSmith.RotationSpeed", 30f);
            if (useCustomLightingToggle != null) useCustomLightingToggle.value = EditorPrefs.GetBool("ThumbSmith.UseCustomLighting", false);
            if (lightIntensityField != null) lightIntensityField.value = EditorPrefs.GetFloat("ThumbSmith.LightIntensity", 1f);
            if (lightColorField != null) lightColorField.value = GetColorFromPrefs("ThumbSmith.LightColor", UnityEngine.Color.white);
            if (lightDirectionField != null) lightDirectionField.value = GetVector3FromPrefs("ThumbSmith.LightDirection", new Vector3(-1, 1, -1));
        }

        /// <summary>
        /// Saves a ThumbSmith value to EditorPrefs
        /// </summary>
        private static void SaveThumbSmithValue(string key, object value)
        {
            string prefKey = $"ThumbSmith.{key}";

            if (value is string str) EditorPrefs.SetString(prefKey, str);
            else if (value is int i) EditorPrefs.SetInt(prefKey, i);
            else if (value is float f) EditorPrefs.SetFloat(prefKey, f);
            else if (value is bool b) EditorPrefs.SetBool(prefKey, b);
            else if (value is Color c) SetColorToPrefs(prefKey, c);
            else if (value is Vector3 v) SetVector3ToPrefs(prefKey, v);
            else if (value is UnityEngine.Object obj) EditorPrefs.SetString(prefKey, obj != null ? AssetDatabase.GetAssetPath(obj) : "");
        }

        /// <summary>
        /// Validates the ThumbSmith target object
        /// </summary>
        private static void ValidateThumbSmithTarget(GameObject target, VisualElement root)
        {
            var statusText = root.Q<Label>("thumbsmith-status-text");
            var generateBtn = root.Q<Button>("thumbsmith-controls-generate");

            if (target == null)
            {
                if (statusText != null) statusText.text = "No target selected.";
                if (generateBtn != null) generateBtn.SetEnabled(false);
                return;
            }

            if (target.GetComponent<Renderer>() == null && target.GetComponentsInChildren<Renderer>().Length == 0)
            {
                if (statusText != null) statusText.text = "Warning: Target has no renderers.";
                if (generateBtn != null) generateBtn.SetEnabled(true);
                return;
            }

            if (statusText != null) statusText.text = "Target validated.";
            if (generateBtn != null) generateBtn.SetEnabled(true);
        }

        /// <summary>
        /// Updates gradient field visibility
        /// </summary>
        private static void UpdateThumbSmithGradientVisibility(bool useGradient, VisualElement root)
        {
            var gradientStartField = root.Q<ColorField>("thumbsmith-settings-gradient-start");
            var gradientEndField = root.Q<ColorField>("thumbsmith-settings-gradient-end");

            if (gradientStartField != null) gradientStartField.style.display = useGradient ? DisplayStyle.Flex : DisplayStyle.None;
            if (gradientEndField != null) gradientEndField.style.display = useGradient ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Updates rotation field visibility
        /// </summary>
        private static void UpdateThumbSmithRotationVisibility(bool autoRotate, VisualElement root)
        {
            var rotationSpeedField = root.Q<FloatField>("thumbsmith-settings-rotation-speed");
            if (rotationSpeedField != null) rotationSpeedField.style.display = autoRotate ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Updates lighting field visibility
        /// </summary>
        private static void UpdateThumbSmithLightingVisibility(bool useCustomLighting, VisualElement root)
        {
            var lightIntensityField = root.Q<FloatField>("thumbsmith-settings-light-intensity");
            var lightColorField = root.Q<ColorField>("thumbsmith-settings-light-color");
            var lightDirectionField = root.Q<Vector3Field>("thumbsmith-settings-light-direction");

            var display = useCustomLighting ? DisplayStyle.Flex : DisplayStyle.None;
            if (lightIntensityField != null) lightIntensityField.style.display = display;
            if (lightColorField != null) lightColorField.style.display = display;
            if (lightDirectionField != null) lightDirectionField.style.display = display;
        }

        /// <summary>
        /// Generates a thumbnail using ThumbSmith
        /// </summary>
        private static void GenerateThumbSmithThumbnail(VisualElement root)
        {
            var targetField = root.Q<UnityEditor.UIElements.ObjectField>("thumbsmith-input-target");
            var statusText = root.Q<Label>("thumbsmith-status-text");
            var progressBar = root.Q<ProgressBar>("thumbsmith-progress-bar");

            var target = targetField?.value as GameObject;
            if (target == null)
            {
                if (statusText != null) statusText.text = "Error: No target selected.";
                return;
            }

            // Use reflection to call ThumbSmith methods
            if (thumbnailControllerInstance == null)
            {
                if (statusText != null) statusText.text = "Error: ThumbSmith not available.";
                return;
            }

            try
            {
                // Get settings from UI
                var outputPathField = root.Q<TextField>("thumbsmith-output-path");
                var widthField = root.Q<IntegerField>("thumbsmith-settings-width");
                var heightField = root.Q<IntegerField>("thumbsmith-settings-height");
                var backgroundColorField = root.Q<ColorField>("thumbsmith-settings-background-color");
                var useGradientToggle = root.Q<Toggle>("thumbsmith-settings-use-gradient");
                var gradientStartField = root.Q<ColorField>("thumbsmith-settings-gradient-start");
                var gradientEndField = root.Q<ColorField>("thumbsmith-settings-gradient-end");
                var autoRotateToggle = root.Q<Toggle>("thumbsmith-settings-auto-rotate");
                var rotationSpeedField = root.Q<FloatField>("thumbsmith-settings-rotation-speed");
                var useCustomLightingToggle = root.Q<Toggle>("thumbsmith-settings-use-custom-lighting");
                var lightIntensityField = root.Q<FloatField>("thumbsmith-settings-light-intensity");
                var lightColorField = root.Q<ColorField>("thumbsmith-settings-light-color");
                var lightDirectionField = root.Q<Vector3Field>("thumbsmith-settings-light-direction");

                // Create thumbnail config from UI settings
                var config = CreateThumbSmithConfig(root);

                // Start thumbnail generation directly
                if (thumbnailControllerInstance != null)
                {
                    if (progressBar != null)
                    {
                        progressBar.value = 0;
                        progressBar.style.display = DisplayStyle.Flex;
                    }

                    // Hook up progress monitoring
                    thumbnailControllerInstance.OnProgress += (progress) => {
                        if (progressBar != null) progressBar.value = progress.Normalized * 100;
                        if (statusText != null) statusText.text = progress.Message;
                    };
                    thumbnailControllerInstance.OnCompleted += () => {
                        if (progressBar != null) progressBar.style.display = DisplayStyle.None;
                        if (statusText != null) statusText.text = "Thumbnail generation completed!";
                        EditorApplication.update -= () => MonitorThumbSmithProgress(root);
                    };

                    // Start generation
                    bool started = thumbnailControllerInstance.Start(config);
                    if (started)
                    {
                        if (statusText != null) statusText.text = "Generating thumbnails...";
                        // Monitor progress
                        EditorApplication.update += () => MonitorThumbSmithProgress(root);
                    }
                    else
                    {
                        if (statusText != null) statusText.text = "Failed to start thumbnail generation.";
                    }
                }
                else
                {
                    if (statusText != null) statusText.text = "Error: ThumbnailController not initialized.";
                }
            }
            catch (System.Exception)
            {
                if (statusText != null) statusText.text = "Error generating thumbnail";
            }
        }

        /// <summary>
        /// Creates thumbnail config from UI settings
        /// </summary>
        private static ThumbnailConfig CreateThumbSmithConfig(VisualElement root)
        {
            var config = new ThumbnailConfig();

            // Get values from UI fields
            var inputFolderField = root.Q<TextField>("thumbsmith-input-folder");
            var outputPathField = root.Q<TextField>("thumbsmith-output-path");
            var widthField = root.Q<IntegerField>("thumbsmith-settings-width");
            var heightField = root.Q<IntegerField>("thumbsmith-settings-height");
            var backgroundColorField = root.Q<ColorField>("thumbsmith-settings-background-color");

            // Set basic config values
            config.inputFolder = inputFolderField?.value ?? "Assets/Prefabs";
            config.outputFolder = outputPathField?.value ?? "Assets/Thumbnails";
            config.outputResolution = widthField?.value ?? 512;

            // For now, use basic defaults for other settings
            // This can be expanded to read all UI fields
            config.margin = 0.1f;
            config.orientation = ThumbnailConfig.Orientation.Front;
            config.cameraMode = ThumbnailConfig.CameraMode.Orthographic;

            return config;
        }

        /// <summary>
        /// Monitors ThumbSmith generation progress
        /// </summary>
        private static void MonitorThumbSmithProgress(VisualElement root)
        {
            // Implementation depends on ThumbSmith's progress reporting API
            // This is a placeholder
            var progressBar = root.Q<ProgressBar>("thumbsmith-progress-bar");
            var statusText = root.Q<Label>("thumbsmith-status-text");

            if (progressBar != null)
            {
                progressBar.value = 100;
                progressBar.style.display = DisplayStyle.None;
            }

            if (statusText != null)
            {
                statusText.text = "Thumbnail generation completed!";
            }

            EditorApplication.update -= () => MonitorThumbSmithProgress(root);
        }

        /// <summary>
        /// Opens the ThumbSmith Thumbnailer window
        /// </summary>
        private static void OpenThumbSmithThumbnailer(VisualElement root)
        {
            try
            {
                // Call the static Open method directly
                ThumbnailerWindow.Open();
            }
            catch (System.Exception)
            {
                // Error opening Thumbnailer window
            }
        }

        /// <summary>
        /// Browses for ThumbSmith output path
        /// </summary>
        private static void BrowseThumbSmithOutputPath(VisualElement root)
        {
            var outputPathField = root.Q<TextField>("thumbsmith-output-path");

            string path = EditorUtility.OpenFolderPanel("Select Output Folder", outputPathField?.value ?? "", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                if (outputPathField != null)
                {
                    outputPathField.value = path;
                    SaveThumbSmithValue("OutputPath", path);
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