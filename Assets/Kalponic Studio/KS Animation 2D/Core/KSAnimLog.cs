using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    public enum KSAnimLogLevel
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Centralized logger for KS Animation 2D. Can be toggled per-category to avoid noisy logs in release builds.
    /// </summary>
    public static class KSAnimLog
    {
        private const string Prefix = "[KS Anim2D]";
        private static readonly HashSet<string> EnabledCategories = new HashSet<string>
        {
            "General",
            "Playback",
            "FSM",
            "Diagnostics"
        };

        private static bool enabled = Application.isEditor || Debug.isDebugBuild;
        private static bool includeStackTrace;

        public static bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public static bool IncludeStackTrace
        {
            get => includeStackTrace;
            set => includeStackTrace = value;
        }

        public static void SetCategoryEnabled(string category, bool enable)
        {
            if (string.IsNullOrEmpty(category))
            {
                return;
            }

            if (enable)
            {
                EnabledCategories.Add(category);
            }
            else
            {
                EnabledCategories.Remove(category);
            }
        }

        public static bool IsCategoryEnabled(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return enabled;
            }
            return enabled && EnabledCategories.Contains(category);
        }

        public static void Info(string message, string category = "General", Object context = null) =>
            Log(KSAnimLogLevel.Info, message, category, context);

        public static void Warn(string message, string category = "General", Object context = null) =>
            Log(KSAnimLogLevel.Warning, message, category, context);

        public static void Error(string message, string category = "General", Object context = null) =>
            Log(KSAnimLogLevel.Error, message, category, context);

        public static void Playback(AnimationId id, string clipName, float normalizedTime, float speed, string details = "", Object context = null)
        {
            Info($"[{id}] {clipName} t={normalizedTime:0.00} speed={speed:0.00} {details}", "Playback", context);
        }

        private static void Log(KSAnimLogLevel level, string message, string category, Object context)
        {
            if (!IsCategoryEnabled(category))
            {
                return;
            }

            string finalMessage = $"{Prefix}[{category}][{level}] {message}";
            if (includeStackTrace)
            {
                var trace = new System.Diagnostics.StackTrace(2, true);
                finalMessage += "\n" + trace;
            }

            switch (level)
            {
                case KSAnimLogLevel.Info:
                    Debug.Log(finalMessage, context);
                    break;
                case KSAnimLogLevel.Warning:
                    Debug.LogWarning(finalMessage, context);
                    break;
                case KSAnimLogLevel.Error:
                    Debug.LogError(finalMessage, context);
                    break;
            }
        }
    }
}
