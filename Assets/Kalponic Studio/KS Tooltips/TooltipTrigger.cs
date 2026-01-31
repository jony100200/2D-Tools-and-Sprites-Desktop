using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

namespace KalponicStudio.Tooltips
{
    /// <summary>
    /// Component that triggers tooltip display when hovering over UI elements.
    /// Game-agnostic - works with any UI framework and tooltip system.
    /// Uses dependency injection for maximum flexibility.
    /// </summary>
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Tooltip Configuration")]
        [Tooltip("Tooltip data to display when hovering")]
        [SerializeField] private TooltipData tooltipData;

        [Tooltip("Tooltip service to use (optional - will find automatically)")]
        [SerializeField] private TooltipManager tooltipService;

        private void Awake()
        {
            // Find tooltip service if not assigned
            if (tooltipService == null)
            {
                tooltipService = FindFirstObjectByType<TooltipManager>();
            }
        }

        /// <summary>
        /// Called when pointer enters the UI element.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltipService != null && tooltipData != null)
            {
                tooltipService.ShowTooltip(tooltipData);
            }
        }

        /// <summary>
        /// Called when pointer exits the UI element.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipService != null)
            {
                tooltipService.HideTooltip();
            }
        }

        /// <summary>
        /// Sets the tooltip data at runtime.
        /// Useful for dynamic tooltips.
        /// </summary>
        public void SetTooltipData(TooltipData data)
        {
            tooltipData = data;
        }

        /// <summary>
        /// Clears the tooltip data.
        /// </summary>
        public void ClearTooltipData()
        {
            tooltipData = null;
        }

        /// <summary>
        /// Sets the tooltip service at runtime.
        /// Useful for testing or custom setups.
        /// </summary>
        public void SetTooltipService(TooltipManager service)
        {
            tooltipService = service;
        }
    }
}
