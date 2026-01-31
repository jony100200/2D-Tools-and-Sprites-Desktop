using System;
using UnityEngine;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// Base class for all ScriptableObject variables and events.
    /// Provides common functionality and editor integration.
    /// </summary>
    public abstract class ScriptableVariableBase : ScriptableObject
    {
        [Header("Metadata")]
        [SerializeField] private string _description;
        [SerializeField] private string _tag;

        public string Description => _description;
        public string Tag => _tag;

        /// <summary>
        /// Called when the variable needs to repaint in the editor
        /// </summary>
        public event Action RepaintRequest;

        protected void OnRepaintRequest()
        {
            RepaintRequest?.Invoke();
        }

        /// <summary>
        /// Get the generic type of this variable for editor purposes
        /// </summary>
        public virtual Type GetGenericType()
        {
            return GetType();
        }

        /// <summary>
        /// Reset the variable to its default state
        /// </summary>
        public virtual void Reset()
        {
            // Override in derived classes
        }
    }
}
