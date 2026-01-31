using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace KalponicStudio.Utilities
{
    /// <summary>
    /// Utility for common input patterns, device detection, and input management
    /// Works with both old and new Input System
    /// </summary>
    public static class InputUtility
    {
        public enum InputDeviceType
        {
            KeyboardMouse,
            Gamepad,
            Touch,
            VR,
            Unknown
        }

        public enum InputScheme
        {
            KeyboardMouse,
            Gamepad,
            Touch,
            VR
        }

        private static InputDeviceType lastDetectedDevice = InputDeviceType.Unknown;
        private static readonly Dictionary<KeyCode, float> keyHoldTimes = new Dictionary<KeyCode, float>();
        private static readonly Dictionary<string, float> axisHoldTimes = new Dictionary<string, float>();
        private static readonly List<InputBinding> activeBindings = new List<InputBinding>();

        /// <summary>
        /// Get the current input device type
        /// </summary>
        public static InputDeviceType GetCurrentDeviceType()
        {
            // Check for VR first
            if (UnityEngine.XR.XRSettings.isDeviceActive)
            {
                return InputDeviceType.VR;
            }

            // Check for touch input
            if (Input.touchCount > 0 || Input.touches.Length > 0)
            {
                return InputDeviceType.Touch;
            }

            // Check for gamepad
#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            {
                return InputDeviceType.Gamepad;
            }
#else
            if (Input.GetJoystickNames() != null && Input.GetJoystickNames().Length > 0)
            {
                return InputDeviceType.Gamepad;
            }
#endif

            // Check for mouse input
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2) ||
                Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f)
            {
                return InputDeviceType.KeyboardMouse;
            }

            // Check for keyboard input
            if (Input.anyKey)
            {
                return InputDeviceType.KeyboardMouse;
            }

            return lastDetectedDevice;
        }

        /// <summary>
        /// Check if device type has changed since last check
        /// </summary>
        public static bool HasDeviceChanged()
        {
            InputDeviceType currentDevice = GetCurrentDeviceType();
            if (currentDevice != lastDetectedDevice && currentDevice != InputDeviceType.Unknown)
            {
                lastDetectedDevice = currentDevice;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get recommended input scheme based on current device
        /// </summary>
        public static InputScheme GetRecommendedScheme()
        {
            return (InputScheme)GetCurrentDeviceType();
        }

        /// <summary>
        /// Check if a key is being held for a specific duration
        /// </summary>
        public static bool IsKeyHeld(KeyCode key, float duration)
        {
            if (Input.GetKey(key))
            {
                if (!keyHoldTimes.ContainsKey(key))
                {
                    keyHoldTimes[key] = 0f;
                }

                keyHoldTimes[key] += Time.deltaTime;

                if (keyHoldTimes[key] >= duration)
                {
                    return true;
                }
            }
            else
            {
                keyHoldTimes.Remove(key);
            }

            return false;
        }

        /// <summary>
        /// Check if an axis is being held in a direction for a specific duration
        /// </summary>
        public static bool IsAxisHeld(string axisName, float threshold, float duration)
        {
            float axisValue = Input.GetAxis(axisName);

            if (Mathf.Abs(axisValue) >= threshold)
            {
                string key = axisName + (axisValue > 0 ? "+" : "-");

                if (!axisHoldTimes.ContainsKey(key))
                {
                    axisHoldTimes[key] = 0f;
                }

                axisHoldTimes[key] += Time.deltaTime;

                if (axisHoldTimes[key] >= duration)
                {
                    return true;
                }
            }
            else
            {
                // Clear both positive and negative directions
                axisHoldTimes.Remove(axisName + "+");
                axisHoldTimes.Remove(axisName + "-");
            }

            return false;
        }

        /// <summary>
        /// Get smoothed axis value with deadzone
        /// </summary>
        public static float GetSmoothedAxis(string axisName, float deadzone = 0.1f, float smoothing = 0.1f)
        {
            float rawValue = Input.GetAxis(axisName);

            // Apply deadzone
            if (Mathf.Abs(rawValue) < deadzone)
            {
                rawValue = 0f;
            }
            else
            {
                // Remap from deadzone to 1
                rawValue = Mathf.Sign(rawValue) * (Mathf.Abs(rawValue) - deadzone) / (1f - deadzone);
            }

            // Apply smoothing (this is a simple implementation)
            return Mathf.Lerp(0f, rawValue, 1f - smoothing);
        }

        /// <summary>
        /// Get mouse world position on a plane
        /// </summary>
        public static Vector3 GetMouseWorldPosition(Camera camera = null, float planeHeight = 0f)
        {
            if (camera == null) camera = Camera.main;
            if (camera == null) return Vector3.zero;

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, planeHeight);

            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Check if mouse is over UI element
        /// </summary>
        public static bool IsMouseOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null &&
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Get scroll wheel delta with smoothing
        /// </summary>
        public static float GetSmoothScrollDelta(float smoothing = 0.1f)
        {
            return Mathf.Lerp(0f, Input.mouseScrollDelta.y, 1f - smoothing);
        }

        /// <summary>
        /// Vibrate controller (if supported)
        /// </summary>
        public static void Vibrate(float leftMotor, float rightMotor, float duration = 0.1f)
        {
#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null)
            {
                Gamepad.current.SetMotorSpeeds(leftMotor, rightMotor);
                Timer.Create(duration, () =>
                {
                    if (Gamepad.current != null)
                    {
                        Gamepad.current.SetMotorSpeeds(0f, 0f);
                    }
                });
            }
#else
            Debug.LogWarning("Controller vibration requires the Input System package.");
#endif
        }

        /// <summary>
        /// Check for double click/tap
        /// </summary>
        public static bool IsDoubleClick(float maxTimeBetweenClicks = 0.3f)
        {
            if (Input.GetMouseButtonDown(0))
            {
                float timeSinceLastClick = Time.time - lastClickTime;

                if (timeSinceLastClick <= maxTimeBetweenClicks)
                {
                    lastClickTime = 0f;
                    return true;
                }

                lastClickTime = Time.time;
            }

            return false;
        }

        private static float lastClickTime = 0f;

        /// <summary>
        /// Get input axis as Vector2
        /// </summary>
        public static Vector2 GetInputAxis(string horizontalAxis = "Horizontal", string verticalAxis = "Vertical")
        {
            return new Vector2(
                Input.GetAxis(horizontalAxis),
                Input.GetAxis(verticalAxis)
            );
        }

        /// <summary>
        /// Check if any key in a set is pressed
        /// </summary>
        public static bool AnyKeyPressed(params KeyCode[] keys)
        {
            foreach (KeyCode key in keys)
            {
                if (Input.GetKey(key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if any key in a set was pressed this frame
        /// </summary>
        public static bool AnyKeyDown(params KeyCode[] keys)
        {
            foreach (KeyCode key in keys)
            {
                if (Input.GetKeyDown(key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get direction from WASD keys
        /// </summary>
        public static Vector2 GetWASDDirection()
        {
            Vector2 direction = Vector2.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) direction.y = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) direction.y = -1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) direction.x = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) direction.x = 1f;

            return direction.normalized;
        }

        /// <summary>
        /// Check for keyboard shortcut (multiple keys pressed simultaneously)
        /// </summary>
        public static bool IsShortcutPressed(params KeyCode[] keys)
        {
            foreach (KeyCode key in keys)
            {
                if (!Input.GetKey(key))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get text input with validation
        /// </summary>
        public static string GetValidatedInput(Func<char, bool> validator = null)
        {
            string input = Input.inputString;

            if (validator != null)
            {
                System.Text.StringBuilder validInput = new System.Text.StringBuilder();

                foreach (char c in input)
                {
                    if (validator(c))
                    {
                        validInput.Append(c);
                    }
                }

                return validInput.ToString();
            }

            return input;
        }

        /// <summary>
        /// Clear all cached input states
        /// </summary>
        public static void ClearCache()
        {
            keyHoldTimes.Clear();
            axisHoldTimes.Clear();
            lastClickTime = 0f;
        }

        /// <summary>
        /// Input binding for custom controls
        /// </summary>
        public struct InputBinding
        {
            public string name;
            public Func<bool> condition;
            public Action action;
            public bool enabled;

            public InputBinding(string name, Func<bool> condition, Action action)
            {
                this.name = name;
                this.condition = condition;
                this.action = action;
                this.enabled = true;
            }
        }

        /// <summary>
        /// Register a custom input binding
        /// </summary>
        public static void RegisterBinding(InputBinding binding)
        {
            activeBindings.Add(binding);
        }

        /// <summary>
        /// Unregister a custom input binding
        /// </summary>
        public static void UnregisterBinding(string name)
        {
            activeBindings.RemoveAll(b => b.name == name);
        }

        /// <summary>
        /// Update all registered bindings (call this from Update)
        /// </summary>
        public static void UpdateBindings()
        {
            foreach (var binding in activeBindings)
            {
                if (binding.enabled && binding.condition())
                {
                    binding.action();
                }
            }
        }

        /// <summary>
        /// Enable/disable binding by name
        /// </summary>
        public static void SetBindingEnabled(string name, bool enabled)
        {
            for (int i = 0; i < activeBindings.Count; i++)
            {
                if (activeBindings[i].name == name)
                {
                    activeBindings[i] = new InputBinding(
                        activeBindings[i].name,
                        activeBindings[i].condition,
                        activeBindings[i].action
                    ) { enabled = enabled };
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Input buffer for combo moves and complex input sequences
    /// </summary>
    public class InputBuffer
    {
        private readonly List<InputEvent> buffer = new List<InputEvent>();
        private readonly float bufferTime;

        public InputBuffer(float bufferTime = 0.5f)
        {
            this.bufferTime = bufferTime;
        }

        public struct InputEvent
        {
            public string inputName;
            public float timestamp;

            public InputEvent(string inputName)
            {
                this.inputName = inputName;
                this.timestamp = Time.time;
            }
        }

        public void AddInput(string inputName)
        {
            buffer.Add(new InputEvent(inputName));
            CleanOldInputs();
        }

        public bool CheckSequence(params string[] sequence)
        {
            if (buffer.Count < sequence.Length) return false;

            CleanOldInputs();

            int bufferIndex = buffer.Count - 1;
            int sequenceIndex = sequence.Length - 1;

            while (sequenceIndex >= 0 && bufferIndex >= 0)
            {
                if (buffer[bufferIndex].inputName == sequence[sequenceIndex])
                {
                    sequenceIndex--;
                }
                bufferIndex--;
            }

            return sequenceIndex < 0;
        }

        public void Clear()
        {
            buffer.Clear();
        }

        private void CleanOldInputs()
        {
            float currentTime = Time.time;
            buffer.RemoveAll(e => currentTime - e.timestamp > bufferTime);
        }
    }
}
