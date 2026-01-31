using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Asynchronous frame processor for background PNG encoding
    /// Prevents render thread stalls and OOM crashes on low-end systems
    /// </summary>
    public class AsyncFrameProcessor
    {
        private Queue<FrameRequest> frameQueue = new Queue<FrameRequest>();
        private const int MAX_QUEUE_SIZE = 10;
        private const int MAX_CONCURRENT_ENCODERS = 2;
        private int activeEncoders = 0;
        private bool isProcessing = false;

        private class FrameRequest
        {
            public Texture2D Texture;
            public string FilePath;
            public System.Action OnComplete;
            public System.Action<Texture2D> OnTextureProcessed;

            public FrameRequest(Texture2D texture, string filePath,
                              System.Action onComplete,
                              System.Action<Texture2D> onTextureProcessed = null)
            {
                Texture = texture;
                FilePath = filePath;
                OnComplete = onComplete;
                OnTextureProcessed = onTextureProcessed;
            }
        }

        /// <summary>
        /// Queue a frame for asynchronous processing
        /// </summary>
        public void QueueFrame(Texture2D texture, string filePath,
                              System.Action onComplete = null,
                              System.Action<Texture2D> onTextureProcessed = null)
        {
            lock (frameQueue)
            {
                // Prevent queue from growing too large
                if (frameQueue.Count >= MAX_QUEUE_SIZE)
                {
                    Debug.LogWarning("AsyncFrameProcessor: Queue full, dropping oldest frame");
                    var oldRequest = frameQueue.Dequeue();
                    if (oldRequest.Texture != null)
                        Object.Destroy(oldRequest.Texture);
                }

                frameQueue.Enqueue(new FrameRequest(texture, filePath, onComplete, onTextureProcessed));
            }

            // Start processing if not already running
            if (!isProcessing)
            {
                StartProcessing();
            }
        }

        /// <summary>
        /// Process queued frames asynchronously
        /// </summary>
        private void StartProcessing()
        {
            if (isProcessing) return;

            isProcessing = true;
            // This would be called from a MonoBehaviour in a real implementation
            // For now, we'll process synchronously but on background threads
            ProcessFramesAsync();
        }

        /// <summary>
        /// Process frames using background threads
        /// </summary>
        private async void ProcessFramesAsync()
        {
            while (true)
            {
                FrameRequest request = null;

                lock (frameQueue)
                {
                    if (frameQueue.Count > 0 && activeEncoders < MAX_CONCURRENT_ENCODERS)
                    {
                        request = frameQueue.Dequeue();
                        activeEncoders++;
                    }
                }

                if (request != null)
                {
                    // Process frame on background thread
                    await ProcessFrameAsync(request);
                    activeEncoders--;
                }
                else
                {
                    // Check if we're done
                    lock (frameQueue)
                    {
                        if (frameQueue.Count == 0 && activeEncoders == 0)
                        {
                            isProcessing = false;
                            break;
                        }
                    }

                    // Wait a bit before checking again
                    await Task.Delay(10);
                }
            }
        }

        /// <summary>
        /// Process a single frame asynchronously
        /// </summary>
        private async Task ProcessFrameAsync(FrameRequest request)
        {
            try
            {
                // Notify that texture processing is starting
                request.OnTextureProcessed?.Invoke(request.Texture);

                // Encode PNG on background thread
                byte[] pngData = await EncodePNGAsync(request.Texture);

                // Write file on background thread
                await WriteFileAsync(request.FilePath, pngData);

                // Cleanup texture
                if (request.Texture != null)
                {
                    Object.Destroy(request.Texture);
                }

                // Notify completion
                request.OnComplete?.Invoke();

                Debug.Log($"AsyncFrameProcessor: Processed frame to {request.FilePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"AsyncFrameProcessor: Failed to process frame {request.FilePath}: {ex.Message}");

                // Cleanup on error
                if (request.Texture != null)
                {
                    Object.Destroy(request.Texture);
                }
            }
        }

        /// <summary>
        /// Encode texture to PNG on background thread
        /// </summary>
        private async Task<byte[]> EncodePNGAsync(Texture2D texture)
        {
            return await Task.Run(() => {
                if (texture == null)
                    throw new System.ArgumentNullException("texture");

                return texture.EncodeToPNG();
            });
        }

        /// <summary>
        /// Write file asynchronously
        /// </summary>
        private async Task WriteFileAsync(string filePath, byte[] data)
        {
            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create,
                                                        System.IO.FileAccess.Write,
                                                        System.IO.FileShare.None, 4096, true))
            {
                await stream.WriteAsync(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Clear all queued frames
        /// </summary>
        public void ClearQueue()
        {
            lock (frameQueue)
            {
                while (frameQueue.Count > 0)
                {
                    var request = frameQueue.Dequeue();
                    if (request.Texture != null)
                        Object.Destroy(request.Texture);
                }
            }
        }

        /// <summary>
        /// Get current queue status
        /// </summary>
        public int QueueSize => frameQueue.Count;
        public int ActiveEncoders => activeEncoders;
        public bool IsProcessing => isProcessing;
    }
}