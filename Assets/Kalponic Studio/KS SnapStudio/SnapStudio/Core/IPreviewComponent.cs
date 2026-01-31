namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Common interface for all preview window components
    /// Ensures consistent lifecycle and naming across components
    /// Follows Universal Unity Coding Guide - Component Interface Standardization
    /// </summary>
    public interface IPreviewComponent
    {
        /// <summary>
        /// Component name for logging and identification
        /// </summary>
        string ComponentName { get; }

        /// <summary>
        /// Initialize the component
        /// </summary>
        void Initialize();

        /// <summary>
        /// Draw the component's GUI
        /// </summary>
        void OnGUI();

        /// <summary>
        /// Cleanup resources when component is no longer needed
        /// </summary>
        void Cleanup();
    }

    /// <summary>
    /// Interface for components that handle user input
    /// </summary>
    public interface IInputHandler
    {
        /// <summary>
        /// Handle input events (mouse, keyboard, etc.)
        /// </summary>
        /// <param name="e">The input event</param>
        /// <param name="activeRect">The active rectangle for this component</param>
        void HandleInput(UnityEngine.Event e, UnityEngine.Rect activeRect);
    }

    /// <summary>
    /// Interface for components that can be enabled/disabled
    /// </summary>
    public interface IToggleable
    {
        /// <summary>
        /// Whether this component is currently enabled/active
        /// </summary>
        bool IsEnabled { get; set; }
    }

    /// <summary>
    /// Interface for components that have configurable settings
    /// </summary>
    public interface IConfigurable
    {
        /// <summary>
        /// Load settings from a configuration source
        /// </summary>
        void LoadSettings();

        /// <summary>
        /// Save current settings
        /// </summary>
        void SaveSettings();
    }
}
