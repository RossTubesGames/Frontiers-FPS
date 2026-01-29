using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Arrows : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private bool useGravity = true;
    [SerializeField] private float lifeTimeSeconds = 30f;
    [SerializeField] private float minSpeedToRotate = 0.5f;

    [Header("Pierce Tuning")]
    [SerializeField] private float postHitForwardNudge = 0.08f; // meters
    [SerializeField] private float postHitVelocityRestore = 1.0f; // 0..1 (1 = full restore)

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
<<<<<<< HEAD

=======
>>>>>>> 7ef5aaee30d1fe9049575437171d978c2f706b06
    private Crossbow playerCrossbowInRange;

    private Vector3 lastVelocity;

    public interface IDamageable
    {
        void TakeDamage(float amount);
    }
<<<<<<< HEAD
=======

>>>>>>> 7ef5aaee30d1fe9049575437171d978c2f706b06
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Helps fast projectiles a lot
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
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
        lastVelocity = velocity;

        Destroy(gameObject, lifeTimeSeconds);
    }

    private void FixedUpdate()
    {
        if (stuck) return;
        lastVelocity = rb.linearVelocity;
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
            return;
        }

        bool isEnemy = collision.collider.CompareTag(enemyTag);

        if (isEnemy)
        {
            ApplyDamageIfPossible(collision.collider);

            // Always ignore future collisions with this enemy collider
            Physics.IgnoreCollision(col, collision.collider, true);

            if (pierceRemaining > 0)
            {
                pierceRemaining--;

                // Undo the bounce: restore pre-impact velocity and push forward out of the collider
                Vector3 restoreVel = Vector3.Lerp(rb.linearVelocity, lastVelocity, Mathf.Clamp01(postHitVelocityRestore));
                rb.linearVelocity = restoreVel;

                Vector3 forward = (restoreVel.sqrMagnitude > 0.001f) ? restoreVel.normalized : transform.forward;
                rb.MovePosition(rb.position + forward * postHitForwardNudge);

                return;
            }

            // No pierce left, stick into the enemy
            StickInto(collision);
            return;
        }

        // Non-enemy hit: stick immediately
        StickInto(collision);
    }

    private void ApplyDamageIfPossible(Collider hitCol)
    {
        IDamageable dmg = hitCol.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
            return;
        }
    }

    private void StickInto(Collision collision)
    {
        stuck = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        ContactPoint cp = collision.GetContact(0);
        transform.position = cp.point;

        Vector3 forward = transform.forward;
        if (lastVelocity.sqrMagnitude > 0.001f)
        {
            forward = lastVelocity.normalized;
        }

        if (forward.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(forward);
        }

        transform.SetParent(collision.transform, true);

        if (becomesPickupWhenStuck)
        {
            col.isTrigger = true;
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
