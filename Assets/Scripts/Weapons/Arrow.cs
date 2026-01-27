using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]

public class Arrow : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private bool useGravity = true;
    [SerializeField] private float lifeTimeSeconds = 30f;
    [SerializeField] private float minSpeedToRotate = 0.5f;

    [Header("Pickup")]
    [SerializeField] private bool becomesPickupWhenStuck = true;
    [SerializeField] private int ammoOnPickup = 1;
    [SerializeField] private float pickupEnableDelay = 0.05f;
    [SerializeField] private KeyCode pickupKey = KeyCode.F;

    private Rigidbody rb;
    private Collider col;

    private float damage;
    private int pierceRemaining;
    private LayerMask hitMask;
    private string enemyTag;

    private bool stuck;
    private bool pickupEnabled;
    private Crossbow playerCrossbowInRange;
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Launch(Vector3 velocity, float damage, int pierceCount, LayerMask hitMask, string enemyTag)
    {
        this.damage = damage;
        this.pierceRemaining = Mathf.Max(0, pierceCount);
        this.hitMask = hitMask;
        this.enemyTag = enemyTag;

        stuck = false;
        pickupEnabled = false;

        rb.isKinematic = false;
        rb.useGravity = useGravity;
        col.isTrigger = false;

        rb.linearVelocity = velocity;

        Destroy(gameObject, lifeTimeSeconds);
    }

    private void Update()
    {
        if (!stuck)
        {
            Vector3 v = rb.linearVelocity;
            if (v.sqrMagnitude > (minSpeedToRotate * minSpeedToRotate))
            {
                transform.rotation = Quaternion.LookRotation(v);
            }
        }
        else
        {
            if (pickupEnabled && playerCrossbowInRange != null && Input.GetKeyDown(pickupKey))
            {
                playerCrossbowInRange.AddReserveAmmo(ammoOnPickup);
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stuck) return;

        // Respect hit mask
        if (((1 << collision.gameObject.layer) & hitMask) == 0)
        {
            // Not in mask, ignore physically but avoid endless bouncing if needed
            return;
        }

        bool isEnemy = collision.collider.CompareTag(enemyTag);

        if (isEnemy)
        {
            ApplyDamageIfPossible(collision.collider);

            // Prevent repeated hits on same collider due to physics jitter
            Physics.IgnoreCollision(col, collision.collider, true);

            if (pierceRemaining > 0)
            {
                pierceRemaining--;
                // Keep flying
                return;
            }

            // No pierce left, stick into the enemy at this contact point
            StickInto(collision);
            return;
        }

        // Non-enemy hit: stick immediately
        StickInto(collision);
    }

    private void ApplyDamageIfPossible(Collider hitCol)
    {
        // Try interface first (recommended)
        IDamageable dmg = hitCol.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
            return;
        }

        // Optional fallback: try a Health component name if you use one
        // var health = hitCol.GetComponentInParent<EnemyHealth>();
        // if (health != null) health.TakeDamage(damage);
    }

    private void StickInto(Collision collision)
    {
        stuck = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        // Snap to contact point with the arrow pointing along its incoming direction if possible
        ContactPoint cp = collision.GetContact(0);
        transform.position = cp.point;

        Vector3 forward = transform.forward;
        if (collision.relativeVelocity.sqrMagnitude > 0.001f)
        {
            forward = collision.relativeVelocity.normalized;
        }

        if (forward.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(forward);
        }

        // Parent to the hit object so it moves with it
        transform.SetParent(collision.transform, true);

        if (becomesPickupWhenStuck)
        {
            // Switch collider to trigger so player can collect
            col.isTrigger = true;
            // Delay enabling pickup to avoid instantly picking it up while shooting
            Invoke(nameof(EnablePickup), pickupEnableDelay);
        }
    }

    private void EnablePickup()
    {
        pickupEnabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!stuck || !pickupEnabled) return;

        // Minimal approach: find Crossbow on player
        Crossbow bow = other.GetComponentInParent<Crossbow>();
        if (bow != null)
        {
            playerCrossbowInRange = bow;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerCrossbowInRange == null) return;

        Crossbow bow = other.GetComponentInParent<Crossbow>();
        if (bow != null && bow == playerCrossbowInRange)
        {
            playerCrossbowInRange = null;
        }
    }
}