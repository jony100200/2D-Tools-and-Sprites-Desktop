// KS Studio Core Interfaces
// Shared interfaces used across both SnapStudio and ThumbSmith

using UnityEngine;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// Interface for scene staging services
    /// </summary>
    public interface ISceneStager
    {
        void Open(ThumbnailRunConfig rc);
        GameObject Spawn(GameObject prefab); // returns instance in staging scene
        void Despawn(GameObject instance);
        void Close();
        Camera GetCamera();
    }

    /// <summary>
    /// Interface for prefab framing services
    /// </summary>
    public interface IPrefabFramer
    {
        void FrameSubject(ThumbnailRunConfig rc, GameObject instance);
    }

    /// <summary>
    /// Interface for rendering services
    /// </summary>
    public interface IRendererService
    {
        Texture2D RenderToTexture(ThumbnailRunConfig rc, GameObject instance, int width, int height);
    }

    /// <summary>
    /// Interface for file services
    /// </summary>
    public interface IFileService
    {
        void SaveTexture(Texture2D tex, string absolutePath, bool forcePng);
    }
}
