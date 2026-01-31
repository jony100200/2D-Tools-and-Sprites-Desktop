using System.Linq;
using UnityEditor;

namespace KalponicStudio
{
    public static class AssetScanner
    {
        public static string[] FindInstantiableAssets(string folderPath, bool includeSubfolders, AssetType type)
        {
            string[] searchInFolders = includeSubfolders ? new[] { folderPath } : null;
            string filter = type switch
            {
                AssetType.Models => "t:Model",
                AssetType.Prefabs => "t:Prefab",
                _ => "t:Model t:Prefab"
            };
            string[] guids = AssetDatabase.FindAssets(filter, searchInFolders);
            return guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
        }
    }
}