using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform yawSource; // Orientation (yaw) recommended

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8.5f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundRayLength = 1.1f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float jumpCoolDown = 0.15f;

    [Header("Jump Count")]
    [SerializeField] private int maxJumps = 2; // 2 = double jump

    [Header("Dash")]
    [SerializeField] private KeyCode dashKey = KeyCode.Z;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;
    [SerializeField] private bool requireGroundedToDash = false;

    private Rigidbody rb;
    private int jumpsRemaining;
    private float nextJumpTime;
    private bool jumpQueued;

    private bool dashQueued;
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;
    private Vector3 dashDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (yawSource == null) yawSource = transform;

        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;

        if (Input.GetKeyDown(dashKey))
            dashQueued = true;
    }

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();

        // Reset jumps when touching ground
        if (grounded)
            jumpsRemaining = maxJumps;

        // Face look yaw
        Vector3 e = transform.eulerAngles;
        e.y = yawSource.eulerAngles.y;
        transform.eulerAngles = e;

        // Start dash (if queued)
        if (dashQueued)
        {
            dashQueued = false;

            if (Time.time >= nextDashTime && (!requireGroundedToDash || grounded))
            {
                StartDash();
            }
        }

        // If dashing, override horizontal movement
        if (isDashing)
        {
            if (Time.time >= dashEndTime)
            {
                isDashing = false;
            }
            else
            {
                Vector3 vDash = rb.linearVelocity;
                rb.linearVelocity = new Vector3(dashDir.x * dashSpeed, vDash.y, dashDir.z * dashSpeed);
                HandleJumpWhileDashing();
                return;
            }
        }

        // Normal Move
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 dir = (transform.right * x + transform.forward * z);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(dir.x * moveSpeed, v.y, dir.z * moveSpeed);

        // Jump (first from ground, remaining can be in air)
        if (jumpQueued && jumpsRemaining > 0 && Time.time >= nextJumpTime)
        {
            jumpQueued = false;
            jumpsRemaining--;
            nextJumpTime = Time.time + jumpCoolDown;

            v = rb.linearVelocity;
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
        // Look direction (yaw) forward, flattened
        Vector3 fwd = yawSource.forward;
        fwd.y = 0f;

        if (fwd.sqrMagnitude < 0.0001f)
            fwd = transform.forward;

        dashDir = fwd.normalized;

        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;

        // Optional: cancel current horizontal speed so dash is consistent
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(0f, v.y, 0f);
    }

    private void HandleJumpWhileDashing()
    {
        // Optional behavior: keep jump working while dashing
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
}
