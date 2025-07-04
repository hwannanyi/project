using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float moveSpeed = 10f;
    public float zoomSpeed = 10f;
    public float minFOV = 20f;
    public float maxFOV = 60f;
    public float minY = 1f;
    public float maxY = 20f;

    public float maxXrotate = 89f; // X축 회전
    public float maxYrotate = 45f; // y축 회전

    public static bool isControlMode = false;
    // 카메라 조작 모드 여부

    public static float Yaw { get; private set; }
    float pitch = 0f; // x축 회전
    float yaw = 0f;   // y축 회전
    Camera cam;

    public static float NormalizedYaw
    {
        get
        {
            float y = Yaw % 360f;
            if (y > 180f) y -= 360f;
            if (y < -180f) y += 360f;
            return y;
        }
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        Vector3 angles = transform.eulerAngles;
        pitch = angles.x;
        yaw = angles.y;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

        // C키로 카메라 조정 모드 토글
        if (Input.GetKeyDown(KeyCode.C))
        {
            CameraMoveMode();
        }
        if(Input.GetMouseButtonDown(1) && isControlMode)
        {
            CameraMoveMode();
        }
        if (!isControlMode) return;

        // 마우스 회전
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxXrotate, maxXrotate); // 짐벌락 방지

        
        yaw += mouseX;
        yaw = Mathf.Clamp(yaw, -maxYrotate, maxYrotate); // y축 회전 제한
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxXrotate, maxXrotate); // 짐벌락 방지

        Yaw = yaw; // static 프로퍼티에 저장

        // roll(좌우 기울기) 방지
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // 카메라 이동
        Vector3 move = Vector3.zero;
        Vector3 forward = transform.forward; forward.y = 0; forward.Normalize();
        Vector3 right = transform.right; right.y = 0; right.Normalize();
        if (Input.GetKey(KeyCode.W)) move += forward;
        if (Input.GetKey(KeyCode.S)) move -= forward;
        if (Input.GetKey(KeyCode.A)) move -= right;
        if (Input.GetKey(KeyCode.D)) move += right;
        if (Input.GetKey(KeyCode.Q)) move += Vector3.up;
        if (Input.GetKey(KeyCode.E)) move -= Vector3.up;

        Vector3 nextPos = transform.position + move.normalized * moveSpeed * Time.deltaTime;
        nextPos.y = Mathf.Clamp(nextPos.y, minY, maxY);
        transform.position = nextPos;

        // 카메라 줌 (FOV 조절)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.fieldOfView -= scroll * zoomSpeed;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFOV, maxFOV);
        }
    }

    public void CameraMoveMode()
    {
        isControlMode = !isControlMode;
        Cursor.lockState = isControlMode ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isControlMode;
    }
}