using UnityEngine;

namespace KalponicStudio.Tooltips
{
    /// <summary>
    /// Generic tooltip manager that works with any UI framework.
    /// Uses dependency injection for maximum flexibility and testability.
    /// Game-agnostic - can be used for any type of game or application.
    /// </summary>
    public class TooltipManager : MonoBehaviour, ITooltipService
    {
        [Header("Configuration")]
        [Tooltip("Tooltip behavior settings")]
        [SerializeField] private TooltipConfig config = new TooltipConfig();

        // Dependencies - injected or found automatically
        private ITooltipDisplay display;
        private ITooltipPositioner positioner;

        // State tracking
        private ITooltipData currentTooltipData;
        private Vector2 currentTriggerPosition;
        private float hoverStartTime;
        private float hideStartTime;
        private bool isHovering;
        private bool shouldHide;

        private void Awake()
        {
            InitializeDependencies();
            InitializePositioner();
        }

        private void Update()
        {
            HandleTooltipTiming();
            UpdateTooltipPosition();
        }

        /// <summary>
        /// Initializes display and positioner dependencies.
        /// Can be overridden for custom implementations.
        /// </summary>
        protected virtual void InitializeDependencies()
        {
            // Try to find display implementation in scene
            display = FindFirstObjectByType<UIToolkitTooltipDisplay>();

            if (display == null)
            {
                Debug.LogWarning("[TooltipManager] No UIToolkitTooltipDisplay found in scene. Tooltips will not be displayed.");
                enabled = false;
            }
        }

        /// <summary>
        /// Initializes the positioner with current settings.
        /// </summary>
        private void InitializePositioner()
        {
            positioner = new MouseFollowPositioner();
            positioner.SetOffset(config.Offset);
            if (config.ClampToScreen)
            {
                positioner.SetBounds(new Vector2(Screen.width, Screen.height));
            }
        }

        /// <summary>
        /// Handles tooltip show/hide timing based on hover state.
        /// </summary>
        private void HandleTooltipTiming()
        {
            if (isHovering && display != null && !display.IsVisible && Time.time - hoverStartTime >= config.ShowDelay)
            {
                ShowTooltipInternal();
            }

            if (shouldHide && Time.time - hideStartTime >= config.HideDelay)
            {
                HideTooltipInternal();
            }
        }

        /// <summary>
        /// Updates tooltip position if following mouse.
        /// </summary>
        private void UpdateTooltipPosition()
        {
            if (!config.FollowMouse || display == null || !display.IsVisible || positioner == null) return;

            Vector2 mousePos = Input.mousePosition;
            Vector2 tooltipSize = display.GetTooltipSize();
            Vector2 newPosition = positioner.CalculatePosition(mousePos, tooltipSize, new Vector2(Screen.width, Screen.height));

            display.UpdatePosition(newPosition);
        }

        /// <summary>
        /// Shows tooltip for the specified data.
        /// </summary>
        public void ShowTooltip(ITooltipData data)
        {
            if (data == null || !data.HasContent)
            {
                Debug.LogWarning("[TooltipManager] Attempted to show tooltip with null or empty data");
                return;
            }

            currentTooltipData = data;
            hoverStartTime = Time.time;
            isHovering = true;
            shouldHide = false;
        }

        /// <summary>
        /// Starts the hide process for the current tooltip.
        /// </summary>
        public void HideTooltip()
        {
            isHovering = false;
            shouldHide = true;
            hideStartTime = Time.time;
        }

        /// <summary>
        /// Immediately hides the tooltip without delay.
        /// </summary>
        public void HideTooltipImmediate()
        {
            isHovering = false;
            shouldHide = false;
            currentTooltipData = null;

            if (display != null)
            {
                display.HideTooltip();
            }
        }

        /// <summary>
        /// Actually displays the tooltip with current data.
        /// </summary>
        private void ShowTooltipInternal()
        {
            if (display == null || currentTooltipData == null) return;

            Vector2 position = config.FollowMouse ? Input.mousePosition : currentTriggerPosition;
            Vector2 tooltipSize = display.GetTooltipSize();
            Vector2 finalPosition = positioner?.CalculatePosition(position, tooltipSize, new Vector2(Screen.width, Screen.height)) ?? position;

            display.ShowTooltip(currentTooltipData, finalPosition);
        }

        /// <summary>
        /// Actually hides the tooltip.
        /// </summary>
        private void HideTooltipInternal()
        {
            currentTooltipData = null;
            if (display != null)
            {
                display.HideTooltip();
            }
        }

        /// <summary>
        /// Updates the tooltip configuration at runtime.
        /// </summary>
        public void UpdateConfiguration(TooltipConfig newConfig)
        {
            config = newConfig;
        }

        /// <summary>
        /// Updates the tooltip position manually.
        /// </summary>
        public void UpdatePosition()
        {
            UpdateTooltipPosition();
        }

        /// <summary>
        /// Whether the tooltip is currently visible.
        /// </summary>
        public bool IsVisible => display != null && display.IsVisible;
    }
}
