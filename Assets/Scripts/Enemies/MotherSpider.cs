using UnityEngine;
using UnityEngine.AI;

public class MotherSpider : MonoBehaviour
{
    private enum State { Shoot, Relocate, SpawnEggs }

    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private float sightRange = 50f;

    [Header("Movement (Relocate)")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float relocateMinDistance = 8f;
    [SerializeField] private float relocateMaxDistance = 14f;
    [SerializeField] private float relocateStopDistance = 1.2f;
    [SerializeField] private float relocateTimeout = 4f;
    [SerializeField] private float minRelocateTime = 0.5f; // prevents instant state flipping

    [Header("Shooting (Venom Projectile)")]
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private GameObject venomProjectilePrefab;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private float shootDuration = 3f;
    [SerializeField] private float shootInterval = 0.6f;
    [SerializeField] private float aimTurnSpeed = 10f;

    [Header("Egg Spawning")]
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private Transform eggDropPoint;
    [SerializeField] private float spawnEggsDuration = 4f;
    [SerializeField] private float eggDropInterval = 1.2f;

    [Header("Start State")]
    [SerializeField] private State startState = State.Shoot;

    private State state;

    private float stateEndTime;
    private float nextShootTime;
    private float nextEggDropTime;

    private bool relocateDestinationSet;
    private float relocateGiveUpTime;
    private float relocateCanFinishTime; // Time.time must be >= this before Relocate can end
    private Vector3 relocateDest;

    private void Awake()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (shootOrigin == null)
            shootOrigin = transform;

        if (eggDropPoint == null)
            eggDropPoint = transform;
    }

    private void OnEnable()
    {
        EnterState(startState);
    }

    private void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer > sightRange) return;

        switch (state)
        {
            case State.Shoot:
                TickShoot();
                break;
            case State.Relocate:
                TickRelocate();
                break;
            case State.SpawnEggs:
                TickSpawnEggs();
                break;
        }
    }

    private void EnterState(State newState)
    {
        state = newState;

        if (state == State.Shoot)
        {
            stateEndTime = Time.time + shootDuration;
            nextShootTime = Time.time; // shoot immediately
            ResumeAgent(); // you can also StopAgent() if you want her stationary while shooting
        }
        else if (state == State.Relocate)
        {
            relocateDestinationSet = false;
            relocateGiveUpTime = Time.time + relocateTimeout;
            relocateCanFinishTime = Time.time + minRelocateTime;
            ResumeAgent();
        }
        else if (state == State.SpawnEggs)
        {
            stateEndTime = Time.time + spawnEggsDuration;
            nextEggDropTime = Time.time; // drop immediately
            StopAgent();
        }
    }

    private void TickShoot()
    {
        // Turn toward player while shooting (helps projectiles feel intentional)
        AimAtPlayer();

        if (Time.time >= nextShootTime)
        {
            FireVenomProjectile();
            nextShootTime = Time.time + shootInterval;
        }

        if (Time.time >= stateEndTime)
        {
            EnterState(State.Relocate);
        }
    }

    private void TickRelocate()
    {
        if (agent == null)
        {
            EnterState(State.SpawnEggs);
            return;
        }

        if (!relocateDestinationSet)
        {
            Vector3 dest;
            bool found = TryGetRelocatePoint(out dest);

            if (!found)
            {
                EnterState(State.SpawnEggs);
                return;
            }

            relocateDest = dest;

            agent.stoppingDistance = relocateStopDistance;
            agent.SetDestination(relocateDest);

            relocateDestinationSet = true;
        }

        // Timeout safety: if pathing is stuck, continue the cycle anyway
        if (Time.time >= relocateGiveUpTime)
        {
            EnterState(State.SpawnEggs);
            return;
        }

        // Wait until a path is computed
        if (agent.pathPending) return;

        // If no valid path, do not instantly "arrive" - just give up into SpawnEggs after timeout
        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        // Do not allow instant completion
        if (Time.time < relocateCanFinishTime)
            return;

        // Arrived check
        if (agent.remainingDistance <= relocateStopDistance + 0.1f)
        {
            EnterState(State.SpawnEggs);
        }
    }

    private void TickSpawnEggs()
    {
        if (Time.time >= nextEggDropTime)
        {
            DropEgg();
            nextEggDropTime = Time.time + eggDropInterval;
        }

        if (Time.time >= stateEndTime)
        {
            EnterState(State.Shoot);
        }
    }

    private void AimAtPlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, aimTurnSpeed * Time.deltaTime);
    }

    private void FireVenomProjectile()
    {
        if (venomProjectilePrefab == null) return;
        if (shootOrigin == null) return;

        Vector3 dir = (player.position - shootOrigin.position).normalized;

        GameObject proj = Instantiate(venomProjectilePrefab, shootOrigin.position, Quaternion.LookRotation(dir));

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = dir * projectileSpeed;
        }
    }

    private void DropEgg()
    {
        if (eggPrefab == null) return;

        Transform drop = eggDropPoint != null ? eggDropPoint : transform;
        Instantiate(eggPrefab, drop.position, drop.rotation);
    }

    private bool TryGetRelocatePoint(out Vector3 result)
    {
        result = transform.position;

        // This is how wide the spider is allowed to "search" for a new spot.
        // Make it big enough to cover your map area.
        float searchRadius = relocateMaxDistance * 3f;

        // Try a handful of random points; if none work, we fail and you can fallback.
        for (int i = 0; i < 25; i++)
        {
            // Random point around the spider on the XZ plane
            Vector3 randomOffset = Random.insideUnitSphere * searchRadius;
            randomOffset.y = 0f;

            Vector3 candidate = transform.position + randomOffset;

            // Snap candidate onto the NavMesh
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(candidate, out hit, 5f, NavMesh.AllAreas))
                continue;

            // Must be at least 16 units away from the player (or your relocateMinDistance)
            float distToPlayer = Vector3.Distance(hit.position, player.position);
            if (distToPlayer < relocateMinDistance)
                continue;

            result = hit.position;
            return true;
        }

        return false;
    }

    private void StopAgent()
    {
        if (agent == null) return;
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void ResumeAgent()
    {
        if (agent == null) return;
        agent.isStopped = false;
    }
}
