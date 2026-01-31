using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Simple character controller that automatically handles idle/walk animations
    /// Just add this to your character with a Rigidbody2D and PlayableAnimatorComponent
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayableAnimatorComponent))]
    public class SimpleCharacterController : MonoBehaviour
    {
    [Header("Movement")]
    [Tooltip("Speed of horizontal movement.")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("Force applied when jumping.")]
    [SerializeField] private float jumpForce = 10f;

    [Header("Animation")]
    [Tooltip("Name of the idle animation state.")]
    [SerializeField] private string idleAnimation = "Idle";
    [Tooltip("Name of the walk animation state.")]
    [SerializeField] private string walkAnimation = "Walk";
    [Tooltip("Name of the jump animation state.")]
    [SerializeField] private string jumpAnimation = "Jump";
    [Tooltip("Minimum input magnitude to trigger walk animation.")]
    [SerializeField] private float movementThreshold = 0.1f;        private Rigidbody2D rb;
        private PlayableAnimatorComponent animator;
        private bool isGrounded = true;
        private float lastHorizontalInput = 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<PlayableAnimatorComponent>();
        }

        private void Update()
        {
            HandleInput();
            UpdateAnimation();
        }

        private void HandleInput()
        {
            // Horizontal movement
            float horizontal = Input.GetAxisRaw("Horizontal");
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

            // Sprite flipping
            if (horizontal > 0)
                transform.localScale = new Vector3(1, 1, 1);
            else if (horizontal < 0)
                transform.localScale = new Vector3(-1, 1, 1);

            lastHorizontalInput = horizontal;

            // Jumping
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                isGrounded = false;
                animator.Play(jumpAnimation);
            }
        }

        private void UpdateAnimation()
        {
            // Only change animation if not jumping
            if (jumpAnimation == "" || !animator.IsPlaying(jumpAnimation))
            {
                if (Mathf.Abs(lastHorizontalInput) > movementThreshold)
                {
                    animator.Play(walkAnimation);
                }
                else
                {
                    animator.Play(idleAnimation);
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Simple ground detection
            if (collision.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
            }
        }
    }
}
