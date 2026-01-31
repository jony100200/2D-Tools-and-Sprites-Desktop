using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Lightweight combo tracker for attack chains. Keeps an index that resets after a timeout.
    /// </summary>
    public sealed class ComboChain
    {
        private readonly int maxCombo;
        private float comboResetTime;
        private float lastInputTime;
        private int currentIndex;

        public ComboChain(int maxCombo = 3, float comboResetTimeSeconds = 0.8f)
        {
            this.maxCombo = Mathf.Max(1, maxCombo);
            comboResetTime = Mathf.Max(0f, comboResetTimeSeconds);
            Reset();
        }

        public void SetResetTime(float seconds) => comboResetTime = Mathf.Max(0f, seconds);

        /// <summary>
        /// Advance to the next combo step, resetting the timer. Returns the current combo index (0-based).
        /// </summary>
        public int Advance()
        {
            if (Time.time - lastInputTime > comboResetTime)
            {
                Reset();
            }

            lastInputTime = Time.time;
            int index = currentIndex;
            currentIndex = (currentIndex + 1) % maxCombo;
            return index;
        }

        /// <summary>
        /// Reset the combo back to start.
        /// </summary>
        public void Reset()
        {
            currentIndex = 0;
            lastInputTime = -comboResetTime * 2f;
        }
    }
}
