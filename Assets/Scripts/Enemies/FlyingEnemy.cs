using UnityEngine;

/// <summary>
/// Simple flying enemy AI:
/// - Chases the player when within chaseRange
/// - Maintains a fixed hover height above the ground
/// - Avoids obstacles with a forward raycast "steer away" impulse
/// - Attacks in melee range, then retreats for a short time (hit-and-run)
/// </summary>
public class FlyingEnemy : MonoBehaviour
{
    // AI state: either chasing the player or retreating away after an attack
    private enum State { Chase, Retreat }

    [Header("Target")]
    [SerializeField] private Transform player;          // Player target to chase/attack
    [SerializeField] private float chaseRange = 30f;    // If player is farther than this, enemy does nothing
    [SerializeField] private float stopDistance = 2.0f; // In Chase state, stop moving forward when close enough

    [Header("Flight")]
    [SerializeField] private float moveSpeed = 6f;      // Base forward movement speed
    [SerializeField] private float turnSpeed = 10f;     // Base turning speed (how fast rotation blends to target direction)

    [Header("Hover Height")]
    [SerializeField] private float chaseHoverHeight = 3.5f;   // Height above ground while chasing
    [SerializeField] private float retreatHoverHeight = 5.0f; // Height above ground while retreating (usually higher)
    [SerializeField] private float hoverAdjustSpeed = 5f;     // How quickly the enemy corrects its Y position to the desired hover height
    [SerializeField] private LayerMask groundMask = ~0;       // Layers considered "ground" for hover raycast

    [Header("Avoidance")]
    [SerializeField] private float avoidDistance = 1.5f;      // Distance for obstacle detection raycast
    [SerializeField] private float avoidStrength = 6f;        // How strongly we steer away when we detect an obstacle
    [SerializeField] private LayerMask obstacleMask = ~0;     // Layers considered obstacles for avoidance raycast

    [Header("Melee")]
    [SerializeField] private float attackRange = 1.6f;        // Distance to trigger an attack attempt
    [SerializeField] private float damage = 10f;              // Damage applied when attacking
    [SerializeField] private float attackCooldown = 1.2f;     // Minimum time between attacks

    [Header("Hit And Run")]
    [SerializeField] private float retreatTime = 0.8f;        // How long retreat state lasts after a hit
    [SerializeField] private float retreatSpeedMult = 1.2f;   // Movement speed multiplier while retreating
    [SerializeField] private float retreatTurnMult = 1.2f;    // Turn speed multiplier while retreating

    // Next time we're allowed to attack again (cooldown tracking)
    private float nextAttackTime;

    // Time at which retreat ends and we switch back to Chase
    private float retreatEndTime;

    // Current AI state
    private State state = State.Chase;

    private void Start()
    {
        // Auto-assign player target if not set in inspector
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        // If we never found the player, do nothing
        if (player == null) return;

        // Vector from enemy to player
        Vector3 toPlayer = player.position - transform.position;

        // Actual distance (uses sqrt); fine here since this script is simple
        float dist = toPlayer.magnitude;

        // Only do AI logic when the player is close enough to care
        if (dist > chaseRange)
            return;

        // If we were retreating and the retreat timer ended, return to chasing
        if (state == State.Retreat && Time.time >= retreatEndTime)
            state = State.Chase;

        // Determine the direction we WANT to go (toward player in Chase, away in Retreat)
        Vector3 desiredDir = GetDesiredDirection(toPlayer);

        // Modify that direction to avoid obstacles
        Vector3 steerDir = ApplyAvoidance(desiredDir);

        // Adjust movement stats depending on state (snappier/faster retreat)
        float curTurn = turnSpeed * (state == State.Retreat ? retreatTurnMult : 1f);
        float curSpeed = moveSpeed * (state == State.Retreat ? retreatSpeedMult : 1f);

        // Rotate toward our steering direction (smoothly)
        // This makes the enemy turn gradually instead of snapping instantly.
        Quaternion targetRot = Quaternion.LookRotation(steerDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, curTurn * Time.deltaTime);

        // Move and do state-specific behavior
        if (state == State.Chase)
        {
            // Only move forward if we are not already close enough to stopDistance
            // (prevents orbiting/overshooting right on top of the player)
            if (dist > stopDistance)
                transform.position += transform.forward * curSpeed * Time.deltaTime;

            // Maintain hover height above ground while chasing
            KeepHoverHeight(chaseHoverHeight);

            // If within attack range, attempt to attack (attack cooldown applies)
            if (dist <= attackRange)
                TryAttack();
        }
        else
        {
            // Retreat: always keep moving forward (direction is set to "away from player" via GetDesiredDirection)
            transform.position += transform.forward * curSpeed * Time.deltaTime;

            // Maintain higher hover height during retreat
            KeepHoverHeight(retreatHoverHeight);
        }
    }

    /// <summary>
    /// Chooses the basic direction we want to fly this frame.
    /// Chase: fly toward player.
    /// Retreat: fly away from player.
    /// </summary>
    private Vector3 GetDesiredDirection(Vector3 toPlayer)
    {
        // If the vector is too small, we cannot normalize it safely.
        // In that rare case, just keep current forward direction.
        if (toPlayer.sqrMagnitude < 0.0001f)
            return transform.forward;

        // Chase means we head toward the player
        if (state == State.Chase)
            return toPlayer.normalized;

        // Retreat means we head away from the player
        return (-toPlayer).normalized;
    }

    /// <summary>
    /// Very simple avoidance:
    /// - Raycast forward
    /// - If we hit something, add a steering vector away from the surface normal
    /// This produces a "nudge" away from walls/obstacles.
    /// </summary>
    private Vector3 ApplyAvoidance(Vector3 desiredDir)
    {
        Vector3 avoid = Vector3.zero;

        // Check for an obstacle directly in front
        // If we hit one, use the hit normal as a push-away direction.
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, avoidDistance, obstacleMask, QueryTriggerInteraction.Ignore))
            avoid = hit.normal * avoidStrength;

        // Combine the desired direction (chase/retreat) with the avoidance offset
        Vector3 steer = desiredDir + avoid;

        // If avoidance cancels out our direction somehow, fall back to desiredDir
        if (steer.sqrMagnitude < 0.0001f)
            steer = desiredDir;

        // Return a normalized steering direction so speed control stays consistent
        return steer.normalized;
    }

    /// <summary>
    /// Keeps the enemy hovering at a fixed height above the ground.
    /// How it works:
    /// - Raycast downward to find the ground below
    /// - Compute the desired Y position = groundY + hoverHeight
    /// - Smoothly move current Y toward desired Y using Lerp
    /// Notes:
    /// - Only affects Y; X and Z are not modified here.
    /// - If no ground is hit (e.g., void), it does nothing and keeps current height.
    /// </summary>
    private void KeepHoverHeight(float hoverHeight)
    {
        // Raycast down to find the ground beneath the enemy
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 80f, groundMask, QueryTriggerInteraction.Ignore))
        {
            // Target Y is the hit point on ground + desired hover offset
            float desiredY = hit.point.y + hoverHeight;

            // Smoothly approach target Y to avoid jitter and harsh snapping
            float newY = Mathf.Lerp(transform.position.y, desiredY, hoverAdjustSpeed * Time.deltaTime);

            // Apply only the Y adjustment; keep current X and Z
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    /// <summary>
    /// Attempts a melee hit if cooldown allows.
    /// On successful attack:
    /// - Deals damage to the player (via PlayerHealth or SendMessage fallback)
    /// - Switches into Retreat for a short time (hit-and-run behavior)
    /// </summary>
    private void TryAttack()
    {
        // Respect attack cooldown
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        // Preferred: direct component call (fast, type-safe)
        PlayerHealth hp = player.GetComponentInParent<PlayerHealth>();
        if (hp != null)
            hp.TakeDamage(damage);
        else
            // Fallback: call TakeDamage on any parent component that implements it (string-based, slower)
            player.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        // After hitting the player, disengage to avoid staying glued on top of them
        state = State.Retreat;
        retreatEndTime = Time.time + retreatTime;
    }

    /// <summary>
    /// Debug visuals when selecting the enemy in the editor:
    /// - chaseRange sphere
    /// - attackRange sphere
    /// - forward avoidance ray
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * avoidDistance);
    }
}
