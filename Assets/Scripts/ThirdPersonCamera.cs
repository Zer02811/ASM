using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;            // Nhân vật mà camera sẽ theo dõi
    public float targetHeight = 1.6f;    // Chiều cao offset của camera so với nhân vật

    [Header("Distance")]
    public float distance = 2.0f;       // Khoảng cách từ camera đến nhân vật
    public float shoulderOffset = 0.5f;  // Offset ngang của camera so với vai

    [Header("Rotation")]
    public float xSpeed = 250.0f;      // Tốc độ xoay ngang
    public float ySpeed = 120.0f;      // Tốc độ xoay dọc
    public float yMinLimit = -80f;     // Góc xoay dọc tối thiểu
    public float yMaxLimit = 80f;      // Góc xoay dọc tối đa

    [Header("Collision")]
    public float collisionRadius = 0.25f; // Bán kính để kiểm tra va chạm
    public LayerMask collisionMask;      // Layer mà camera sẽ va chạm

    private float x = 0.0f;             // Góc xoay ngang hiện tại
    private float y = 0.0f;             // Góc xoay dọc hiện tại
    private float currentShoulderOffset; // Offset vai hiện tại (-1: trái, 1: phải)
    private bool isRightShoulder = true;
    private bool cursorLocked = true;   // Biến để theo dõi trạng thái khóa chuột

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Camera cần một target để theo dõi!");
            enabled = false;
            return;
        }

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        LockCursor(); // Khóa chuột khi bắt đầu
        currentShoulderOffset = isRightShoulder ? shoulderOffset : -shoulderOffset;
    }

    void Update()
    {
        HandleCursorLocking(); // Xử lý khóa/mở khóa chuột
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Xoay camera bằng chuột CHỈ khi chuột đang khóa
        if (cursorLocked)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;

            y = ClampAngle(y, yMinLimit, yMaxLimit);
        }

        // Xử lý chuyển đổi vai
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isRightShoulder = !isRightShoulder;
            currentShoulderOffset = isRightShoulder ? shoulderOffset : -shoulderOffset;
        }

        // Tính toán rotation
        Quaternion rotation = Quaternion.Euler(y, x, 0);

        // Tính toán vị trí mong muốn của camera
        Vector3 targetPosition = target.position + Vector3.up * targetHeight;
        Vector3 right = Quaternion.Euler(0, x, 0) * Vector3.right; // Right vector based on character's rotation
        Vector3 offsetPosition = targetPosition - rotation * Vector3.forward * distance + right * currentShoulderOffset;

        // Xử lý va chạm
        RaycastHit hit;
        Vector3 collisionDirection = offsetPosition - targetPosition;
        float actualDistance = Vector3.Distance(targetPosition, offsetPosition); // Use actual distance
        if (Physics.SphereCast(targetPosition, collisionRadius, collisionDirection.normalized, out hit, actualDistance, collisionMask))
        {
            offsetPosition = targetPosition + collisionDirection.normalized * (hit.distance - collisionRadius);
        }

        // Di chuyển camera
        transform.rotation = rotation;
        transform.position = offsetPosition;

        // Xoay nhân vật theo camera (xoay ngang)
        if (cursorLocked) // Chỉ xoay nhân vật khi chuột đang khóa
        {
            target.rotation = Quaternion.Euler(0, x, 0);
        }
    }

    void HandleCursorLocking()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if (cursorLocked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorLocked = true;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLocked = false;
    }

    // Hàm hỗ trợ để giới hạn góc xoay
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}