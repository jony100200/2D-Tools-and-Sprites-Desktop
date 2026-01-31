using UnityEngine;

namespace KalponicStudio.Tooltips
{
    /// <summary>
    /// Core interfaces for the KS Tooltip System.
    /// Follows SOLID principles and dependency inversion for maximum flexibility.
    /// Keep it simple - just what we need, nothing more.
    /// </summary>

    /// <summary>
    /// Represents tooltip content and styling data.
    /// Pure data contract - no behavior.
    /// </summary>
    public interface ITooltipData
    {
        string Title { get; }
        string Description { get; }
        float MaxWidth { get; }
        Color BackgroundTint { get; }
        Color TitleColor { get; }
        Color DescriptionColor { get; }
        bool HasContent { get; }
    }

    /// <summary>
    /// Main tooltip service that handles everything.
    /// Single responsibility: manage tooltip display and timing.
    /// </summary>
    public interface ITooltipService
    {
        void ShowTooltip(ITooltipData data);
        void HideTooltip();
        void UpdatePosition();
        bool IsVisible { get; }
    }

    /// <summary>
    /// Configuration for tooltip behavior.
    /// Simple data class for all timing and positioning settings.
    /// </summary>
    [System.Serializable]
    public class TooltipConfig
    {
        [Tooltip("Delay before showing tooltip")]
        public float ShowDelay = 0.5f;

        [Tooltip("Delay before hiding tooltip")]
        public float HideDelay = 0.2f;

        [Tooltip("Offset from mouse position")]
        public Vector2 Offset = new Vector2(10f, 10f);

        [Tooltip("Follow mouse cursor")]
        public bool FollowMouse = true;

        [Tooltip("Keep tooltip on screen")]
        public bool ClampToScreen = true;
    }

    /// <summary>
    /// Handles tooltip positioning logic.
    /// Separated for different positioning strategies.
    /// </summary>
    public interface ITooltipPositioner
    {
        Vector2 CalculatePosition(Vector2 triggerPosition, Vector2 tooltipSize, Vector2 screenSize);
        void SetOffset(Vector2 offset);
        void SetBounds(Vector2 bounds);
    }

    /// <summary>
    /// Handles tooltip display and hiding.
    /// Abstracted to support different UI frameworks.
    /// </summary>
    public interface ITooltipDisplay
    {
        void ShowTooltip(ITooltipData data, Vector2 position);
        void HideTooltip();
        void UpdatePosition(Vector2 position);
        bool IsVisible { get; }
        Vector2 GetTooltipSize();
    }
}
