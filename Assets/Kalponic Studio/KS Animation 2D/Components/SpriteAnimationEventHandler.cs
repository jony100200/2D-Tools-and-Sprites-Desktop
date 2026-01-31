using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Component allowing Sprite Animation events to be passed upwards through an object hierarchy
    /// Based on PowerSpriteAnimator's SpriteAnimEventHandler but integrated with KS Animation 2D
    /// </summary>
    [DisallowMultipleComponent]
    public class SpriteAnimationEventHandler : MonoBehaviour
    {
        /// <summary>
        /// Utility class contains functions to split an event's string parameter to a message name and value
        /// Used for KS Animation 2D events with parameters
        /// </summary>
        public static class EventParser
        {
            public static readonly char MESSAGE_DELIMITER = '\t';
            public static readonly string MESSAGE_NOPARAM = "_KSAnim";
            public static readonly string MESSAGE_INT = "_KSAnimInt";
            public static readonly string MESSAGE_FLOAT = "_KSAnimFloat";
            public static readonly string MESSAGE_STRING = "_KSAnimString";
            public static readonly string MESSAGE_OBJECT_FUNCNAME = "_KSAnimObjectFunc";
            public static readonly string MESSAGE_OBJECT_DATA = "_KSAnimObjectData";

            /// <summary>
            /// Parses value from the passed messageString, and modifies it to just contain the message function name
            /// </summary>
            public static int ParseInt(ref string messageString)
            {
                // Data is in form "<functionname>\t<int>"
                int splitAt = messageString.IndexOf(MESSAGE_DELIMITER);
                int result = 0;
                int.TryParse(messageString.Substring(splitAt + 1), out result);
                messageString = messageString.Substring(0, splitAt);
                return result;
            }

            /// <summary>
            /// Parses value from the passed messageString, and modifies it to just contain the message function name
            /// </summary>
            public static float ParseFloat(ref string messageString)
            {
                // Data is in form "<functionname>\t<float>"
                int splitAt = messageString.IndexOf(MESSAGE_DELIMITER);
                float result = 0;
                float.TryParse(messageString.Substring(splitAt + 1), out result);
                messageString = messageString.Substring(0, splitAt);
                return result;
            }

            /// <summary>
            /// Parses value from the passed messageString, and modifies it to just contain the message function name
            /// </summary>
            public static string ParseString(ref string messageString)
            {
                // Data is in form "<functionname>\t<string>"
                int splitAt = messageString.IndexOf(MESSAGE_DELIMITER);
                string result = messageString.Substring(splitAt + 1);
                messageString = messageString.Substring(0, splitAt);
                return result;
            }
        }

        private string m_eventWithObjectMessage = null;
        private object m_eventWithObjectData = null;

        #region Animation Events

        /*
            These messages are used so you can have a sprite nested under the object with logic and still have animation events sent to the parent object.
            The function name and data is encoded in a delimited string.
            It's rather inefficient, but you're generally not going to have loads of anim events each frame, so shouldn't matter.
            If "require receiver" is desired, a duplicate set of functions should be added with different function names.
            They're prefixed with _KS to make it obvious they're not normal messages and make it quick for editor to check the prefix.
        */

        // Animation event with No param
        void _KSAnim(string function)
        {
            SendMessageUpwards(function, SendMessageOptions.DontRequireReceiver);
        }

        // Animation event with Int param
        void _KSAnimInt(string messageString)
        {
            int param = EventParser.ParseInt(ref messageString);
            SendMessageUpwards(messageString, param, SendMessageOptions.DontRequireReceiver);
        }

        // Animation event with Float param
        void _KSAnimFloat(string messageString)
        {
            float param = EventParser.ParseFloat(ref messageString);
            SendMessageUpwards(messageString, param, SendMessageOptions.DontRequireReceiver);
        }

        // Animation event with String param
        void _KSAnimString(string messageString)
        {
            string param = EventParser.ParseString(ref messageString);
            SendMessageUpwards(messageString, param, SendMessageOptions.DontRequireReceiver);
        }

        // Animation event with Object params are split into 2 functions in order to get the function name as well as object. Big hack, bleah.
        void _KSAnimObjectFunc(string funcName)
        {
            if (m_eventWithObjectData != null)
            {
                SendMessageUpwards(funcName, m_eventWithObjectData, SendMessageOptions.DontRequireReceiver);
                m_eventWithObjectMessage = null;
                m_eventWithObjectData = null;
            }
            else
            {
                if (string.IsNullOrEmpty(m_eventWithObjectMessage) == false)
                    KSAnimLog.Warn("Animation event with object parameter had no object", "Playback", this);
                m_eventWithObjectMessage = funcName;
            }
        }

        void _KSAnimObjectData(Object data)
        {
            if (string.IsNullOrEmpty(m_eventWithObjectMessage) == false)
            {
                SendMessageUpwards(m_eventWithObjectMessage, data, SendMessageOptions.DontRequireReceiver);
                m_eventWithObjectMessage = null;
                m_eventWithObjectData = null;
            }
            else
            {
                if (m_eventWithObjectData != null)
                    KSAnimLog.Warn("Animation event with object parameter had no object", "Playback", this);
                m_eventWithObjectData = data;
            }
        }

        #endregion

        /// <summary>
        /// Send a custom animation event upwards through the hierarchy
        /// </summary>
        public void SendAnimationEvent(string functionName)
        {
            SendMessageUpwards(functionName, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Send a custom animation event with int parameter
        /// </summary>
        public void SendAnimationEvent(string functionName, int parameter)
        {
            SendMessageUpwards(functionName, parameter, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Send a custom animation event with float parameter
        /// </summary>
        public void SendAnimationEvent(string functionName, float parameter)
        {
            SendMessageUpwards(functionName, parameter, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Send a custom animation event with string parameter
        /// </summary>
        public void SendAnimationEvent(string functionName, string parameter)
        {
            SendMessageUpwards(functionName, parameter, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Send a custom animation event with object parameter
        /// </summary>
        public void SendAnimationEvent(string functionName, Object parameter)
        {
            SendMessageUpwards(functionName, parameter, SendMessageOptions.DontRequireReceiver);
        }
    }
}
