using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Frame processor for batch processing with memory management
    /// </summary>
    public class FrameProcessor : IFrameProcessor
    {
        private readonly Queue<object> frameQueue = new Queue<object>();
        private readonly IMemoryManager memoryManager;
        private readonly ITexturePool texturePool;
        private bool isProcessing = false;
        private const int MAX_QUEUE_SIZE = 10; // Prevent unlimited queue growth

        public int QueueSize => frameQueue.Count;
        public bool IsProcessing => isProcessing;

        public FrameProcessor(IMemoryManager memoryMgr, ITexturePool texPool)
        {
            memoryManager = memoryMgr;
            texturePool = texPool;
        }

        public void QueueFrame(object frameData)
        {
            lock (frameQueue)
            {
                // Prevent queue from growing too large
                if (frameQueue.Count >= MAX_QUEUE_SIZE)
                {
                    Debug.LogWarning("FrameProcessor: Queue full, dropping oldest frame");
                    frameQueue.Dequeue();
                }

                frameQueue.Enqueue(frameData);
            }

            // Start processing if not already running
            if (!isProcessing)
            {
                StartProcessing();
            }
        }

        public IEnumerator ProcessFrames(System.Action<object> onFrameProcessed)
        {
            isProcessing = true;

            while (true)
            {
                object frameData = null;

                lock (frameQueue)
                {
                    if (frameQueue.Count > 0)
                        frameData = frameQueue.Dequeue();
                }

                if (frameData != null)
                {
                    // Check memory before processing
                    if (memoryManager.IsMemoryLow)
                    {
                        Debug.Log("FrameProcessor: Memory low, forcing cleanup before processing frame");
                        yield return memoryManager.CleanupRoutine();
                    }

                    // Process the frame
                    onFrameProcessed?.Invoke(frameData);

                    // Small delay to prevent overwhelming the system
                    yield return new WaitForSeconds(0.01f);
                }
                else
                {
                    // No frames to process
                    yield return new WaitForSeconds(0.05f);
                }

                // Check if we should stop processing
                lock (frameQueue)
                {
                    if (frameQueue.Count == 0)
                        break;
                }
            }

            isProcessing = false;
        }

        public void ClearQueue()
        {
            lock (frameQueue)
            {
                frameQueue.Clear();
            }
        }

        private void StartProcessing()
        {
            // This would be called from a MonoBehaviour
            // For now, we'll rely on the caller to start the coroutine
        }
    }
}