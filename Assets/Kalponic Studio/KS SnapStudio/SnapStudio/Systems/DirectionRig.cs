using UnityEngine;
using System.Collections.Generic;
using KalponicGames.KS_SnapStudio;

namespace KalponicGames.KS_SnapStudio.Systems
{
    /// <summary>
    /// Handles camera positioning and rotation for different game types and directions.
    /// Provides preset camera poses for side-scroller, top-down, and isometric views.
    /// </summary>
    public static class DirectionRig
    {
        /// <summary>
        /// Gets the supported directions for a given game type.
        /// </summary>
        /// <param name="gameType">The game type to get directions for.</param>
        /// <returns>Array of direction names.</returns>
        public static string[] GetDirectionsFor(GameType gameType)
        {
            switch (gameType)
            {
                case GameType.SideScroller:
                    return new[] { "right" }; // Only right, left created via mirroring
                case GameType.TopDown:
                    return new[] { "north", "east", "south", "west" };
                case GameType.Iso:
                    return new[] { "north", "east", "south", "west" };
                default:
                    return new[] { "right" };
            }
        }

        /// <summary>
        /// Gets the supported directions for a given game type, filtered by direction mask.
        /// </summary>
        /// <param name="gameType">The game type to get directions for.</param>
        /// <param name="directionMask">The direction mask to filter by.</param>
        /// <returns>Array of direction names.</returns>
        public static string[] GetDirectionsFor(GameType gameType, DirectionMask directionMask)
        {
            var allDirections = GetDirectionsFor(gameType);
            var filteredDirections = new System.Collections.Generic.List<string>();

            foreach (var direction in allDirections)
            {
                if (ShouldIncludeDirection(direction, directionMask))
                {
                    filteredDirections.Add(direction);
                }
            }

            return filteredDirections.ToArray();
        }

        /// <summary>
        /// Checks if a direction should be included based on the direction mask.
        /// </summary>
        /// <param name="direction">The direction name.</param>
        /// <param name="directionMask">The direction mask.</param>
        /// <returns>True if the direction should be included.</returns>
        private static bool ShouldIncludeDirection(string direction, DirectionMask directionMask)
        {
            switch (direction)
            {
                case "north":
                    return (directionMask & DirectionMask.North) != 0;
                case "east":
                    return (directionMask & DirectionMask.East) != 0;
                case "south":
                    return (directionMask & DirectionMask.South) != 0;
                case "west":
                    return (directionMask & DirectionMask.West) != 0;
                case "right":
                    return (directionMask & DirectionMask.East) != 0; // Right maps to East
                default:
                    return true;
            }
        }

        /// <summary>
        /// Poses the camera for a specific direction.
        /// </summary>
        /// <param name="camera">The camera to position.</param>
        /// <param name="target">The target object to focus on.</param>
        /// <param name="direction">The direction name.</param>
        /// <param name="pixelSize">The pixel size for orthographic size calculation.</param>
        /// <param name="gameType">The game type for camera positioning logic.</param>
        /// <param name="facingAxis">The facing axis that defines "right".</param>
        /// <param name="baseYawOffset">Base yaw offset in degrees.</param>
        /// <param name="rotateAxis">The axis to rotate around for direction changes.</param>
        public static void PoseCamera(Camera camera, GameObject target, string direction, int pixelSize, GameType gameType, FacingAxis facingAxis, float baseYawOffset, RotateAxis rotateAxis, float? customOrthoSize = null)
        {
            if (camera == null || target == null)
                return;

            // Calculate base rotation based on facing axis
            float baseYaw = baseYawOffset;
            switch (facingAxis)
            {
                case FacingAxis.PositiveX:
                    baseYaw += 0f;
                    break;
                case FacingAxis.NegativeX:
                    baseYaw += 180f;
                    break;
                case FacingAxis.PositiveY:
                    baseYaw += 90f;
                    break;
                case FacingAxis.NegativeY:
                    baseYaw += 270f;
                    break;
            }

            Vector3 position;
            Quaternion rotation;
            float orthoSize = customOrthoSize ?? CalculateOrthographicSize(pixelSize);

            switch (gameType)
            {
                case GameType.SideScroller:
                    // Camera: Orthographic, pos (0,0,-10), rot (0,0,0), BG RGBA(0,0,0,0), size based on pixelSize
                    // Model: at world (0,0,0). Do NOT change prefab rotation.
                    // Direction logic: Character should be oriented correctly in scene.
                    // For left/right, rely on animation authoring or simple mirroring.
                    position = new Vector3(0, 0, -10);
                    rotation = Quaternion.Euler(0, 0, 0); // Camera NEVER rotates for side-scroller
                    break;

                case GameType.TopDown:
                    // Camera goes overhead, model yaw offsets: 0, 90, 180, 270 added to base yaw
                    position = new Vector3(0, 10, 0);
                    rotation = Quaternion.Euler(90, 0, 0);
                    
                    float topDownYaw = baseYaw;
                    switch (direction)
                    {
                        case "north": topDownYaw += 0; break;
                        case "east": topDownYaw += 90; break;
                        case "south": topDownYaw += 180; break;
                        case "west": topDownYaw += 270; break;
                    }
                    target.transform.rotation = CreateRotation(rotateAxis, topDownYaw);
                    break;

                case GameType.Iso:
                    // Camera fixed iso, model yaw offsets: 0, 90, 180, 270 added to base yaw
                    position = new Vector3(-10, 10, -10);
                    rotation = Quaternion.Euler(35, 45, 0);
                    
                    float isoYaw = baseYaw;
                    switch (direction)
                    {
                        case "north": isoYaw += 0; break;
                        case "east": isoYaw += 90; break;
                        case "south": isoYaw += 180; break;
                        case "west": isoYaw += 270; break;
                    }
                    target.transform.rotation = CreateRotation(rotateAxis, isoYaw);
                    break;

                default:
                    position = new Vector3(0, 0, -10);
                    rotation = Quaternion.Euler(0, baseYaw, 0);
                    break;
            }

            camera.transform.SetPositionAndRotation(position, rotation);
            camera.orthographicSize = orthoSize;
        }

        /// <summary>
        /// Creates a rotation quaternion around the specified axis.
        /// </summary>
        /// <param name="axis">The axis to rotate around.</param>
        /// <param name="angle">The angle in degrees.</param>
        /// <returns>The rotation quaternion.</returns>
        private static Quaternion CreateRotation(RotateAxis axis, float angle)
        {
            switch (axis)
            {
                case RotateAxis.X:
                    return Quaternion.Euler(angle, 0, 0);
                case RotateAxis.Y:
                    return Quaternion.Euler(0, angle, 0);
                case RotateAxis.Z:
                    return Quaternion.Euler(0, 0, angle);
                default:
                    return Quaternion.Euler(0, angle, 0);
            }
        }

        /// <summary>
        /// Cleans up proxy objects that may have been created by previous versions.
        /// </summary>
        /// <param name="target">The target character object.</param>
        private static void CleanupProxyObjects(GameObject target)
        {
            Transform proxy = target.transform.Find("KS_Yaw");
            if (proxy != null)
            {
                Object.DestroyImmediate(proxy.gameObject);
            }
        }

        /// <summary>
        /// Calculates the orthographic size based on pixel size.
        /// </summary>
        /// <param name="pixelSize">The pixel size.</param>
        /// <returns>The orthographic size.</returns>
        private static float CalculateOrthographicSize(int pixelSize)
        {
            // Base size for 16 pixel units, adjust as needed
            return pixelSize / 2f;
        }
    }
}
