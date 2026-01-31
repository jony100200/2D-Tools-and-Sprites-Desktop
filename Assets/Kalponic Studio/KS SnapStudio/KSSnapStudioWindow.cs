// Copyright (c) 2025 Kalponic Games. All rights reserved.
// KS SnapStudio - Professional 3D to 2D Animation Capture Tool

using KalponicGames.KS_SnapStudio;
using KalponicGames.KS_SnapStudio.SnapStudio;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using KalponicGames.KS_SnapStudio.Editor;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// Available tabs in the KS SnapStudio window
    /// </summary>
    public enum SelectedTab
    {
        AnimationCapture,
        ThumbnailGeneration,
        Settings
    }

    public class KSSnapStudioWindow : EditorWindow
    {
        private const string WINDOW_TITLE = "KS SnapStudio";
        private const string PREFS_SELECTED_TAB = "KS_SnapStudio_SelectedTab";

        // UI Elements
        private Toolbar tabToolbar;
        private VisualElement contentContainer;
        private Button[] tabButtons;
        private VisualElement[] tabContents;

        // State
        private SelectedTab currentTab;

        // SnapStudio Components
        private KalponicGames.KS_SnapStudio.KSPlayModeCapture currentCaptureComponent = null;

        #region Menu
        [MenuItem("Tools/Kalponic Studio/Animation/KS SnapStudio")]
        public static void ShowWindow()
        {
            var win = GetWindow<KSSnapStudioWindow>(utility: false, title: WINDOW_TITLE);
            win.minSize = new Vector2(600, 400);
            win.maxSize = new Vector2(1200, 800);
            win.position = new Rect(100, 100, 800, 600);
            win.Show();
        }
        #endregion

        private void CreateGUI()
        {
            // Load UI from UXML and USS
            var root = rootVisualElement;

            // Try to load the main UI template
            var visualTree = PluginAssetLocator.FindUxmlAsset("KSSnapStudioWindow");
            var styleSheet = PluginAssetLocator.FindUssAsset("KSSnapStudioWindow");

            if (visualTree != null)
            {
                visualTree.CloneTree(root);

                if (styleSheet != null)
                {
                    root.styleSheets.Add(styleSheet);
                }

                // Get references to UI elements
                tabToolbar = root.Q<Toolbar>("tab-toolbar");
                contentContainer = root.Q<VisualElement>("content-container");
            }

            // Create fallback UI if UXML loading failed
            if (tabToolbar == null || contentContainer == null)
            {
                CreateFallbackUI(root);
                return;
            }

            // Initialize tab system
            InitializeTabs();

            // Load persisted tab selection
            LoadSelectedTab();

            // Set up keyboard navigation
            SetupKeyboardNavigation();
        }

        /// <summary>
        /// Creates a fallback UI when UXML loading fails
        /// </summary>
        private void CreateFallbackUI(VisualElement root)
        {
            root.Clear();

            // Create basic tab toolbar
            tabToolbar = new Toolbar();
            tabToolbar.name = "tab-toolbar";
            tabToolbar.AddToClassList("tab-toolbar");
            root.Add(tabToolbar);

            // Create content container
            contentContainer = new VisualElement();
            contentContainer.name = "content-container";
            contentContainer.AddToClassList("content-container");
            root.Add(contentContainer);

            // Add basic styling
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;
            tabToolbar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            tabToolbar.style.paddingLeft = 4;
            tabToolbar.style.paddingRight = 4;
            tabToolbar.style.paddingTop = 4;
            tabToolbar.style.paddingBottom = 4;
            contentContainer.style.flexGrow = 1;
            contentContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);

            Debug.Log("Created fallback UI for KS SnapStudio window");
        }

        /// <summary>
        /// Initializes the tab system with buttons and content
        /// </summary>
        private void InitializeTabs()
        {
            // Create tab buttons
            tabButtons = new Button[3];
            tabContents = new VisualElement[3];

            // Animation Capture tab
            tabButtons[0] = new Button(() => SelectTab(SelectedTab.AnimationCapture)) { text = "Animation Capture" };
            tabButtons[0].AddToClassList("tab-button");
            ApplyTabButtonStyling(tabButtons[0]);
            tabToolbar.Add(tabButtons[0]);

            // Thumbnail Generation tab
            tabButtons[1] = new Button(() => SelectTab(SelectedTab.ThumbnailGeneration)) { text = "Thumbnail Generation" };
            tabButtons[1].AddToClassList("tab-button");
            ApplyTabButtonStyling(tabButtons[1]);
            tabToolbar.Add(tabButtons[1]);

            // Settings tab
            tabButtons[2] = new Button(() => SelectTab(SelectedTab.Settings)) { text = "Settings" };
            tabButtons[2].AddToClassList("tab-button");
            ApplyTabButtonStyling(tabButtons[2]);
            tabToolbar.Add(tabButtons[2]);

            // Pre-create content (lazy loading)
            tabContents[0] = null; // Will be created on demand
            tabContents[1] = null;
            tabContents[2] = null;
        }

        /// <summary>
        /// Applies basic styling to tab buttons for fallback mode
        /// </summary>
        private void ApplyTabButtonStyling(Button button)
        {
            button.style.backgroundColor = new Color(0.27f, 0.27f, 0.27f, 1f);
            button.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            button.style.paddingLeft = 12;
            button.style.paddingRight = 12;
            button.style.paddingTop = 6;
            button.style.paddingBottom = 6;
            button.style.marginRight = 2;
            button.style.fontSize = 12;
            button.style.borderTopLeftRadius = 4;
            button.style.borderTopRightRadius = 4;
            button.style.borderBottomLeftRadius = 0;
            button.style.borderBottomRightRadius = 0;
        }

        /// <summary>
        /// Loads the persisted selected tab from EditorPrefs
        /// </summary>
        private void LoadSelectedTab()
        {
            int savedTab = EditorPrefs.GetInt(PREFS_SELECTED_TAB, 0);
            if (savedTab >= 0 && savedTab < 3)
            {
                currentTab = (SelectedTab)savedTab;
            }
            else
            {
                currentTab = SelectedTab.AnimationCapture;
            }

            // Apply initial styling
            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (i == (int)currentTab)
                {
                    ApplyActiveTabStyling(tabButtons[i]);
                }
                else
                {
                    ApplyInactiveTabStyling(tabButtons[i]);
                }
            }

            SelectTab(currentTab);
        }

        /// <summary>
        /// Sets up keyboard navigation for tabs
        /// </summary>
        private void SetupKeyboardNavigation()
        {
            // Add keyboard event handler to the root
            rootVisualElement.focusable = true;
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        /// <summary>
        /// Handles keyboard navigation between tabs
        /// </summary>
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.LeftArrow)
            {
                int currentIndex = (int)currentTab;
                int nextIndex = (currentIndex - 1 + 3) % 3;
                SelectTab((SelectedTab)nextIndex);
                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.RightArrow)
            {
                int currentIndex = (int)currentTab;
                int nextIndex = (currentIndex + 1) % 3;
                SelectTab((SelectedTab)nextIndex);
                evt.StopPropagation();
            }
        }

        /// <summary>
        /// Selects a tab and updates the UI
        /// </summary>
        private void SelectTab(SelectedTab tab)
        {
            if (currentTab == tab) return;

            currentTab = tab;

            // Update button styles
            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (i == (int)tab)
                {
                    tabButtons[i].AddToClassList("tab--active");
                    ApplyActiveTabStyling(tabButtons[i]);
                }
                else
                {
                    tabButtons[i].RemoveFromClassList("tab--active");
                    ApplyInactiveTabStyling(tabButtons[i]);
                }
            }

            // Update content
            UpdateContent();

            // Save selection
            EditorPrefs.SetInt(PREFS_SELECTED_TAB, (int)tab);

            // Repaint
            Repaint();
        }

        /// <summary>
        /// Applies active styling to the selected tab button
        /// </summary>
        private void ApplyActiveTabStyling(Button button)
        {
            button.style.backgroundColor = new Color(0.22f, 0.58f, 0.85f, 1f); // Blue highlight
            button.style.color = Color.white;
        }

        /// <summary>
        /// Applies inactive styling to tab buttons
        /// </summary>
        private void ApplyInactiveTabStyling(Button button)
        {
            button.style.backgroundColor = new Color(0.27f, 0.27f, 0.27f, 1f);
            button.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        }

        /// <summary>
        /// Updates the content area with the selected tab's content
        /// </summary>
        private void UpdateContent()
        {
            contentContainer.Clear();

            int tabIndex = (int)currentTab;
            if (tabContents[tabIndex] == null)
            {
                // Create content on demand
                try
                {
                    switch (currentTab)
                    {
                        case SelectedTab.AnimationCapture:
                            tabContents[tabIndex] = SnapStudioTabController.CreateSnapStudioContent();
                            break;
                        case SelectedTab.ThumbnailGeneration:
                            tabContents[tabIndex] = ThumbSmithTabController.CreateThumbSmithContent();
                            break;
                        case SelectedTab.Settings:
                            tabContents[tabIndex] = SettingsTabController.CreateSettingsContent();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error creating content for tab {currentTab}: {e.Message}");
                    tabContents[tabIndex] = CreateErrorContent($"Failed to load {currentTab} content: {e.Message}");
                }
            }

            if (tabContents[tabIndex] != null)
            {
                contentContainer.Add(tabContents[tabIndex]);
            }
        }

        /// <summary>
        /// Creates error content when tab content fails to load
        /// </summary>
        private VisualElement CreateErrorContent(string message)
        {
            var container = new VisualElement();
            container.style.paddingTop = 20;
            container.style.paddingBottom = 20;
            container.style.paddingLeft = 20;
            container.style.paddingRight = 20;

            var errorLabel = new Label(message);
            errorLabel.style.color = Color.red;
            errorLabel.style.whiteSpace = WhiteSpace.Normal;
            container.Add(errorLabel);

            return container;
        }

        private void OnEnable()
        {
            Debug.Log("KS SnapStudio: OnEnable called");

            // Register with Service Locator for global access
            ServiceLocator.MainWindow = this;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Play Mode has ended, restore target reference
                SnapStudioPersistence.RestoreTargetReference(null);
                // Update create scene button state after scene changes
                SnapStudioUIHelpers.UpdateCreateSceneButtonState(null);
            }
        }

        private void OnHierarchyChanged()
        {
            // Update create scene button state when hierarchy changes
            SnapStudioUIHelpers.UpdateCreateSceneButtonState(null);
        }

        private void InitializeSnapStudioUI(VisualElement root)
        {
            // Bind fields - using the same element names as in SnapStudioWindow.uxml
            var targetField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-input-target");
            var animatorControllerField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-settings-animator-controller");
            var widthField = root.Q<IntegerField>("ks-snapstudio-settings-width");
            var heightField = root.Q<IntegerField>("ks-snapstudio-settings-height");
            var fpsField = root.Q<IntegerField>("ks-snapstudio-settings-fps");
            var maxFramesField = root.Q<IntegerField>("ks-snapstudio-settings-max-frames");
            var trimSpritesToggle = root.Q<Toggle>("ks-snapstudio-settings-trim-sprites");
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

            // Create the Character Fill Slider programmatically (Unity 6 compatibility)
            var characterFillField = new Slider("Character Fill %", 10, 100) { value = 75 };
            characterFillField.name = "ks-snapstudio-settings-character-fill";
            characterFillField.tooltip = "How much of the frame the character should fill when trimming is disabled. Higher values make characters appear larger.";

            // Insert the slider between trimSpritesToggle and mirrorSpritesToggle
            if (trimSpritesToggle != null && mirrorSpritesToggle != null)
            {
                var parent = trimSpritesToggle.parent;
                int trimIndex = parent.IndexOf(trimSpritesToggle);
                parent.Insert(trimIndex + 1, characterFillField);
            }

            // Set up field types and initial values
            if (targetField != null) targetField.objectType = typeof(GameObject);
            if (animatorControllerField != null) animatorControllerField.objectType = typeof(RuntimeAnimatorController);
            if (animationClipField != null) animationClipField.objectType = typeof(AnimationClip);

            // Initialize AnimatorController field
            if (animatorControllerField != null)
            {
                SnapStudioPersistence.LoadPersistedAnimatorController(root);
                Debug.Log("AnimatorController field initialized successfully");
            }
            else
            {
                Debug.LogWarning("AnimatorController field not found in UI");
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

        private VisualElement CreateSnapStudioFallbackContent()
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

        private void OpenThumbSmithWindow()
        {
            // Try to open the ThumbSmith window using reflection to avoid compile-time dependencies
            try
            {
                var assembly = System.Reflection.Assembly.Load("Assembly-CSharp-Editor");
                var windowType = assembly.GetType("KalponicGames.KS_SnapStudio.ThumbnailerWindow");
                if (windowType != null)
                {
                    var openMethod = windowType.GetMethod("Open", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (openMethod != null)
                    {
                        openMethod.Invoke(null, null);
                        Debug.Log("KS ThumbSmith window opened successfully");
                    }
                    else
                    {
                        Debug.LogError("Could not find Open method on ThumbnailerWindow");
                        FallbackThumbSmithMessage();
                    }
                }
                else
                {
                    Debug.LogError("Could not find ThumbnailerWindow type");
                    FallbackThumbSmithMessage();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error opening ThumbSmith window: {e.Message}");
                FallbackThumbSmithMessage();
            }
        }

        private void FallbackThumbSmithMessage()
        {
            EditorUtility.DisplayDialog(
                "KS ThumbSmith",
                "The ThumbSmith window could not be opened automatically. Please use the menu: Kalponic -> KSThumbSmith",
                "OK"
            );
        }

        private void OnDestroy()
        {
            // Cleanup components
            // if (snapStudioWindow != null)
            // {
            //     // Cleanup SnapStudio
            // }
        }

        // SnapStudio Helper Methods

        private void CreateSnapStudioCaptureScene(VisualElement root)
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

        private void RefreshSnapStudioUI(VisualElement root)
        {
            Debug.Log("🔄 FORCE REFRESH: Refreshing SnapStudio UI elements");

            // Re-query elements
            var animatorControllerField = root.Q<UnityEditor.UIElements.ObjectField>("ks-snapstudio-settings-animator-controller");

            if (animatorControllerField != null)
            {
                Debug.Log("✅ REFRESH: AnimatorController field found");
            }
            else
            {
                Debug.LogError("❌ REFRESH: AnimatorController field not found");
            }
        }

        private void TestSnapStudioConfiguration(VisualElement root)
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

        private string CalculateSnapStudioSpriteEstimate(VisualElement root, GameObject target)
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

        private void StartSnapStudioCapture(VisualElement root)
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

        private void MonitorCaptureProgress(VisualElement root)
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

        private void BrowseSnapStudioOutputPath(VisualElement root)
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