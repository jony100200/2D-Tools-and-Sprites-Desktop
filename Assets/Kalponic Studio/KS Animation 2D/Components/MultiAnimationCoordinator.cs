using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Coordinates multiple animation components (body/weapon/effects) so they can be driven together.
    /// </summary>
    public sealed class MultiAnimationCoordinator : MonoBehaviour
    {
        [SerializeField] private List<AnimationComponentBase> components = new List<AnimationComponentBase>();

        public void Register(AnimationComponentBase component)
        {
            if (component != null && !components.Contains(component))
            {
                components.Add(component);
            }
        }

        public void Unregister(AnimationComponentBase component)
        {
            if (component != null)
            {
                components.Remove(component);
            }
        }

        public void PlayAnimation(AnimationType type, float speed = 1f, bool loop = true)
        {
            foreach (var c in components)
            {
                c?.PlayAnimation(type, speed, loop);
            }
        }

        public void PauseAll()
        {
            foreach (var c in components)
            {
                c?.PauseAnimation();
            }
        }

        public void ResumeAll()
        {
            foreach (var c in components)
            {
                c?.ResumeAnimation();
            }
        }

        public void StopAll()
        {
            foreach (var c in components)
            {
                c?.StopAnimation();
            }
        }
    }
}
