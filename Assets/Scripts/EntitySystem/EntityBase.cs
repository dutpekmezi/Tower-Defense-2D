using UnityEngine;

namespace dutpekmezi
{
    [RequireComponent(typeof(Collider2D))]
    public class EntityBase : MonoBehaviour
    {
        [Header("Assigned Data")]
        [SerializeField] private EntityData entityData;

        [Header("References")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D col;

        [Header("Physics Settings")]
        public float restitution = 1f;  // bounce strength
        public bool useGravity = false;
        public float gravityScale = 1f;

        private Vector2 lastVelocity;

        private void Awake()
        {
            if (!rb)
                rb = GetComponentInChildren<Rigidbody2D>();
            if (!col)
                col = GetComponentInChildren<Collider2D>();

            if (rb != null)
            {
                rb.gravityScale = 0;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            // Force collider to be trigger for custom physics
            if (col != null)
                col.isTrigger = true;
        }

        private void FixedUpdate()
        {
            if (rb != null)
                lastVelocity = rb.linearVelocity;
        }

        public void Launch(Vector2 velocity, float linearDrag)
        {
            if (!rb) return;

            rb.gravityScale = useGravity ? gravityScale : 0f;
            rb.linearDamping = linearDrag;
            rb.angularDamping = 0f;

            rb.linearVelocity = velocity;
            rb.angularVelocity = 0f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (rb == null || lastVelocity.sqrMagnitude < 0.001f)
                return;

            // Find closest point and surface normal
            Vector2 contactPoint = other.ClosestPoint(rb.position);
            Vector2 normal = (rb.position - contactPoint).normalized;

            Debug.DrawRay(contactPoint, normal * 0.3f, Color.red, 1f);

            // Reflect using normal
            Vector2 reflected = Vector2.Reflect(lastVelocity, normal) * restitution;
            rb.linearVelocity = reflected;
        }

        public Rigidbody2D GetRigidbody() => rb;
        public Collider2D GetCollider() => col;
    }
}
