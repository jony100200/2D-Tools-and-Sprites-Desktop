using UnityEngine;
using System.Collections.Generic;

namespace KalponicStudio.Health.UI
{
    /// <summary>
    /// Manager for multiple health UI instances.
    /// Useful for games with multiple characters, enemies, or health displays.
    /// </summary>
    public class HealthUIManager : MonoBehaviour
    {
        [System.Serializable]
        public class HealthUIEntry
        {
            public string id;
            public HealthUIController uiController;
            public HealthSystem healthSystem;
        }

        [Header("UI Instances")]
        [SerializeField] private List<HealthUIEntry> healthUIEntries = new List<HealthUIEntry>();

        [Header("Settings")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool updateAllInRealTime = false;

        private Dictionary<string, HealthUIEntry> uiDictionary = new Dictionary<string, HealthUIEntry>();

        private void Awake()
        {
            if (autoInitialize)
            {
                InitializeUI();
            }
        }

        private void Update()
        {
            if (updateAllInRealTime)
            {
                UpdateAllUI();
            }
        }

        private void InitializeUI()
        {
            uiDictionary.Clear();

            foreach (var entry in healthUIEntries)
            {
                if (!string.IsNullOrEmpty(entry.id) && entry.uiController != null)
                {
                    uiDictionary[entry.id] = entry;

                    // Connect health system if provided
                    if (entry.healthSystem != null)
                    {
                        entry.uiController.SetHealthSystem(entry.healthSystem);
                    }
                }
            }
        }

        public void RegisterHealthUI(string id, HealthUIController uiController, HealthSystem healthSystem = null)
        {
            if (string.IsNullOrEmpty(id) || uiController == null) return;

            HealthUIEntry entry = new HealthUIEntry
            {
                id = id,
                uiController = uiController,
                healthSystem = healthSystem
            };

            healthUIEntries.Add(entry);
            uiDictionary[id] = entry;

            // Connect health system if provided
            if (healthSystem != null)
            {
                uiController.SetHealthSystem(healthSystem);
            }
        }

        public void UnregisterHealthUI(string id)
        {
            if (uiDictionary.ContainsKey(id))
            {
                healthUIEntries.Remove(uiDictionary[id]);
                uiDictionary.Remove(id);
            }
        }

        public HealthUIController GetHealthUI(string id)
        {
            return uiDictionary.ContainsKey(id) ? uiDictionary[id].uiController : null;
        }

        public void UpdateHealthSystem(string id, HealthSystem newHealthSystem)
        {
            if (uiDictionary.ContainsKey(id))
            {
                var entry = uiDictionary[id];
                entry.healthSystem = newHealthSystem;

                if (entry.uiController != null)
                {
                    entry.uiController.SetHealthSystem(newHealthSystem);
                }
            }
        }

        public void UpdateAllUI()
        {
            foreach (var entry in healthUIEntries)
            {
                if (entry.uiController != null)
                {
                    entry.uiController.ForceUpdate();
                }
            }
        }

        public void ShowHealthUI(string id, bool show)
        {
            var uiController = GetHealthUI(id);
            if (uiController != null)
            {
                uiController.gameObject.SetActive(show);
            }
        }

        public void ShowAllHealthUI(bool show)
        {
            foreach (var entry in healthUIEntries)
            {
                if (entry.uiController != null)
                {
                    entry.uiController.gameObject.SetActive(show);
                }
            }
        }

        // Utility methods for common operations
        public void CreatePlayerHealthUI(GameObject playerObject, string uiPrefabPath = null)
        {
            if (playerObject == null) return;

            HealthSystem healthSystem = playerObject.GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                Debug.LogWarning("Player object does not have HealthSystem component");
                return;
            }

            // Create UI GameObject
            GameObject uiObject = new GameObject($"PlayerHealthUI_{playerObject.name}");
            uiObject.transform.SetParent(transform);

            // Add UI Controller
            HealthUIController uiController = uiObject.AddComponent<HealthUIController>();
            uiController.SetHealthSystem(healthSystem);

            // Register with manager
            RegisterHealthUI($"Player_{playerObject.name}", uiController, healthSystem);
        }

        public void CreateEnemyHealthUI(GameObject enemyObject, string uiPrefabPath = null)
        {
            if (enemyObject == null) return;

            HealthSystem healthSystem = enemyObject.GetComponent<HealthSystem>();
            if (healthSystem == null) return;

            // Create UI GameObject
            GameObject uiObject = new GameObject($"EnemyHealthUI_{enemyObject.name}");
            uiObject.transform.SetParent(transform);

            // Add UI Controller
            HealthUIController uiController = uiObject.AddComponent<HealthUIController>();
            uiController.SetHealthSystem(healthSystem);

            // Register with manager
            RegisterHealthUI($"Enemy_{enemyObject.name}", uiController, healthSystem);
        }

        // Get all registered UI IDs
        public List<string> GetAllUIIds()
        {
            return new List<string>(uiDictionary.Keys);
        }

        // Cleanup method
        public void ClearAllUI()
        {
            foreach (var entry in healthUIEntries)
            {
                if (entry.uiController != null)
                {
                    Destroy(entry.uiController.gameObject);
                }
            }

            healthUIEntries.Clear();
            uiDictionary.Clear();
        }
    }
}
