using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KalponicStudio
{
    public static class PrefabCreator
    {
        public static void CreatePrefabs(List<(string path, GameObject instance)> populated, string outputFolder, bool mirrorStructure, bool overwriteExisting, string inputFolder)
        {
            int total = populated.Count;
            for (int i = 0; i < total; i++)
            {
                var (path, instance) = populated[i];
                if (!IsValidForPrefab(instance)) continue;
                EditorUtility.DisplayProgressBar("Creating Prefabs", $"Processing {instance.name}", (float)i / total);
                string relativeDir = mirrorStructure ? GetRelativePath(path, inputFolder) : "";
                string prefabName = instance.name + ".prefab";
                string outputPath = Path.Combine(outputFolder, relativeDir, prefabName).Replace("\\", "/");
                string dir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                if (!overwriteExisting && File.Exists(outputPath)) continue;
                PrefabUtility.SaveAsPrefabAsset(instance, outputPath);
            }
            EditorUtility.ClearProgressBar();
            Debug.Log($"Created {total} prefabs in {outputFolder}");
        }

        private static bool IsValidForPrefab(GameObject instance)
        {
            if (instance.GetComponent<MeshFilter>() == null && instance.GetComponent<SkinnedMeshRenderer>() == null)
            {
                Debug.LogWarning($"Skipping {instance.name}: No mesh component found.");
                return false;
            }
            return true;
        }

        private static string GetRelativePath(string fullPath, string basePath)
        {
            if (fullPath.StartsWith(basePath))
            {
                string rel = fullPath.Substring(basePath.Length).TrimStart('/');
                return Path.GetDirectoryName(rel) ?? "";
            }
            return "";
        }
    }
}