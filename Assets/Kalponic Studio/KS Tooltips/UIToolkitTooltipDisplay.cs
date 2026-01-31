using UnityEngine;
using UnityEngine.UIElements;

namespace KalponicStudio.Tooltips
{
    /// <summary>
    /// UI Toolkit implementation of tooltip display.
    /// Handles showing and hiding tooltips using Unity's UI Toolkit.
    /// </summary>
    public class UIToolkitTooltipDisplay : MonoBehaviour, ITooltipDisplay
    {
        [Header("UI References")]
        [Tooltip("UIDocument containing the tooltip UI")]
        [SerializeField] private UIDocument uiDocument;

        // UI elements
        private VisualElement tooltipContainer;
        private Label titleLabel;
        private Label descriptionLabel;

        private bool isVisible;
        private Vector2 currentPosition;

        private void Awake()
        {
            InitializeUI();
        }

        /// <summary>
        /// Initializes UI elements for tooltip display.
        /// </summary>
        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                uiDocument = FindFirstObjectByType<UIDocument>();
                if (uiDocument == null)
                {
                    Debug.LogError("[UIToolkitTooltipDisplay] No UIDocument found in scene!");
                    return;
                }
            }

            var root = uiDocument.rootVisualElement;

            // Find tooltip elements
            tooltipContainer = root.Q<VisualElement>("TooltipContainer");
            titleLabel = root.Q<Label>("TooltipTitle");
            descriptionLabel = root.Q<Label>("TooltipDescription");

            if (tooltipContainer == null)
            {
                Debug.LogError("[UIToolkitTooltipDisplay] TooltipContainer not found in UI!");
                return;
            }

            // Start hidden
            HideTooltip();
        }

        /// <summary>
        /// Shows the tooltip with the specified data at the given position.
        /// </summary>
        public void ShowTooltip(ITooltipData data, Vector2 position)
        {
            if (tooltipContainer == null || data == null) return;

            // Update content
            if (titleLabel != null)
            {
                titleLabel.text = data.Title;
                titleLabel.style.color = data.TitleColor;
            }

            if (descriptionLabel != null)
            {
                descriptionLabel.text = data.Description;
                descriptionLabel.style.color = data.DescriptionColor;
            }

            // Update styling
            tooltipContainer.style.maxWidth = data.MaxWidth;
            tooltipContainer.style.backgroundColor = data.BackgroundTint;

            // Set position
            UpdatePosition(position);

            // Show tooltip
            tooltipContainer.style.display = DisplayStyle.Flex;
            isVisible = true;

            // Mark for layout update
            if (titleLabel != null) titleLabel.MarkDirtyRepaint();
            if (descriptionLabel != null) descriptionLabel.MarkDirtyRepaint();
            tooltipContainer.MarkDirtyRepaint();
        }

        /// <summary>
        /// Hides the tooltip.
        /// </summary>
        public void HideTooltip()
        {
            if (tooltipContainer != null)
            {
                tooltipContainer.style.display = DisplayStyle.None;
            }
            isVisible = false;
        }

        /// <summary>
        /// Updates the tooltip position.
        /// </summary>
        public void UpdatePosition(Vector2 position)
        {
            if (tooltipContainer == null) return;

            currentPosition = position;
            tooltipContainer.style.left = position.x;
            tooltipContainer.style.top = position.y;
        }

        /// <summary>
        /// Whether the tooltip is currently visible.
        /// </summary>
        public bool IsVisible => isVisible;

        /// <summary>
        /// Gets the current tooltip size.
        /// </summary>
        public Vector2 GetTooltipSize()
        {
            if (tooltipContainer == null) return Vector2.zero;

            return new Vector2(
                tooltipContainer.resolvedStyle.width > 0 ? tooltipContainer.resolvedStyle.width : 200f,
                tooltipContainer.resolvedStyle.height > 0 ? tooltipContainer.resolvedStyle.height : 100f
            );
        }
    }
}
