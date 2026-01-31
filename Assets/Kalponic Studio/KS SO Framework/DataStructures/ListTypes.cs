using UnityEngine;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// List of GameObjects
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Lists/GameObject List", fileName = "GameObjectList")]
    public class GameObjectList : ScriptableList<GameObject>
    {
    }

    /// <summary>
    /// List of integers
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Lists/Int List", fileName = "IntList")]
    public class IntList : ScriptableList<int>
    {
    }

    /// <summary>
    /// List of strings
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Lists/String List", fileName = "StringList")]
    public class StringList : ScriptableList<string>
    {
    }

    /// <summary>
    /// List of Vector3 positions
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Lists/Vector3 List", fileName = "Vector3List")]
    public class Vector3List : ScriptableList<Vector3>
    {
    }
}
