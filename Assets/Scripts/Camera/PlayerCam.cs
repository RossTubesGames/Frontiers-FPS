using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 100f;
    public float sensY = 100f;

    [Header("Camera Limits")]
    public float lookUpLimit = -85f;     // How far up you can look
    public float lookDownLimit = 45f;    // How far down you can look

    public Transform orientation;

    float xRotation;
    float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float sensitivityMultiplier = PlayerPrefs.GetFloat("MouseSensitivity", 1f);

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX * sensitivityMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY * sensitivityMultiplier;

        yRotation += mouseX;
        xRotation -= mouseY;

        // IMPORTANT: clamp upward limit first (negative), then downward limit (positive)
        xRotation = Mathf.Clamp(xRotation, lookUpLimit, lookDownLimit);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
