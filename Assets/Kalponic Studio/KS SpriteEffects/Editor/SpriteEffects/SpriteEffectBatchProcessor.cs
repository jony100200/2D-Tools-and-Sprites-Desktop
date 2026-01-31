using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KalponicStudio.SpriteEffects
{
    public sealed class SpriteEffectBatchProcessor
    {
        private readonly ISpriteLocator locator;
        private readonly ISpriteExtractor extractor;
        private readonly IEffectProcessor processor;
        private readonly ITextureSaver saver;

        public event Action<int, int, string> OnTextureProcessed;
        public event Action<string> OnTextureFailed;

        public SpriteEffectBatchProcessor(ISpriteLocator locator, ISpriteExtractor extractor, IEffectProcessor processor, ITextureSaver saver)
        {
            this.locator = locator;
            this.extractor = extractor;
            this.processor = processor;
            this.saver = saver;
        }

        public void ProcessFolder(string inputFolder, string outputFolder, Material material, string prefix = "", string suffix = "")
        {
            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            List<Sprite> sprites = new List<Sprite>(locator.FindSprites(inputFolder));
            if (sprites.Count == 0)
            {
                Debug.LogWarning("No sprites found to process.");
                return;
            }

            Dictionary<Texture2D, Sprite> uniqueTextures = new Dictionary<Texture2D, Sprite>();
            foreach (Sprite sprite in sprites)
            {
                if (sprite == null || sprite.texture == null)
                {
                    continue;
                }

                if (!uniqueTextures.ContainsKey(sprite.texture))
                {
                    uniqueTextures.Add(sprite.texture, sprite);
                }
            }

            int total = uniqueTextures.Count;
            int index = 1;
            foreach (KeyValuePair<Texture2D, Sprite> entry in uniqueTextures)
            {
                ProcessSpriteAsset(entry.Value, outputFolder, material, prefix, suffix, index, total);
                index++;
            }
        }

        public void ProcessSingleSprite(Sprite sprite, string outputFolder, Material material, string prefix = "", string suffix = "")
        {
            if (sprite == null)
            {
                Debug.LogWarning("No sprite selected to process.");
                return;
            }

            ProcessSpriteAsset(sprite, outputFolder, material, prefix, suffix, 1, 1);
        }

        private void ProcessSpriteAsset(Sprite sprite, string outputFolder, Material material, string prefix, string suffix, int index, int total)
        {
            try
            {
                Texture2D source = extractor.ExtractSpriteTexture(sprite, true);
                if (source == null)
                {
                    OnTextureFailed?.Invoke(sprite != null ? sprite.name : "(null sprite)");
                    return;
                }

                Texture2D processed = processor.Process(source, material);
                if (processed == null)
                {
                    OnTextureFailed?.Invoke(sprite.name);
                    return;
                }

                string baseName = sprite != null && sprite.texture != null ? sprite.texture.name : sprite.name;
                string sourcePath = sprite != null && sprite.texture != null ? AssetDatabase.GetAssetPath(sprite.texture) : null;
                saver.Save(processed, outputFolder, baseName, prefix, suffix, sourcePath);
                UnityEngine.Object.DestroyImmediate(processed);
                UnityEngine.Object.DestroyImmediate(source);

                OnTextureProcessed?.Invoke(index, total, baseName);
            }
            catch (Exception ex)
            {
                string spriteId = sprite != null ? sprite.name : "(null sprite)";
                Debug.LogError($"Failed to process sprite {spriteId}: {ex.Message}");
                OnTextureFailed?.Invoke(spriteId);
            }
        }
    }
}
