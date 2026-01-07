using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform view; // camera or "eyes" transform (optional)

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8.5f;
    [SerializeField] private float groundFriction = 18f; // higher = snappier stop
    [SerializeField] private float airControl = 0.35f;   // 0..1 (Doom-ish: low)

    [Header("Jump (optional)")]
    [SerializeField] private bool allowJump = true;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float jumpCoolDown = 0.1f;

    private CharacterController controller;
    private Vector3 velocity; // y is vertical velocity, x/z is horizontal velocity
    private bool readyToJump = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (view == null && Camera.main != null)
            view = Camera.main.transform;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        bool grounded = controller.isGrounded;

        // Input (WASD)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Build desired direction (relative to view yaw, like Doom movement)
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        if (view != null)
        {
            Vector3 flatForward = view.forward;
            flatForward.y = 0f;
            flatForward.Normalize();

            Vector3 flatRight = view.right;
            flatRight.y = 0f;
            flatRight.Normalize();

            forward = flatForward;
            right = flatRight;
        }

        Vector3 wishDir = (right * x + forward * z);
        if (wishDir.sqrMagnitude > 1f) wishDir.Normalize();

        // Horizontal velocity handling (snappy Doom-like)
        Vector3 horizontalVel = new Vector3(velocity.x, 0f, velocity.z);

        if (grounded)
        {
            // Apply strong friction when on ground
            horizontalVel = Vector3.MoveTowards(horizontalVel, Vector3.zero, groundFriction * Time.deltaTime);

            // Immediate acceleration toward target speed (arcade-like)
            Vector3 targetVel = wishDir * moveSpeed;
            horizontalVel = targetVel; // Doom-ish: very direct

            // Keep player "stuck" to ground a bit
            if (velocity.y < 0f)
                velocity.y = -2f;

            // Jump
            if (allowJump && readyToJump && Input.GetKeyDown(KeyCode.Space))
            {
                readyToJump = false;
                velocity.y = jumpForce;
                Invoke(nameof(ResetJump), jumpCoolDown);
            }
        }
        else
        {
            // In air: limited control (optional, Doom-ish)
            Vector3 targetVel = wishDir * moveSpeed;
            horizontalVel = Vector3.Lerp(horizontalVel, targetVel, airControl * Time.deltaTime);
        }

        // Gravity
        velocity.y -= gravity * Time.deltaTime;

        // Recompose velocity
        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;

        // Move
        controller.Move(velocity * Time.deltaTime);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
