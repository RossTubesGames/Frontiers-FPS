using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossEnemy : MonoBehaviour
{
    private enum State { Chase, Charge, Recover }

    [Header("Weakpoint")]
    [SerializeField] private Transform weakpoint;

    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private float chaseRange = 350f;
    [SerializeField] private float stopDistance = 2.0f;
    [SerializeField] private float repathInterval = 0.2f;

    [Header("Melee")]
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Anti-jitter")]
    [SerializeField] private float attackExitBuffer = 0.4f;

    [Header("Charge")]
    [SerializeField] private float chargeTriggerDistance = 10f;
    [SerializeField] private float chargeSpeed = 12f;
    [SerializeField] private float chargeStopDistance = 1.0f;
    [SerializeField] private float chargeCooldown = 2.0f;
    [SerializeField] private float chargeFailSafeTime = 2.5f;

    [Header("Slam (end of charge)")]
    [SerializeField] private float slamRadius = 4f;
    [SerializeField] private float slamDamage = 25f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private ParticleSystem slamVFX;
    [SerializeField] private float slamVFXLifetime = 3f;
    [SerializeField] private float postSlamWait = 5f;

    private NavMeshAgent agent;
    private float nextRepathTime;
    private float nextAttackTime;

    private State state = State.Chase;

    private float baseSpeed;
    private float baseStoppingDistance;

    private Vector3 lockedChargeTarget;
    private float chargeStartTime;
    private float nextChargeTime;

    private float recoverEndTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        agent.autoBraking = true;

        baseSpeed = agent.speed;
        baseStoppingDistance = stopDistance;
        agent.stoppingDistance = baseStoppingDistance;
    }

    private void Update()
    {
        if (player == null) return;

        if (state == State.Recover)
        {
            if (Time.time >= recoverEndTime)
            {
                state = State.Chase;
                ResumeAgent();
                nextRepathTime = Time.time + repathInterval;
            }
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (state == State.Chase && dist > chaseRange)
        {
            StopAgent();
            return;
        }

        if (state == State.Charge)
        {
            UpdateCharge();
            return;
        }

        // Start charge if far enough and cooldown ready
        if (dist >= chargeTriggerDistance && Time.time >= nextChargeTime)
        {
            StartCharge();
            return;
        }

        UpdateChase(dist);
    }

    private void UpdateChase(float dist)
    {
        if (!agent.isStopped && Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathInterval;
            agent.SetDestination(player.position);
        }

        float startAttackDist = attackRange;
        float resumeChaseDist = attackRange + attackExitBuffer;

        if (dist <= startAttackDist)
        {
            StopAgent();
            TryAttack();
        }
        else if (dist >= resumeChaseDist)
        {
            ResumeAgent();
        }
    }

    private void StartCharge()
    {
        state = State.Charge;

        lockedChargeTarget = player.position;

        agent.isStopped = false;
        agent.speed = chargeSpeed;
        agent.stoppingDistance = chargeStopDistance;

        chargeStartTime = Time.time;
        agent.SetDestination(lockedChargeTarget);
    }

    private void UpdateCharge()
    {
        if (Time.time - chargeStartTime >= chargeFailSafeTime)
        {
            EndCharge();
            return;
        }

        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (agent.velocity.sqrMagnitude < 0.01f)
                    EndCharge();
            }
        }
    }

    private void EndCharge()
    {
        // Stop at the charge point and do the slam
        StopAgent();
        DoSlam();

        // Start charge cooldown AFTER reaching the position
        nextChargeTime = Time.time + chargeCooldown;

        // Restore normal movement settings (but do not chase yet)
        agent.speed = baseSpeed;
        agent.stoppingDistance = baseStoppingDistance;

        // Enter recovery/wait state
        state = State.Recover;
        recoverEndTime = Time.time + postSlamWait;
    }

    private void DoSlam()
    {
        Vector3 center = transform.position;

        // Spawn visual effect
        if (slamVFX != null)
        {
            ParticleSystem vfx = Instantiate(slamVFX, center, Quaternion.identity);
            vfx.Play();
            Destroy(vfx.gameObject, slamVFXLifetime);
        }

        // Damage players in radius (tag-based)
        Collider[] hits = Physics.OverlapSphere(center, slamRadius, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].CompareTag(playerTag))
                continue;

            PlayerHealth hp = hits[i].GetComponentInParent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage(slamDamage);
            }
            else
            {
                hits[i].SendMessageUpwards(
                    "TakeDamage",
                    slamDamage,
                    SendMessageOptions.DontRequireReceiver
                );
            }
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
    }

    private void StopAgent()
    {
        if (agent.isStopped) return;
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void ResumeAgent()
    {
        if (!agent.isStopped) return;
        agent.isStopped = false;
        if (player != null) agent.SetDestination(player.position);
    }

    public Transform GetWeakpoint() => weakpoint;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(1f, 0.3f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, chargeTriggerDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(lockedChargeTarget, 0.25f);
        Gizmos.DrawLine(transform.position, lockedChargeTarget);

        Gizmos.color = new Color(1f, 0.7f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, slamRadius);

        if (weakpoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(weakpoint.position, 0.25f);
        }
    }
}
