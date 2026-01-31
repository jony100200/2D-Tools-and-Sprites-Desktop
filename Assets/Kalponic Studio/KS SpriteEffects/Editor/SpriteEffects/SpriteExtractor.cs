using UnityEditor;
using UnityEngine;

namespace KalponicStudio.SpriteEffects
{
    public interface ISpriteExtractor
    {
        Texture2D ExtractSpriteTexture(Sprite sprite, bool useFullTexture = false);
    }

    public sealed class SpriteReadableExtractor : ISpriteExtractor
    {
        public Texture2D ExtractSpriteTexture(Sprite sprite, bool useFullTexture = false)
        {
            if (sprite == null || sprite.texture == null)
            {
                return null;
            }

            EnsureTextureReadable(sprite.texture);

            Rect rect = useFullTexture ? new Rect(0, 0, sprite.texture.width, sprite.texture.height) : sprite.rect;
            int width = Mathf.FloorToInt(rect.width);
            int height = Mathf.FloorToInt(rect.height);
            int startX = Mathf.FloorToInt(rect.x);
            int startY = Mathf.FloorToInt(rect.y);

            Texture2D extracted = new Texture2D(width, height, TextureFormat.ARGB32, false);
            extracted.SetPixels(sprite.texture.GetPixels(startX, startY, width, height));
            extracted.Apply();

            return extracted;
        }

        private static void EnsureTextureReadable(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}
