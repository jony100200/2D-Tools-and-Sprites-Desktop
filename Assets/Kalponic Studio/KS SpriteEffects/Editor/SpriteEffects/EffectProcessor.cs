using UnityEngine;

namespace KalponicStudio.SpriteEffects
{
    public interface IEffectProcessor
    {
        Texture2D Process(Texture2D source, Material material);
    }

    public sealed class MaterialEffectProcessor : IEffectProcessor
    {
        public Texture2D Process(Texture2D source, Material material)
        {
            if (source == null || material == null)
            {
                return null;
            }

            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);

            Graphics.Blit(source, rt, material);

            Texture2D result = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
            result.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }
    }
}
