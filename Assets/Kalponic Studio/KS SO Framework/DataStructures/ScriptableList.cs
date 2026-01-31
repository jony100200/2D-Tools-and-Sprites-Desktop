using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// ScriptableObject-based list that can be referenced across the game.
    /// Provides observable collection functionality.
    /// </summary>
    /// <typeparam name="T">The type of items in the list</typeparam>
    public abstract class ScriptableList<T> : ScriptableVariableBase
    {
        [SerializeField] private List<T> _list = new List<T>();

        /// <summary>
        /// Event raised when the list changes
        /// </summary>
        public event Action OnChanged;

        /// <summary>
        /// Event raised when an item is added
        /// </summary>
        public event Action<T> OnItemAdded;

        /// <summary>
        /// Event raised when an item is removed
        /// </summary>
        public event Action<T> OnItemRemoved;

        /// <summary>
        /// Read-only access to the list
        /// </summary>
        public ReadOnlyCollection<T> List => _list.AsReadOnly();

        /// <summary>
        /// Get the count of items in the list
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// Check if the list contains an item
        /// </summary>
        public bool Contains(T item) => _list.Contains(item);

        /// <summary>
        /// Get item at index
        /// </summary>
        public T this[int index] => _list[index];

        /// <summary>
        /// Add an item to the list
        /// </summary>
        public void Add(T item)
        {
            _list.Add(item);
            OnItemAdded?.Invoke(item);
            OnChanged?.Invoke();
            OnRepaintRequest();
        }

        /// <summary>
        /// Remove an item from the list
        /// </summary>
        public bool Remove(T item)
        {
            bool removed = _list.Remove(item);
            if (removed)
            {
                OnItemRemoved?.Invoke(item);
                OnChanged?.Invoke();
                OnRepaintRequest();
            }
            return removed;
        }

        /// <summary>
        /// Remove item at index
        /// </summary>
        public void RemoveAt(int index)
        {
            if (index >= 0 && index < _list.Count)
            {
                T item = _list[index];
                _list.RemoveAt(index);
                OnItemRemoved?.Invoke(item);
                OnChanged?.Invoke();
                OnRepaintRequest();
            }
        }

        /// <summary>
        /// Clear all items from the list
        /// </summary>
        public void Clear()
        {
            _list.Clear();
            OnChanged?.Invoke();
            OnRepaintRequest();
        }

        /// <summary>
        /// Insert item at index
        /// </summary>
        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            OnItemAdded?.Invoke(item);
            OnChanged?.Invoke();
            OnRepaintRequest();
        }

        /// <summary>
        /// Find first item matching predicate
        /// </summary>
        public T Find(Predicate<T> match) => _list.Find(match);

        /// <summary>
        /// Find all items matching predicate
        /// </summary>
        public List<T> FindAll(Predicate<T> match) => _list.FindAll(match);

        /// <summary>
        /// Get enumerator for foreach loops
        /// </summary>
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        /// <summary>
        /// Copy list to array
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        /// <summary>
        /// Set the entire list (replaces all items)
        /// </summary>
        public void SetList(IEnumerable<T> newList)
        {
            _list.Clear();
            _list.AddRange(newList);
            OnChanged?.Invoke();
            OnRepaintRequest();
        }
    }
}
