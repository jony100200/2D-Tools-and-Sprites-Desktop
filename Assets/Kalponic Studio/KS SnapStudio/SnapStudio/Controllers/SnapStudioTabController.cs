// SnapStudioTabController.cs
// Controller for SnapStudio tab functionality
// Following SOLID principles - Single Responsibility for SnapStudio tab management

using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System.IO;
using KalponicGames.KS_SnapStudio.Editor;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// Controller class for SnapStudio tab functionality
    /// Handles all SnapStudio-specific UI creation and event handling
    /// </summary>
    public static class SnapStudioTabController
    {
        // SnapStudio Components (shared across the application)
        private static KalponicGames.KS_SnapStudio.KSPlayModeCapture currentCaptureComponent = null;

        /// <summary>
        /// Creates the SnapStudio tab content
        /// </summary>
        public static VisualElement CreateSnapStudioContent()
        {
            var content = new VisualElement();
            content.style.flexGrow = 1;

            try
            {
                // Create modern UI Toolkit interface for SnapStudio
                CreateSnapStudioUIToolkitContent(content);
            }
            catch (System.Exception)
            {
                return CreateSnapStudioFallbackContent();
            }

            return content;
        }

        /// <summary>
        /// Creates modern UI Toolkit content for SnapStudio
        /// </summary>
        private static void CreateSnapStudioUIToolkitContent(VisualElement content)
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

            var title = new Label("KS SnapStudio - Animation Capture");
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new UnityEngine.Color(0.9f, 0.9f, 0.9f, 1f);

            var version = new Label("v1.0");
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

            // Input/Source section
            var inputSection = CreateInputSection();
            scrollView.Add(inputSection);

            // Settings section
            var settingsSection = CreateSettingsSection();
            scrollView.Add(settingsSection);

            // Output section
            var outputSection = CreateOutputSection();
            scrollView.Add(outputSection);

            // Controls section
            var controlsSection = CreateControlsSection();
            scrollView.Add(controlsSection);

            content.Add(scrollView);

            // Status bar
            var statusBar = CreateStatusBar();
            content.Add(statusBar);

            // Initialize the UI with current values
            InitializeSnapStudioUI(content);
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
        /// Creates the input/source section
        /// </summary>
        private static VisualElement CreateInputSection()
        {
            var section = CreateSectionContainer("Input/Source");

            // Target object field
            var targetField = new ObjectField("Target Object");
            targetField.objectType = typeof(GameObject);
            targetField.name = "ks-snapstudio-input-target";
            targetField.tooltip = "Select the character GameObject that contains Animator or Animation components to capture animations from.";
            targetField.style.marginBottom = 10;
            section.Add(targetField);

            // Validation indicator
            var validationIndicator = new VisualElement();
            validationIndicator.name = "ks-snapstudio-target-validation-indicator";
            validationIndicator.AddToClassList("validation-indicator");
            validationIndicator.style.width = 16;
            validationIndicator.style.height = 16;
            validationIndicator.style.backgroundColor = UnityEngine.Color.yellow;
            validationIndicator.style.marginBottom = 10;
            section.Add(validationIndicator);

            // Help box
            var helpBox = new HelpBox("Select the GameObject with Animator or Animation component.", HelpBoxMessageType.Info);
            section.Add(helpBox);

            return section;
        }

        /// <summary>
        /// Creates the settings section
        /// </summary>
        private static VisualElement CreateSettingsSection()
        {
            var section = CreateSectionContainer("Settings");

            // Resolution settings
            var resolutionContainer = new VisualElement();
            resolutionContainer.style.flexDirection = FlexDirection.Row;
            resolutionContainer.style.alignItems = Align.Center;
            resolutionContainer.style.marginBottom = 10;

            var widthField = new IntegerField("Width");
            widthField.name = "ks-snapstudio-settings-width";
            widthField.value = 1024;
            widthField.tooltip = "Output image width in pixels. Higher values create larger sprites but increase file size.";
            widthField.style.flexGrow = 1;
            widthField.style.marginRight = 5;

            var heightField = new IntegerField("Height");
            heightField.name = "ks-snapstudio-settings-height";
            heightField.value = 1024;
            heightField.tooltip = "Output image height in pixels. Higher values create larger sprites but increase file size.";
            heightField.style.flexGrow = 1;
            heightField.style.marginLeft = 5;

            resolutionContainer.Add(widthField);
            resolutionContainer.Add(heightField);
            section.Add(resolutionContainer);

            // FPS and Max Frames
            var timingContainer = new VisualElement();
            timingContainer.style.flexDirection = FlexDirection.Row;
            timingContainer.style.alignItems = Align.Center;
            timingContainer.style.marginBottom = 10;

            var fpsField = new IntegerField("FPS");
            fpsField.name = "ks-snapstudio-settings-fps";
            fpsField.value = 24;
            fpsField.tooltip = "Frames per second for animation capture. Higher values create smoother animations but more files.";
            fpsField.style.flexGrow = 1;
            fpsField.style.marginRight = 5;

            var maxFramesField = new IntegerField("Max Frames");
            maxFramesField.name = "ks-snapstudio-settings-max-frames";
            maxFramesField.value = 24;
            maxFramesField.tooltip = "Maximum frames to capture per animation. Limits capture time and file count.";
            maxFramesField.style.flexGrow = 1;
            maxFramesField.style.marginLeft = 5;

            timingContainer.Add(fpsField);
            timingContainer.Add(maxFramesField);
            section.Add(timingContainer);

            // Quality settings
            var trimSpritesToggle = new Toggle("Trim Sprites");
            trimSpritesToggle.name = "ks-snapstudio-settings-trim-sprites";
            trimSpritesToggle.value = true;
            trimSpritesToggle.tooltip = "Remove transparent borders around sprites. When disabled, shows Character Fill % control for auto-sizing.";
            trimSpritesToggle.style.marginBottom = 10;
            section.Add(trimSpritesToggle);

            // Character fill slider (created programmatically as in original)
            var characterFillField = new Slider("Character Fill %", 10, 100) { value = 75 };
            characterFillField.name = "ks-snapstudio-settings-character-fill";
            characterFillField.tooltip = "How much of the frame the character should fill when trimming is disabled. Higher values make characters appear larger.";
            characterFillField.style.marginBottom = 10;
            section.Add(characterFillField);

            var mirrorSpritesToggle = new Toggle("Mirror Sprites");
            mirrorSpritesToggle.name = "ks-snapstudio-settings-mirror-sprites";
            mirrorSpritesToggle.value = true;
            mirrorSpritesToggle.tooltip = "Automatically create left-facing versions of right-facing animations for complete character sets.";
            mirrorSpritesToggle.style.marginBottom = 10;
            section.Add(mirrorSpritesToggle);

            var pixelSizeField = new IntegerField("Pixel Size");
            pixelSizeField.name = "ks-snapstudio-settings-pixel-size";
            pixelSizeField.value = 16;
            pixelSizeField.tooltip = "Pixelation level for retro/stylized look. Higher values create more visible pixels.";
            pixelSizeField.style.marginBottom = 10;
            section.Add(pixelSizeField);

            var hdrToggle = new Toggle("HDR Capture (if available)");
            hdrToggle.name = "ks-snapstudio-settings-hdr";
            hdrToggle.value = false;
            hdrToggle.tooltip = "Use HDR rendering for better quality in high dynamic range scenes. Only available on compatible hardware.";
            hdrToggle.style.marginBottom = 10;
            section.Add(hdrToggle);

            // Animation settings
            var animatorControllerField = new ObjectField("Animator Controller (Optional)");
            animatorControllerField.objectType = typeof(RuntimeAnimatorController);
            animatorControllerField.name = "ks-snapstudio-settings-animator-controller";
            animatorControllerField.tooltip = "Optional: Select an Animator Controller to help populate available animations.";
            animatorControllerField.style.marginBottom = 10;
            section.Add(animatorControllerField);

            var captureAllToggle = new Toggle("Capture All Animations");
            captureAllToggle.name = "ks-snapstudio-settings-capture-all";
            captureAllToggle.value = false;
            captureAllToggle.tooltip = "Capture all animations from the character. When disabled, shows Animation to Capture selector.";
            captureAllToggle.style.marginBottom = 10;
            section.Add(captureAllToggle);

            var animationClipField = new ObjectField("Animation to Capture");
            animationClipField.objectType = typeof(AnimationClip);
            animationClipField.name = "ks-snapstudio-settings-animation-select";
            animationClipField.tooltip = "Select which specific animation to capture when Capture All Animations is disabled.";
            animationClipField.style.marginBottom = 10;
            section.Add(animationClipField);

            return section;
        }

        /// <summary>
        /// Creates the output section
        /// </summary>
        private static VisualElement CreateOutputSection()
        {
            var section = CreateSectionContainer("Output");

            // Output path
            var outputPathField = new TextField("Output Path");
            outputPathField.name = "ks-snapstudio-output-path";
            outputPathField.value = "KS_SnapStudio_Renders";
            outputPathField.tooltip = "Folder name where captured sprites will be saved. Created relative to your project's Assets folder.";
            outputPathField.style.marginBottom = 10;
            section.Add(outputPathField);

            // Browse button
            var browseBtn = new Button(() => { /* TODO: Implement browse functionality */ });
            browseBtn.name = "ks-snapstudio-output-browse";
            browseBtn.text = "Browse...";
            browseBtn.tooltip = "Open folder browser to select a different output location.";
            section.Add(browseBtn);

            return section;
        }

        /// <summary>
        /// Creates the controls section
        /// </summary>
        private static VisualElement CreateControlsSection()
        {
            var section = CreateSectionContainer("Controls");

            // Control buttons
            var createSceneBtn = new Button(() => { /* TODO: Implement functionality */ });
            createSceneBtn.name = "ks-snapstudio-controls-create-scene";
            createSceneBtn.text = "Create Capture Scene";
            createSceneBtn.tooltip = "Set up the capture environment with proper lighting and camera. Required before capturing.";
            createSceneBtn.style.marginBottom = 5;
            section.Add(createSceneBtn);

            var refreshUIBtn = new Button(() => { /* TODO: Implement functionality */ });
            refreshUIBtn.name = "ks-snapstudio-controls-refresh-ui";
            refreshUIBtn.text = "Refresh UI (Debug)";
            refreshUIBtn.tooltip = "Refresh the interface elements. Use if UI appears unresponsive.";
            refreshUIBtn.style.marginBottom = 5;
            section.Add(refreshUIBtn);

            var testBtn = new Button(() => { /* TODO: Implement functionality */ });
            testBtn.name = "ks-snapstudio-controls-test";
            testBtn.text = "Test Configuration";
            testBtn.tooltip = "Validate your current settings without starting a full capture.";
            testBtn.style.marginBottom = 5;
            section.Add(testBtn);

            var startCaptureBtn = new Button(() => { /* TODO: Implement functionality */ });
            startCaptureBtn.name = "ks-snapstudio-controls-start-capture";
            startCaptureBtn.text = "Start Capture";
            startCaptureBtn.tooltip = "Begin capturing animation frames. Make sure Play Mode is active and scene is set up.";
            startCaptureBtn.style.marginBottom = 5;
            section.Add(startCaptureBtn);

            return section;
        }

        /// <summary>
        /// Creates the status bar
        /// </summary>
        private static VisualElement CreateStatusBar()
        {
            var statusBar = new VisualElement();
            statusBar.style.flexDirection = FlexDirection.Row;
            statusBar.style.alignItems = Align.Center;
            statusBar.style.paddingTop = 10;
            statusBar.style.paddingBottom = 10;
            statusBar.style.paddingLeft = 15;
            statusBar.style.paddingRight = 15;
            statusBar.style.backgroundColor = new UnityEngine.Color(0.1f, 0.1f, 0.1f, 1f);
            statusBar.style.borderTopWidth = 1;
            statusBar.style.borderTopColor = new UnityEngine.Color(0.3f, 0.3f, 0.3f, 1f);

            var statusText = new Label("Ready");
            statusText.name = "status-text";
            statusText.style.flexGrow = 1;
            statusBar.Add(statusText);

            var progressBar = new ProgressBar();
            progressBar.name = "progress-bar";
            progressBar.value = 0;
            progressBar.style.width = 200;
            progressBar.style.marginLeft = 10;
            statusBar.Add(progressBar);

            return statusBar;
        }

        /// <summary>
        /// Creates fallback content when UXML loading fails
        /// </summary>
        private static VisualElement CreateSnapStudioFallbackContent()
        {
            var fallback = new VisualElement();
            fallback.style.flexGrow = 1;

            var scrollView = new ScrollView();
            fallback.Add(scrollView);

            var errorLabel = new Label("⚠️ KS SnapStudio UI could not be loaded. Plugin may not be properly installed.");
            errorLabel.style.fontSize = 14;
            errorLabel.style.color = UnityEngine.Color.red;
            errorLabel.style.whiteSpace = WhiteSpace.Normal;
            scrollView.Add(errorLabel);

            var placeholder = new Label("KS SnapStudio - Animation Capture");
            placeholder.style.fontSize = 18;
            placeholder.style.unityFontStyleAndWeight = FontStyle.Bold;
            placeholder.style.marginBottom = 20;
            scrollView.Add(placeholder);

            var description = new Label("Capture high-quality sprite animations from 3D models with automatic pivot calculation and multi-angle support.");
            description.style.whiteSpace = WhiteSpace.Normal;
            description.style.marginBottom = 20;
            scrollView.Add(description);

            return fallback;
        }

        /// <summary>
        /// Initializes the SnapStudio UI components and event handlers
        /// </summary>
        private static void InitializeSnapStudioUI(VisualElement root)
        {
            // Bind fields - using the same element names as in SnapStudioWindow.uxml
            var targetField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-input-target");
            var animatorControllerField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-settings-animator-controller");
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
            var animationClipField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-settings-animation-select");
            var outputPathField = root.Q<TextField>("ks-snapstudio-output-path");
            var createSceneBtn = root.Q<Button>("ks-snapstudio-controls-create-scene");
            var refreshUIBtn = root.Q<Button>("ks-snapstudio-controls-refresh-ui");
            var testBtn = root.Q<Button>("ks-snapstudio-controls-test");
            var startCaptureBtn = root.Q<Button>("ks-snapstudio-controls-start-capture");
            var statusText = root.Q<Label>("status-text");
            var progressBar = root.Q<ProgressBar>("progress-bar");

            // Set up field types and initial values
            if (targetField != null) targetField.objectType = typeof(GameObject);
            if (animatorControllerField != null) animatorControllerField.objectType = typeof(RuntimeAnimatorController);
            if (animationClipField != null) animationClipField.objectType = typeof(AnimationClip);

            // Initialize AnimatorController field
            if (animatorControllerField != null)
            {
                SnapStudioPersistence.LoadPersistedAnimatorController(root);
            }
            else
            {
                // AnimatorController field not found
            }

            // Load persisted values
            SnapStudioPersistence.LoadPersistedValues(root);

            // Set up event handlers
            if (targetField != null) targetField.RegisterValueChangedCallback(evt => {
                SnapStudioValidation.ValidateTarget(evt.newValue as GameObject, root);
                SnapStudioPersistence.SaveTargetReference(evt.newValue as GameObject);
            });

            if (widthField != null) widthField.RegisterValueChangedCallback(evt => SnapStudioPersistence.SaveValue("Width", evt.newValue));
            if (heightField != null) heightField.RegisterValueChangedCallback(evt => SnapStudioPersistence.SaveValue("Height", evt.newValue));
            if (fpsField != null) fpsField.RegisterValueChangedCallback(evt => SnapStudioPersistence.SaveValue("Fps", evt.newValue));
            if (maxFramesField != null) maxFramesField.RegisterValueChangedCallback(evt => SnapStudioPersistence.SaveValue("MaxFrames", evt.newValue));
            if (trimSpritesToggle != null) trimSpritesToggle.RegisterValueChangedCallback(evt => {
                SnapStudioPersistence.SaveValue("TrimSprites", evt.newValue);
                SnapStudioUIHelpers.UpdateCharacterFillVisibility(evt.newValue, root);
            });
            if (characterFillField != null) characterFillField.RegisterValueChangedCallback(evt => SnapStudioPersistence.SaveValue("CharacterFill", evt.newValue));
            if (mirrorSpritesToggle != null) mirrorSpritesToggle.RegisterValueChangedCallback(evt => SnapStudioPersistence.SaveValue("MirrorSprites", evt.newValue));
            if (hdrToggle != null) hdrToggle.RegisterValueChangedCallback(evt => SnapStudioPersistence.SaveValue("HDR", evt.newValue));
            if (pixelSizeField != null) pixelSizeField.RegisterValueChangedCallback(evt => SnapStudioPersistence.SaveValue("PixelSize", evt.newValue));
            if (captureAllToggle != null) captureAllToggle.RegisterValueChangedCallback(evt => {
                SnapStudioPersistence.SaveValue("CaptureAll", evt.newValue);
                SnapStudioUIHelpers.UpdateAnimationFieldVisibility(evt.newValue, root);
            });
            if (animatorControllerField != null) animatorControllerField.RegisterValueChangedCallback(evt => SnapStudioPersistence.OnAnimatorControllerChanged(evt.newValue as RuntimeAnimatorController));
            if (outputPathField != null) outputPathField.RegisterValueChangedCallback(evt => SnapStudioPersistence.SaveValue("OutputPath", evt.newValue));

            // Button handlers
            if (createSceneBtn != null) createSceneBtn.clicked += () => CreateSnapStudioCaptureScene(root);
            if (refreshUIBtn != null) refreshUIBtn.clicked += () => RefreshSnapStudioUI(root);
            if (testBtn != null) testBtn.clicked += () => TestSnapStudioConfiguration(root);
            if (startCaptureBtn != null) startCaptureBtn.clicked += () => StartSnapStudioCapture(root);

            // Browse button
            var browseBtn = root.Q<Button>("ks-snapstudio-output-browse");
            if (browseBtn != null)
            {
                browseBtn.clicked += () => BrowseSnapStudioOutputPath(root);
            }

            // Initial validation and setup
            SnapStudioUIHelpers.UpdateAnimationFieldVisibility(captureAllToggle != null ? captureAllToggle.value : false, root);
            SnapStudioUIHelpers.UpdateCharacterFillVisibility(trimSpritesToggle != null ? trimSpritesToggle.value : true, root);
            SnapStudioUIHelpers.UpdateCreateSceneButtonState(root);

            if (targetField != null)
            {
                SnapStudioValidation.ValidateTarget(targetField.value as GameObject, root);
            }
        }

        /// <summary>
        /// Creates a capture scene for SnapStudio
        /// </summary>
        private static void CreateSnapStudioCaptureScene(VisualElement root)
        {
            // Check if capture elements are already present in current scene
            if (SnapStudioUIHelpers.HasCaptureElementsInCurrentScene())
            {
                var statusText = root.Q<Label>("status-text");
                if (statusText != null)
                    statusText.text = "Capture scene elements already present in current scene. Clear them first if needed.";
                return;
            }

            // Check if SceneBuilder thinks it's ready (separate capture scene)
            if (SceneBuilder.IsCaptureSceneReady())
            {
                var statusText = root.Q<Label>("status-text");
                if (statusText != null) statusText.text = "Capture scene is already set up.";
                return;
            }

            // Get current settings
            var trimSpritesToggle = root.Q<Toggle>("ks-snapstudio-settings-trim-sprites");
            var characterFillField = root.Q<Slider>("ks-snapstudio-settings-character-fill");
            var statusLabel = root.Q<Label>("status-text");

            bool trimSprites = trimSpritesToggle != null ? trimSpritesToggle.value : true;
            float characterFill = characterFillField != null ? (float)characterFillField.value : 75f;

            SceneBuilder.CreateCaptureScene(trimSprites, characterFill);
            if (statusLabel != null) statusLabel.text = "Capture scene created with auto-fit settings.";
            SnapStudioUIHelpers.UpdateCreateSceneButtonState(root);
        }

        /// <summary>
        /// Refreshes the SnapStudio UI
        /// </summary>
        private static void RefreshSnapStudioUI(VisualElement root)
        {
            // Re-query elements
            var animatorControllerField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-settings-animator-controller");

            if (animatorControllerField != null)
            {
                // AnimatorController field found
            }
            else
            {
                // AnimatorController field not found
            }
        }

        /// <summary>
        /// Tests the SnapStudio configuration
        /// </summary>
        private static void TestSnapStudioConfiguration(VisualElement root)
        {
            var targetField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-input-target");
            var outputPathField = root.Q<TextField>("ks-snapstudio-output-path");
            var statusText = root.Q<Label>("status-text");

            var target = targetField?.value as GameObject;
            var outputPath = outputPathField?.value;

            if (target == null || (target.GetComponentInChildren<Animator>() == null && target.GetComponentInChildren<Animation>() == null))
            {
                if (statusText != null) statusText.text = "Test failed: Invalid target.";
                return;
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                if (statusText != null) statusText.text = "Test failed: Output path required.";
                return;
            }

            // Calculate estimated sprite count
            string spriteEstimate = CalculateSnapStudioSpriteEstimate(root, target);
            if (statusText != null) statusText.text = $"Test passed: {spriteEstimate}";
        }

        /// <summary>
        /// Calculates sprite estimate for SnapStudio configuration
        /// </summary>
        private static string CalculateSnapStudioSpriteEstimate(VisualElement root, GameObject target)
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
        /// Starts SnapStudio capture
        /// </summary>
        private static void StartSnapStudioCapture(VisualElement root)
        {
            var targetField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-input-target");
            var statusText = root.Q<Label>("status-text");
            var progressBar = root.Q<ProgressBar>("progress-bar");

            var target = targetField?.value as GameObject;
            if (target == null)
            {
                if (statusText != null) statusText.text = "Error: No target selected.";
                return;
            }

            var captureComponent = target.GetComponent<KalponicGames.KS_SnapStudio.KSPlayModeCapture>();
            if (captureComponent == null)
            {
                captureComponent = target.AddComponent<KalponicGames.KS_SnapStudio.KSPlayModeCapture>();
            }

            // Store reference for progress tracking
            currentCaptureComponent = captureComponent;

            // Apply settings from UI
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

            captureComponent.CharacterObject = target;
            captureComponent.Width = widthField != null ? widthField.value : 1024;
            captureComponent.Height = heightField != null ? heightField.value : 1024;
            captureComponent.Fps = fpsField != null ? fpsField.value : 24;
            captureComponent.MaxFrames = maxFramesField != null ? maxFramesField.value : 24;
            captureComponent.TrimSprites = trimSpritesToggle != null ? trimSpritesToggle.value : true;
            captureComponent.CharacterFillPercent = characterFillField != null ? (float)characterFillField.value : 75f;
            captureComponent.MirrorSprites = mirrorSpritesToggle != null ? mirrorSpritesToggle.value : true;
            captureComponent.PixelSize = pixelSizeField != null ? pixelSizeField.value : 16;
            captureComponent.CaptureWithHDRIfPossible = hdrToggle != null ? hdrToggle.value : false;
            captureComponent.OutRoot = outputPathField != null ? outputPathField.value : "KS_SnapStudio_Renders";
            captureComponent.CaptureSingleAnimation = !(captureAllToggle != null ? captureAllToggle.value : false);

            // Set selected animation if in single mode
            if (!(captureAllToggle != null ? captureAllToggle.value : false))
            {
                AnimationClip selectedClip = null;

                // First priority: Get clip from ObjectField (user's direct selection)
                var animationClipField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-settings-animation-select");
                if (animationClipField != null && animationClipField.value != null)
                {
                    selectedClip = animationClipField.value as AnimationClip;
                }

                // If no ObjectField selection, try to find the persisted animation
                if (selectedClip == null)
                {
                    string selectedAnimationName = EditorPrefs.GetString("KS.SnapStudio.SelectedAnimation", "");
                    if (!string.IsNullOrEmpty(selectedAnimationName))
                    {
                        string[] guids = AssetDatabase.FindAssets($"t:AnimationClip {selectedAnimationName}");
                        if (guids.Length > 0)
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                            if (clip != null && clip.name == selectedAnimationName)
                            {
                                selectedClip = clip;
                            }
                        }
                    }
                }

                // Fallbacks
                if (selectedClip == null)
                {
                    // Fallback 1: Try from AnimatorController if one is selected
                    var animatorControllerField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-settings-animator-controller");
                    if (animatorControllerField != null && animatorControllerField.value != null)
                    {
                        var controller = animatorControllerField.value as RuntimeAnimatorController;
                        var controllerClips = ClipSampler.GetClipsFromController(controller);
                        if (controllerClips != null && controllerClips.Length > 0)
                        {
                            selectedClip = controllerClips[0];
                        }
                    }

                    // Fallback 2: Try from target object
                    if (selectedClip == null && target != null)
                    {
                        var targetClips = ClipSampler.GetClips(target);
                        if (targetClips != null && targetClips.Length > 0)
                        {
                            selectedClip = targetClips[0];
                        }
                    }
                }

                if (selectedClip != null)
                {
                    captureComponent.SelectedClip = selectedClip;
                }
            }

            if (!Application.isPlaying)
            {
                if (statusText != null) statusText.text = "Error: Must be in Play Mode to capture.";
                return;
            }

            // Initialize progress tracking
            if (progressBar != null)
            {
                progressBar.value = 0;
                progressBar.style.display = DisplayStyle.Flex;
            }

            // Start progress monitoring using EditorApplication.update
            EditorApplication.update += () => MonitorCaptureProgress(root);

            captureComponent.StartCapture();
            if (statusText != null) statusText.text = "Capture started...";
        }

        /// <summary>
        /// Monitors capture progress
        /// </summary>
        private static void MonitorCaptureProgress(VisualElement root)
        {
            if (currentCaptureComponent == null || !currentCaptureComponent.IsCapturing)
            {
                // Capture completed or stopped
                var progressBarElement = root.Q<ProgressBar>("progress-bar");
                var statusTextElement = root.Q<Label>("status-text");
                var startCaptureBtnElement = root.Q<Button>("ks-snapstudio-controls-start-capture");

                if (progressBarElement != null)
                {
                    progressBarElement.value = 100;
                    progressBarElement.style.display = DisplayStyle.None;
                }

                if (statusTextElement != null)
                {
                    statusTextElement.text = "Capture completed!";
                }

                if (startCaptureBtnElement != null)
                {
                    startCaptureBtnElement.SetEnabled(true);
                }

                currentCaptureComponent = null;
                EditorApplication.update -= () => MonitorCaptureProgress(root);
                return;
            }

            // Update progress based on captured frames vs total expected frames
            var statusTextElement2 = root.Q<Label>("status-text");
            var progressBarElement2 = root.Q<ProgressBar>("progress-bar");

            if (currentCaptureComponent.Sampling != null && currentCaptureComponent.Sampling.TrimMainTextures != null)
            {
                int capturedFrames = currentCaptureComponent.Sampling.SelectedFrames.Count;
                int totalFrames = currentCaptureComponent.Sampling.TrimMainTextures.Length;

                if (totalFrames > 0)
                {
                    float progress = (float)capturedFrames / totalFrames;
                    if (progressBarElement2 != null)
                    {
                        progressBarElement2.value = progress * 100f;
                    }

                    if (statusTextElement2 != null)
                    {
                        statusTextElement2.text = $"Capturing... {capturedFrames}/{totalFrames} frames ({(progress * 100f):F1}%)";
                    }
                }
            }
        }

        /// <summary>
        /// Browses for SnapStudio output path
        /// </summary>
        private static void BrowseSnapStudioOutputPath(VisualElement root)
        {
            var outputPathField = root.Q<TextField>("ks-snapstudio-output-path");

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
                    SnapStudioPersistence.SaveValue("OutputPath", path);
                }
            }
        }
    }
}