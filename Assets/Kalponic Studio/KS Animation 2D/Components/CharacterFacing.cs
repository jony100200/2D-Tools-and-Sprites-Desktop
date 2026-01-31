using UnityEngine;
using UnityEngine.Events;

namespace KalponicStudio
{
    /// <summary>
    /// Handles left/right facing with UnityEvent hooks when the direction changes.
    /// </summary>
    public sealed class CharacterFacing : MonoBehaviour
    {
        [SerializeField] private UnityEvent onFaceRight = new UnityEvent();
        [SerializeField] private UnityEvent onFaceLeft = new UnityEvent();
        [SerializeField] private UnityEvent onFaceFlip = new UnityEvent();

        public void FaceDirection(Vector2 input)
        {
            if (input.x < 0 && transform.localScale.x > 0)
            {
                FlipDirection();
                onFaceLeft?.Invoke();
            }
            else if (input.x > 0 && transform.localScale.x < 0)
            {
                FlipDirection();
                onFaceRight?.Invoke();
            }
        }

        private void FlipDirection()
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
            onFaceFlip?.Invoke();
        }
    }
}
