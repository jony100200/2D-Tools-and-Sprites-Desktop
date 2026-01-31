using UnityEngine;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Common math utility functions for game development
    /// Reusable across all projects
    /// </summary>
    public static class MathUtility
    {
        // Constants
        public const float TAU = 6.283185307179586f; // 2 * PI

        /// <summary>
        /// Remap a value from one range to another
        /// </summary>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));
        }

        /// <summary>
        /// Remap a value from 0-1 to another range
        /// </summary>
        public static float Remap01(float t, float toMin, float toMax)
        {
            return Mathf.Lerp(toMin, toMax, t);
        }

        /// <summary>
        /// Wrap an angle to 0-360 degrees
        /// </summary>
        public static float WrapAngle(float angle)
        {
            angle %= 360f;
            return angle < 0 ? angle + 360f : angle;
        }

        /// <summary>
        /// Wrap an angle to -180 to 180 degrees
        /// </summary>
        public static float WrapAngle180(float angle)
        {
            angle %= 360f;
            return angle > 180f ? angle - 360f : angle < -180f ? angle + 360f : angle;
        }

        /// <summary>
        /// Get the shortest angle difference between two angles
        /// </summary>
        public static float ShortestAngle(float from, float to)
        {
            float difference = WrapAngle(to - from);
            return difference > 180f ? difference - 360f : difference;
        }

        /// <summary>
        /// Smooth damp angle (handles 360 degree wrapping)
        /// </summary>
        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed = Mathf.Infinity, float deltaTime = -1f)
        {
            if (deltaTime < 0f) deltaTime = Time.deltaTime;

            target = current + ShortestAngle(current, target);
            return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        /// <summary>
        /// Check if a point is inside a circle
        /// </summary>
        public static bool IsPointInCircle(Vector2 point, Vector2 center, float radius)
        {
            return Vector2.SqrMagnitude(point - center) <= radius * radius;
        }

        /// <summary>
        /// Get a point on a circle
        /// </summary>
        public static Vector2 PointOnCircle(Vector2 center, float radius, float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            return center + new Vector2(
                Mathf.Cos(angleRadians) * radius,
                Mathf.Sin(angleRadians) * radius
            );
        }

        /// <summary>
        /// Calculate distance from point to line segment
        /// </summary>
        public static float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float length = line.magnitude;
            if (length == 0f) return Vector2.Distance(point, lineStart);

            Vector2 lineDirection = line / length;
            float projection = Vector2.Dot(point - lineStart, lineDirection);
            projection = Mathf.Clamp(projection, 0f, length);

            Vector2 closestPoint = lineStart + lineDirection * projection;
            return Vector2.Distance(point, closestPoint);
        }

        /// <summary>
        /// Smooth step function (S-curve interpolation)
        /// </summary>
        public static float SmoothStep(float from, float to, float t)
        {
            t = Mathf.Clamp01(t);
            t = t * t * (3f - 2f * t);
            return Mathf.Lerp(from, to, t);
        }

        /// <summary>
        /// Smoother step function (even smoother S-curve)
        /// </summary>
        public static float SmootherStep(float from, float to, float t)
        {
            t = Mathf.Clamp01(t);
            t = t * t * t * (t * (t * 6f - 15f) + 10f);
            return Mathf.Lerp(from, to, t);
        }

        /// <summary>
        /// Exponential ease out
        /// </summary>
        public static float EaseOutExpo(float t)
        {
            return t >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
        }

        /// <summary>
        /// Exponential ease in
        /// </summary>
        public static float EaseInExpo(float t)
        {
            return t <= 0f ? 0f : Mathf.Pow(2f, 10f * (t - 1f));
        }

        /// <summary>
        /// Bounce ease out
        /// </summary>
        public static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2f / d1)
            {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            }
            else
            {
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
            }
        }

        /// <summary>
        /// Round to nearest multiple
        /// </summary>
        public static float RoundToNearest(float value, float multiple)
        {
            return Mathf.Round(value / multiple) * multiple;
        }

        /// <summary>
        /// Round to nearest power of 2
        /// </summary>
        public static int RoundToNearestPowerOf2(int value)
        {
            if (value <= 0) return 1;
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;
            return value;
        }

        /// <summary>
        /// Check if two floats are approximately equal
        /// </summary>
        public static bool Approximately(float a, float b, float tolerance = 0.0001f)
        {
            return Mathf.Abs(a - b) < tolerance;
        }

        /// <summary>
        /// Clamp a vector's magnitude
        /// </summary>
        public static Vector2 ClampMagnitude(Vector2 vector, float maxMagnitude)
        {
            if (vector.sqrMagnitude > maxMagnitude * maxMagnitude)
            {
                return vector.normalized * maxMagnitude;
            }
            return vector;
        }

        /// <summary>
        /// Get the angle of a vector in degrees
        /// </summary>
        public static float VectorToAngle(Vector2 vector)
        {
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Create a vector from an angle in degrees
        /// </summary>
        public static Vector2 AngleToVector(float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
        }
    }
}
