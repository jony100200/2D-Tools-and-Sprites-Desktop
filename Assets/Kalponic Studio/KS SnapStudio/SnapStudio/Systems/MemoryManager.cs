using System;
using System.Collections;
using UnityEngine;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Interface for memory management and optimization
    /// </summary>
    public interface IMemoryManager
    {
        bool IsMemoryLow { get; }
        long GetCurrentMemoryUsage();
        void LogMemoryUsage(string context);
        bool ShouldReduceQuality(int currentResolution);
        int GetOptimalResolution(int targetWidth, int targetHeight);
        void ForceCleanup();
        IEnumerator CleanupRoutine();
    }

    /// <summary>
    /// Memory manager implementation for low-end systems
    /// </summary>
    public class MemoryManager : IMemoryManager
    {
        private const long WARNING_MEMORY_MB = 256;
        private const long CRITICAL_MEMORY_MB = 512;
        private const int MIN_RESOLUTION = 256;

        public bool IsMemoryLow => GetCurrentMemoryUsage() > WARNING_MEMORY_MB;

        public long GetCurrentMemoryUsage()
        {
            return System.GC.GetTotalMemory(false) / (1024 * 1024);
        }

        public void LogMemoryUsage(string context)
        {
            long memoryUsed = GetCurrentMemoryUsage();
            Debug.Log($"MemoryManager [{context}]: {memoryUsed}MB used");
        }

        public bool ShouldReduceQuality(int currentResolution)
        {
            return GetCurrentMemoryUsage() > WARNING_MEMORY_MB && currentResolution > MIN_RESOLUTION;
        }

        public int GetOptimalResolution(int targetWidth, int targetHeight)
        {
            long memoryUsage = GetCurrentMemoryUsage();

            if (memoryUsage < WARNING_MEMORY_MB)
                return targetWidth; // Use target resolution

            if (memoryUsage < CRITICAL_MEMORY_MB)
                return Mathf.Max(MIN_RESOLUTION, targetWidth / 2); // Half resolution

            return MIN_RESOLUTION; // Minimum resolution
        }

        public void ForceCleanup()
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        public IEnumerator CleanupRoutine()
        {
            ForceCleanup();
            yield return new WaitForSeconds(0.1f); // Allow cleanup to complete
        }
    }
}