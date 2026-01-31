using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KalponicStudio.SpriteEffects
{
    public interface ISpriteLocator
    {
        IEnumerable<Sprite> FindSprites(string folderPath);
    }

    public sealed class FolderSpriteLocator : ISpriteLocator
    {
        public IEnumerable<Sprite> FindSprites(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                yield break;
            }

            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    yield return sprite;
                }
            }
        }
    }
}
