using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform yawSource;

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

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float jumpCoolDown = 0.15f;

    [Header("Jump Count")]
    [SerializeField] private int maxJumps = 2;

    [Header("Dash")]
    [SerializeField] private KeyCode dashKey = KeyCode.Z;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;
    [SerializeField] private bool requireGroundedToDash = false;

    private Rigidbody rb;
    private CapsuleCollider col;
    //these are to calculate how fast the player is moving now which is needed for the footstep sound
    private Vector3 lastPosition;
    private float currentSpeed;
    //cooldown for the footsteps
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
        lastPosition=transform.position;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (yawSource == null) yawSource = transform;

        if (!grappler) grappler = GetComponentInChildren<GrapplerGun>();

        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        cooldown++;
        if (cooldown >= 25)
        {
            cooldown=0;
        }
        //calculate current speed
        currentSpeed=(transform.position-lastPosition).magnitude/Time.deltaTime;
        lastPosition=transform.position;
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

        // Jump
        if (jumpQueued && jumpsRemaining > 0 && Time.time >= nextJumpTime)
        {
            jumpQueued = false;
            jumpsRemaining--;
            nextJumpTime = Time.time + jumpCoolDown;

            Vector3 v = rb.linearVelocity;
            rb.linearVelocity = new Vector3(v.x, 0f, v.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
        else
        {
            jumpQueued = false;
        }
    }

    private void StartDash()
    {
        Vector3 fwd = yawSource.forward;
        fwd.y = 0f;

        if (fwd.sqrMagnitude < 0.0001f)
            fwd = transform.forward;

        dashDir = fwd.normalized;

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
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
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
        if(currentSpeed>6 && cooldown<1 && IsGrounded())
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/Walking");
        }
        else
        {
            return;
        }
    }
}
