using UnityEngine;

public class MeleeAnimatedSlash : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Punch Shape")]
    [SerializeField] private float range = 1.2f;
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float maxAngle = 55f; // 3D cone in front

    [Header("Timing")]
    [SerializeField] private float cooldown = 0.25f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTriggerName = "Attack";

    private float nextTime;

    private void Awake()
    {
        // Auto-find animator if not assigned
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextTime)
        {
            nextTime = Time.time + cooldown;
            Punch();
        }
    }

    private void Punch()
    {
        // Trigger attack animation
        if (animator != null)
            animator.SetTrigger(attackTriggerName);

        // Play the sword swinging sound
        FMODUnity.RuntimeManager.PlayOneShot("event:/Axe swings");

        // Hit detection
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward.normalized;
        Vector3 center = origin + forward * range;

        Collider[] hits = Physics.OverlapSphere(
            center,
            radius,
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i];

            if (!c.CompareTag(enemyTag))
                continue;

            Vector3 toTarget = c.bounds.center - origin;
            if (toTarget.sqrMagnitude < 0.0001f)
                continue;

            float angle = Vector3.Angle(forward, toTarget.normalized);
            if (angle > maxAngle)
                continue;

            EnemyHealth hp = c.GetComponentInParent<EnemyHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
            }
            else
            {
                c.SendMessageUpwards(
                    "TakeDamage",
                    damage,
                    SendMessageOptions.DontRequireReceiver
                );
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 origin = transform.position;
        Vector3 forward = transform.forward.normalized;
        Gizmos.DrawWireSphere(origin + forward * range, radius);
    }
}
