using UnityEngine;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// Void event (no parameters)
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Events/Void Event", fileName = "VoidEvent")]
    public class VoidEvent : ScriptableEvent
    {
    }

    /// <summary>
    /// Integer event
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Events/Int Event", fileName = "IntEvent")]
    public class IntEvent : ScriptableEvent<int>
    {
    }

    /// <summary>
    /// Float event
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Events/Float Event", fileName = "FloatEvent")]
    public class FloatEvent : ScriptableEvent<float>
    {
    }

    /// <summary>
    /// Boolean event
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Events/Bool Event", fileName = "BoolEvent")]
    public class BoolEvent : ScriptableEvent<bool>
    {
    }

    /// <summary>
    /// String event
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Events/String Event", fileName = "StringEvent")]
    public class StringEvent : ScriptableEvent<string>
    {
    }

    /// <summary>
    /// GameObject event
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Events/GameObject Event", fileName = "GameObjectEvent")]
    public class GameObjectEvent : ScriptableEvent<GameObject>
    {
    }

    /// <summary>
    /// Vector3 event
    /// </summary>
    [CreateAssetMenu(menuName = "SO Framework/Events/Vector3 Event", fileName = "Vector3Event")]
    public class Vector3Event : ScriptableEvent<Vector3>
    {
    }
}
