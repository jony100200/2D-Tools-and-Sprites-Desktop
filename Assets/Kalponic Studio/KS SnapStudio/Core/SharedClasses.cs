// KS Studio Core Shared Classes
// Shared classes used across both SnapStudio and ThumbSmith

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// Represents a single animation frame
    /// </summary>
    public class Frame
    {
        public int Index;
        public float Time;

        public Frame(int index, float time)
        {
            this.Index = index;
            this.Time = time;
        }
    }

    /// <summary>
    /// Animation sampling configuration and data container
    /// </summary>
    public class Sampling
    {
        public readonly UnityEngine.Texture2D[] RawMainTextures = null;
        public readonly UnityEngine.Texture2D[] TrimMainTextures = null;
        public readonly float[] FrameTimes = null;
        public readonly System.Collections.Generic.List<Frame> SelectedFrames = null;

        public Sampling(int numOfFrames)
        {
            RawMainTextures = new UnityEngine.Texture2D[numOfFrames];
            TrimMainTextures = new UnityEngine.Texture2D[numOfFrames];
            FrameTimes = new float[numOfFrames];
            SelectedFrames = new System.Collections.Generic.List<Frame>();
        }

        // Parameterless constructor for compatibility
        public Sampling()
        {
            SelectedFrames = new System.Collections.Generic.List<Frame>();
            TrimMainTextures = new UnityEngine.Texture2D[0];
        }
    }
}
