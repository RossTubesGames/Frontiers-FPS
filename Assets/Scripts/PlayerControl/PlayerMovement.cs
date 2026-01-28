using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform yawSource;

    [Header("Gravity Feel")]
    [Tooltip("Extra downward acceleration added only while falling. This is added on top of Physics.gravity magnitude.")]
    [SerializeField] private float extraFallGravity = 15f;

    [Header("Jump Feel")]
    [Tooltip("How high the jump goes (meters).")]
    [SerializeField] private float jumpHeight = 3f;

    [Tooltip("How long it takes to reach the top of the jump (seconds).")]
    [SerializeField] private float timeToApex = 0.4f;

    [Tooltip("Minimum delay between jumps (seconds).")]
    [SerializeField] private float jumpCoolDown = 0.15f;

    [Header("Jump Count")]
    [SerializeField] private int maxJumps = 2;

    [Header("Optional Grapple")]
    [SerializeField] private GrapplerGun grappler;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8.5f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundRayLength = 1.1f;

    [Header("Walls Anti-Stick")]
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float wallNormalMinY = 0.2f;
    [SerializeField] private float extraWallSlideDown = 0.0f;

    [Header("Dash")]
    [SerializeField] private KeyCode dashKey = KeyCode.Z;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;
    [SerializeField] private bool requireGroundedToDash = false;

    private Rigidbody rb;
    private CapsuleCollider col;

    private Vector3 lastPosition;
    private float currentSpeed;
    private float cooldown;

    private int jumpsRemaining;
    private float nextJumpTime;
    private bool jumpQueued;

    private bool dashQueued;
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;
    private Vector3 dashDir;

    private bool touchingWall;
    private Vector3 wallNormal;

    // Computed jump physics
    private float jumpVelocity;     // initial upward velocity to reach jumpHeight in timeToApex
    private float gravityUp;        // upward-phase gravity magnitude (positive number)
    private float gravityDownBase;  // Physics.gravity magnitude + extraFallGravity (positive number)

    private void Awake()
    {
        lastPosition = transform.position;

        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // We control gravity ourselves for consistent jump feel.
        rb.useGravity = false;

        if (yawSource == null) yawSource = transform;
        if (!grappler) grappler = GetComponentInChildren<GrapplerGun>();

        jumpsRemaining = maxJumps;

        RecalculateJumpPhysics();
    }

    private void OnValidate()
    {
        // Keep values sane
        if (jumpHeight < 0.1f) jumpHeight = 0.1f;
        if (timeToApex < 0.05f) timeToApex = 0.05f;
        if (extraFallGravity < 0f) extraFallGravity = 0f;
        if (maxJumps < 1) maxJumps = 1;
        if (jumpCoolDown < 0f) jumpCoolDown = 0f;

        RecalculateJumpPhysics();
    }

    private void RecalculateJumpPhysics()
    {
        // Kinematics:
        // v0 = 2h / t
        // g  = 2h / t^2
        jumpVelocity = (2f * jumpHeight) / timeToApex;
        gravityUp = (2f * jumpHeight) / (timeToApex * timeToApex);

        // Downward gravity is based on project gravity + extra fall gravity
        gravityDownBase = Mathf.Abs(Physics.gravity.y) + extraFallGravity;
    }

    private void Update()
    {
        cooldown++;
        if (cooldown >= 25) cooldown = 0;

        currentSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        if (Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;

        if (Input.GetKeyDown(dashKey))
            dashQueued = true;

        PlayFootstep();
    }

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();
        bool isGrappling = (grappler != null && grappler.IsGrappling);

        if (grounded)
            jumpsRemaining = maxJumps;

        // Face look yaw
        Vector3 e = transform.eulerAngles;
        e.y = yawSource.eulerAngles.y;
        transform.eulerAngles = e;

        // Start dash
        if (dashQueued)
        {
            dashQueued = false;

            if (!isGrappling && Time.time >= nextDashTime && (!requireGroundedToDash || grounded))
                StartDash();
        }

        // Input movement (horizontal)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 dir = (transform.right * x + transform.forward * z);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        Vector3 wishVel = new Vector3(dir.x * moveSpeed, 0f, dir.z * moveSpeed);

        // Wall anti-stick (air only)
        if (!grounded && touchingWall)
        {
            Vector3 n = wallNormal;
            n.y = 0f;

            float nMag = n.magnitude;
            if (nMag > 0.0001f)
            {
                n /= nMag;
                float intoWall = Vector3.Dot(wishVel, -n);
                if (intoWall > 0f) wishVel += n * intoWall;
            }

            if (extraWallSlideDown > 0f)
            {
                Vector3 vNow = rb.linearVelocity;
                rb.linearVelocity = new Vector3(vNow.x, vNow.y - extraWallSlideDown, vNow.z);
            }
        }

        // Dash override
        if (isDashing)
        {
            if (Time.time >= dashEndTime)
            {
                isDashing = false;
            }
            else
            {
                Vector3 dashVel = new Vector3(dashDir.x * dashSpeed, 0f, dashDir.z * dashSpeed);

                if (!grounded && touchingWall)
                {
                    Vector3 n = wallNormal;
                    n.y = 0f;

                    float nMag = n.magnitude;
                    if (nMag > 0.0001f)
                    {
                        n /= nMag;
                        float intoWall = Vector3.Dot(dashVel, -n);
                        if (intoWall > 0f) dashVel += n * intoWall;
                    }
                }

                Vector3 vDash = rb.linearVelocity;
                rb.linearVelocity = new Vector3(dashVel.x, vDash.y, dashVel.z);

                HandleJumpWhileDashing();
                ApplyCustomGravity(grounded);
                return;
            }
        }

        // Apply movement (unless grappling)
        if (!isGrappling)
        {
            Vector3 v = rb.linearVelocity;
            rb.linearVelocity = new Vector3(wishVel.x, v.y, wishVel.z);
        }

        // Jump
        if (jumpQueued && jumpsRemaining > 0 && Time.time >= nextJumpTime)
        {
            jumpQueued = false;
            jumpsRemaining--;
            nextJumpTime = Time.time + jumpCoolDown;

            Vector3 v = rb.linearVelocity;

            // Reset vertical, then set jump velocity deterministically
            rb.linearVelocity = new Vector3(v.x, 0f, v.z);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
        }
        else
        {
            jumpQueued = false;
        }

        ApplyCustomGravity(grounded);
    }

    private void ApplyCustomGravity(bool grounded)
    {
        // If grounded and moving downward, keep the player pinned lightly
        // without accumulating negative velocity.
        Vector3 v = rb.linearVelocity;

        if (grounded && v.y < 0f)
        {
            rb.linearVelocity = new Vector3(v.x, -1f, v.z);
            return;
        }

        float vy = rb.linearVelocity.y;

        // Ascending: use gravityUp. Falling: use base gravity + extraFallGravity.
        float g = (vy > 0f) ? gravityUp : gravityDownBase;

        // Apply as acceleration (mass-independent)
        rb.AddForce(Vector3.down * g, ForceMode.Acceleration);
    }

    private void StartDash()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 dir = (transform.right * x + transform.forward * z);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
        {
            Vector3 fwd = yawSource != null ? yawSource.forward : transform.forward;
            fwd.y = 0f;

            if (fwd.sqrMagnitude < 0.0001f)
                fwd = transform.forward;

            dir = fwd;
        }

        dashDir = dir.normalized;

        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;

        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(0f, v.y, 0f);
    }

    private void HandleJumpWhileDashing()
    {
        if (jumpQueued && jumpsRemaining > 0 && Time.time >= nextJumpTime)
        {
            jumpQueued = false;
            jumpsRemaining--;
            nextJumpTime = Time.time + jumpCoolDown;

            Vector3 v = rb.linearVelocity;
            rb.linearVelocity = new Vector3(v.x, 0f, v.z);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
        }
        else
        {
            jumpQueued = false;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundRayLength, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & wallMask) == 0)
            return;

        Vector3 best = Vector3.zero;
        float bestScore = -1f;

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 n = collision.GetContact(i).normal;

            if (n.y > wallNormalMinY)
                continue;

            Vector3 v = rb.linearVelocity;
            Vector3 vHoriz = new Vector3(v.x, 0f, v.z);
            if (vHoriz.sqrMagnitude < 0.0001f)
                continue;

            float score = Vector3.Dot(n, -vHoriz.normalized);
            if (score > bestScore)
            {
                bestScore = score;
                best = n;
            }
        }

        if (best != Vector3.zero)
        {
            touchingWall = true;
            wallNormal = best;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & wallMask) == 0)
            return;

        touchingWall = false;
        wallNormal = Vector3.zero;
    }

    private void PlayFootstep()
    {
        if (currentSpeed > 6f && cooldown < 1f && IsGrounded())
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/Walking");
        }
    }
}
