using System.IO;
using UnityEditor;
using UnityEngine;

namespace KalponicStudio.SpriteEffects
{
    public interface ITextureSaver
    {
        string Save(Texture2D texture, string outputFolder, string baseName, string prefix = "", string suffix = "", string sourceAssetPath = null);
    }

    public sealed class SpriteTextureSaver : ITextureSaver
    {
        public string Save(Texture2D texture, string outputFolder, string baseName, string prefix = "", string suffix = "", string sourceAssetPath = null)
        {
            if (texture == null || string.IsNullOrEmpty(outputFolder))
            {
                return null;
            }

            string safeName = string.IsNullOrEmpty(baseName) ? "Sprite" : baseName;
            string candidateName = prefix + safeName + suffix;
            string uniqueName = GenerateUniqueName(outputFolder, string.IsNullOrEmpty(candidateName) ? safeName : candidateName);
            string filePath = Path.Combine(outputFolder, uniqueName + ".png").Replace("\\", "/");

            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);
            AssetDatabase.ImportAsset(filePath);

            ConfigureSpriteImporter(filePath, sourceAssetPath);
            return filePath;
        }

        private static string GenerateUniqueName(string folder, string baseName)
        {
            string sanitized = string.IsNullOrEmpty(baseName) ? "Sprite" : baseName;
            string candidate = sanitized;
            int counter = 1;
            while (AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(folder, candidate + ".png").Replace("\\", "/")) != null)
            {
                candidate = sanitized + "_" + counter;
                counter++;
            }

            return candidate;
        }

        private static void ConfigureSpriteImporter(string assetPath, string sourceAssetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            TextureImporter sourceImporter = !string.IsNullOrEmpty(sourceAssetPath)
                ? AssetImporter.GetAtPath(sourceAssetPath) as TextureImporter
                : null;

            if (sourceImporter != null)
            {
                CopySpriteImporterSettings(sourceImporter, importer);
            }
            else
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Compressed;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteExtrude = 1;
                importer.SetTextureSettings(settings);
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        private static void CopySpriteImporterSettings(TextureImporter source, TextureImporter destination)
        {
            var textureSettings = new TextureImporterSettings();
            source.ReadTextureSettings(textureSettings);

            destination.textureType = source.textureType;
            destination.spriteImportMode = source.spriteImportMode;
            destination.spritePixelsPerUnit = source.spritePixelsPerUnit;
            destination.mipmapEnabled = source.mipmapEnabled;
            destination.filterMode = source.filterMode;
            destination.textureCompression = source.textureCompression;
            destination.wrapMode = source.wrapMode;
            destination.wrapModeU = source.wrapModeU;
            destination.wrapModeV = source.wrapModeV;
            destination.alphaIsTransparency = source.alphaIsTransparency;
            destination.sRGBTexture = source.sRGBTexture;
            destination.spritePackingTag = source.spritePackingTag;
            destination.spritePivot = source.spritePivot;
            destination.spriteBorder = source.spriteBorder;
            destination.SetTextureSettings(textureSettings);

            if (source.spriteImportMode == SpriteImportMode.Multiple)
            {
                destination.spritesheet = source.spritesheet;
            }
        }
    }
}
