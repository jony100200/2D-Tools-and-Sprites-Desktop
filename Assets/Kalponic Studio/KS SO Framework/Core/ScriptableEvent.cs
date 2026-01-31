using System;
using UnityEngine;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// ScriptableObject-based event system that can be raised from anywhere.
    /// Replaces UnityEvents for better decoupling and editor integration.
    /// </summary>
    /// <typeparam name="T">The type of parameter passed to listeners</typeparam>
    public abstract class ScriptableEvent<T> : ScriptableVariableBase
    {
        /// <summary>
        /// Event raised when this event is triggered
        /// </summary>
        public event Action<T> OnRaised;

        [Header("Debug")]
        [SerializeField] private bool _debugLog = false;
        [SerializeField] private string _debugMessage = "Event Raised";

        /// <summary>
        /// Raise the event with the given parameter
        /// </summary>
        /// <param name="param">The parameter to pass to listeners</param>
        public void Raise(T param)
        {
            if (_debugLog)
                Debug.Log($"[{name}] {_debugMessage}: {param}");

            OnRaised?.Invoke(param);
            OnRepaintRequest();
        }

        /// <summary>
        /// Raise the event without parameters (for void events)
        /// </summary>
        public void Raise()
        {
            Raise(default(T));
        }

        /// <summary>
        /// Register a listener to this event
        /// </summary>
        public void RegisterListener(Action<T> listener)
        {
            OnRaised += listener;
        }

        /// <summary>
        /// Unregister a listener from this event
        /// </summary>
        public void UnregisterListener(Action<T> listener)
        {
            OnRaised -= listener;
        }

        /// <summary>
        /// Clear all listeners (useful for cleanup)
        /// </summary>
        public void ClearListeners()
        {
            OnRaised = null;
        }
    }

    /// <summary>
    /// Void event (no parameters)
    /// </summary>
    public abstract class ScriptableEvent : ScriptableEvent<VoidType>
    {
        private event Action _voidOnRaised;

        public new void Raise() => _voidOnRaised?.Invoke();

        /// <summary>
        /// Register a listener to this void event
        /// </summary>
        public void RegisterListener(Action listener)
        {
            _voidOnRaised += listener;
        }

        /// <summary>
        /// Unregister a listener from this void event
        /// </summary>
        public void UnregisterListener(Action listener)
        {
            _voidOnRaised -= listener;
        }

        /// <summary>
        /// Clear all void listeners
        /// </summary>
        public void ClearVoidListeners()
        {
            _voidOnRaised = null;
        }
    }

    /// <summary>
    /// Helper struct for void events
    /// </summary>
    public struct VoidType
    {
        public static readonly VoidType Value = new VoidType();
    }
}
