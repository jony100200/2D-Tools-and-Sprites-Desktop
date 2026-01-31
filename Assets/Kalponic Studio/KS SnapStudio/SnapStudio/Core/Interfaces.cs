using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Interface for animation playback control components
    /// Enables better testability and decoupling
    /// </summary>
    public interface IAnimationController
    {
        bool IsPlaying { get; }
        bool IsLooping { get; }
        int CurrentFrameIndex { get; }
        int TotalFrames { get; set; }

        void Initialize();
        void Update(int maxFrames);
        void Play();
        void Pause();
        void TogglePlayPause();
        void Reset();
        void SetFrame(int frameIndex, int maxFrames);
    }

    /// <summary>
    /// Interface for timeline control components
    /// </summary>
    public interface ITimelineController
    {
        int TotalFrames { get; set; }

        void Initialize();
        void DrawTimeline();
        event System.Action<int> OnFrameSelected;
    }

    /// <summary>
    /// Interface for zoom control components
    /// </summary>
    public interface IZoomController
    {
        float ZoomLevel { get; }

        void Initialize();
        void DrawZoomControls();
        void HandleZoomInput(Event e, Rect displayRect);
        Rect GetZoomedRect(Rect originalRect, Rect containerRect);
        event System.Action<float> OnZoomChanged;
    }

    /// <summary>
    /// Interface for frame information display components
    /// </summary>
    public interface IFrameInfoDisplay
    {
        bool ShowFrameInfo { get; set; }

        void Initialize();
        void DrawInfoToggle();
        void DrawFrameInfo(Rect displayRect, int currentFrameIndex, int totalFrames, int originalFrameIndex, Texture2D texture);
    }

    /// <summary>
    /// Interface for animation export components
    /// </summary>
    public interface IAnimationExporter
    {
        void Initialize();
        void DrawExportControls(List<Frame> frames);
    }

    /// <summary>
    /// Interface for capture strategies (GPU, CPU, chunked processing)
    /// </summary>
    public interface ICaptureStrategy
    {
        bool IsSupported { get; }
        string StrategyName { get; }
        IEnumerator CaptureFrame(Camera camera, RenderTexture renderTexture, int frameNumber, System.Action<Texture2D> onComplete);
        void Cleanup();
    }

    /// <summary>
    /// Interface for frame processing and batching
    /// </summary>
    public interface IFrameProcessor
    {
        void QueueFrame(object frameData);
        IEnumerator ProcessFrames(System.Action<object> onFrameProcessed);
        void ClearQueue();
        int QueueSize { get; }
        bool IsProcessing { get; }
    }

    /// <summary>
    /// Interface for pivot calculation strategies
    /// </summary>
    public interface IPivotCalculator
    {
        Vector2 CalculatePivot(Camera camera, int texWidth, int texHeight, RectInt? trimRect = null);
        Vector2 CalculatePivotFromBone(Camera camera, Transform bone, int texWidth, int texHeight);
        Vector2 RemapPivotAfterTrim(Vector2 pivot, int originalWidth, int originalHeight, RectInt trimRect);
    }

    /// <summary>
    /// Interface for chunked processing to save memory
    /// </summary>
    public interface IChunkedProcessor
    {
        int ChunkSize { get; set; }
        IEnumerator ProcessInChunks<T>(T[] items, System.Func<T, IEnumerator> processItem, System.Action onChunkComplete = null);
        void SetMemoryLimits(long maxMemoryMB, int maxConcurrentItems);
    }
}
