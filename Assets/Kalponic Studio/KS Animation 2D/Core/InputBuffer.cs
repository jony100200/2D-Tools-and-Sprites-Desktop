using System;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Lightweight input buffer to capture actions within a time window (e.g., jump/attack pressed slightly early).
    /// Call <see cref="Record"/> when the input occurs, then <see cref="Consume"/> inside your state to
    /// check/clear the buffered action.
    /// </summary>
    public sealed class InputBuffer
    {
        private float bufferTime;
        private bool buffered;
        private float timestamp;

        public InputBuffer(float bufferTimeSeconds = 0.1f)
        {
            bufferTime = Mathf.Max(0f, bufferTimeSeconds);
        }

        /// <summary>
        /// Change the buffer duration at runtime.
        /// </summary>
        public void SetBufferTime(float seconds)
        {
            bufferTime = Mathf.Max(0f, seconds);
        }

        /// <summary>
        /// Record an input event. Typically called when a key/button is pressed.
        /// </summary>
        public void Record()
        {
            buffered = true;
            timestamp = Time.time;
        }

        /// <summary>
        /// Returns true and clears the buffer if the input is still within the buffer window.
        /// </summary>
        public bool Consume()
        {
            if (!buffered) return false;
            if (Time.time - timestamp > bufferTime)
            {
                buffered = false;
                return false;
            }
            buffered = false;
            return true;
        }

        /// <summary>
        /// Returns true if the input is currently buffered and not expired.
        /// </summary>
        public bool IsBuffered()
        {
            return buffered && (Time.time - timestamp <= bufferTime);
        }
    }
}
