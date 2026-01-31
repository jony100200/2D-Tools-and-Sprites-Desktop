using UnityEngine;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    /// <summary>
    /// Pivot calculation strategies for sprite anchoring
    /// </summary>
    public class PivotCalculator : IPivotCalculator
    {
        public Vector2 CalculatePivot(Camera camera, int texWidth, int texHeight, RectInt? trimRect = null)
        {
            // Default implementation - center pivot
            return new Vector2(0.5f, 0.5f);
        }

        public Vector2 CalculatePivotFromBone(Camera camera, Transform bone, int texWidth, int texHeight)
        {
            if (bone == null || camera == null)
                return new Vector2(0.5f, 0.5f);

            // Get bone position in screen space
            Vector3 screenPos = camera.WorldToScreenPoint(bone.position);

            // Convert to normalized texture coordinates
            float normalizedX = screenPos.x / Screen.width;
            float normalizedY = screenPos.y / Screen.height;

            // Clamp to valid range
            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedY = Mathf.Clamp01(normalizedY);

            return new Vector2(normalizedX, normalizedY);
        }

        public Vector2 RemapPivotAfterTrim(Vector2 pivot, int originalWidth, int originalHeight, RectInt trimRect)
        {
            if (trimRect.width == 0 || trimRect.height == 0)
                return pivot;

            // Calculate new pivot position after trimming
            float newPivotX = (pivot.x * originalWidth - trimRect.x) / trimRect.width;
            float newPivotY = (pivot.y * originalHeight - trimRect.y) / trimRect.height;

            // Clamp to valid range
            newPivotX = Mathf.Clamp01(newPivotX);
            newPivotY = Mathf.Clamp01(newPivotY);

            return new Vector2(newPivotX, newPivotY);
        }
    }
}