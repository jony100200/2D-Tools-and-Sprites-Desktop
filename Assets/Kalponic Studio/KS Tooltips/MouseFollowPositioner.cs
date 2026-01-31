using UnityEngine;

namespace KalponicStudio.Tooltips
{
    /// <summary>
    /// Simple positioner that follows the mouse cursor with offset and screen clamping.
    /// Implements ITooltipPositioner for clean separation of concerns.
    /// </summary>
    public class MouseFollowPositioner : ITooltipPositioner
    {
        private Vector2 offset = new Vector2(10f, 10f);
        private Vector2 bounds = new Vector2(Screen.width, Screen.height);
        private bool clampToBounds = true;

        /// <summary>
        /// Calculates the tooltip position based on trigger position, tooltip size, and screen bounds.
        /// </summary>
        public Vector2 CalculatePosition(Vector2 triggerPosition, Vector2 tooltipSize, Vector2 screenSize)
        {
            Vector2 position = triggerPosition + offset;

            if (clampToBounds)
            {
                // Keep tooltip within screen bounds
                position.x = Mathf.Clamp(position.x, 0, screenSize.x - tooltipSize.x);
                position.y = Mathf.Clamp(position.y, tooltipSize.y, screenSize.y);
            }

            return position;
        }

        /// <summary>
        /// Sets the offset from the trigger position.
        /// </summary>
        public void SetOffset(Vector2 newOffset)
        {
            offset = newOffset;
        }

        /// <summary>
        /// Sets the screen bounds for clamping.
        /// </summary>
        public void SetBounds(Vector2 newBounds)
        {
            bounds = newBounds;
        }
    }
}
