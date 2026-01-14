using UnityEngine;

public class PlayerMovementCC : MonoBehaviour
{
    public float speed = 5f;
    public float jump = 5f;
    public float gravity = 9.81f;

    private CharacterController cc;
    private Vector3 velocity;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        cc .Move(move * speed * Time.deltaTime);

        if (cc.isGrounded)
        {
            velocity.y = 0f;
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = jump;
                cc.Move(velocity * Time.deltaTime);
            }
        }
    }
}
