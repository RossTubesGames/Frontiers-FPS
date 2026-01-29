using UnityEngine;

public class GrapplerGun : MonoBehaviour
{
    private enum GrappleMode
    {
        None,
        PullPlayerToPoint,
        PullEnemyToPlayer
    }

    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private LineRenderer grappleLine;

    [Header("Wall Recognize")]
    [SerializeField] private LayerMask wallMask;

    [Header("Enemy Recognize")]
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Shoot")]
    [SerializeField] private float maxDistance = 40f;
    [SerializeField] private KeyCode fireKey = KeyCode.V;

    [Header("Pull Settings")]
    [SerializeField] private float pullForce = 35f;
    [SerializeField] private float maxPullSpeed = 18f;
    [SerializeField] private float stopDistance = 2.0f;
    [SerializeField] private float enemyStopDistance = 2.0f;

    [Header("Line")]
    [SerializeField] private Transform lineStart;
    [SerializeField] private float lineRetractSpeed = 30f;

    private GrappleMode mode = GrappleMode.None;

    private Vector3 grapplePoint;
    private Rigidbody enemyRb;
    private Transform enemyTransform;

    // This is what PlayerMovement will read
    public bool IsGrappling
    {
        get { return mode != GrappleMode.None; }
    }

    private void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!playerRb) playerRb = GetComponentInParent<Rigidbody>();
        if (!grappleLine) grappleLine = GetComponent<LineRenderer>();

        if (!lineStart) lineStart = transform;

        if (grappleLine)
        {
            grappleLine.positionCount = 0;
            grappleLine.enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(fireKey))
        {
            if (IsGrappling)
                StopGrapple();
            else
                TryStartGrapple();
        }

        UpdateLine();
    }

    private void FixedUpdate()
    {
        if (mode == GrappleMode.PullPlayerToPoint)
            PullPlayerToPoint();
        else if (mode == GrappleMode.PullEnemyToPlayer)
            PullEnemyToPlayer();
    }

    private void TryStartGrapple()
    {
        if (!cam || !playerRb) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, maxDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            StopGrapple();
            return;
        }

        // Enemy: pull enemy to player
        if (hit.collider != null && hit.collider.CompareTag(enemyTag))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                enemyRb = rb;
                enemyTransform = hit.collider.transform;
                mode = GrappleMode.PullEnemyToPlayer;

                grapplePoint = hit.point;
                StartLine();
                return;
            }

            StopGrapple();
            return;
        }

        // Wall: pull player to point (only if on wallMask)
        if (((1 << hit.collider.gameObject.layer) & wallMask) != 0)
        {
            grapplePoint = hit.point;
            mode = GrappleMode.PullPlayerToPoint;
            StartLine();
            return;
        }

        StopGrapple();
    }

    private void PullPlayerToPoint()
    {
        Vector3 toPoint = grapplePoint - playerRb.position;
        float dist = toPoint.magnitude;

        if (dist <= stopDistance)
        {
            StopGrapple();
            return;
        }

        Vector3 dir = toPoint / Mathf.Max(dist, 0.0001f);

        playerRb.AddForce(dir * pullForce, ForceMode.Acceleration);
        ClampRigidbodySpeed(playerRb, maxPullSpeed);
    }

    private void PullEnemyToPlayer()
    {
        if (enemyRb == null)
        {
            StopGrapple();
            return;
        }

        Vector3 toPlayer = playerRb.position - enemyRb.position;
        float dist = toPlayer.magnitude;

        if (dist <= enemyStopDistance)
        {
            StopGrapple();
            return;
        }

        Vector3 dir = toPlayer / Mathf.Max(dist, 0.0001f);

        enemyRb.AddForce(dir * pullForce, ForceMode.Acceleration);
        ClampRigidbodySpeed(enemyRb, maxPullSpeed);
    }

    private void ClampRigidbodySpeed(Rigidbody rb, float maxSpeed)
    {
        Vector3 v = rb.linearVelocity;
        float speed = v.magnitude;

        if (speed > maxSpeed)
            rb.linearVelocity = v * (maxSpeed / speed);
    }

    private void StartLine()
    {
        if (!grappleLine) return;

        grappleLine.enabled = true;
        grappleLine.positionCount = 2;

        Vector3 startPos = (lineStart != null) ? lineStart.position : transform.position;
        grappleLine.SetPosition(0, startPos);
        grappleLine.SetPosition(1, startPos);
    }

    private void UpdateLine()
    {
        if (!grappleLine || !grappleLine.enabled) return;

        Vector3 startPos = (lineStart != null) ? lineStart.position : transform.position;
        grappleLine.SetPosition(0, startPos);

        Vector3 endTarget = GetCurrentLineEnd();

        Vector3 currentEnd = grappleLine.GetPosition(1);
        Vector3 newEnd = Vector3.MoveTowards(currentEnd, endTarget, lineRetractSpeed * Time.deltaTime);

        grappleLine.SetPosition(1, newEnd);
    }

    private Vector3 GetCurrentLineEnd()
    {
        if (mode == GrappleMode.PullPlayerToPoint)
            return grapplePoint;

        if (mode == GrappleMode.PullEnemyToPlayer)
        {
            if (enemyRb != null) return enemyRb.worldCenterOfMass;
            if (enemyTransform != null) return enemyTransform.position;
        }

        return (lineStart != null) ? lineStart.position : transform.position;
    }

    public void StopGrapple()
    {
        mode = GrappleMode.None;
        enemyRb = null;
        enemyTransform = null;

        if (grappleLine)
        {
            grappleLine.positionCount = 0;
            grappleLine.enabled = false;
        }
    }
}
