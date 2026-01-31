using UnityEngine;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// Integer variable ScriptableObject
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Int Variable", fileName = "IntVariable")]
    public class IntVariable : ScriptableVariable<int>
    {
        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                Value = PlayerPrefs.GetInt(GetSaveKey(), InitialValue);
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                PlayerPrefs.SetInt(GetSaveKey(), Value);
                PlayerPrefs.Save();
            }
        }
    }

    /// <summary>
    /// Float variable ScriptableObject
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Float Variable", fileName = "FloatVariable")]
    public class FloatVariable : ScriptableVariable<float>
    {
        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                Value = PlayerPrefs.GetFloat(GetSaveKey(), InitialValue);
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                PlayerPrefs.SetFloat(GetSaveKey(), Value);
                PlayerPrefs.Save();
            }
        }
    }

    /// <summary>
    /// Boolean variable ScriptableObject
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/Bool Variable", fileName = "BoolVariable")]
    public class BoolVariable : ScriptableVariable<bool>
    {
        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                Value = PlayerPrefs.GetInt(GetSaveKey(), InitialValue ? 1 : 0) == 1;
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                PlayerPrefs.SetInt(GetSaveKey(), Value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }

    /// <summary>
    /// String variable ScriptableObject
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Variables/String Variable", fileName = "StringVariable")]
    public class StringVariable : ScriptableVariable<string>
    {
        public override void Load()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                Value = PlayerPrefs.GetString(GetSaveKey(), InitialValue ?? "");
            }
        }

        public override void Save()
        {
            if (!string.IsNullOrEmpty(GetSaveKey()))
            {
                PlayerPrefs.SetString(GetSaveKey(), Value ?? "");
                PlayerPrefs.Save();
            }
        }
    }
}
