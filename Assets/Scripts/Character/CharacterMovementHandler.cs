using Fusion;
using UnityEngine;

namespace GDT.Character
{
    public class CharacterMovementHandler : NetworkBehaviour
    {
        [SerializeField] private float acceleration;
        [SerializeField] private float maxSpeed;
        [SerializeField] private float jumpForce;
        [SerializeField] private float drag;
        
        private NetworkRigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<NetworkRigidbody2D>();
        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                _rb.InterpolationDataSource = InterpolationDataSources.Predicted;
            }
        }

        public void Move(Vector2 direction)
        {
            direction.Normalize();
            
            _rb.Rigidbody.drag = 0f;
            _rb.Rigidbody.velocity += direction * acceleration * Runner.DeltaTime;
            _rb.Rigidbody.velocity = Vector2.ClampMagnitude(_rb.Rigidbody.velocity, maxSpeed);
        }

        public void Jump()
        {
            _rb.Rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        
        public void SetDrag()
        {
            _rb.Rigidbody.drag = drag;
        }

        public bool IsFallingDown()
        {
            return _rb.Rigidbody.velocity.y < 0;
        }
    }
}