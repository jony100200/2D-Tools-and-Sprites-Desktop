using System.Collections;
using UnityEngine;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Chunked processor for memory-efficient batch processing
    /// Processes items in small chunks to prevent memory overflow
    /// </summary>
    public class ChunkedProcessor : IChunkedProcessor
    {
        private int chunkSize = 5; // Process 5 items at a time by default
        private long maxMemoryMB = 256; // Stop if memory usage exceeds this
        private int maxConcurrentItems = 3; // Maximum concurrent processing items
        private IMemoryManager memoryManager;
        private MonoBehaviour coroutineRunner; // Reference to start coroutines

        public int ChunkSize { get => chunkSize; set => chunkSize = Mathf.Max(1, value); }

        public ChunkedProcessor(IMemoryManager memoryMgr, MonoBehaviour runner = null)
        {
            memoryManager = memoryMgr;
            coroutineRunner = runner;
        }

        public void SetMemoryLimits(long maxMemoryMB, int maxConcurrentItems)
        {
            this.maxMemoryMB = maxMemoryMB;
            this.maxConcurrentItems = Mathf.Max(1, maxConcurrentItems);
        }

        public IEnumerator ProcessInChunks<T>(T[] items, System.Func<T, IEnumerator> processItem, System.Action onChunkComplete = null)
        {
            if (items == null || items.Length == 0)
                yield break;

            int totalItems = items.Length;
            int processedCount = 0;

            while (processedCount < totalItems)
            {
                // Check memory before processing next chunk
                if (memoryManager.GetCurrentMemoryUsage() > maxMemoryMB)
                {
                    Debug.LogWarning($"ChunkedProcessor: Memory limit exceeded ({memoryManager.GetCurrentMemoryUsage()}MB > {maxMemoryMB}MB). Waiting for cleanup...");
                    if (coroutineRunner != null)
                    {
                        yield return coroutineRunner.StartCoroutine(memoryManager.CleanupRoutine());
                    }
                    else
                    {
                        yield return memoryManager.CleanupRoutine();
                    }
                    continue;
                }

                // Calculate chunk size for this iteration
                int currentChunkSize = Mathf.Min(chunkSize, totalItems - processedCount);
                int chunkStart = processedCount;
                int chunkEnd = chunkStart + currentChunkSize;

                Debug.Log($"ChunkedProcessor: Processing chunk {chunkStart}-{chunkEnd-1} of {totalItems}");

                // Process items in this chunk concurrently (up to maxConcurrentItems)
                int concurrentCount = 0;
                for (int i = chunkStart; i < chunkEnd; i++)
                {
                    if (concurrentCount >= maxConcurrentItems)
                    {
                        // Wait for some concurrent operations to complete
                        yield return new WaitForSeconds(0.01f);
                        concurrentCount = 0;
                    }

                    var item = items[i];
                    if (coroutineRunner != null)
                    {
                        coroutineRunner.StartCoroutine(ProcessItemWithCallback(item, processItem, () => concurrentCount--));
                    }
                    else
                    {
                        // If no coroutine runner, process synchronously
                        var enumerator = ProcessItemWithCallback(item, processItem, () => concurrentCount--);
                        while (enumerator.MoveNext()) { }
                    }
                    concurrentCount++;
                }

                // Wait for all items in this chunk to complete
                while (concurrentCount > 0)
                {
                    yield return null;
                }

                processedCount += currentChunkSize;

                // Callback for chunk completion
                onChunkComplete?.Invoke();

                // Small delay between chunks to allow memory cleanup
                yield return new WaitForSeconds(0.05f);
            }

            Debug.Log($"ChunkedProcessor: Completed processing {totalItems} items in chunks");
        }

        private IEnumerator ProcessItemWithCallback<T>(T item, System.Func<T, IEnumerator> processItem, System.Action onComplete)
        {
            if (coroutineRunner != null)
            {
                yield return coroutineRunner.StartCoroutine(processItem(item));
            }
            else
            {
                // If no coroutine runner, process synchronously
                var enumerator = processItem(item);
                while (enumerator.MoveNext()) { }
            }
            onComplete?.Invoke();
        }
    }
}