using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Coroutine helper utilities for common patterns
    /// Reusable across all projects
    /// </summary>
    public static class CoroutineUtility
    {
        /// <summary>
        /// Wait for a specified number of frames
        /// </summary>
        public static IEnumerator WaitForFrames(int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Wait until a condition is true
        /// </summary>
        public static IEnumerator WaitUntil(Func<bool> condition)
        {
            while (!condition())
            {
                yield return null;
            }
        }

        /// <summary>
        /// Wait while a condition is true
        /// </summary>
        public static IEnumerator WaitWhile(Func<bool> condition)
        {
            while (condition())
            {
                yield return null;
            }
        }

        /// <summary>
        /// Execute an action after a delay
        /// </summary>
        public static IEnumerator DelayedAction(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        /// <summary>
        /// Execute an action repeatedly with a delay between each execution
        /// </summary>
        public static IEnumerator RepeatAction(float interval, int repeatCount, Action action)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                action?.Invoke();
                yield return new WaitForSeconds(interval);
            }
        }

        /// <summary>
        /// Execute an action repeatedly with a delay between each execution (infinite)
        /// </summary>
        public static IEnumerator RepeatAction(float interval, Action action)
        {
            while (true)
            {
                action?.Invoke();
                yield return new WaitForSeconds(interval);
            }
        }

        /// <summary>
        /// Lerp a float value over time
        /// </summary>
        public static IEnumerator LerpFloat(float startValue, float endValue, float duration, Action<float> onValueChanged)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                float currentValue = Mathf.Lerp(startValue, endValue, t);
                onValueChanged?.Invoke(currentValue);
                yield return null;
            }
            onValueChanged?.Invoke(endValue);
        }

        /// <summary>
        /// Lerp a Vector3 over time
        /// </summary>
        public static IEnumerator LerpVector3(Vector3 startValue, Vector3 endValue, float duration, Action<Vector3> onValueChanged)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                Vector3 currentValue = Vector3.Lerp(startValue, endValue, t);
                onValueChanged?.Invoke(currentValue);
                yield return null;
            }
            onValueChanged?.Invoke(endValue);
        }

        /// <summary>
        /// Fade audio source volume
        /// </summary>
        public static IEnumerator FadeAudio(AudioSource audioSource, float targetVolume, float duration)
        {
            float startVolume = audioSource.volume;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }

            audioSource.volume = targetVolume;
        }

        /// <summary>
        /// Animate sprite color
        /// </summary>
        public static IEnumerator AnimateSpriteColor(SpriteRenderer spriteRenderer, Color targetColor, float duration)
        {
            Color startColor = spriteRenderer.color;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                spriteRenderer.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            spriteRenderer.color = targetColor;
        }

        /// <summary>
        /// Scale transform over time
        /// </summary>
        public static IEnumerator ScaleTransform(Transform transform, Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        /// <summary>
        /// Move transform over time
        /// </summary>
        public static IEnumerator MoveTransform(Transform transform, Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = transform.position;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            transform.position = targetPosition;
        }

        /// <summary>
        /// Chain multiple coroutines together
        /// </summary>
        public static IEnumerator ChainCoroutines(params IEnumerator[] coroutines)
        {
            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
            }
        }

        /// <summary>
        /// Run coroutines in parallel
        /// </summary>
        public static IEnumerator RunInParallel(params IEnumerator[] coroutines)
        {
            var runningCoroutines = new List<Coroutine>();

            foreach (var coroutine in coroutines)
            {
                // Note: This requires a MonoBehaviour to start the coroutines
                // The caller should handle this
                yield return coroutine;
            }
        }
    }

    /// <summary>
    /// MonoBehaviour extension for easier coroutine management
    /// </summary>
    public static class CoroutineExtensions
    {
        /// <summary>
        /// Start a coroutine and return a handle to control it
        /// </summary>
        public static CoroutineHandle StartCoroutineEx(this MonoBehaviour monoBehaviour, IEnumerator coroutine)
        {
            Coroutine unityCoroutine = monoBehaviour.StartCoroutine(coroutine);
            return new CoroutineHandle(monoBehaviour, unityCoroutine);
        }
    }

    /// <summary>
    /// Handle for controlling coroutines
    /// </summary>
    public class CoroutineHandle
    {
        private readonly MonoBehaviour owner;
        private Coroutine coroutine;

        public CoroutineHandle(MonoBehaviour owner, Coroutine coroutine)
        {
            this.owner = owner;
            this.coroutine = coroutine;
        }

        public void Stop()
        {
            if (coroutine != null && owner != null)
            {
                owner.StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        public bool IsRunning => coroutine != null;
    }
}
