using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Linq;
using KalponicGames.KS_SnapStudio.Systems;
using KalponicGames.KS_SnapStudio.SnapStudio;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// Play-mode capture component that provides preview window and auto-capture functionality.
    /// Add this to your scene and configure before hitting play.
    /// </summary>
    /// <summary>
    /// Pivot calculation modes for sprite anchoring
    /// </summary>
    public enum PivotMode
    {
        /// <summary>
        /// No pivot calculation - use default behavior (backward compatible)
        /// </summary>
        None,

        /// <summary>
        /// Fixed pivot at bottom-center (recommended for 2D games)
        /// </summary>
        Fixed,

        /// <summary>
        /// Pivot based on selected bone position
        /// </summary>
        BoneAnchored,

        /// <summary>
        /// No trimming, fixed canvas with consistent pivot
        /// </summary>
        ConstantCanvas
    }

    [ExecuteInEditMode]
    public class KSPlayModeCapture : MonoBehaviour
    {
        [Header("Model Settings")]
        [Tooltip("The character object in the scene to capture animations from")]
        [SerializeField] private GameObject characterObject;

        [Header("Pivot Settings")]
        [Tooltip("How to calculate sprite pivot points for 2D game usage")]
        [SerializeField] private PivotMode pivotMode = PivotMode.None;

        [Tooltip("Fixed pivot coordinates (normalized 0-1) when using Fixed mode")]
        [SerializeField] private Vector2 fixedPivot = new Vector2(0.5f, 0f); // Bottom-center default

        [Tooltip("Bone name to use for bone-anchored pivot")]
        [SerializeField] private string pivotBoneName = "Hips";

        [Tooltip("Pixels per unit for sprite import settings")]
        [SerializeField] private float pixelsPerUnit = 100f;

        [Header("Output Settings")]
        [Tooltip("Root directory for output renders")]
        [SerializeField] private string outRoot = "KS_SnapStudio_Renders";

        [Header("Capture Settings")]
        [Tooltip("Type of game view for camera positioning")]
        [SerializeField] private GameType gameType = GameType.SideScroller;

        [Tooltip("Capture mode: single animation or all animations")]
        [SerializeField] private bool captureSingleAnimation = true;

        [Tooltip("Output width in pixels")]
        [SerializeField] private int width = 1024;

        [Tooltip("Output height in pixels")]
        [SerializeField] private int height = 1024;

        [Tooltip("Frames per second for animation capture")]
        [SerializeField] private int fps = 24;

        [Tooltip("Maximum frames per animation (16-30 recommended for smooth looping)")]
        [SerializeField] private int maxFrames = 24;

        [Tooltip("Pixel size for pixelation and stabilization")]
        [SerializeField] private int pixelSize = 16;

        [Header("Quality Settings")]
        [Tooltip("Capture with HDR if possible for better quality in high dynamic range scenes")]
        [SerializeField] private bool captureWithHDRIfPossible = false;

        [Header("Animation Settings")]
        [Tooltip("Drop the last frame to create seamless loops")]
        [SerializeField] private bool dropLastFrame = true;

        [Tooltip("Layers to render (only these layers will be visible during capture)")]
        [SerializeField] private LayerMask captureLayers = -1;

        [Tooltip("Trim transparent borders from sprites")]
        [SerializeField] private bool trimSprites = true;

        [Tooltip("Character fill percentage for auto-fit when not trimming (0-100)")]
        [Range(10, 100)] [SerializeField] private float characterFillPercent = 75f;

        [Tooltip("Automatically create left-facing versions of right-facing animations")]
        [SerializeField] private bool mirrorSprites = true;

        [Tooltip("Facing axis that defines 'right' direction")]
        [SerializeField] private FacingAxis facingAxis = FacingAxis.PositiveX;

        [Tooltip("Base yaw offset in degrees")]
        [SerializeField] private float baseYawOffset = 0f;

        [Tooltip("Direction mask for selecting which angles to render")]
        [SerializeField] private DirectionMask directionMask = DirectionMask.All;

        [Tooltip("Disable root motion on animator (allows static props)")]
        [SerializeField] private bool disableRootMotion = true;

        [Tooltip("Axis to rotate around for direction changes")]
        [SerializeField] private RotateAxis rotateAxis = RotateAxis.Y;

        [Tooltip("Generate Unity AnimationClips from exported PNG sequences")]
        [SerializeField] private bool generateClips = false;

        [Header("Play Mode Settings")]
        [Tooltip("Automatically start capture when entering play mode")]
        [SerializeField] private bool autoCaptureOnPlay = true;

        [Tooltip("Show preview window in play mode")]
        [SerializeField] private bool showPreviewWindow = true;

        [Tooltip("UI Canvas for preview window (auto-created if none assigned)")]
        [SerializeField] private Canvas previewCanvas;

        [Tooltip("Current frame for preview scrubbing")]
        [Range(0, 100)] [SerializeField] private int currentFrame = 0;

        [Tooltip("Selected animation clip for preview")]
        [SerializeField] private AnimationClip selectedClip;

        // Public properties for inspector access
        public GameObject CharacterObject { get => characterObject; set => characterObject = value; }
        public string OutRoot { get => outRoot; set => outRoot = value; }
        public GameType GameType { get => gameType; set => gameType = value; }
        public bool CaptureSingleAnimation { get => captureSingleAnimation; set => captureSingleAnimation = value; }
        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public int Fps { get => fps; set => fps = value; }
        public int MaxFrames { get => maxFrames; set => maxFrames = value; }
        public int PixelSize { get => pixelSize; set => pixelSize = value; }
        public bool CaptureWithHDRIfPossible { get => captureWithHDRIfPossible; set => captureWithHDRIfPossible = value; }

        // HDR support properties
        public bool CanCaptureWithHDR
        {
            get
            {
#if UNITY_2021_3_OR_NEWER
                return HDROutputSettings.main != null && HDROutputSettings.main.available;
#else
                return false;
#endif
            }
        }

        public bool CaptureWithHDR { get { return captureWithHDRIfPossible && CanCaptureWithHDR; } }

        // Dynamic texture formats based on HDR setting
        private RenderTextureFormat RenderTextureFormat
        {
            get { return CaptureWithHDR ? UnityEngine.RenderTextureFormat.DefaultHDR : UnityEngine.RenderTextureFormat.ARGB32; }
        }

        private TextureFormat TextureFormat
        {
            get { return CaptureWithHDR ? UnityEngine.TextureFormat.RGBAHalf : UnityEngine.TextureFormat.ARGB32; }
        }
        public LayerMask CaptureLayers { get => captureLayers; set => captureLayers = value; }
        public bool TrimSprites { get => trimSprites; set => trimSprites = value; }
        public float CharacterFillPercent { get => characterFillPercent; set => characterFillPercent = value; }
        public bool MirrorSprites { get => mirrorSprites; set => mirrorSprites = value; }
        public FacingAxis FacingAxis { get => facingAxis; set => facingAxis = value; }
        public float BaseYawOffset { get => baseYawOffset; set => baseYawOffset = value; }
        public DirectionMask DirectionMask { get => directionMask; set => directionMask = value; }
        public bool DisableRootMotion { get => disableRootMotion; set => disableRootMotion = value; }
        public RotateAxis RotateAxis { get => rotateAxis; set => rotateAxis = value; }
        public bool GenerateClips { get => generateClips; set => generateClips = value; }
        public bool DropLastFrame { get => dropLastFrame; set => dropLastFrame = value; }
        public bool AutoCaptureOnPlay { get => autoCaptureOnPlay; set => autoCaptureOnPlay = value; }
        public bool ShowPreviewWindow { get => showPreviewWindow; set => showPreviewWindow = value; }
        public Canvas PreviewCanvas { get => previewCanvas; set => previewCanvas = value; }
        public int CurrentFrame { get => currentFrame; set => currentFrame = value; }
        public AnimationClip SelectedClip { get => selectedClip; set => selectedClip = value; }
        public Sampling Sampling { get => sampling; }

        // Pivot system properties
        public PivotMode PivotModeSetting { get => pivotMode; set => pivotMode = value; }
        public Vector2 FixedPivot { get => fixedPivot; set => fixedPivot = value; }
        public string PivotBoneName { get => pivotBoneName; set => pivotBoneName = value; }
        public float PixelsPerUnit { get => pixelsPerUnit; set => pixelsPerUnit = value; }

        // Preview components
        private RawImage previewImage;
        private Text frameText;
        private Text clipText;
        private Camera previewCamera;
        private RenderTexture previewRT;
        private Texture2D previewTexture;

        // Capture state
        private bool isCapturing = false;
        public bool IsCapturing { get { return isCapturing; } }
        private Animator characterAnimator;
        private Animation characterAnimation;
        private KSClipRunner clipRunner;
        private AnimationClip[] availableClips;
        private Sampling sampling;

        // Output path for current capture session
        private string currentOutputPath = "";

        // Memory optimization components (new architecture)
        private MemoryManager memoryManager;
        private TexturePool texturePool;
        private ChunkedProcessor chunkedProcessor;
        private FrameProcessor frameProcessor;
        private PivotCalculator pivotCalculator;
        private ICaptureStrategy captureStrategy;
        private AsyncFrameProcessor asyncFrameProcessor;

        private void Start()
        {
#if UNITY_EDITOR
            // Register with Service Locator for global access
            // TODO: ServiceLocator.CaptureSystem = this; // Commented out due to assembly reference issues
            Debug.Log("KS PlayMode Capture: Started (ServiceLocator registration pending)");
#endif

            if (Application.isPlaying)
            {
                InitializePlayMode();
            }
        }

        private void OnDestroy()
        {
            CleanupPreview();
        }

        private void InitializePlayMode()
        {
            if (!ValidateSettings())
            {
                Debug.LogError("KS PlayMode Capture: Invalid settings, capture aborted.");
                return;
            }

            // Get character animator, animation component, or clip runner
            characterAnimator = characterObject.GetComponentInChildren<Animator>();
            characterAnimation = characterObject.GetComponentInChildren<Animation>();
            clipRunner = characterObject.GetComponentInChildren<KSClipRunner>();
            
            if (characterAnimator == null && characterAnimation == null && clipRunner == null)
            {
                Debug.LogError("KS PlayMode Capture: No Animator, Animation, or KSClipRunner component found on character object.");
                return;
            }

            // Get available clips based on component type
            if (clipRunner != null)
            {
                availableClips = clipRunner.clips.ToArray();
                Debug.Log($"KS PlayMode Capture: Found KSClipRunner with {availableClips.Length} clips: {string.Join(", ", availableClips.Select(c => c.name))}");
            }
            else
            {
                availableClips = ClipSampler.GetClips(characterObject);
                Debug.Log($"KS PlayMode Capture: Found {availableClips.Length} clips from character object: {string.Join(", ", availableClips.Select(c => c.name))}");
            }
            if (availableClips.Length == 0)
            {
                Debug.LogError("KS PlayMode Capture: No animation clips found.");
                return;
            }

            // Set selected clip if none selected (don't override user selections)
            if (SelectedClip == null && availableClips.Length > 0)
            {
                SelectedClip = availableClips[0];
                Debug.Log($"KS PlayMode Capture: Defaulted to first available clip: {SelectedClip.name}");
            }
            else if (SelectedClip != null)
            {
                Debug.Log($"KS PlayMode Capture: Using pre-selected clip: {SelectedClip.name}");
            }

            // Create preview UI if enabled
            if (ShowPreviewWindow)
            {
                CreatePreviewUI();
            }

            // Initialize memory optimization components
            InitializeOptimizationComponents();

            // Auto capture if enabled
            if (AutoCaptureOnPlay)
            {
                StartCoroutine(AutoCaptureRoutine());
            }
        }

        private void InitializeOptimizationComponents()
        {
            try
            {
                // Initialize memory management components
                memoryManager = new MemoryManager();
                texturePool = new TexturePool(20, memoryManager); // Pool up to 20 textures
                chunkedProcessor = new ChunkedProcessor(memoryManager, this);
                frameProcessor = new FrameProcessor(memoryManager, texturePool);
                pivotCalculator = new PivotCalculator();

                // Initialize async frame processor for background encoding
                asyncFrameProcessor = new AsyncFrameProcessor();

                // Select appropriate capture strategy based on hardware capabilities
                captureStrategy = CaptureStrategyFactory.CreateStrategy(memoryManager, texturePool);

                Debug.Log("KS PlayMode Capture: Memory optimization components initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"KS PlayMode Capture: Failed to initialize optimization components, falling back to original system: {ex.Message}");
                // Components remain null, system will use fallback paths
            }
        }

        private void CleanupOptimizationComponents()
        {
            try
            {
                if (texturePool != null)
                {
                    texturePool.ClearPool();
                }

                if (frameProcessor != null)
                {
                    frameProcessor.ClearQueue();
                }

                if (captureStrategy != null)
                {
                    captureStrategy.Cleanup();
                }

                Debug.Log("KS PlayMode Capture: Optimization components cleaned up");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"KS PlayMode Capture: Error during component cleanup: {ex.Message}");
            }
        }

        private void CreatePreviewUI()
        {
            // Create or use existing canvas
            if (previewCanvas == null)
            {
                GameObject canvasGO = new GameObject("KS_PreviewCanvas");
                previewCanvas = canvasGO.AddComponent<Canvas>();
                previewCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // Create preview panel
            GameObject panelGO = new GameObject("PreviewPanel");
            panelGO.transform.SetParent(previewCanvas.transform, false);

            Image panel = panelGO.AddComponent<Image>();
            panel.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.02f, 0.6f);
            panelRect.anchorMax = new Vector2(0.35f, 0.98f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Create preview image
            GameObject imageGO = new GameObject("PreviewImage");
            imageGO.transform.SetParent(panelGO.transform, false);

            previewImage = imageGO.AddComponent<RawImage>();
            previewImage.color = Color.white;

            RectTransform imageRect = imageGO.GetComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(0.05f, 0.4f);
            imageRect.anchorMax = new Vector2(0.95f, 0.9f);
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;

            // Create clip selector
            GameObject clipGO = new GameObject("ClipText");
            clipGO.transform.SetParent(panelGO.transform, false);

            clipText = clipGO.AddComponent<Text>();
            clipText.text = "Clip: " + (SelectedClip != null ? SelectedClip.name : "None");
            clipText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            clipText.color = Color.white;
            clipText.fontSize = 14;
            clipText.alignment = TextAnchor.MiddleLeft;

            RectTransform clipRect = clipGO.GetComponent<RectTransform>();
            clipRect.anchorMin = new Vector2(0.05f, 0.3f);
            clipRect.anchorMax = new Vector2(0.95f, 0.35f);
            clipRect.offsetMin = Vector2.zero;
            clipRect.offsetMax = Vector2.zero;

            // Create frame text
            GameObject frameTextGO = new GameObject("FrameText");
            frameTextGO.transform.SetParent(panelGO.transform, false);

            frameText = frameTextGO.AddComponent<Text>();
            frameText.text = $"Frame: {currentFrame}";
            frameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            frameText.color = Color.white;
            frameText.fontSize = 12;
            frameText.alignment = TextAnchor.MiddleCenter;

            RectTransform frameTextRect = frameTextGO.GetComponent<RectTransform>();
            frameTextRect.anchorMin = new Vector2(0.05f, 0.2f);
            frameTextRect.anchorMax = new Vector2(0.95f, 0.25f);
            frameTextRect.offsetMin = Vector2.zero;
            frameTextRect.offsetMax = Vector2.zero;

            // Create preview camera
            GameObject cameraGO = new GameObject("KS_PreviewCam");
            previewCamera = cameraGO.AddComponent<Camera>();
            previewCamera.orthographic = true;
            previewCamera.orthographicSize = Height / 2f;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0, 0, 0, 0);
            previewCamera.cullingMask = CaptureLayers;
            
            // Configure camera for proper lighting rendering
            previewCamera.renderingPath = RenderingPath.Forward; // Use forward rendering for consistent lighting
            previewCamera.allowHDR = CaptureWithHDR; // Match HDR settings
            previewCamera.allowMSAA = false; // Disable MSAA for capture consistency
            previewCamera.useOcclusionCulling = false; // Disable occlusion culling for capture

            // Create render texture
            previewRT = new RenderTexture(Width, Height, 24, RenderTextureFormat);
            previewCamera.targetTexture = previewRT;

            // Update preview immediately to show camera working when Play mode starts
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (!Application.isPlaying || SelectedClip == null || previewCamera == null)
                return;

            // Pose camera for preview (use first direction)
            string[] directions = DirectionRig.GetDirectionsFor(GameType, DirectionMask);
            DirectionRig.PoseCamera(previewCamera, characterObject, directions.Length > 0 ? directions[0] : "front", PixelSize, GameType, FacingAxis, BaseYawOffset, RotateAxis);

            // Calculate max frames for current clip
            int maxFramesForClip = Mathf.Min(MaxFrames, Mathf.CeilToInt(selectedClip.length * Fps));

            // Sample animation at current frame
            if (clipRunner != null)
            {
                // Use KSClipRunner for playback
                int clipIndex = System.Array.IndexOf(availableClips, selectedClip);
                if (clipIndex >= 0)
                {
                    clipRunner.PlayIndex(clipIndex, immediate: true);
                    // Manually set time for preview scrubbing
                    // Note: KSClipRunner doesn't expose direct time control, so preview scrubbing may be limited
                    float normalizedTime = currentFrame / (float)maxFramesForClip;
                    // For now, we'll just play the clip and accept limited scrubbing
                }
            }
            else if (characterAnimator != null)
            {
                // Use more precise time calculation for smoother animation
                float normalizedTime = currentFrame / (float)maxFramesForClip;
                float time = Mathf.Lerp(0f, selectedClip.length, normalizedTime);
                characterAnimator.Rebind();
                characterAnimator.Play(SelectedClip.name, 0, 0f);
                SelectedClip.SampleAnimation(characterObject, time);
            }
            else if (characterAnimation != null)
            {
                // For legacy Animation component
                float normalizedTime = currentFrame / (float)maxFramesForClip;
                float time = Mathf.Lerp(0f, selectedClip.length, normalizedTime);
                SelectedClip.SampleAnimation(characterObject, time);
            }

            // Render to texture
            previewCamera.Render();

            // Create texture from render texture
            if (previewTexture != null)
            {
                Destroy(previewTexture);
            }
            previewTexture = new Texture2D(Width, Height, TextureFormat, false);
            RenderTexture.active = previewRT;
            previewTexture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            previewTexture.Apply();
            RenderTexture.active = null;

            // Trim if enabled
            if (TrimSprites)
            {
                previewTexture = CaptureService.TrimTexture(previewTexture);
            }

            // Update preview image
            if (previewImage != null)
            {
                previewImage.texture = previewTexture;
            }
        }

        private IEnumerator AutoCaptureRoutine()
        {
            yield return new WaitForSeconds(0.1f); // Wait for scene to stabilize

            Debug.Log("KS PlayMode Capture: Starting automatic capture...");
            Debug.Log($"KS PlayMode Capture: CaptureSingleAnimation={CaptureSingleAnimation}");
            Debug.Log($"KS PlayMode Capture: SelectedClip={SelectedClip?.name ?? "NULL"}");
            Debug.Log($"KS PlayMode Capture: availableClips.Length={availableClips?.Length ?? 0}");

            // Set deterministic timing
            Application.targetFrameRate = Fps;
            Time.fixedDeltaTime = 1f / Fps;
            Time.captureFramerate = Fps;

            // Create capture camera
            Camera captureCamera = new GameObject("KS_CaptureCam").AddComponent<Camera>();
            captureCamera.orthographic = true;
            
            // Set camera orthographic size based on trimming mode
            if (TrimSprites)
            {
                // When trimming, calculate orthographic size to fit the character bounds
                // This ensures the camera captures everything regardless of character position
                captureCamera.orthographicSize = CalculateAutoFitOrthographicSize(captureCamera, 100f); // 100% fill for trimming
                Debug.Log($"📷 Camera setup for trimming: orthographic size = {captureCamera.orthographicSize} (fits character bounds)");
            }
            else
            {
                // When not trimming, apply auto-fit based on character bounds and fill percentage
                captureCamera.orthographicSize = CalculateAutoFitOrthographicSize(captureCamera, CharacterFillPercent);
                Debug.Log($"📷 Camera setup for auto-fit: orthographic size = {captureCamera.orthographicSize}, fill = {CharacterFillPercent}%");
            }
            
            // Store original character position to restore later
            Vector3 originalCharacterPosition = characterObject.transform.position;
            
            // Calculate character bounds first
            Bounds characterBounds = CalculateCharacterBounds();
            
            // For auto-fit mode (non-trimmed), calculate proper vertical centering based on game type
            if (!TrimSprites)
            {
                // Calculate orthographic size first
                float orthoSize = CalculateAutoFitOrthographicSize(captureCamera, CharacterFillPercent);
                captureCamera.orthographicSize = orthoSize;
                
                // Set target Y based on game type (where camera will be positioned by DirectionRig)
                float targetY;
                switch (GameType)
                {
                    case GameType.SideScroller:
                        targetY = 0f; // Camera at Y=0 for side-scroller
                        break;
                    case GameType.TopDown:
                        targetY = 10f; // Camera at Y=10 for top-down
                        break;
                    case GameType.Iso:
                        targetY = 10f; // Camera at Y=10 for isometric
                        break;
                    default:
                        targetY = 0f;
                        break;
                }
                
                // Position character so its bounds center is at the target Y for perfect vertical centering
                // This accounts for the character's pivot point not being at the bounds center
                float offsetY = targetY - characterBounds.center.y;
                Vector3 newPosition = new Vector3(originalCharacterPosition.x, originalCharacterPosition.y + offsetY, originalCharacterPosition.z);
                characterObject.transform.position = newPosition;
                
                Debug.Log($"📷 Auto-fit mode: GameType={GameType}, targetY={targetY}, bounds.center.y={characterBounds.center.y}, offsetY={offsetY}, final character Y={newPosition.y}");
            }
            else
            {
                // For trimmed mode, use original positioning logic - don't move the character
                Debug.Log($"📷 Trimmed mode: using original positioning, orthographic size = {captureCamera.orthographicSize}");
            }
            
            captureCamera.clearFlags = CameraClearFlags.SolidColor;
            captureCamera.backgroundColor = new Color(0, 0, 0, 0);
            captureCamera.cullingMask = CaptureLayers;

            // Get directions
            string[] directions = DirectionRig.GetDirectionsFor(GameType, DirectionMask);

            // Start batch capture
            AnimationClip[] clipsToCapture = CaptureSingleAnimation && SelectedClip != null ? 
                new AnimationClip[] { SelectedClip } : availableClips;
            
            Debug.Log($"KS PlayMode Capture: CaptureSingleAnimation={CaptureSingleAnimation}, SelectedClip={SelectedClip?.name}, availableClips.Length={availableClips.Length}");
            Debug.Log($"KS PlayMode Capture: Will capture {clipsToCapture.Length} clips: {string.Join(", ", clipsToCapture.Select(c => c.name))}");
            
            var captureComponent = characterObject.AddComponent<BatchCaptureComponent>();
            float? customOrthoSize = TrimSprites ? null : captureCamera.orthographicSize;
            captureComponent.StartBatchCapture(clipsToCapture, characterAnimator, characterAnimation, clipRunner, captureCamera, Width, Height, Fps, MaxFrames, OutRoot, directions, DropLastFrame, PixelSize, TrimSprites, MirrorSprites, GameType, FacingAxis, BaseYawOffset, DirectionMask, DisableRootMotion, RotateAxis, GenerateClips, customOrthoSize, memoryManager, texturePool, asyncFrameProcessor);

            isCapturing = true;

            // Wait for capture to complete and reset the flag
            yield return new WaitUntil(() => captureComponent == null); // BatchCaptureComponent destroys itself when done
            isCapturing = false;

            // Perform final memory cleanup if components are available
            if (memoryManager != null)
            {
                yield return memoryManager.CleanupRoutine();
                memoryManager.LogMemoryUsage("Final cleanup after capture");
            }

            // Cleanup optimization components if needed
            CleanupOptimizationComponents();

            // Restore original character position
            characterObject.transform.position = originalCharacterPosition;
            Debug.Log($"📷 Restored character position to {originalCharacterPosition}");

            Debug.Log("KS PlayMode Capture: Capture completed!");
        }

        private IEnumerator SamplingRoutine()
        {
            yield return new WaitForSeconds(0.1f); // Wait for scene to stabilize

            Debug.Log("KS PlayMode Capture: Starting sampling...");

            // Set deterministic timing
            Application.targetFrameRate = Fps;
            Time.fixedDeltaTime = 1f / Fps;
            Time.captureFramerate = Fps;

            // Create output directory for sampling
            currentOutputPath = System.IO.Path.Combine(OutRoot, CharacterObject.name);
            if (!System.IO.Directory.Exists(currentOutputPath))
            {
                System.IO.Directory.CreateDirectory(currentOutputPath);
            }

            // Store original character position to restore later
            Vector3 originalCharacterPosition = characterObject.transform.position;

            // Create capture camera
            Camera captureCamera = new GameObject("KS_SamplingCam").AddComponent<Camera>();
            captureCamera.orthographic = true;
            captureCamera.orthographicSize = Height / 2f;
            captureCamera.clearFlags = CameraClearFlags.SolidColor;
            captureCamera.backgroundColor = new Color(0, 0, 0, 0);
            captureCamera.cullingMask = CaptureLayers;
            
            // Configure camera for proper lighting rendering
            captureCamera.renderingPath = RenderingPath.Forward; // Use forward rendering for consistent lighting
            captureCamera.allowHDR = CaptureWithHDR; // Match HDR settings
            captureCamera.allowMSAA = false; // Disable MSAA for capture consistency
            captureCamera.useOcclusionCulling = false; // Disable occlusion culling for capture

            // Calculate character bounds and position for centering
            Bounds characterBounds = CalculateCharacterBounds();
            float targetY;
            switch (GameType)
            {
                case GameType.SideScroller:
                    targetY = 0f;
                    break;
                case GameType.TopDown:
                    targetY = 10f;
                    break;
                case GameType.Iso:
                    targetY = 10f;
                    break;
                default:
                    targetY = 0f;
                    break;
            }
            
            // Position character so its bounds center is at the target Y
            float offsetY = targetY - characterBounds.center.y;
            Vector3 newPosition = new Vector3(originalCharacterPosition.x, originalCharacterPosition.y + offsetY, originalCharacterPosition.z);
            characterObject.transform.position = newPosition;
            
            Debug.Log($"📷 Sampling: GameType={GameType}, targetY={targetY}, bounds.center.y={characterBounds.center.y}, offsetY={offsetY}, final character Y={newPosition.y}");

            // Get directions
            string[] directions = DirectionRig.GetDirectionsFor(GameType, DirectionMask);

            // Sample animation frames
            yield return StartCoroutine(SampleAnimationFrames(SelectedClip, characterAnimator, characterAnimation, captureCamera, directions));

            // Restore original character position
            characterObject.transform.position = originalCharacterPosition;
            Debug.Log($"📷 Sampling: Restored character position to {originalCharacterPosition}");

            isCapturing = false;

            Debug.Log("KS PlayMode Capture: Sampling completed!");
        }

        private IEnumerator SampleAnimationFrames(AnimationClip clip, Animator animator, Animation animation, Camera camera, string[] directions)
        {
            // Calculate frame count (use full animation length for smooth playback)
            int totalFrames = Mathf.Min(MaxFrames * 2, (int)(clip.length * Fps)); // Allow up to 2x maxFrames for longer animations
            if (DropLastFrame)
                totalFrames = Mathf.Max(0, totalFrames - 1);

            // Initialize sampling data
            sampling = new Sampling(totalFrames);

            // Disable animator/clip runner during sampling (only for Animator and KSClipRunner)
            bool wasAnimatorEnabled = false;
            bool wasClipRunnerEnabled = false;
            if (animator != null)
            {
                wasAnimatorEnabled = animator.enabled;
                animator.enabled = false;
            }
            if (clipRunner != null)
            {
                wasClipRunnerEnabled = clipRunner.enabled;
                clipRunner.enabled = false;
            }

            try
            {
                for (int frame = 0; frame < totalFrames; frame++)
                {
                    // Sample animation at evenly distributed time points
                    float normalizedTime = frame / (float)(totalFrames - (dropLastFrame ? 1 : 0));
                    float time = Mathf.Lerp(0f, clip.length, normalizedTime);
                    clip.SampleAnimation(animator != null ? animator.gameObject : animation.gameObject, time);

                    // Wait for the pose to be applied
                    yield return null;

                        // Capture frame for each direction
                        foreach (string direction in directions)
                        {
                            // Pose camera for direction
                            DirectionRig.PoseCamera(camera, characterObject, direction, pixelSize, gameType, facingAxis, baseYawOffset, rotateAxis);

                            // Capture frame synchronously for sampling
                            Texture2D frameTexture = CaptureFrameSync(camera, Width, Height);
                            sampling.RawMainTextures[frame] = frameTexture;

                            // Trim if enabled
                            if (TrimSprites)
                            {
                                sampling.TrimMainTextures[frame] = CaptureService.TrimTexture(frameTexture);
                                Debug.Log($"✂️ Trimmed frame {frame}: original {frameTexture.width}x{frameTexture.height} -> trimmed {sampling.TrimMainTextures[frame]?.width ?? 0}x{sampling.TrimMainTextures[frame]?.height ?? 0}");
                            }
                            else
                            {
                                sampling.TrimMainTextures[frame] = frameTexture;
                            }

                            // Calculate and log pivot if enabled
                            if (PivotModeSetting != PivotMode.None)
                            {
                                Vector2 pivot = ComputePivotForFrame(camera, Width, Height,
                                    TrimSprites ? new RectInt(0, 0, sampling.TrimMainTextures[frame].width, sampling.TrimMainTextures[frame].height) : (RectInt?)null);
                                Debug.Log($"📍 Frame {frame} pivot: {pivot} (mode: {PivotModeSetting})");
                            }

                            break; // Only capture first direction for sampling
                        }                    // Update progress
                    if (frame % 10 == 0)
                    {
                        float progress = (float)frame / totalFrames;
#if UNITY_EDITOR
                        EditorUtility.DisplayProgressBar("Sampling Animation", $"Frame {frame + 1}/{totalFrames}", progress);
#endif
                    }
                }
            }
            finally
            {
                // Re-enable animator and clip runner
                if (animator != null)
                {
                    animator.enabled = wasAnimatorEnabled;
                }
                if (clipRunner != null)
                {
                    clipRunner.enabled = wasClipRunnerEnabled;
                }
#if UNITY_EDITOR
                EditorUtility.ClearProgressBar();
#endif
            }

            // Cleanup camera
            Object.Destroy(camera.gameObject);
        }

        private Texture2D CaptureFrameSync(Camera camera, int width, int height)
        {
            // Create render texture with appropriate format for HDR support
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat);
            camera.targetTexture = rt;

            // Render
            camera.Render();
            var texture = new Texture2D(width, height, TextureFormat, false);
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            // Cleanup
            RenderTexture.active = null;
            camera.targetTexture = null;
            Object.Destroy(rt);

            return texture;
        }

        private void CaptureFrame(Camera camera, int width, int height)
        {
            // Try optimized capture first
            if (captureStrategy != null && texturePool != null && asyncFrameProcessor != null)
            {
                try
                {
                    // Create render texture with appropriate format for HDR support
                    var rt = new RenderTexture(width, height, 24, RenderTextureFormat);

                    // Use capture strategy with async processing
                    var enumerator = captureStrategy.CaptureFrame(camera, rt, 0, (texture) => {
                        // Queue frame for async processing instead of returning it
                        string fileName = $"frame_{currentFrame:D4}.png";
                        string filePath = System.IO.Path.Combine(currentOutputPath, fileName);

                        asyncFrameProcessor.QueueFrame(texture, filePath,
                            () => {
                                // Frame saved successfully
                                Debug.Log($"Frame {currentFrame} saved to {filePath}");
                            },
                            (processedTexture) => {
                                // Texture processed, can add to sampling data
                                if (Sampling.TrimMainTextures != null &&
                                    currentFrame < Sampling.TrimMainTextures.Length)
                                {
                                    Sampling.TrimMainTextures[currentFrame] = processedTexture;
                                    Sampling.SelectedFrames.Add(new Frame(currentFrame, Time.time));
                                }
                            });
                    });

                    // Run the coroutine synchronously for this context
                    while (enumerator.MoveNext()) { }

                    // Cleanup render texture
                    Object.Destroy(rt);

                    return;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Optimized capture failed, falling back to original method: {ex.Message}");
                }
            }

            // Fallback to original capture method
            CaptureFrameOriginal(camera, width, height);
        }

        private void CaptureFrameOriginal(Camera camera, int width, int height)
        {
            // Create render texture with appropriate format for HDR support
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat);
            camera.targetTexture = rt;

            // Render
            camera.Render();
            var texture = new Texture2D(width, height, TextureFormat, false);
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            // Cleanup
            RenderTexture.active = null;
            camera.targetTexture = null;
            Object.Destroy(rt);
        }

        private void CleanupPreview()
        {
            if (previewRT != null)
            {
                Destroy(previewRT);
            }
            if (previewTexture != null)
            {
                Destroy(previewTexture);
            }
            if (previewCamera != null)
            {
                Destroy(previewCamera.gameObject);
            }
        }

        private float CalculateAutoFitOrthographicSize(Camera camera, float fillPercent)
        {
            // Calculate character bounds in world space
            Bounds characterBounds = CalculateCharacterBounds();

            // Get camera aspect ratio (width/height)
            float aspectRatio = (float)Width / Height;

            // Calculate the orthographic size needed to fit the character bounds
            // We need to fit both width and height considering the camera's aspect ratio
            float boundsWidth = characterBounds.size.x;
            float boundsHeight = characterBounds.size.y;

            // Apply fill percentage (convert percentage to decimal and invert for scaling)
            float fillScale = 100f / fillPercent;

            // For orthographic camera:
            // - View height = 2 * orthographicSize
            // - View width = 2 * orthographicSize * aspectRatio

            // Calculate orthographic size needed to fit width: boundsWidth should fit in view width
            // viewWidth = 2 * orthoSize * aspectRatio
            // So: orthoSize = (boundsWidth * fillScale) / (2 * aspectRatio)
            float orthoSizeForWidth = (boundsWidth * fillScale) / (2f * aspectRatio);

            // Calculate orthographic size needed to fit height: boundsHeight should fit in view height
            // viewHeight = 2 * orthoSize
            // So: orthoSize = (boundsHeight * fillScale) / 2
            float orthoSizeForHeight = (boundsHeight * fillScale) / 2f;

            // Use the larger of the two to ensure the entire character fits
            float requiredOrthoSize = Mathf.Max(orthoSizeForWidth, orthoSizeForHeight);

            // CRITICAL FIX: For smaller resolutions, ensure we don't zoom in too much
            // Base this on a reference resolution (1024) - smaller resolutions need larger ortho size
            float referenceResolution = 1024f;
            float resolutionScale = referenceResolution / Mathf.Max(Width, Height);
            requiredOrthoSize *= Mathf.Max(1f, resolutionScale * 0.8f); // Scale up for smaller resolutions

            // Ensure minimum size to prevent extreme zoom
            requiredOrthoSize = Mathf.Max(requiredOrthoSize, 0.1f);

            Debug.Log($"📏 Auto-fit calculation: bounds=({boundsWidth}, {boundsHeight}), aspectRatio={aspectRatio}, fillPercent={fillPercent}%, resolutionScale={resolutionScale}, orthoSizeForWidth={orthoSizeForWidth}, orthoSizeForHeight={orthoSizeForHeight}, finalOrthoSize={requiredOrthoSize}");

            return requiredOrthoSize;
        }

        private Bounds CalculateCharacterBounds()
        {
            // Get all renderers from the character object
            Renderer[] renderers = characterObject.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                // Fallback: use colliders if no renderers
                Collider[] colliders = characterObject.GetComponentsInChildren<Collider>();
                if (colliders.Length > 0)
                {
                    Bounds colliderBounds = colliders[0].bounds;
                    for (int i = 1; i < colliders.Length; i++)
                    {
                        colliderBounds.Encapsulate(colliders[i].bounds);
                    }
                    Debug.Log($"📏 Using collider bounds: center={colliderBounds.center}, size={colliderBounds.size}");
                    return colliderBounds;
                }

                // Ultimate fallback: use transform position with default size
                Debug.LogWarning("📏 No renderers or colliders found, using transform position with default size");
                return new Bounds(characterObject.transform.position, Vector3.one * 2f);
            }

            // Calculate combined bounds from all renderers
            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            // Ensure bounds have minimum size
            if (combinedBounds.size.x < 0.01f) combinedBounds.size = new Vector3(0.01f, combinedBounds.size.y, combinedBounds.size.z);
            if (combinedBounds.size.y < 0.01f) combinedBounds.size = new Vector3(combinedBounds.size.x, 0.01f, combinedBounds.size.y);
            if (combinedBounds.size.z < 0.01f) combinedBounds.size = new Vector3(combinedBounds.size.x, combinedBounds.size.y, 0.01f);

            Debug.Log($"📏 Character bounds: center={combinedBounds.center}, size={combinedBounds.size}");
            return combinedBounds;
        }

        /// <summary>
        /// Computes pivot point for a frame based on the current pivot mode
        /// </summary>
        public Vector2 ComputePivotForFrame(Camera camera, int texWidth, int texHeight, RectInt? trimRect = null)
        {
            Vector2 pivot = Vector2.zero;

            switch (PivotModeSetting)
            {
                case PivotMode.None:
                    // No pivot calculation - return zero (backward compatible)
                    pivot = Vector2.zero;
                    break;

                case PivotMode.Fixed:
                    // Use fixed pivot coordinates
                    pivot = FixedPivot;
                    break;

                case PivotMode.BoneAnchored:
                    // Calculate pivot from bone position
                    Transform bone = FindBoneByName(PivotBoneName);
                    if (bone != null)
                    {
                        pivot = ComputePivotFromBone(camera, bone, texWidth, texHeight);
                    }
                    else
                    {
                        Debug.LogWarning($"Pivot bone '{PivotBoneName}' not found, falling back to fixed pivot");
                        pivot = FixedPivot;
                    }
                    break;

                case PivotMode.ConstantCanvas:
                    // For constant canvas, use fixed pivot (no trimming)
                    pivot = FixedPivot;
                    break;
            }

            // If trimmed, remap pivot to trimmed coordinate space
            if (trimRect.HasValue && TrimSprites && PivotModeSetting != PivotMode.None)
            {
                pivot = RemapPivotAfterTrim(pivot, texWidth, texHeight, trimRect.Value);
            }

            return pivot;
        }

        /// <summary>
        /// Finds a bone by name in the character hierarchy
        /// </summary>
        private Transform FindBoneByName(string boneName)
        {
            if (characterObject == null) return null;

            // Search through all transforms in the character hierarchy
            Transform[] allTransforms = characterObject.GetComponentsInChildren<Transform>();
            return System.Array.Find(allTransforms, t => t.name == boneName);
        }

        /// <summary>
        /// Computes normalized pivot coordinates from a bone's screen space position
        /// </summary>
        public static Vector2 ComputePivotFromBone(Camera cam, Transform bone, int texW, int texH)
        {
            if (cam == null || bone == null) return Vector2.zero;

            // Get bone position in screen space
            Vector3 screenPos = cam.WorldToScreenPoint(bone.position);

            // Convert to normalized coordinates (0-1) within texture bounds
            // Unity screen coordinates: bottom-left is (0,0), top-right is (Screen.width, Screen.height)
            // But for texture coordinates, we want left-bottom as (0,0), right-top as (1,1)
            float px = screenPos.x / texW;
            float py = screenPos.y / texH;

            // Clamp to valid range
            px = Mathf.Clamp01(px);
            py = Mathf.Clamp01(py);

            return new Vector2(px, py);
        }

        /// <summary>
        /// Remaps pivot coordinates after trimming to maintain correct positioning
        /// </summary>
        public static Vector2 RemapPivotAfterTrim(Vector2 pivot, int originalW, int originalH, RectInt trimRect)
        {
            // Original pivot in normalized coordinates (0-1)
            // Convert to pixel coordinates in original image
            float pivotPixelX = pivot.x * originalW;
            float pivotPixelY = pivot.y * originalH;

            // Calculate new pivot in trimmed coordinate system
            // trimRect: (x, y, width, height) in original pixel coordinates
            float newPivotX = (pivotPixelX - trimRect.x) / trimRect.width;
            float newPivotY = (pivotPixelY - trimRect.y) / trimRect.height;

            // Clamp to valid range (0-1)
            newPivotX = Mathf.Clamp01(newPivotX);
            newPivotY = Mathf.Clamp01(newPivotY);

            return new Vector2(newPivotX, newPivotY);
        }        private bool ValidateSettings()
        {
            if (CharacterObject == null)
            {
                string errorMsg = "KS PlayMode Capture: Character object not assigned.";
                Debug.LogError(errorMsg);
#if UNITY_EDITOR
                // TODO: ServiceLocator.Events.OnErrorOccurred?.Invoke(errorMsg); // Commented out due to assembly reference issues
#endif
                return false;
            }

            if (string.IsNullOrEmpty(OutRoot))
            {
                string errorMsg = "KS PlayMode Capture: Output root directory not specified.";
                Debug.LogError(errorMsg);
#if UNITY_EDITOR
                // TODO: ServiceLocator.Events.OnErrorOccurred?.Invoke(errorMsg); // Commented out due to assembly reference issues
#endif
                return false;
            }

            if (Width <= 0 || Height <= 0)
            {
                string errorMsg = "KS PlayMode Capture: Width and height must be positive values.";
                Debug.LogError(errorMsg);
#if UNITY_EDITOR
                // TODO: ServiceLocator.Events.OnErrorOccurred?.Invoke(errorMsg); // Commented out due to assembly reference issues
#endif
                return false;
            }

            if (Fps <= 0)
            {
                string errorMsg = "KS PlayMode Capture: FPS must be a positive value.";
                Debug.LogError(errorMsg);
#if UNITY_EDITOR
                // TODO: ServiceLocator.Events.OnErrorOccurred?.Invoke(errorMsg); // Commented out due to assembly reference issues
#endif
                return false;
            }

            if (PixelSize <= 0)
            {
                string errorMsg = "KS PlayMode Capture: Pixel size must be a positive value.";
                Debug.LogError(errorMsg);
#if UNITY_EDITOR
                // TODO: ServiceLocator.Events.OnErrorOccurred?.Invoke(errorMsg); // Commented out due to assembly reference issues
#endif
                return false;
            }

            return true;
        }

        // Editor-time validation (Fail Fast principle)
        private void OnValidate()
        {
            // Validate in editor to catch issues early
            if (CharacterObject == null)
            {
                Debug.LogWarning("KS SnapStudio: Character object not assigned. Please assign a GameObject with Animator or Animation component.");
            }

            // Validate resolution settings
            if (Width <= 0) Width = 512;
            if (Height <= 0) Height = 512;

            // Warn about very small resolutions that might cause cropping
            if (Width < 256 || Height < 256)
            {
                Debug.LogWarning($"KS SnapStudio: Very small resolution ({Width}x{Height}) may cause character cropping. Consider using 512x512 or larger for better results.");
            }

            if (Fps <= 0) Fps = 24;
            if (PixelSize <= 0) PixelSize = 1;

            // Validate pivot settings
            if (PivotModeSetting == PivotMode.BoneAnchored && string.IsNullOrEmpty(PivotBoneName))
            {
                Debug.LogWarning("KS SnapStudio: Bone-anchored pivot selected but no bone name specified. Using 'Hips' as default.");
                PivotBoneName = "Hips";
            }
        }

        // Public methods for external control
        public void StartCapture()
        {
            if (!isCapturing && Application.isPlaying)
            {
#if UNITY_EDITOR
                // TODO: ServiceLocator.Events.OnCaptureStarted?.Invoke(); // Commented out due to assembly reference issues
#endif
                StartCoroutine(AutoCaptureRoutine());
            }
            else if (!Application.isPlaying)
            {
                string errorMsg = "KS SnapStudio: Capture can only be started in Play mode.";
                Debug.LogError(errorMsg);
#if UNITY_EDITOR
                // TODO: ServiceLocator.Events.OnErrorOccurred?.Invoke(errorMsg); // Commented out due to assembly reference issues
#endif
            }
        }

        public void StartSampling()
        {
            if (!isCapturing && Application.isPlaying && SelectedClip != null)
            {
                StartCoroutine(SamplingRoutine());
            }
            else if (SelectedClip == null)
            {
                string errorMsg = "KS SnapStudio: No animation clip selected for sampling.";
                Debug.LogError(errorMsg);
#if UNITY_EDITOR
                // TODO: ServiceLocator.Events.OnErrorOccurred?.Invoke(errorMsg); // Commented out due to assembly reference issues
#endif
            }
        }

        public void UpdatePreviewFrame(int frame)
        {
            currentFrame = frame;
            frameText.text = $"Frame: {currentFrame}";
            UpdatePreview();
        }

        public void SetSelectedClip(AnimationClip clip)
        {
            SelectedClip = clip;
            if (clipText != null)
            {
                clipText.text = "Clip: " + clip.name;
            }
            UpdatePreview();
        }
    }
}
