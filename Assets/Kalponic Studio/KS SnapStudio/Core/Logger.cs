using UnityEngine;
using System;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// Centralized logging utility for consistent logging across components
    /// Follows Universal Unity Coding Guide - Logging and Debugging
    /// </summary>
    public static class Logger
    {
        // Emoji prefixes for different log types
        private const string INFO_PREFIX = "ℹ️";
        private const string WARNING_PREFIX = "⚠️";
        private const string ERROR_PREFIX = "❌";
        private const string SUCCESS_PREFIX = "✅";
        private const string DEBUG_PREFIX = "🔍";

        // Component-specific prefixes
        private static readonly string[] COMPONENT_EMOJIS = {
            "🎬", // AnimationPlaybackController
            "⏱️",  // TimelineController
            "🔍", // ZoomController
            "📊", // FrameInfoDisplay
            "💾", // AnimationExporter
            "🎭", // KSAnimationPreviewer
            "🖼️",  // KSFrameSelector
            "🏭", // PreviewComponentFactory
            "🧪", // IntegrationTestRunner
            "⚙️"   // General/ServiceLocator
        };

        /// <summary>
        /// Get emoji prefix for a component
        /// </summary>
        private static string GetComponentEmoji(string componentName)
        {
            // Simple hash-based emoji selection for consistency
            int hash = componentName.GetHashCode();
            int index = Math.Abs(hash) % COMPONENT_EMOJIS.Length;
            return COMPONENT_EMOJIS[index];
        }

        /// <summary>
        /// Log an info message with component context
        /// </summary>
        public static void LogInfo(string componentName, string message)
        {
            string emoji = GetComponentEmoji(componentName);
            Debug.Log($"{emoji} {componentName}: {message}");
        }

        /// <summary>
        /// Log a warning message with component context
        /// </summary>
        public static void LogWarning(string componentName, string message)
        {
            string emoji = GetComponentEmoji(componentName);
            Debug.LogWarning($"{WARNING_PREFIX} {emoji} {componentName}: {message}");
        }

        /// <summary>
        /// Log an error message with component context
        /// </summary>
        public static void LogError(string componentName, string message)
        {
            string emoji = GetComponentEmoji(componentName);
            Debug.LogError($"{ERROR_PREFIX} {emoji} {componentName}: {message}");
        }

        /// <summary>
        /// Log a success message with component context
        /// </summary>
        public static void LogSuccess(string componentName, string message)
        {
            string emoji = GetComponentEmoji(componentName);
            Debug.Log($"{SUCCESS_PREFIX} {emoji} {componentName}: {message}");
        }

        /// <summary>
        /// Log a debug message with component context (only in development builds)
        /// </summary>
        public static void LogDebug(string componentName, string message)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string emoji = GetComponentEmoji(componentName);
            Debug.Log($"{DEBUG_PREFIX} {emoji} {componentName}: {message}");
#endif
        }

        /// <summary>
        /// Log an exception with component context
        /// </summary>
        public static void LogException(string componentName, Exception exception, string context = null)
        {
            string emoji = GetComponentEmoji(componentName);
            string message = context != null ?
                $"{emoji} {componentName}: Exception in {context}: {exception.Message}" :
                $"{emoji} {componentName}: Exception: {exception.Message}";

            Debug.LogError(message);
            Debug.LogException(exception);
        }

        /// <summary>
        /// Log a performance timing message
        /// </summary>
        public static void LogPerformance(string componentName, string operation, float durationMs)
        {
            string emoji = GetComponentEmoji(componentName);
            string level = durationMs > 100f ? "slow" :
                          durationMs > 16f ? "moderate" : "fast";
            Debug.Log($"{emoji} {componentName}: {operation} completed in {durationMs:F2}ms ({level})");
        }

        /// <summary>
        /// Log component lifecycle events
        /// </summary>
        public static void LogLifecycle(string componentName, string eventName)
        {
            LogDebug(componentName, $"Lifecycle: {eventName}");
        }

        /// <summary>
        /// Log user interaction events
        /// </summary>
        public static void LogInteraction(string componentName, string interaction, string details = null)
        {
            string message = details != null ?
                $"{interaction} - {details}" :
                interaction;
            LogDebug(componentName, $"User Interaction: {message}");
        }
    }
}
