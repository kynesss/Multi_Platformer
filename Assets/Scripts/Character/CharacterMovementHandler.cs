using Fusion;
using GDT.Data;
using GDT.Projectiles;
using UnityEngine;

namespace GDT.Character
{
    public class CharacterMovementHandler : NetworkBehaviour
    {
        [SerializeField] private float acceleration;
        [SerializeField] private float maxVelocity;
        [SerializeField] private float jumpForce;
        [SerializeField] private float drag;

        [SerializeField] private float wallSlidingMultiplier;
        [SerializeField] private float fallMultiplier;
        [SerializeField] private float lowJumpMultiplier;

        [SerializeField] private Vector2 horizontalVelocityReduction;
        [SerializeField] private Vector2 verticalVelocityReduction;

        private NetworkRigidbody2D _rb;
        private CharacterAnimationHandler _animationHandler;

        private void Awake()
        {
            _rb = GetComponent<NetworkRigidbody2D>();
            _animationHandler = GetComponent<CharacterAnimationHandler>();
        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                _rb.InterpolationDataSource = InterpolationDataSources.Predicted;
            }
        }

        public void Move(NetworkInputData input)
        {
            _rb.Rigidbody.drag = 0f;
            _animationHandler.SetMovementAnimation(true);

            if (input.GetButton(InputButton.Left))
            {
                if (_rb.Rigidbody.velocity.x > 0f)
                {
                    _rb.Rigidbody.velocity *= Vector2.up;
                }

                _rb.Rigidbody.AddForce(Vector2.left * acceleration * Runner.DeltaTime, ForceMode2D.Force);
                _animationHandler.SetSpriteDirection(Vector2.left);
            }

            if (input.GetButton(InputButton.Right))
            {
                if (_rb.Rigidbody.velocity.x < 0f)
                {
                    _rb.Rigidbody.velocity *= Vector2.up;
                }

                _rb.Rigidbody.AddForce(Vector2.right * acceleration * Runner.DeltaTime, ForceMode2D.Force);
                _animationHandler.SetSpriteDirection(Vector2.right);
            }
        }

        public void Jump(NetworkButtons pressedButtons, CharacterTouchDetector touchDetector)
        {
            if (pressedButtons.IsSet(InputButton.Jump))
            {
                if (touchDetector.IsGrounded || touchDetector.IsSliding)
                {
                    _rb.Rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                }

                _animationHandler.SetJumpAnimation(pressedButtons);
            }
        }

        public void BetterJumpLogic(NetworkInputData input, CharacterTouchDetector touchDetector)
        {
            if (touchDetector.IsGrounded) return;

            if (IsFallingDown())
            {
                if (touchDetector.IsSliding && input.AxisPressed())
                {
                    _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (wallSlidingMultiplier - 1) *
                                              Runner.DeltaTime;
                }
                else
                {
                    _rb.Rigidbody.velocity +=
                        Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Runner.DeltaTime;
                }
            }
            else if (_rb.Rigidbody.velocity.y > 0f && !input.GetButton(InputButton.Jump))
            {
                _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Runner.DeltaTime;
            }
        }

        public void LimitSpeed()
        {
            if (Mathf.Abs(_rb.Rigidbody.velocity.x) > maxVelocity)
            {
                _rb.Rigidbody.velocity *= horizontalVelocityReduction;
            }

            if (Mathf.Abs(_rb.Rigidbody.velocity.y) > maxVelocity)
            {
                _rb.Rigidbody.velocity *= verticalVelocityReduction;
            }
        }

        public void Slide(CharacterTouchDetector touchDetector)
        {
            if (touchDetector.IsSliding)
            {
                _rb.Rigidbody.velocity = new Vector2(_rb.Rigidbody.velocity.x,
                    Mathf.Clamp(_rb.Rigidbody.velocity.y, -wallSlidingMultiplier, float.MaxValue));
            }
        }

        public void SetDrag()
        {
            _rb.Rigidbody.drag = drag;
            _animationHandler.SetMovementAnimation(false);
        }

        public bool IsFallingDown()
        {
            return _rb.Rigidbody.velocity.y < 0f;
        }
    }
}