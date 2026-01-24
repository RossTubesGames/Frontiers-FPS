using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform yawSource;

    [Header("Gravity Feel")]
    [SerializeField] private float extraFallGravity = 15f; // extra downward accel ONLY while falling (keeps falls snappy personally between 10 and 20 is a smooth gravity feel)

    [Header("Jump Feel")]
    [SerializeField] private float jumpHeight = 1.6f;   // meters: how high the jump should reach
    [SerializeField] private float timeToApex = 0.35f;  // seconds: how fast you reach the top (lower = faster upward snap) so if its 2.5 then it takes 2 and a half seconds to jump all the way up so basically in slowmotion

    // Computed from jumpHeight + timeToApex (so you can control "height" and "up speed" separately)
    private float customGravityY;     // negative value (e.g. -26)
    private float jumpVelocityY;      // initial upward velocity (e.g. 9)

    [Header("Optional Grapple")]
    [SerializeField] private GrapplerGun grappler;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8.5f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundRayLength = 1.1f;

    [Header("Walls Anti-Stick")]
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float wallNormalMinY = 0.2f; // ignore mostly-floor normals
    [SerializeField] private float extraWallSlideDown = 0.0f; // optional: 0..3 to force more slide

    [Header("Jump Cooldown / Count")]
    [SerializeField] private float jumpCoolDown = 0.15f;
    [SerializeField] private int maxJumps = 2;

    [Header("Dash")]
    [SerializeField] private KeyCode dashKey = KeyCode.Z;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;
    [SerializeField] private bool requireGroundedToDash = false;

    private Rigidbody rb;
    private CapsuleCollider col;

    // Footstep speed tracking
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

    // Wall contact info (from collision)
    private bool touchingWall;
    private Vector3 wallNormal;

    private void Awake()
    {
        lastPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // IMPORTANT: we use custom gravity for jump feel, so disable Unity gravity on this Rigidbody.
        // (If you leave Use Gravity on, you'd apply gravity twice.)
        rb.useGravity = false;

        if (yawSource == null) yawSource = transform;
        if (!grappler) grappler = GetComponentInChildren<GrapplerGun>();

        jumpsRemaining = maxJumps;

        // Compute jump parameters from designer-friendly values.
        // Physics:
        //   jumpVelocity = 2H / T
        //   gravity      = -2H / T^2
        // Where H is jumpHeight and T is timeToApex.
        timeToApex = Mathf.Max(0.05f, timeToApex); // safety clamp (avoid divide by zero)
        jumpHeight = Mathf.Max(0.01f, jumpHeight);

        customGravityY = -2f * jumpHeight / (timeToApex * timeToApex);
        jumpVelocityY = 2f * jumpHeight / timeToApex;
    }

    private void Update()
    {
        // simple cooldown counter for footsteps
        cooldown++;
        if (cooldown >= 25)
        {
            cooldown = 0;
        }

        // calculate current speed
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

        if (grounded)
            jumpsRemaining = maxJumps;

        bool isGrappling = (grappler != null && grappler.IsGrappling);

        // Face look yaw
        Vector3 e = transform.eulerAngles;
        e.y = yawSource.eulerAngles.y;
        transform.eulerAngles = e;

        // ------------------------------------------------------------
        // Custom gravity (New)
        // ------------------------------------------------------------
        // We apply our own gravity every physics step.
        // This makes jump "up speed" controllable via timeToApex.
        rb.AddForce(new Vector3(0f, customGravityY, 0f), ForceMode.Acceleration);

        // Extra fall gravity: only when moving downward.
        // Keeps falling snappy without making the rise feel overly heavy.
        if (!grounded && rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Vector3.down * extraFallGravity, ForceMode.Acceleration);
        }

        // Start dash (if queued) - optional: block dash while grappling
        if (dashQueued)
        {
            dashQueued = false;

            // If you want to allow dash while grappling, remove "!isGrappling"
            if (!isGrappling && Time.time >= nextDashTime && (!requireGroundedToDash || grounded))
                StartDash();
        }

        // Build desired horizontal velocity (player input)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 dir = (transform.right * x + transform.forward * z);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        Vector3 wishVel = new Vector3(dir.x * moveSpeed, 0f, dir.z * moveSpeed);

        // If airborne and touching a wall, remove the "into wall" component
        if (!grounded && touchingWall)
        {
            Vector3 n = wallNormal;
            n.y = 0f;
            float nMag = n.magnitude;

            if (nMag > 0.0001f)
            {
                n /= nMag;

                float intoWall = Vector3.Dot(wishVel, -n);
                if (intoWall > 0f)
                    wishVel += n * intoWall;
            }

            if (extraWallSlideDown > 0f)
            {
                Vector3 vNow = rb.linearVelocity;
                rb.linearVelocity = new Vector3(vNow.x, vNow.y - extraWallSlideDown, vNow.z);
            }
        }

        // If dashing, override horizontal movement (still apply anti-stick)
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
                return;
            }
        }

        // Apply movement:
        // If grappling, do NOT overwrite horizontal velocity (grapple controls it)
        if (!isGrappling)
        {
            Vector3 v = rb.linearVelocity;
            rb.linearVelocity = new Vector3(wishVel.x, v.y, wishVel.z);
        }

        // ------------------------------------------------------------
        // Jump (Updated)
        // ------------------------------------------------------------
        // Instead of adding an arbitrary force, we set the upward velocity
        // to a computed value so we control:
        // - jumpHeight (meters)
        // - timeToApex (seconds) = how fast you rise
        if (jumpQueued && jumpsRemaining > 0 && Time.time >= nextJumpTime)
        {
            jumpQueued = false;
            jumpsRemaining--;
            nextJumpTime = Time.time + jumpCoolDown;

            Vector3 v = rb.linearVelocity;

            // Preserve horizontal velocity; set vertical velocity directly to our computed jump speed.
            // This makes the jump feel consistent and "snappy" when timeToApex is low.
            rb.linearVelocity = new Vector3(v.x, jumpVelocityY, v.z);
        }
        else
        {
            jumpQueued = false;
        }
    }

    private void StartDash()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 dir = (transform.right * x + transform.forward * z);
        dir.y = 0f;

        // If no input, dash forward (based on yawSource)
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
        // Same jump logic as normal: set vertical velocity to computed value
        if (jumpQueued && jumpsRemaining > 0 && Time.time >= nextJumpTime)
        {
            jumpQueued = false;
            jumpsRemaining--;
            nextJumpTime = Time.time + jumpCoolDown;

            Vector3 v = rb.linearVelocity;
            rb.linearVelocity = new Vector3(v.x, jumpVelocityY, v.z);
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
        if (currentSpeed > 6 && cooldown < 1 && IsGrounded())
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/Walking");
        }
        else
        {
            return;
        }
    }
}
