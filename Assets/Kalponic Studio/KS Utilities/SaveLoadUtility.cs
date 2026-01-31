using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Utility for handling save/load operations with multiple formats and platforms
    /// Supports JSON, Binary, and PlayerPrefs with automatic platform detection
    /// </summary>
    public static class SaveLoadUtility
    {
        public enum SaveFormat
        {
            JSON,
            Binary,
            PlayerPrefs
        }

        public enum SaveLocation
        {
            PersistentDataPath,
            ApplicationDataPath,
            Custom
        }

        private static string customSavePath = "";

        /// <summary>
        /// Set custom save path for SaveLocation.Custom
        /// </summary>
        public static void SetCustomSavePath(string path)
        {
            customSavePath = path;
        }

        /// <summary>
        /// Save data to file with specified format
        /// </summary>
        public static bool Save<T>(T data, string fileName, SaveFormat format = SaveFormat.JSON,
                                 SaveLocation location = SaveLocation.PersistentDataPath)
        {
            try
            {
                string filePath = GetFilePath(fileName, location);

                switch (format)
                {
                    case SaveFormat.JSON:
                        return SaveJson(data, filePath);

                    case SaveFormat.Binary:
                        return SaveBinary(data, filePath);

                    case SaveFormat.PlayerPrefs:
                        return SavePlayerPrefs(data, fileName);

                    default:
                        Debug.LogError($"Unsupported save format: {format}");
                        return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save {fileName}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load data from file with specified format
        /// </summary>
        public static T Load<T>(string fileName, SaveFormat format = SaveFormat.JSON,
                              SaveLocation location = SaveLocation.PersistentDataPath)
        {
            try
            {
                string filePath = GetFilePath(fileName, location);

                switch (format)
                {
                    case SaveFormat.JSON:
                        return LoadJson<T>(filePath);

                    case SaveFormat.Binary:
                        return LoadBinary<T>(filePath);

                    case SaveFormat.PlayerPrefs:
                        return LoadPlayerPrefs<T>(fileName);

                    default:
                        Debug.LogError($"Unsupported load format: {format}");
                        return default;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load {fileName}: {e.Message}");
                return default;
            }
        }

        /// <summary>
        /// Check if save file exists
        /// </summary>
        public static bool SaveExists(string fileName, SaveFormat format = SaveFormat.JSON,
                                    SaveLocation location = SaveLocation.PersistentDataPath)
        {
            if (format == SaveFormat.PlayerPrefs)
            {
                return PlayerPrefs.HasKey(fileName);
            }

            string filePath = GetFilePath(fileName, location);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Delete save file
        /// </summary>
        public static bool DeleteSave(string fileName, SaveFormat format = SaveFormat.JSON,
                                    SaveLocation location = SaveLocation.PersistentDataPath)
        {
            try
            {
                if (format == SaveFormat.PlayerPrefs)
                {
                    PlayerPrefs.DeleteKey(fileName);
                    return true;
                }

                string filePath = GetFilePath(fileName, location);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete {fileName}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get all save files in a directory
        /// </summary>
        public static string[] GetSaveFiles(SaveLocation location = SaveLocation.PersistentDataPath,
                                          string extension = "*")
        {
            try
            {
                string directory = GetDirectoryPath(location);
                if (!Directory.Exists(directory))
                {
                    return new string[0];
                }

                string searchPattern = $"*.{extension}";
                if (extension == "*") searchPattern = "*.*";

                return Directory.GetFiles(directory, searchPattern);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get save files: {e.Message}");
                return new string[0];
            }
        }

        /// <summary>
        /// Asynchronous save operation
        /// </summary>
        public static async Task<bool> SaveAsync<T>(T data, string fileName,
                                                  SaveFormat format = SaveFormat.JSON,
                                                  SaveLocation location = SaveLocation.PersistentDataPath)
        {
            return await Task.Run(() => Save(data, fileName, format, location));
        }

        /// <summary>
        /// Asynchronous load operation
        /// </summary>
        public static async Task<T> LoadAsync<T>(string fileName,
                                               SaveFormat format = SaveFormat.JSON,
                                               SaveLocation location = SaveLocation.PersistentDataPath)
        {
            return await Task.Run(() => Load<T>(fileName, format, location));
        }

        // Private helper methods
        private static string GetFilePath(string fileName, SaveLocation location)
        {
            string directory = GetDirectoryPath(location);
            return Path.Combine(directory, fileName);
        }

        private static string GetDirectoryPath(SaveLocation location)
        {
            switch (location)
            {
                case SaveLocation.PersistentDataPath:
                    return Application.persistentDataPath;

                case SaveLocation.ApplicationDataPath:
                    return Application.dataPath;

                case SaveLocation.Custom:
                    return string.IsNullOrEmpty(customSavePath) ? Application.persistentDataPath : customSavePath;

                default:
                    return Application.persistentDataPath;
            }
        }

        private static bool SaveJson<T>(T data, string filePath)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(filePath, json);
            return true;
        }

        private static T LoadJson<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return default;
            }

            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<T>(json);
        }

        private static bool SaveBinary<T>(T data, string filePath)
        {
            using (FileStream stream = File.Open(filePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                string json = JsonUtility.ToJson(data);
                writer.Write(json);
            }
            return true;
        }

        private static T LoadBinary<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return default;
            }

            using (FileStream stream = File.Open(filePath, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                string json = reader.ReadString();
                return JsonUtility.FromJson<T>(json);
            }
        }

        private static bool SavePlayerPrefs<T>(T data, string key)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
            return true;
        }

        private static T LoadPlayerPrefs<T>(string key)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                return default;
            }

            string json = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<T>(json);
        }
    }

    /// <summary>
    /// Base class for saveable objects with automatic save/load
    /// </summary>
    [Serializable]
    public abstract class SaveableObject
    {
        [SerializeField] protected string saveKey;
        [SerializeField] protected SaveLoadUtility.SaveFormat saveFormat = SaveLoadUtility.SaveFormat.JSON;
        [SerializeField] protected SaveLoadUtility.SaveLocation saveLocation = SaveLoadUtility.SaveLocation.PersistentDataPath;

        protected virtual string GetDefaultSaveKey()
        {
            return GetType().Name;
        }

        public virtual void Save()
        {
            string key = string.IsNullOrEmpty(saveKey) ? GetDefaultSaveKey() : saveKey;
            SaveLoadUtility.Save(this, key, saveFormat, saveLocation);
        }

        public virtual void Load()
        {
            string key = string.IsNullOrEmpty(saveKey) ? GetDefaultSaveKey() : saveKey;
            var loaded = SaveLoadUtility.Load<SaveableObject>(key, saveFormat, saveLocation);
            if (loaded != null)
            {
                CopyFrom(loaded);
            }
        }

        protected abstract void CopyFrom(SaveableObject other);

        public virtual bool SaveExists()
        {
            string key = string.IsNullOrEmpty(saveKey) ? GetDefaultSaveKey() : saveKey;
            return SaveLoadUtility.SaveExists(key, saveFormat, saveLocation);
        }

        public virtual void DeleteSave()
        {
            string key = string.IsNullOrEmpty(saveKey) ? GetDefaultSaveKey() : saveKey;
            SaveLoadUtility.DeleteSave(key, saveFormat, saveLocation);
        }
    }
}
