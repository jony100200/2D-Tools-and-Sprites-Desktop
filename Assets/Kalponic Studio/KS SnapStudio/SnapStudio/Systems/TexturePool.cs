using System.Collections.Generic;
using UnityEngine;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Interface for texture pooling and reuse
    /// </summary>
    public interface ITexturePool
    {
        Texture2D GetTexture(int width, int height, TextureFormat format, bool useCompression);
        void ReturnTexture(Texture2D texture);
        void ClearPool();
        int PoolSize { get; }
        long GetPoolMemoryUsage();
    }

    /// <summary>
    /// Texture pool implementation for memory efficiency
    /// </summary>
    public class TexturePool : ITexturePool
    {
        private readonly Queue<Texture2D> pool = new Queue<Texture2D>();
        private readonly int maxPoolSize;
        private readonly IMemoryManager memoryManager;

        public TexturePool(int maxSize, IMemoryManager memoryMgr)
        {
            maxPoolSize = maxSize;
            memoryManager = memoryMgr;
        }

        public int PoolSize => pool.Count;

        public Texture2D GetTexture(int width, int height, TextureFormat format, bool useCompression)
        {
            // Try to reuse existing texture
            if (pool.Count > 0)
            {
                var texture = pool.Dequeue();

                // Check if texture matches requirements, recreate if not
                if (texture.width != width || texture.height != height || texture.format != format)
                {
                    Object.Destroy(texture);
                    return CreateNewTexture(width, height, format, useCompression);
                }

                return texture;
            }

            return CreateNewTexture(width, height, format, useCompression);
        }

        private Texture2D CreateNewTexture(int width, int height, TextureFormat format, bool useCompression)
        {
            // Use compressed format if requested and memory is low
            if (useCompression && memoryManager.IsMemoryLow)
            {
                format = TextureFormat.DXT1; // Compressed format
            }

            var texture = new Texture2D(width, height, format, false);
            texture.name = $"PooledTexture_{width}x{height}";
            return texture;
        }

        public void ReturnTexture(Texture2D texture)
        {
            if (texture == null || pool.Count >= maxPoolSize)
            {
                if (texture != null)
                    Object.Destroy(texture);
                return;
            }

            // Clear texture data before pooling
            texture.SetPixels32(new Color32[texture.width * texture.height]);
            texture.Apply();

            pool.Enqueue(texture);
        }

        public void ClearPool()
        {
            while (pool.Count > 0)
            {
                var texture = pool.Dequeue();
                if (texture != null)
                    Object.Destroy(texture);
            }
        }

        public long GetPoolMemoryUsage()
        {
            long totalBytes = 0;
            foreach (var texture in pool)
            {
                if (texture != null)
                {
                    // Estimate memory usage (width * height * bytes per pixel)
                    int bytesPerPixel = texture.format == TextureFormat.RGBA32 ? 4 :
                                       texture.format == TextureFormat.DXT1 ? 1 : 4; // Conservative estimate
                    totalBytes += (long)texture.width * texture.height * bytesPerPixel;
                }
            }
            return totalBytes / (1024 * 1024); // Convert to MB
        }
    }
}