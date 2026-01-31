using System.Collections;
using UnityEngine;
#if UNITY_2018_1_OR_NEWER
using UnityEngine.Rendering;
#endif

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// GPU-based capture strategy with async readback for low-end systems
    /// </summary>
    public class GPUCaptureStrategy : ICaptureStrategy
    {
        private readonly IMemoryManager memoryManager;
        private readonly ITexturePool texturePool;

        public bool IsSupported => SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null;
        public string StrategyName => "GPU Capture (Async)";

        public GPUCaptureStrategy(IMemoryManager memoryMgr, ITexturePool texPool)
        {
            memoryManager = memoryMgr;
            texturePool = texPool;
        }

        public IEnumerator CaptureFrame(Camera camera, RenderTexture renderTexture, int frameNumber, System.Action<Texture2D> onComplete)
        {
            if (!IsSupported)
            {
                Debug.LogError("GPUCaptureStrategy: GPU not supported, cannot capture frame");
                yield break;
            }

            // Check memory before capture
            if (memoryManager.IsMemoryLow)
            {
                Debug.LogWarning("GPUCaptureStrategy: Memory low, reducing quality for frame capture");
                // Could reduce render texture size here
            }

            // Set camera target
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;

            // Render
            camera.Render();

#if UNITY_2018_1_OR_NEWER
            // Use async GPU readback for better performance on low-end systems
            var request = AsyncGPUReadback.Request(renderTexture, 0, renderTexture.graphicsFormat,
                (AsyncGPUReadbackRequest req) => {
                    if (!req.hasError)
                    {
                        // Create texture from readback data
                        Texture2D texture = texturePool.GetTexture(renderTexture.width, renderTexture.height,
                            TextureFormat.RGBA32, memoryManager.IsMemoryLow);

                        // Load the raw texture data
                        texture.LoadRawTextureData(req.GetData<byte>());
                        texture.Apply();

                        onComplete?.Invoke(texture);
                    }
                    else
                    {
                        Debug.LogError($"GPUCaptureStrategy: Async readback failed for frame {frameNumber}");
                        // Fallback to sync method
                        CaptureFrameSync(camera, renderTexture, frameNumber, onComplete);
                    }
                });

            // Wait for async readback to complete with timeout
            float startTime = Time.time;
            float timeout = 5f; // 5 second timeout

            while (!request.done && (Time.time - startTime) < timeout)
            {
                yield return null;
            }

            if (!request.done)
            {
                Debug.LogWarning($"GPUCaptureStrategy: Async readback timeout for frame {frameNumber}, falling back to sync");
                CaptureFrameSync(camera, renderTexture, frameNumber, onComplete);
            }
#else
            // Fallback to sync method for older Unity versions
            CaptureFrameSync(camera, renderTexture, frameNumber, onComplete);
#endif

            // Cleanup
            RenderTexture.active = null;
            camera.targetTexture = null;

            memoryManager.LogMemoryUsage($"GPU Capture Frame {frameNumber}");
        }

        /// <summary>
        /// Synchronous fallback capture method
        /// </summary>
        private void CaptureFrameSync(Camera camera, RenderTexture renderTexture, int frameNumber, System.Action<Texture2D> onComplete)
        {
            try
            {
                // Get texture from pool
                Texture2D texture = texturePool.GetTexture(renderTexture.width, renderTexture.height,
                    TextureFormat.RGBA32, memoryManager.IsMemoryLow);

                // Read pixels synchronously
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();
                RenderTexture.active = null;

                onComplete?.Invoke(texture);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GPUCaptureStrategy: Sync capture failed for frame {frameNumber}: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            // GPU resources are managed by Unity
        }
    }

    /// <summary>
    /// CPU-based capture strategy for systems without GPU
    /// </summary>
    public class CPUCaptureStrategy : ICaptureStrategy
    {
        private readonly IMemoryManager memoryManager;
        private readonly ITexturePool texturePool;

        public bool IsSupported => true; // CPU capture always supported as fallback
        public string StrategyName => "CPU Capture (Fallback)";

        public CPUCaptureStrategy(IMemoryManager memoryMgr, ITexturePool texPool)
        {
            memoryManager = memoryMgr;
            texturePool = texPool;
        }

        public IEnumerator CaptureFrame(Camera camera, RenderTexture renderTexture, int frameNumber, System.Action<Texture2D> onComplete)
        {
            // For CPU capture, we use a smaller texture and simpler processing
            int width = renderTexture != null ? renderTexture.width : 512;
            int height = renderTexture != null ? renderTexture.height : 512;

            // Use compressed format to save memory
            Texture2D texture = texturePool.GetTexture(width, height, TextureFormat.DXT1, true);

            // Simple CPU-based capture (placeholder - would need actual implementation)
            // This is a fallback that creates a simple colored texture
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.gray; // Placeholder
            }
            texture.SetPixels(pixels);
            texture.Apply();

            Debug.LogWarning($"CPUCaptureStrategy: Using CPU fallback capture for frame {frameNumber} (limited functionality)");

            onComplete?.Invoke(texture);

            memoryManager.LogMemoryUsage($"CPU Capture Frame {frameNumber}");

            yield return null; // Make this a proper coroutine
        }

        public void Cleanup()
        {
            // CPU resources are managed by texture pool
        }
    }

    /// <summary>
    /// Factory for creating appropriate capture strategies based on hardware capabilities
    /// </summary>
    public static class CaptureStrategyFactory
    {
        public static ICaptureStrategy CreateStrategy(IMemoryManager memoryManager, ITexturePool texturePool = null)
        {
            // Try GPU capture first if supported
            var gpuStrategy = new GPUCaptureStrategy(memoryManager, texturePool);
            if (gpuStrategy.IsSupported)
            {
                Debug.Log("CaptureStrategyFactory: Using GPU capture strategy");
                return gpuStrategy;
            }

            // Fallback to CPU capture
            Debug.Log("CaptureStrategyFactory: GPU not supported, using CPU capture strategy");
            return new CPUCaptureStrategy(memoryManager, texturePool);
        }
    }
}