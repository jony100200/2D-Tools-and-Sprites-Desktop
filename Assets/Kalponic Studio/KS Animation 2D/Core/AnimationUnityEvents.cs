using System;
using UnityEngine.Events;

namespace KalponicStudio
{
    [Serializable]
    public sealed class AnimationUnityEvents
    {
        public UnityEvent onAnimationStart = new UnityEvent();
        public UnityEvent onAnimationEnd = new UnityEvent();
        public UnityEvent onAnimationLoop = new UnityEvent();
    }
}
