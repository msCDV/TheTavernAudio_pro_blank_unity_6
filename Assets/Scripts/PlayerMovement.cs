using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Ruch")]
    public float moveSpeed = 5f;

    [Header("Myszka")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;
    public float maxLookAngle = 80f;

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * h + transform.forward * v).normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
