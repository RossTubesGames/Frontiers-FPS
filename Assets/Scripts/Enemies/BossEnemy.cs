using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
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

    public float Damage => (damage);

    [Header("Anti-jitter")]
    [SerializeField] private float attackExitBuffer = 0.4f;

    [Header("Charge")]
    [SerializeField] private float chargeTriggerDistanceMax = 10f;
    [SerializeField] private float chargeTriggerDistanceMin = 3f;

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
    private Animator animator;

    private float nextRepathTime;
    private float nextAttackTime;

    private State state = State.Chase;

    private float baseSpeed;
    private float baseStoppingDistance;

    private Vector3 lockedChargeTarget;
    private float chargeStartTime;
    private float nextChargeTime;
    private float recoverEndTime;

    // Animator hashes
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsChargingHash = Animator.StringToHash("IsCharging");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
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

        // Update movement animation (Idle / Walk)
        animator.SetFloat(SpeedHash, agent.velocity.magnitude);

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
        
        // Start charge
        if (dist <= chargeTriggerDistanceMax && dist >= chargeTriggerDistanceMin)
        {
           
            if(Time.time >= nextChargeTime)
            {
                StartCharge();
                return;
                
            }
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

        animator.SetBool("IsCharging", true);

        

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

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance &&
            agent.velocity.sqrMagnitude < 0.01f)
        {
            EndCharge();
        }
    }

    private void EndCharge()
    {
        animator.SetBool("IsCharging", false);

        StopAgent();
        DoSlam();

        nextChargeTime = Time.time + chargeCooldown;

        agent.speed = baseSpeed;
        agent.stoppingDistance = baseStoppingDistance;

        state = State.Recover;
        recoverEndTime = Time.time + postSlamWait;
    }

    private void DoSlam()
    {
        Vector3 center = transform.position;

        if (slamVFX != null)
        {
            ParticleSystem vfx = Instantiate(slamVFX, center, Quaternion.identity);
            vfx.Play();
            Destroy(vfx.gameObject, slamVFXLifetime);
        }

        Collider[] hits = Physics.OverlapSphere(center, slamRadius, ~0, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(playerTag)) continue;

            PlayerHealth hp = hit.GetComponentInParent<PlayerHealth>();
            if (hp != null)
                hp.TakeDamage(slamDamage);
            else
                hit.SendMessageUpwards("TakeDamage", slamDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;


        animator.SetTrigger(AttackHash);

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
        agent.SetDestination(player.position);
    }

    public Transform GetWeakpoint() => weakpoint;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, chargeTriggerDistanceMax);

        Gizmos.color = new Color(1f, 0.7f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, slamRadius);

        if (weakpoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(weakpoint.position, 0.25f);
        }
    }
}
