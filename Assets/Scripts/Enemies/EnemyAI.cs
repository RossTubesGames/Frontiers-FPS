using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Chase")]
    [SerializeField] private float chaseRange = 26f;
    [SerializeField] private float stopDistance = 1.6f;
    [SerializeField] private float repathInterval = 0.2f;

    [Header("Melee")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1.0f;

    private NavMeshAgent agent;
    private Animator animator;

    private float nextRepathTime;
    private float nextAttackTime;

    // Animator hashes 
    private static readonly int DistanceHash = Animator.StringToHash("DistanceToTarget");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
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

        agent.stoppingDistance = stopDistance;
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Update animator distance
        animator.SetFloat(DistanceHash, dist);

        if (dist <= chaseRange)
        {
            if (Time.time >= nextRepathTime)
            {
                nextRepathTime = Time.time + repathInterval;
                agent.SetDestination(player.position);
            }

            if (dist <= attackRange)
            {
                agent.isStopped = true;
                animator.SetBool(IsMovingHash, false);

                TryAttack();
            }
            else
            {
                agent.isStopped = false;
                animator.SetBool(IsMovingHash, true);
            }
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool(IsMovingHash, false);
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        // Trigger attack animation
        animator.SetTrigger(AttackHash);

        // Damage application
        PlayerHealth hp = player.GetComponentInParent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            return;
        }

        player.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }
}
