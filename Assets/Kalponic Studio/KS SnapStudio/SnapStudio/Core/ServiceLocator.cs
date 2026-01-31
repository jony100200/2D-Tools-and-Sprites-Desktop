// Copyright (c) 2025 Kalponic Games. All rights reserved.
// KS SnapStudio - Professional 3D to 2D Animation Capture Tool

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using KalponicGames.KS_SnapStudio;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Global service locator for KS SnapStudio components
    /// Provides centralized access to main editor windows and systems
    /// </summary>
    public static class ServiceLocator
    {
        // Main editor window reference
        private static KSSnapStudioWindow _mainWindow;
        public static KSSnapStudioWindow MainWindow
        {
            get => _mainWindow;
            set => _mainWindow = value;
        }

        // Play mode capture system reference
        private static KSPlayModeCapture _captureSystem;
        public static KSPlayModeCapture CaptureSystem
        {
            get => _captureSystem;
            set => _captureSystem = value;
        }

        /// <summary>
        /// Event system for component communication
        /// </summary>
        public static class Events
        {
            // Animation preview events
            public static Action<AnimationClip> OnAnimationSelected;
            public static Action<bool> OnPreviewToggled;
            public static Action<float> OnPlaybackTimeChanged;

            // Component registration events
            public static Action<IPreviewComponent> OnComponentRegistered;
            public static Action<IPreviewComponent> OnComponentUnregistered;

            // Settings events
            public static Action OnSettingsChanged;

            // Error and capture events
            public static Action<string> OnErrorOccurred;
            public static Action OnCaptureStarted;
            public static Action OnCaptureCompleted;
            public static Action<int> OnFrameCaptured;
            public static Action OnSamplingChanged;
        }

        /// <summary>
        /// Initialize the service locator
        /// Called during editor startup
        /// </summary>
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // Clear references on domain reload
            _mainWindow = null;
            _captureSystem = null;

            Logger.LogInfo("ServiceLocator", "ServiceLocator initialized");
        }

        /// <summary>
        /// Register a preview component with the service locator
        /// </summary>
        public static void RegisterComponent(IPreviewComponent component)
        {
            if (component == null)
            {
                Logger.LogError("ServiceLocator", "Cannot register null component");
                return;
            }

            Events.OnComponentRegistered?.Invoke(component);
            Logger.LogInfo("ServiceLocator", $"Component registered: {component.GetType().Name}");
        }

        /// <summary>
        /// Unregister a preview component from the service locator
        /// </summary>
        public static void UnregisterComponent(IPreviewComponent component)
        {
            if (component == null)
            {
                Logger.LogError("ServiceLocator", "Cannot unregister null component");
                return;
            }

            Events.OnComponentUnregistered?.Invoke(component);
            Logger.LogInfo("ServiceLocator", $"Component unregistered: {component.GetType().Name}");
        }

        /// <summary>
        /// Reset all services and events
        /// </summary>
        public static void Reset()
        {
            MainWindow = null;
            CaptureSystem = null;

            Events.OnErrorOccurred = null;
            Events.OnCaptureStarted = null;
            Events.OnCaptureCompleted = null;
            Events.OnFrameCaptured = null;
            Events.OnSamplingChanged = null;
        }

        /// <summary>
        /// Check if all required services are registered
        /// </summary>
        public static bool AreServicesReady()
        {
            return MainWindow != null && CaptureSystem != null;
        }
    }
}
#endif
