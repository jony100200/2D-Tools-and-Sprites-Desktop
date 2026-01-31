using UnityEngine;
using KalponicStudio.Utilities;

namespace KalponicStudio.Examples
{
    /// <summary>
    /// Example demonstrating how to use multiple utilities together
    /// This shows a complete game object with timer, state machine, and event system
    /// </summary>
    public class UtilityExample : MonoBehaviour
    {
        [Header("Example Settings")]
        [Tooltip("Speed of movement for the example character.")]
        [SerializeField] private float moveSpeed = 5f;
        [Tooltip("Force applied when jumping.")]
        [SerializeField] private float jumpForce = 10f;
        [Tooltip("SpriteRenderer for visual feedback.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        // State machine for character states
        private StateMachine<CharacterState> stateMachine;

        // Timer for jump cooldown
        private TimerHandle jumpCooldownTimer;

        private void Awake()
        {
            // Initialize state machine
            stateMachine = new StateMachine<CharacterState>();
            stateMachine.Initialize(new IdleState(this));

            // Subscribe to game events
            EventSystem.Subscribe<GameEvents.PlayerHealthChanged>(OnHealthChanged);
            EventSystem.Subscribe<GameEvents.GamePaused>(OnGamePaused);
        }

        private void Start()
        {
            // Example of using Timer for delayed action
            Timer.Create(2f, () => {
                Debug.Log("Game started! Player is ready.");
                EventSystem.Publish(new GameEvents.PlayerSpawned {
                    Position = transform.position
                });
            });
        }

        private void Update()
        {
            // Update utilities that need manual ticking
            Timer.UpdateAll();
            UpdateUtility.UpdateAll();

            // Update state machine
            stateMachine.Update();

            // Example input handling
            float moveX = Input.GetAxisRaw("Horizontal");

            if (Mathf.Abs(moveX) > 0.1f)
            {
                // Switch to walking state
                if (!(stateMachine.CurrentState is WalkingState))
                {
                    stateMachine.ChangeState(new WalkingState(this, moveX > 0));
                }
            }
            else
            {
                // Switch to idle state
                if (!(stateMachine.CurrentState is IdleState))
                {
                    stateMachine.ChangeState(new IdleState(this));
                }
            }

            // Jump input (with cooldown)
            if (Input.GetKeyDown(KeyCode.Space) && jumpCooldownTimer?.IsPaused != false)
            {
                Jump();
            }
        }

        private void Jump()
        {
            // Add jump force
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(
                GetComponent<Rigidbody2D>().linearVelocity.x,
                jumpForce
            );

            // Visual feedback using MathUtility easing
            StartCoroutine(CoroutineUtility.AnimateSpriteColor(
                spriteRenderer,
                Color.cyan,
                0.2f
            ));

            // Start jump cooldown
            jumpCooldownTimer = Timer.Create(1f, () => {
                Debug.Log("Jump cooldown ended");
            });

            // Switch to jumping state
            stateMachine.ChangeState(new JumpingState(this));
        }

        private void OnHealthChanged(GameEvents.PlayerHealthChanged data)
        {
            float healthPercent = data.CurrentHealth / data.MaxHealth;

            // Visual feedback based on health
            Color healthColor = Color.Lerp(Color.red, Color.green, healthPercent);
            StartCoroutine(CoroutineUtility.AnimateSpriteColor(
                spriteRenderer,
                healthColor,
                0.5f
            ));
        }

        private void OnGamePaused(GameEvents.GamePaused data)
        {
            // Pause/resume timers and updates
            if (data.IsPaused)
            {
                jumpCooldownTimer?.Pause();
                UpdateUtility.PauseAll("Movement");
            }
            else
            {
                jumpCooldownTimer?.Resume();
                UpdateUtility.ResumeAll("Movement");
            }
        }

        private void OnDestroy()
        {
            // Cleanup
            EventSystem.Unsubscribe<GameEvents.PlayerHealthChanged>(OnHealthChanged);
            EventSystem.Unsubscribe<GameEvents.GamePaused>(OnGamePaused);
        }
    }

    // Example state classes
    public abstract class CharacterState : IState
    {
        protected readonly UtilityExample character;

        protected CharacterState(UtilityExample character)
        {
            this.character = character;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void LateUpdate() { }
        public virtual void Exit() { }
    }

    public class IdleState : CharacterState
    {
        public IdleState(UtilityExample character) : base(character) { }

        public override void Enter()
        {
            Debug.Log("Entering Idle State");
            // Could play idle animation here
        }

        public override void Update()
        {
            // Idle behavior
        }
    }

    public class WalkingState : CharacterState
    {
        private readonly bool facingRight;

        public WalkingState(UtilityExample character, bool facingRight) : base(character)
        {
            this.facingRight = facingRight;
        }

        public override void Enter()
        {
            Debug.Log($"Entering Walking State (facing {(facingRight ? "right" : "left")})");
            // Could play walk animation here
        }

        public override void Update()
        {
            // Walking behavior - could be handled in main Update
        }
    }

    public class JumpingState : CharacterState
    {
        public JumpingState(UtilityExample character) : base(character) { }

        public override void Enter()
        {
            Debug.Log("Entering Jumping State");
            // Could play jump animation here
        }

        public override void Update()
        {
            // Check if landed and transition back to idle/walking
            if (character.GetComponent<Rigidbody2D>().linearVelocity.y <= 0)
            {
                // Could check ground collision here and transition back
            }
        }
    }
}
