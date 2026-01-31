using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Utility for managing scene loading/unloading with progress tracking and callbacks
    /// Supports additive loading, unloading, and scene transitions
    /// </summary>
    public static class SceneManagementUtility
    {
        public enum LoadMode
        {
            Single,
            Additive
        }

        public enum UnloadMode
        {
            Single,
            AllAdditive
        }

        /// <summary>
        /// Load a scene asynchronously with progress tracking
        /// </summary>
        public static async Task<Scene> LoadSceneAsync(string sceneName,
                                                     LoadMode loadMode = LoadMode.Single,
                                                     Action<float> onProgress = null,
                                                     Action<Scene> onComplete = null)
        {
            LoadSceneMode mode = loadMode == LoadMode.Single ?
                                LoadSceneMode.Single : LoadSceneMode.Additive;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);

            if (operation == null)
            {
                Debug.LogError($"Failed to load scene: {sceneName}");
                return default;
            }

            // Track progress
            while (!operation.isDone)
            {
                onProgress?.Invoke(operation.progress);
                await Task.Yield();
            }

            onProgress?.Invoke(1f);

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            onComplete?.Invoke(loadedScene);

            return loadedScene;
        }

        /// <summary>
        /// Load a scene by build index asynchronously
        /// </summary>
        public static async Task<Scene> LoadSceneAsync(int buildIndex,
                                                     LoadMode loadMode = LoadMode.Single,
                                                     Action<float> onProgress = null,
                                                     Action<Scene> onComplete = null)
        {
            LoadSceneMode mode = loadMode == LoadMode.Single ?
                                LoadSceneMode.Single : LoadSceneMode.Additive;

            AsyncOperation operation = SceneManager.LoadSceneAsync(buildIndex, mode);

            if (operation == null)
            {
                Debug.LogError($"Failed to load scene at build index: {buildIndex}");
                return default;
            }

            // Track progress
            while (!operation.isDone)
            {
                onProgress?.Invoke(operation.progress);
                await Task.Yield();
            }

            onProgress?.Invoke(1f);

            Scene loadedScene = SceneManager.GetSceneByBuildIndex(buildIndex);
            onComplete?.Invoke(loadedScene);

            return loadedScene;
        }

        /// <summary>
        /// Unload a scene asynchronously
        /// </summary>
        public static async Task<bool> UnloadSceneAsync(string sceneName,
                                                       Action<float> onProgress = null,
                                                       Action<bool> onComplete = null)
        {
            try
            {
                AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

                if (operation == null)
                {
                    Debug.LogError($"Failed to unload scene: {sceneName}");
                    onComplete?.Invoke(false);
                    return false;
                }

                // Track progress
                while (!operation.isDone)
                {
                    onProgress?.Invoke(operation.progress);
                    await Task.Yield();
                }

                onProgress?.Invoke(1f);
                onComplete?.Invoke(true);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error unloading scene {sceneName}: {e.Message}");
                onComplete?.Invoke(false);
                return false;
            }
        }

        /// <summary>
        /// Unload a scene by build index asynchronously
        /// </summary>
        public static async Task<bool> UnloadSceneAsync(int buildIndex,
                                                       Action<float> onProgress = null,
                                                       Action<bool> onComplete = null)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(buildIndex);
            if (!scene.isLoaded)
            {
                Debug.LogWarning($"Scene at build index {buildIndex} is not loaded");
                onComplete?.Invoke(true);
                return true;
            }

            return await UnloadSceneAsync(scene.name, onProgress, onComplete);
        }

        /// <summary>
        /// Transition between two scenes with loading screen
        /// </summary>
        public static async Task<bool> TransitionScenes(string fromScene, string toScene,
                                                       float transitionDelay = 0.5f,
                                                       Action<float> onProgress = null,
                                                       Action onTransitionStart = null,
                                                       Action onTransitionComplete = null)
        {
            try
            {
                // Start transition
                onTransitionStart?.Invoke();

                // Wait for transition delay
                if (transitionDelay > 0)
                {
                    await Task.Delay((int)(transitionDelay * 1000));
                }

                // Load new scene
                Task<Scene> loadTask = LoadSceneAsync(toScene, LoadMode.Single, onProgress);

                // Wait for loading to complete
                await loadTask;

                onTransitionComplete?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Scene transition failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get all loaded scenes
        /// </summary>
        public static Scene[] GetLoadedScenes()
        {
            int sceneCount = SceneManager.sceneCount;
            Scene[] scenes = new Scene[sceneCount];

            for (int i = 0; i < sceneCount; i++)
            {
                scenes[i] = SceneManager.GetSceneAt(i);
            }

            return scenes;
        }

        /// <summary>
        /// Check if a scene is loaded
        /// </summary>
        public static bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.isLoaded;
        }

        /// <summary>
        /// Check if a scene is loaded by build index
        /// </summary>
        public static bool IsSceneLoaded(int buildIndex)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(buildIndex);
            return scene.isLoaded;
        }

        /// <summary>
        /// Get the currently active scene
        /// </summary>
        public static Scene GetActiveScene()
        {
            return SceneManager.GetActiveScene();
        }

        /// <summary>
        /// Set a scene as active
        /// </summary>
        public static bool SetActiveScene(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                return SceneManager.SetActiveScene(scene);
            }
            return false;
        }

        /// <summary>
        /// Get scene build index by name
        /// </summary>
        public static int GetBuildIndex(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.buildIndex;
        }

        /// <summary>
        /// Get scene name by build index
        /// </summary>
        public static string GetSceneName(int buildIndex)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(buildIndex);
            return scene.name;
        }

        /// <summary>
        /// Coroutine version for Unity components that can't use async/await
        /// </summary>
        public static IEnumerator LoadSceneCoroutine(string sceneName,
                                                   LoadMode loadMode = LoadMode.Single,
                                                   Action<float> onProgress = null,
                                                   Action<Scene> onComplete = null)
        {
            LoadSceneMode mode = loadMode == LoadMode.Single ?
                                LoadSceneMode.Single : LoadSceneMode.Additive;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);

            if (operation == null)
            {
                Debug.LogError($"Failed to load scene: {sceneName}");
                yield break;
            }

            while (!operation.isDone)
            {
                onProgress?.Invoke(operation.progress);
                yield return null;
            }

            onProgress?.Invoke(1f);

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            onComplete?.Invoke(loadedScene);
        }

        /// <summary>
        /// Coroutine version for scene unloading
        /// </summary>
        public static IEnumerator UnloadSceneCoroutine(string sceneName,
                                                     Action<float> onProgress = null,
                                                     Action<bool> onComplete = null)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

            if (operation == null)
            {
                Debug.LogError($"Failed to unload scene: {sceneName}");
                onComplete?.Invoke(false);
                yield break;
            }

            while (!operation.isDone)
            {
                onProgress?.Invoke(operation.progress);
                yield return null;
            }

            onProgress?.Invoke(1f);
            onComplete?.Invoke(true);
        }
    }

    /// <summary>
    /// MonoBehaviour helper for scene management
    /// </summary>
    public class SceneManagerHelper : MonoBehaviour
    {
        [Header("Scene Loading")]
        [Tooltip("Name of the loading scene to display during transitions.")]
        [SerializeField] private string loadingSceneName = "Loading";
        [Tooltip("UI Slider to show loading progress.")]
        [SerializeField] private UnityEngine.UI.Slider progressBar;
        [Tooltip("TextMeshPro text to display progress percentage.")]
        [SerializeField] private TMPro.TextMeshProUGUI progressText;

        private Coroutine currentOperation;

        public void LoadScene(string sceneName, SceneManagementUtility.LoadMode loadMode = SceneManagementUtility.LoadMode.Single)
        {
            if (currentOperation != null)
            {
                StopCoroutine(currentOperation);
            }

            currentOperation = StartCoroutine(LoadSceneCoroutine(sceneName, loadMode));
        }

        public void UnloadScene(string sceneName)
        {
            if (currentOperation != null)
            {
                StopCoroutine(currentOperation);
            }

            currentOperation = StartCoroutine(UnloadSceneCoroutine(sceneName));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName, SceneManagementUtility.LoadMode loadMode)
        {
            yield return SceneManagementUtility.LoadSceneCoroutine(
                sceneName,
                loadMode,
                progress => UpdateProgress(progress),
                scene => OnSceneLoaded(scene)
            );
        }

        private IEnumerator UnloadSceneCoroutine(string sceneName)
        {
            yield return SceneManagementUtility.UnloadSceneCoroutine(
                sceneName,
                progress => UpdateProgress(progress),
                success => OnSceneUnloaded(success)
            );
        }

        private void UpdateProgress(float progress)
        {
            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            if (progressText != null)
            {
                progressText.text = $"{(progress * 100):F0}%";
            }
        }

        private void OnSceneLoaded(Scene scene)
        {
            Debug.Log($"Scene loaded: {scene.name}");
            currentOperation = null;
        }

        private void OnSceneUnloaded(bool success)
        {
            Debug.Log($"Scene unloaded: {success}");
            currentOperation = null;
        }
    }
}
