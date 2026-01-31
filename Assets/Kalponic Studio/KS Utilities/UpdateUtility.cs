using System.Collections.Generic;
using System;
using UnityEngine;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Update utility for continuous actions without requiring MonoBehaviour
    /// </summary>
    public static class UpdateUtility
    {
        internal class UpdateInstance
        {
            public Func<bool> UpdateAction { get; private set; }
            public bool IsPaused { get; set; }
            public string Name { get; private set; }

            public UpdateInstance(Func<bool> updateAction, string name)
            {
                UpdateAction = updateAction;
                Name = name;
                IsPaused = false;
            }
        }

        private static readonly List<UpdateInstance> activeUpdates = new List<UpdateInstance>();
        private static readonly List<UpdateInstance> updatesToRemove = new List<UpdateInstance>();

        /// <summary>
        /// Create a continuous update action that runs until it returns true
        /// </summary>
        public static UpdateHandle Create(Func<bool> updateAction, string name = "")
        {
            var update = new UpdateInstance(updateAction, name);
            activeUpdates.Add(update);

            return new UpdateHandle(update);
        }

        /// <summary>
        /// Create a continuous update action from an Action (never completes)
        /// </summary>
        public static UpdateHandle Create(Action updateAction, string name = "")
        {
            return Create(() => { updateAction(); return false; }, name);
        }

        /// <summary>
        /// Stop all updates with a specific name
        /// </summary>
        public static void StopAll(string name)
        {
            foreach (var update in activeUpdates)
            {
                if (update.Name == name)
                {
                    updatesToRemove.Add(update);
                }
            }
        }

        /// <summary>
        /// Stop the first update with a specific name
        /// </summary>
        public static void StopFirst(string name)
        {
            foreach (var update in activeUpdates)
            {
                if (update.Name == name)
                {
                    updatesToRemove.Add(update);
                    break;
                }
            }
        }

        /// <summary>
        /// Pause all updates with a specific name
        /// </summary>
        public static void PauseAll(string name)
        {
            foreach (var update in activeUpdates)
            {
                if (update.Name == name)
                {
                    update.IsPaused = true;
                }
            }
        }

        /// <summary>
        /// Resume all updates with a specific name
        /// </summary>
        public static void ResumeAll(string name)
        {
            foreach (var update in activeUpdates)
            {
                if (update.Name == name)
                {
                    update.IsPaused = false;
                }
            }
        }

        /// <summary>
        /// Update all active update actions (call this from a MonoBehaviour's Update)
        /// </summary>
        public static void UpdateAll()
        {
            foreach (var update in activeUpdates)
            {
                if (!update.IsPaused && update.UpdateAction())
                {
                    updatesToRemove.Add(update);
                }
            }

            // Remove completed updates
            foreach (var update in updatesToRemove)
            {
                activeUpdates.Remove(update);
            }
            updatesToRemove.Clear();
        }

        /// <summary>
        /// Clear all active updates
        /// </summary>
        public static void ClearAll()
        {
            activeUpdates.Clear();
            updatesToRemove.Clear();
        }

        /// <summary>
        /// Handle for controlling individual update actions
        /// </summary>
        public class UpdateHandle
        {
            private readonly UpdateInstance update;

            internal UpdateHandle(UpdateInstance update)
            {
                this.update = update;
            }

            public void Pause() => update.IsPaused = true;
            public void Resume() => update.IsPaused = false;
            public void Stop() => updatesToRemove.Add(update);
            public bool IsPaused => update.IsPaused;
        }
    }
}
