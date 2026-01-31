using System;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;

namespace KalponicStudio.SO_Framework
{
    /// <summary>
    /// Base class for event listener components that can be added to GameObjects
    /// to respond to ScriptableObject events without writing code.
    /// </summary>
    public abstract class EventListenerBase : MonoBehaviour
    {
        [Header("Event Configuration")]
        [BoxGroup("Event Settings")]
        [Required("Event source cannot be empty")]
        [SerializeField] protected ScriptableVariableBase _eventSource;

        [BoxGroup("Event Settings")]
        [SerializeField] protected bool _listenOnEnable = true;

        [Header("Debug Information")]
        [BoxGroup("Debug Info")]
        [ReadOnly]
        [SerializeField] protected bool _isListening = false;

        [BoxGroup("Debug Info")]
        [ReadOnly]
        [SerializeField] protected int _triggerCount = 0;

        [BoxGroup("Debug Info")]
        [ReadOnly]
        [SerializeField] protected string _lastTriggered = "Never";

        /// <summary>
        /// Called when the component is enabled
        /// </summary>
        protected virtual void OnEnable()
        {
            if (_listenOnEnable)
            {
                StartListening();
            }
        }

        /// <summary>
        /// Called when the component is disabled
        /// </summary>
        protected virtual void OnDisable()
        {
            StopListening();
        }

        /// <summary>
        /// Start listening to the event
        /// </summary>
        [Button("Start Listening")]
        public virtual void StartListening()
        {
            if (_eventSource == null)
            {
                Debug.LogWarning($"[{name}] Event source is not assigned");
                return;
            }

            RegisterEventCallbacks();
            _isListening = true;
            OnRepaintRequest();
        }

        /// <summary>
        /// Stop listening to the event
        /// </summary>
        [Button("Stop Listening")]
        public virtual void StopListening()
        {
            if (_eventSource != null)
            {
                UnregisterEventCallbacks();
            }

            _isListening = false;
            OnRepaintRequest();
        }

        /// <summary>
        /// Register callbacks for the event
        /// </summary>
        protected abstract void RegisterEventCallbacks();

        /// <summary>
        /// Unregister callbacks for the event
        /// </summary>
        protected abstract void UnregisterEventCallbacks();

        /// <summary>
        /// Handle event trigger
        /// </summary>
        protected virtual void OnEventTriggered()
        {
            _triggerCount++;
            _lastTriggered = DateTime.Now.ToString("HH:mm:ss");
            OnRepaintRequest();
        }

        /// <summary>
        /// Request a repaint of the inspector
        /// </summary>
        protected virtual void OnRepaintRequest()
        {
#if UNITY_EDITOR
            // Use the event system instead of direct API call
            // This allows custom editors to subscribe to repaint requests
#endif
        }
    }

    /// <summary>
    /// Listens to void events and invokes UnityEvents
    /// </summary>
    [AddComponentMenu("SO Framework/Event Listeners/Void Event Listener")]
    public class VoidEventListener : EventListenerBase
    {
        [Header("Event Response")]
        [BoxGroup("Response Actions")]
        [SerializeField] private UnityEvent _onEventTriggered;

        [BoxGroup("Response Actions")]
        [SerializeField] private bool _logTrigger = false;

        [BoxGroup("Response Actions")]
        [ShowIf("_logTrigger")]
        [SerializeField] private string _logMessage = "Void event triggered";

        protected override void RegisterEventCallbacks()
        {
            if (_eventSource is ScriptableEvent scriptableEvent)
            {
                scriptableEvent.RegisterListener(OnVoidEvent);
            }
        }

        protected override void UnregisterEventCallbacks()
        {
            if (_eventSource is ScriptableEvent scriptableEvent)
            {
                scriptableEvent.UnregisterListener(OnVoidEvent);
            }
        }

        private void OnVoidEvent()
        {
            if (_logTrigger)
            {
                Debug.Log($"[{name}] {_logMessage}");
            }

            _onEventTriggered?.Invoke();
            OnEventTriggered();
        }
    }

    /// <summary>
    /// Listens to typed events and invokes UnityEvents with the event data
    /// </summary>
    /// <typeparam name="T">The type of data carried by the event</typeparam>
    public abstract class TypedEventListener<T> : EventListenerBase
    {
        [Header("Event Response")]
        [BoxGroup("Response Actions")]
        [SerializeField] protected UnityEvent<T> _onEventTriggered;

        [BoxGroup("Response Actions")]
        [SerializeField] protected bool _logTrigger = false;

        [BoxGroup("Response Actions")]
        [ShowIf("_logTrigger")]
        [SerializeField] protected string _logMessage = "Event triggered with value: {0}";

        protected override void RegisterEventCallbacks()
        {
            if (_eventSource is ScriptableEvent<T> typedEvent)
            {
                typedEvent.OnRaised += OnTypedEvent;
            }
        }

        protected override void UnregisterEventCallbacks()
        {
            if (_eventSource is ScriptableEvent<T> typedEvent)
            {
                typedEvent.OnRaised -= OnTypedEvent;
            }
        }

        private void OnTypedEvent(T value)
        {
            if (_logTrigger)
            {
                Debug.Log(string.Format($"[{name}] {_logMessage}", value));
            }

            _onEventTriggered?.Invoke(value);
            OnEventTriggered();
        }
    }

    /// <summary>
    /// Listens to int events
    /// </summary>
    [AddComponentMenu("SO Framework/Event Listeners/Int Event Listener")]
    public class IntEventListener : TypedEventListener<int> { }

    /// <summary>
    /// Listens to float events
    /// </summary>
    [AddComponentMenu("SO Framework/Event Listeners/Float Event Listener")]
    public class FloatEventListener : TypedEventListener<float> { }

    /// <summary>
    /// Listens to bool events
    /// </summary>
    [AddComponentMenu("SO Framework/Event Listeners/Bool Event Listener")]
    public class BoolEventListener : TypedEventListener<bool> { }

    /// <summary>
    /// Listens to string events
    /// </summary>
    [AddComponentMenu("SO Framework/Event Listeners/String Event Listener")]
    public class StringEventListener : TypedEventListener<string> { }

    /// <summary>
    /// Listens to Vector3 events
    /// </summary>
    [AddComponentMenu("SO Framework/Event Listeners/Vector3 Event Listener")]
    public class Vector3EventListener : TypedEventListener<Vector3> { }

    /// <summary>
    /// Listens to Color events
    /// </summary>
    [AddComponentMenu("SO Framework/Event Listeners/Color Event Listener")]
    public class ColorEventListener : TypedEventListener<Color> { }

    /// <summary>
    /// Listens to GameObject events
    /// </summary>
    [AddComponentMenu("SO Framework/Event Listeners/GameObject Event Listener")]
    public class GameObjectEventListener : TypedEventListener<GameObject> { }

    /// <summary>
    /// Advanced event listener with conditional responses
    /// </summary>
    [AddComponentMenu("SO Framework/Event Listeners/Conditional Event Listener")]
    public class ConditionalEventListener : EventListenerBase
    {
        [System.Serializable]
        public class ConditionalResponse
        {
            [SerializeField] private string _conditionName;
            [SerializeField] private UnityEvent _response;

            public string ConditionName => _conditionName;
            public UnityEvent Response => _response;
        }

        [Header("Conditional Responses")]
        [BoxGroup("Conditional Logic")]
        [SerializeField] private ConditionalResponse[] _conditionalResponses;

        [BoxGroup("Conditional Logic")]
        [SerializeField] private UnityEvent _defaultResponse;

        [BoxGroup("Conditional Logic")]
        [SerializeField] private bool _logConditionEvaluation = false;

        protected override void RegisterEventCallbacks()
        {
            // This would need to be implemented based on the specific event type
            // For now, it's a placeholder for advanced conditional logic
        }

        protected override void UnregisterEventCallbacks()
        {
            // Implementation would mirror RegisterEventCallbacks
        }

        /// <summary>
        /// Evaluate conditions and invoke appropriate responses
        /// </summary>
        protected virtual void EvaluateConditions(object eventData)
        {
            bool conditionMet = false;

            foreach (var response in _conditionalResponses)
            {
                if (EvaluateCondition(response.ConditionName, eventData))
                {
                    if (_logConditionEvaluation)
                    {
                        Debug.Log($"[{name}] Condition '{response.ConditionName}' met, invoking response");
                    }

                    response.Response?.Invoke();
                    conditionMet = true;
                }
            }

            if (!conditionMet)
            {
                if (_logConditionEvaluation)
                {
                    Debug.Log($"[{name}] No conditions met, invoking default response");
                }

                _defaultResponse?.Invoke();
            }
        }

        /// <summary>
        /// Evaluate a specific condition (override in derived classes)
        /// </summary>
        protected virtual bool EvaluateCondition(string conditionName, object eventData)
        {
            // Default implementation - override for custom logic
            return false;
        }
    }

    /// <summary>
    /// Event listener that can trigger Unity animations
    /// </summary>
    [AddComponentMenu("SO Framework/Event Listeners/Animation Event Listener")]
    public class AnimationEventListener : EventListenerBase
    {
        [Header("Animation Settings")]
        [BoxGroup("Animation Control")]
        [SerializeField] private Animator _animator;

        [BoxGroup("Animation Control")]
        [SerializeField] private string _triggerParameter = "Trigger";

        [BoxGroup("Animation Control")]
        [SerializeField] private string _boolParameter = "";

        [BoxGroup("Animation Control")]
        [SerializeField] private bool _boolValue = true;

        [BoxGroup("Animation Control")]
        [SerializeField] private string _intParameter = "";

        [BoxGroup("Animation Control")]
        [MinValue(0)]
        [SerializeField] private int _intValue = 0;

        [BoxGroup("Animation Control")]
        [SerializeField] private string _floatParameter = "";

        [BoxGroup("Animation Control")]
        [SerializeField] private float _floatValue = 0f;

        protected override void RegisterEventCallbacks()
        {
            if (_eventSource is ScriptableEvent scriptableEvent)
            {
                scriptableEvent.RegisterListener(OnAnimationEvent);
            }
        }

        protected override void UnregisterEventCallbacks()
        {
            if (_eventSource is ScriptableEvent scriptableEvent)
            {
                scriptableEvent.UnregisterListener(OnAnimationEvent);
            }
        }

        private void OnAnimationEvent()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
                if (_animator == null)
                {
                    Debug.LogWarning($"[{name}] No Animator component found");
                    return;
                }
            }

            // Set animation parameters
            if (!string.IsNullOrEmpty(_triggerParameter))
            {
                _animator.SetTrigger(_triggerParameter);
            }

            if (!string.IsNullOrEmpty(_boolParameter))
            {
                _animator.SetBool(_boolParameter, _boolValue);
            }

            if (!string.IsNullOrEmpty(_intParameter))
            {
                _animator.SetInteger(_intParameter, _intValue);
            }

            if (!string.IsNullOrEmpty(_floatParameter))
            {
                _animator.SetFloat(_floatParameter, _floatValue);
            }

            OnEventTriggered();
        }
    }

    /// <summary>
    /// Event listener that can play audio clips
    /// </summary>
    [AddComponentMenu("SO Framework/Event Listeners/Audio Event Listener")]
    public class AudioEventListener : EventListenerBase
    {
        [Header("Audio Settings")]
        [BoxGroup("Audio Control")]
        [SerializeField] private AudioSource _audioSource;

        [BoxGroup("Audio Control")]
        [SerializeField] private AudioClip _audioClip;

        [BoxGroup("Audio Control")]
        [SerializeField] private bool _useRandomPitch = false;

        [BoxGroup("Audio Control")]
        [ShowIf("_useRandomPitch")]
        [MinMaxSlider(0f, 2f)]
        [SerializeField] private Vector2 _pitchRange = new Vector2(0.8f, 1.2f);

        [BoxGroup("Audio Control")]
        [SerializeField] private bool _useRandomVolume = false;

        [BoxGroup("Audio Control")]
        [ShowIf("_useRandomVolume")]
        [MinMaxSlider(0f, 1f)]
        [SerializeField] private Vector2 _volumeRange = new Vector2(0.8f, 1f);

        protected override void RegisterEventCallbacks()
        {
            if (_eventSource is ScriptableEvent scriptableEvent)
            {
                scriptableEvent.RegisterListener(OnAudioEvent);
            }
        }

        protected override void UnregisterEventCallbacks()
        {
            if (_eventSource is ScriptableEvent scriptableEvent)
            {
                scriptableEvent.UnregisterListener(OnAudioEvent);
            }
        }

        private void OnAudioEvent()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    Debug.LogWarning($"[{name}] No AudioSource component found");
                    return;
                }
            }

            if (_audioClip != null)
            {
                _audioSource.clip = _audioClip;
            }

            // Apply random variations
            if (_useRandomPitch)
            {
                _audioSource.pitch = UnityEngine.Random.Range(_pitchRange.x, _pitchRange.y);
            }

            if (_useRandomVolume)
            {
                _audioSource.volume = UnityEngine.Random.Range(_volumeRange.x, _volumeRange.y);
            }

            _audioSource.Play();
            OnEventTriggered();
        }
    }
}
