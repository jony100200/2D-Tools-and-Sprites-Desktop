using System;
using System.Collections.Generic;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Simple event system for loose coupling between components
    /// Type-safe event management without Unity's SendMessage overhead
    /// </summary>
    public static class EventSystem
    {
        private static readonly Dictionary<Type, Delegate> eventDictionary = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Subscribe to an event
        /// </summary>
        public static void Subscribe<T>(Action<T> listener) where T : struct
        {
            Type eventType = typeof(T);

            if (eventDictionary.ContainsKey(eventType))
            {
                eventDictionary[eventType] = Delegate.Combine(eventDictionary[eventType], listener);
            }
            else
            {
                eventDictionary[eventType] = listener;
            }
        }

        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        public static void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            Type eventType = typeof(T);

            if (eventDictionary.ContainsKey(eventType))
            {
                eventDictionary[eventType] = Delegate.Remove(eventDictionary[eventType], listener);

                if (eventDictionary[eventType] == null)
                {
                    eventDictionary.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Publish an event to all subscribers
        /// </summary>
        public static void Publish<T>(T eventData) where T : struct
        {
            Type eventType = typeof(T);

            if (eventDictionary.ContainsKey(eventType))
            {
                (eventDictionary[eventType] as Action<T>)?.Invoke(eventData);
            }
        }

        /// <summary>
        /// Clear all event subscriptions
        /// </summary>
        public static void ClearAll()
        {
            eventDictionary.Clear();
        }

        /// <summary>
        /// Get subscriber count for an event type
        /// </summary>
        public static int GetSubscriberCount<T>() where T : struct
        {
            Type eventType = typeof(T);

            if (eventDictionary.ContainsKey(eventType))
            {
                return eventDictionary[eventType].GetInvocationList().Length;
            }

            return 0;
        }
    }

    /// <summary>
    /// Event data structures for common use cases
    /// </summary>
    public static class GameEvents
    {
        // Player events
        public struct PlayerHealthChanged { public float CurrentHealth; public float MaxHealth; }
        public struct PlayerDied { }
        public struct PlayerSpawned { public UnityEngine.Vector3 Position; }

        // Game state events
        public struct GamePaused { public bool IsPaused; }
        public struct GameStarted { }
        public struct GameEnded { public bool PlayerWon; }

        // UI events
        public struct UIOpened { public string UIName; }
        public struct UIClosed { public string UIName; }

        // Level events
        public struct LevelLoaded { public string LevelName; }
        public struct LevelCompleted { public string LevelName; }

        // Input events
        public struct InputAction { public string ActionName; public bool Pressed; }
    }
}
