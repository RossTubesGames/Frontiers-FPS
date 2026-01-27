using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Chase")]
    [SerializeField] private float chaseRange = 25f;
    [SerializeField] private float stopDistance = 1.6f;
    [SerializeField] private float repathInterval = 0.2f;

    [Header("Melee")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1.0f;

    private UnityEngine.AI.NavMeshAgent agent;
    private float nextRepathTime;
    private float nextAttackTime;

    private void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
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
                TryAttack();
            }
            else
            {
                agent.isStopped = false;
            }
        }
        else
        {
            agent.isStopped = true;
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        // If you have a PlayerHealth script, this will work immediately
        PlayerHealth hp = player.GetComponentInParent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            return;
        }

        // Fallback if you do not: add a TakeDamage(float) method on your player
        player.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }
}