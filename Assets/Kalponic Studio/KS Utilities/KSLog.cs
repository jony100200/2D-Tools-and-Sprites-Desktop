using UnityEngine;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Centralized logger that enforces consistent formatting and keeps
    /// informational logs disabled in player builds unless explicitly enabled.
    /// </summary>
    public static class KSLog
    {
#if UNITY_EDITOR
        private static bool infoLoggingEnabled = true;
#else
        private static bool infoLoggingEnabled = false;
#endif

        private const string DefaultPrefix = "[KS]";

        /// <summary>
        /// Toggle informational logs (warnings and errors remain active).
        /// </summary>
        public static void SetInfoLoggingEnabled(bool enabled)
        {
            infoLoggingEnabled = enabled;
        }

        public static void Info(string message, string category = null, Object context = null)
        {
            LogInternal(LogType.Log, message, category, context);
        }

        public static void Warning(string message, string category = null, Object context = null)
        {
            LogInternal(LogType.Warning, message, category, context);
        }

        public static void Error(string message, string category = null, Object context = null)
        {
            LogInternal(LogType.Error, message, category, context);
        }

        private static void LogInternal(LogType logType, string message, string category, Object context)
        {
            if (logType == LogType.Log && !infoLoggingEnabled)
            {
                return;
            }

            string prefix = string.IsNullOrEmpty(category)
                ? DefaultPrefix
                : $"{DefaultPrefix}[{category}]";

            string formatted = $"{prefix} {message}";

            switch (logType)
            {
                case LogType.Error:
                    if (context != null)
                    {
                        Debug.LogError(formatted, context);
                    }
                    else
                    {
                        Debug.LogError(formatted);
                    }
                    break;
                case LogType.Warning:
                    if (context != null)
                    {
                        Debug.LogWarning(formatted, context);
                    }
                    else
                    {
                        Debug.LogWarning(formatted);
                    }
                    break;
                default:
                    if (context != null)
                    {
                        Debug.Log(formatted, context);
                    }
                    else
                    {
                        Debug.Log(formatted);
                    }
                    break;
            }
        }
    }
}
