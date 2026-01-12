using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    private enum State { Chase, Retreat }

    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private float chaseRange = 30f;
    [SerializeField] private float stopDistance = 2.0f;

    [Header("Flight")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float turnSpeed = 10f;

    [Header("Hover Height")]
    [SerializeField] private float chaseHoverHeight = 3.5f;   // higher than before
    [SerializeField] private float retreatHoverHeight = 5.0f; // flies higher while disengaging
    [SerializeField] private float hoverAdjustSpeed = 5f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Avoidance")]
    [SerializeField] private float avoidDistance = 1.5f;
    [SerializeField] private float avoidStrength = 6f;
    [SerializeField] private LayerMask obstacleMask = ~0;

    [Header("Melee")]
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Hit And Run")]
    [SerializeField] private float retreatTime = 0.8f;        // how long it moves away after attacking
    [SerializeField] private float retreatSpeedMult = 1.2f;   // slightly faster while retreating
    [SerializeField] private float retreatTurnMult = 1.2f;    // turns a bit snappier while retreating

    private float nextAttackTime;
    private float retreatEndTime;
    private State state = State.Chase;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist > chaseRange)
            return;

        if (state == State.Retreat && Time.time >= retreatEndTime)
            state = State.Chase;

        Vector3 desiredDir = GetDesiredDirection(toPlayer);
        Vector3 steerDir = ApplyAvoidance(desiredDir);

        float curTurn = turnSpeed * (state == State.Retreat ? retreatTurnMult : 1f);
        float curSpeed = moveSpeed * (state == State.Retreat ? retreatSpeedMult : 1f);

        // Rotate toward movement direction
        Quaternion targetRot = Quaternion.LookRotation(steerDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, curTurn * Time.deltaTime);

        // Move
        if (state == State.Chase)
        {
            if (dist > stopDistance)
                transform.position += transform.forward * curSpeed * Time.deltaTime;

            KeepHoverHeight(chaseHoverHeight);

            if (dist <= attackRange)
                TryAttack();
        }
        else
        {
            // Retreat: always move away for retreatTime
            transform.position += transform.forward * curSpeed * Time.deltaTime;
            KeepHoverHeight(retreatHoverHeight);
        }
    }

    private Vector3 GetDesiredDirection(Vector3 toPlayer)
    {
        if (toPlayer.sqrMagnitude < 0.0001f)
            return transform.forward;

        if (state == State.Chase)
            return toPlayer.normalized;

        // Retreat: fly away from player
        return (-toPlayer).normalized;
    }

    private Vector3 ApplyAvoidance(Vector3 desiredDir)
    {
        Vector3 avoid = Vector3.zero;

        // Simple forward obstacle check
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, avoidDistance, obstacleMask, QueryTriggerInteraction.Ignore))
            avoid = hit.normal * avoidStrength;

        Vector3 steer = desiredDir + avoid;
        if (steer.sqrMagnitude < 0.0001f)
            steer = desiredDir;

        return steer.normalized;
    }

    private void KeepHoverHeight(float hoverHeight)
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 80f, groundMask, QueryTriggerInteraction.Ignore))
        {
            float desiredY = hit.point.y + hoverHeight;
            float newY = Mathf.Lerp(transform.position.y, desiredY, hoverAdjustSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        PlayerHealth hp = player.GetComponentInParent<PlayerHealth>();
        if (hp != null)
            hp.TakeDamage(damage);
        else
            player.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        // Immediately disengage after a hit (hit-and-run)
        state = State.Retreat;
        retreatEndTime = Time.time + retreatTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * avoidDistance);
    }
}
