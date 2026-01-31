#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using System.IO;

namespace KalponicGames.KS_SnapStudio.Editor
{
    /// <summary>
    /// Utility class for locating plugin assets dynamically, regardless of plugin installation location.
    /// This enables true plug-and-play functionality by finding assets by name rather than hardcoded paths.
    /// </summary>
    public static class PluginAssetLocator
    {
        private const string PLUGIN_NAME = "SnapStudio";
        private static string _pluginRootPath;

        /// <summary>
        /// Gets the root path of the KS SnapStudio plugin in the current project.
        /// </summary>
        public static string PluginRootPath
        {
            get
            {
                if (string.IsNullOrEmpty(_pluginRootPath))
                {
                    _pluginRootPath = FindPluginRootPath();
                }
                return _pluginRootPath;
            }
        }

        /// <summary>
        /// Finds a VisualTreeAsset (UXML) by filename within the plugin.
        /// </summary>
        /// <param name="fileName">The UXML filename (without extension)</param>
        /// <returns>The VisualTreeAsset if found, null otherwise</returns>
        public static VisualTreeAsset FindUxmlAsset(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets($"{fileName} t:VisualTreeAsset");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"PluginAssetLocator: Could not find UXML asset '{fileName}'");
                return null;
            }

            // If multiple found, prefer the one in our plugin directory
            string assetPath = null;
            if (!string.IsNullOrEmpty(PluginRootPath))
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains(PLUGIN_NAME))
                    {
                        assetPath = path;
                        break;
                    }
                }
            }

            // Fallback to first found
            if (string.IsNullOrEmpty(assetPath) && guids.Length > 0)
            {
                assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            }

            if (!string.IsNullOrEmpty(assetPath))
            {
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
            }

            return null;
        }

        /// <summary>
        /// Finds a StyleSheet (USS) by filename within the plugin.
        /// </summary>
        /// <param name="fileName">The USS filename (without extension)</param>
        /// <returns>The StyleSheet if found, null otherwise</returns>
        public static StyleSheet FindUssAsset(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets($"{fileName} t:StyleSheet");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"PluginAssetLocator: Could not find USS asset '{fileName}'");
                return null;
            }

            // If multiple found, prefer the one in our plugin directory
            string assetPath = null;
            if (!string.IsNullOrEmpty(PluginRootPath))
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains(PLUGIN_NAME))
                    {
                        assetPath = path;
                        break;
                    }
                }
            }

            // Fallback to first found
            if (string.IsNullOrEmpty(assetPath) && guids.Length > 0)
            {
                assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            }

            if (!string.IsNullOrEmpty(assetPath))
            {
                return AssetDatabase.LoadAssetAtPath<StyleSheet>(assetPath);
            }

            return null;
        }

        /// <summary>
        /// Finds any asset by type and name within the plugin.
        /// </summary>
        /// <typeparam name="T">The asset type to find</typeparam>
        /// <param name="assetName">The asset name (without extension)</param>
        /// <returns>The asset if found, null otherwise</returns>
        public static T FindAsset<T>(string assetName) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"{assetName} t:{typeof(T).Name}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"PluginAssetLocator: Could not find {typeof(T).Name} asset '{assetName}'");
                return null;
            }

            // If multiple found, prefer the one in our plugin directory
            string assetPath = null;
            if (!string.IsNullOrEmpty(PluginRootPath))
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains(PLUGIN_NAME))
                    {
                        assetPath = path;
                        break;
                    }
                }
            }

            // Fallback to first found
            if (string.IsNullOrEmpty(assetPath) && guids.Length > 0)
            {
                assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            }

            if (!string.IsNullOrEmpty(assetPath))
            {
                return AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }

            return null;
        }

        /// <summary>
        /// Gets a default output folder path relative to the project.
        /// </summary>
        /// <param name="folderName">The folder name (e.g., "Captures", "Thumbnails")</param>
        /// <returns>A relative path like "Assets/[folderName]"</returns>
        public static string GetDefaultOutputFolder(string folderName)
        {
            return $"Assets/{folderName}";
        }

        /// <summary>
        /// Gets the default captures folder path.
        /// </summary>
        public static string DefaultCapturesFolder => GetDefaultOutputFolder("Captures");

        /// <summary>
        /// Gets the default thumbnails folder path.
        /// </summary>
        public static string DefaultThumbnailsFolder => GetDefaultOutputFolder("Thumbnails");

        /// <summary>
        /// Gets the default prefabs folder path.
        /// </summary>
        public static string DefaultPrefabsFolder => GetDefaultOutputFolder("Prefabs");

        /// <summary>
        /// Finds the plugin root path by locating a known script file.
        /// </summary>
        private static string FindPluginRootPath()
        {
            // Find this script file to determine plugin location
            string[] guids = AssetDatabase.FindAssets("PluginAssetLocator t:MonoScript");
            if (guids.Length == 0)
            {
                Debug.LogWarning("PluginAssetLocator: Could not find PluginAssetLocator script to determine plugin root");
                return null;
            }

            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(scriptPath))
            {
                return null;
            }

            // Navigate up from the script location to find the plugin root
            // Structure: Assets/PluginName/Editor/Utilities/PluginAssetLocator.cs
            DirectoryInfo dir = new FileInfo(scriptPath).Directory;
            while (dir != null && dir.Name != "Assets")
            {
                // Check if this directory contains our plugin name
                if (dir.Name.Contains(PLUGIN_NAME))
                {
                    return $"Assets/{dir.Name}";
                }
                // Move up one directory
                dir = dir.Parent;
            }

            // Fallback: try to find any directory containing our plugin name
            try
            {
                string[] allDirs = Directory.GetDirectories("Assets", "*", SearchOption.AllDirectories);
                foreach (string directory in allDirs)
                {
                    if (Path.GetFileName(directory).Contains(PLUGIN_NAME))
                    {
                        return directory.Replace("\\", "/");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"PluginAssetLocator: Error searching directories: {e.Message}");
            }

            Debug.LogWarning("PluginAssetLocator: Could not determine plugin root path");
            return null;
        }
    }
}
#endif
