using UnityEngine;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// KSSnappable - Marks a GameObject as snappable for KS SnapStudio capture.
    /// Add this component to any GameObject you want to include in captures.
    /// </summary>
    public class KSSnappable : MonoBehaviour
    {
        [Tooltip("Display name for this snappable object")]
        public string displayName = "";

        [Tooltip("Category for organization in UI")]
        public string category = "Default";

        [Tooltip("Whether this object should be captured by default")]
        public bool captureByDefault = true;

        [Tooltip("Whether this object is currently enabled for capture")]
        public bool enabledForCapture = true;

        [Tooltip("Custom pivot point for this object (if null, uses transform.position)")]
        public Transform customPivot;

        void OnValidate()
        {
            if (string.IsNullOrEmpty(displayName))
                displayName = gameObject.name;
        }

        /// <summary>
        /// Get the effective pivot point for this snappable
        /// </summary>
        public Vector3 GetPivot()
        {
            return customPivot != null ? customPivot.position : transform.position;
        }

        /// <summary>
        /// Get the effective pivot rotation for this snappable
        /// </summary>
        public Quaternion GetPivotRotation()
        {
            return customPivot != null ? customPivot.rotation : transform.rotation;
        }

        /// <summary>
        /// Check if this snappable should be included in capture
        /// </summary>
        public bool ShouldCapture()
        {
            return enabledForCapture && gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Toggle capture state
        /// </summary>
        public void ToggleCapture()
        {
            enabledForCapture = !enabledForCapture;
        }
    }
}
