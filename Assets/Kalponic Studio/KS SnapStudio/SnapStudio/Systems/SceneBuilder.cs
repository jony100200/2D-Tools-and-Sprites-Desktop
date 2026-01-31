using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace KalponicGames.KS_SnapStudio
{
    using Systems;
    /// <summary>
    /// SceneBuilder - Builds and manages capture scenes for KS SnapStudio.
    /// Handles scene setup, lighting, camera positioning, and object placement.
    /// </summary>
    public static class SceneBuilder
    {
        // Scene management
        private static SceneAsset currentScene;
        private static string originalScenePath;

        // Lighting setup
        private static Light mainLight;
        private static Light fillLight;
        private static Light rimLight;

        // Camera setup
        private static Camera captureCamera;
        private static GameObject cameraRig;

        /// <summary>
        /// Initialize capture scene with proper lighting and camera setup
        /// </summary>
        public static void InitializeCaptureScene()
        {
            // Save current scene
            originalScenePath = EditorSceneManager.GetActiveScene().path;

            // Create new scene for capture
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);

            SetupLighting();
            SetupCamera();
        }

        /// <summary>
        /// Setup professional lighting rig for captures
        /// </summary>
        private static void SetupLighting()
        {
            // Main key light (warm, directional)
            mainLight = new GameObject("MainLight").AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.color = new Color(1f, 0.95f, 0.9f); // Warm white
            mainLight.intensity = 1.2f;
            mainLight.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            mainLight.shadows = LightShadows.Soft;

            // Fill light (cool, directional)
            fillLight = new GameObject("FillLight").AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.8f, 0.9f, 1f); // Cool blue
            fillLight.intensity = 0.4f;
            fillLight.transform.rotation = Quaternion.Euler(15f, 150f, 0f);
            fillLight.shadows = LightShadows.None;

            // Rim light (bright, directional)
            rimLight = new GameObject("RimLight").AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.color = Color.white;
            rimLight.intensity = 0.8f;
            rimLight.transform.rotation = Quaternion.Euler(-20f, 210f, 0f);
            rimLight.shadows = LightShadows.Soft;
        }

        /// <summary>
        /// Setup capture camera with proper settings
        /// </summary>
        private static void SetupCamera()
        {
            cameraRig = new GameObject("CaptureCameraRig");
            captureCamera = new GameObject("CaptureCamera").AddComponent<Camera>();
            captureCamera.transform.SetParent(cameraRig.transform);

            // Camera settings for high-quality captures
            captureCamera.clearFlags = CameraClearFlags.SolidColor;
            captureCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            captureCamera.orthographic = false;
            captureCamera.fieldOfView = 60f;
            captureCamera.nearClipPlane = 0.1f;
            captureCamera.farClipPlane = 1000f;
            captureCamera.depth = 0;

            // Position camera at reasonable default
            cameraRig.transform.position = new Vector3(0, 1, -5);
            cameraRig.transform.LookAt(Vector3.zero);
        }

        /// <summary>
        /// Add snappable objects to the capture scene
        /// </summary>
        public static void AddSnappables(IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                if (obj == null) continue;

                // Create instance in capture scene
                var instance = Object.Instantiate(obj);
                instance.name = obj.name;

                // Ensure it has KSSnappable component
                if (!instance.GetComponent<KSSnappable>())
                    instance.AddComponent<KSSnappable>();

                // Position in scene
                PositionObjectForCapture(instance);
            }
        }

        /// <summary>
        /// Position object appropriately for capture
        /// </summary>
        private static void PositionObjectForCapture(GameObject obj)
        {
            var snappable = obj.GetComponent<KSSnappable>();
            if (snappable != null)
            {
                // Center on custom pivot or transform position
                var pivot = snappable.GetPivot();
                obj.transform.position = -pivot + Vector3.zero;
            }
            else
            {
                // Default centering
                var bounds = CalculateBounds(obj);
                obj.transform.position = -bounds.center;
            }
        }

        /// <summary>
        /// Calculate bounds of a GameObject and its children
        /// </summary>
        private static Bounds CalculateBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(obj.transform.position, Vector3.one);

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }

        /// <summary>
        /// Position camera to frame the objects properly
        /// </summary>
        public static void FrameCameraOnObjects(IEnumerable<GameObject> objects)
        {
            if (captureCamera == null || !objects.Any()) return;

            var allBounds = new Bounds();
            bool first = true;

            foreach (var obj in objects)
            {
                if (obj == null) continue;

                var bounds = CalculateBounds(obj);
                if (first)
                {
                    allBounds = bounds;
                    first = false;
                }
                else
                {
                    allBounds.Encapsulate(bounds);
                }
            }

            if (first) return; // No valid objects

            // Position camera to frame the bounds
            var size = allBounds.size.magnitude;
            var distance = size / Mathf.Tan(captureCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);

            cameraRig.transform.position = allBounds.center + Vector3.forward * distance;
            cameraRig.transform.LookAt(allBounds.center);
        }

        /// <summary>
        /// Apply direction rig positioning to camera
        /// </summary>
        public static void ApplyDirectionRig(string direction, GameObject target, int pixelSize, GameType gameType, FacingAxis facingAxis, float baseYawOffset, RotateAxis rotateAxis, float? customOrthoSize = null)
        {
            if (captureCamera == null || target == null) return;

            DirectionRig.PoseCamera(captureCamera, target, direction, pixelSize, gameType, facingAxis, baseYawOffset, rotateAxis, customOrthoSize);
        }

        /// <summary>
        /// Cleanup capture scene and restore original
        /// </summary>
        public static void CleanupCaptureScene()
        {
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath);
            }

            // Destroy temporary objects
            if (mainLight != null) Object.DestroyImmediate(mainLight.gameObject);
            if (fillLight != null) Object.DestroyImmediate(fillLight.gameObject);
            if (rimLight != null) Object.DestroyImmediate(rimLight.gameObject);
            if (cameraRig != null) Object.DestroyImmediate(cameraRig);

            currentScene = null;
            originalScenePath = null;
        }

        /// <summary>
        /// Get the current capture camera
        /// </summary>
        public static Camera GetCaptureCamera()
        {
            return captureCamera;
        }

        /// <summary>
        /// Check if capture scene is ready
        /// </summary>
        public static bool IsCaptureSceneReady()
        {
            return currentScene != null && captureCamera != null;
        }

        /// <summary>
        /// Create capture scene with specified settings
        /// </summary>
        public static void CreateCaptureScene(bool trimSprites, float? characterFillPercent = null)
        {
            InitializeCaptureScene();
            
            // Additional setup based on parameters
            if (characterFillPercent.HasValue)
            {
                // Convert fill percentage to background color (grayscale)
                float normalizedFill = Mathf.Clamp01(characterFillPercent.Value / 100f);
                Color backgroundColor = new Color(normalizedFill, normalizedFill, normalizedFill, 1f);
                
                // Set background color if specified
                if (captureCamera != null)
                {
                    captureCamera.backgroundColor = backgroundColor;
                }
            }
        }

        /// <summary>
        /// Get all snappable objects in current scene
        /// </summary>
        public static IEnumerable<GameObject> GetSnappablesInScene()
        {
            return Object.FindObjectsByType<KSSnappable>(FindObjectsSortMode.None)
                .Where(s => s.ShouldCapture())
                .Select(s => s.gameObject);
        }
    }
}
