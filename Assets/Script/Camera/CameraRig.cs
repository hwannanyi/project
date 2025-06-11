using UnityEngine;

public class CameraRig : MonoBehaviour
{
    public float rotateSpeed = 5f;
    public Transform cameraTransform; // MainCamera¸¦ ÇÒ´ç

    private float yaw;
    private float pitch;

    void Awake()
    {
        cameraTransform = GetComponent<Transform>();
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = cameraTransform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
    }

    void Update()
    {
        if (!CameraZoom.isControlMode) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * rotateSpeed;
        pitch -= mouseY * rotateSpeed;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}