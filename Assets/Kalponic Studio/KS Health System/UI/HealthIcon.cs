using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace KalponicStudio.Health.UI
{
    /// <summary>
    /// Health icon component that displays different sprites based on health percentage.
    /// Perfect for health states like full, damaged, critical, etc.
    /// </summary>
    public class HealthIcon : MonoBehaviour
    {
        [System.Serializable]
        public class HealthIconState
        {
            public string name;
            public float minHealthPercent; // 0-1
            public Sprite icon;
            public Color tintColor = Color.white;
            public Vector2 size = new Vector2(32, 32);
        }

        [Header("UI References")]
        [SerializeField] private Image iconImage;

        [Header("Icon States")]
        [SerializeField] private List<HealthIconState> healthStates = new List<HealthIconState>()
        {
            new HealthIconState { name = "Critical", minHealthPercent = 0f, tintColor = Color.red },
            new HealthIconState { name = "Damaged", minHealthPercent = 0.3f, tintColor = Color.yellow },
            new HealthIconState { name = "Healthy", minHealthPercent = 0.7f, tintColor = Color.green }
        };

        [Header("Animation")]
        [SerializeField] private bool animateTransitions = true;
        [SerializeField] private float transitionDuration = 0.2f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Settings")]
        [SerializeField] private bool maintainAspectRatio = true;
        [SerializeField] private bool useStateSize = true;

        private HealthIconState currentState;
        private Coroutine transitionCoroutine;

        private void Awake()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (iconImage == null)
                iconImage = GetComponent<Image>();

            if (iconImage == null)
            {
                Debug.LogError("HealthIcon: No Image component found!");
                return;
            }

            // Sort states by health percentage (highest first)
            healthStates.Sort((a, b) => b.minHealthPercent.CompareTo(a.minHealthPercent));

            // Set initial state
            UpdateHealthState(1f);
        }

        public void UpdateHealthState(float healthPercent)
        {
            HealthIconState newState = GetStateForHealth(healthPercent);

            if (newState != currentState)
            {
                if (animateTransitions)
                {
                    TransitionToState(newState);
                }
                else
                {
                    ApplyState(newState);
                }
            }
        }

        private HealthIconState GetStateForHealth(float healthPercent)
        {
            foreach (var state in healthStates)
            {
                if (healthPercent >= state.minHealthPercent)
                {
                    return state;
                }
            }

            // Fallback to first state if no match
            return healthStates.Count > 0 ? healthStates[0] : null;
        }

        private void TransitionToState(HealthIconState newState)
        {
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);

            transitionCoroutine = StartCoroutine(AnimateStateTransition(newState));
        }

        private System.Collections.IEnumerator AnimateStateTransition(HealthIconState newState)
        {
            HealthIconState oldState = currentState;
            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = transitionCurve.Evaluate(elapsed / transitionDuration);

                // Interpolate color
                if (oldState != null && iconImage != null)
                {
                    iconImage.color = Color.Lerp(oldState.tintColor, newState.tintColor, t);
                }

                yield return null;
            }

            ApplyState(newState);
            transitionCoroutine = null;
        }

        private void ApplyState(HealthIconState state)
        {
            currentState = state;

            if (state == null || iconImage == null) return;

            // Apply sprite
            if (state.icon != null)
            {
                iconImage.sprite = state.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }

            // Apply color
            iconImage.color = state.tintColor;

            // Apply size if enabled
            if (useStateSize)
            {
                RectTransform rectTransform = iconImage.rectTransform;
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = state.size;
                }
            }

            // Set aspect ratio
            if (maintainAspectRatio && state.icon != null)
            {
                iconImage.preserveAspect = true;
            }
        }

        // Public API for customization
        public void AddHealthState(string name, float minPercent, Sprite icon, Color color, Vector2 size)
        {
            HealthIconState newState = new HealthIconState
            {
                name = name,
                minHealthPercent = minPercent,
                icon = icon,
                tintColor = color,
                size = size
            };

            healthStates.Add(newState);
            healthStates.Sort((a, b) => b.minHealthPercent.CompareTo(a.minHealthPercent));
        }

        public void RemoveHealthState(string name)
        {
            healthStates.RemoveAll(state => state.name == name);
        }

        public void UpdateStateIcon(string name, Sprite newIcon)
        {
            HealthIconState state = healthStates.Find(s => s.name == name);
            if (state != null)
            {
                state.icon = newIcon;
                if (currentState == state)
                {
                    ApplyState(state);
                }
            }
        }

        public void UpdateStateColor(string name, Color newColor)
        {
            HealthIconState state = healthStates.Find(s => s.name == name);
            if (state != null)
            {
                state.tintColor = newColor;
                if (currentState == state)
                {
                    ApplyState(state);
                }
            }
        }

        public void SetAnimationSettings(bool animate, float duration, AnimationCurve curve)
        {
            animateTransitions = animate;
            transitionDuration = duration;
            transitionCurve = curve;
        }

        public HealthIconState GetCurrentState()
        {
            return currentState;
        }

        public List<HealthIconState> GetAllStates()
        {
            return new List<HealthIconState>(healthStates);
        }
    }
}
