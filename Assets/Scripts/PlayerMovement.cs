using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    // public float rotationSpeed = 500f; // Không dùng trực tiếp nếu xoay theo camera
    // public float smoothRotationTime = 0.12f; // Không dùng trực tiếp nếu xoay theo camera

    [Header("Jump")]
    public float jumpForce = 7f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundMask;

    [Header("Audio")]
    public AudioClip walkSound;
    public AudioClip runSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    private AudioSource audioSource;
    public float stepInterval = 0.5f;
    private float stepTimer = 0f;

    [Header("Animation")]
    public Animator animator;
    // Các hash được lấy từ Constants.cs hoặc định nghĩa trực tiếp ở đây
    // Ví dụ lấy từ Constants.cs:
    // private readonly int _moveSpeedHash = AnimationParameters.MoveSpeed;
    // Hoặc định nghĩa lại ở đây cho rõ ràng context:
    private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int _horizontalHash = Animator.StringToHash("Horizontal");
    private readonly int _verticalHash = Animator.StringToHash("Vertical");
    private readonly int _isJumpingHash = Animator.StringToHash("IsJumping"); // Dùng Trigger

    [Header("Physics Tuning")]
    public float maxVelocityChange = 10.0f; // Giới hạn sự thay đổi vận tốc đột ngột
    public ForceMode movementForceMode = ForceMode.VelocityChange; // Kiểu lực áp dụng

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private float currentSpeed;
    // private float currentRotationVelocity; // Không cần nếu xoay theo camera

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(); // Thử tìm trong children nếu cần
            if (animator == null)
            {
                Debug.LogError("Animator chưa được gán hoặc không tìm thấy cho PlayerMovement script!");
                enabled = false;
                return; // Thêm return để dừng nếu thiếu animator
            }
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        else
        {
            Debug.LogError("Không tìm thấy Rigidbody trên đối tượng này. Di chuyển sẽ không hoạt động!");
             enabled = false; // Tắt script nếu không có Rigidbody
             return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Kiểm tra chạm đất
        CheckGrounded();

        // Lấy input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Tính toán hướng di chuyển (World Space)
        // Note: Chuyển đổi sang local space sẽ thực hiện trong FixedUpdate khi áp dụng lực
        moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Xử lý chạy
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Xử lý nhảy (Input đọc ở Update, thực hiện ở FixedUpdate hoặc Update tùy ý)
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
             // Thực hiện nhảy ngay lập tức trong Update cho cảm giác nhạy hơn
             rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Dùng Impulse cho lực tức thời
             if (animator != null) animator.SetTrigger(_isJumpingHash);
             PlaySound(jumpSound);
        }

        UpdateAnimation(horizontalInput, verticalInput); // Cập nhật animation dựa trên input

        // Xử lý âm thanh đi bộ/chạy
        HandleFootstepSounds(isRunning);
    }

    void FixedUpdate() // Di chuyển vật lý nên thực hiện trong FixedUpdate
    {
        MovePlayer();
    }

    void CheckGrounded()
    {
         bool previouslyGrounded = isGrounded;
         // Vị trí bắt đầu của SphereCast nên ở gần chân nhân vật
         Vector3 sphereCastOrigin = transform.position + Vector3.up * (collisionRadius * 1.1f); // Ví dụ: Nâng lên một chút
         float sphereCastRadius = collisionRadius; // Bán kính kiểm tra
         float maxDistance = groundCheckDistance + (collisionRadius * 1.1f) - collisionRadius; // Khoảng cách từ tâm sphere đến mặt đất

         isGrounded = Physics.SphereCast(sphereCastOrigin, sphereCastRadius, Vector3.down, out RaycastHit hit, maxDistance, groundMask);

         // Phát âm thanh tiếp đất
         if (!previouslyGrounded && isGrounded)
         {
             PlaySound(landSound);
         }
         if (animator != null)
         {
            animator.SetBool(_isGroundedHash, isGrounded);
         }
    }
    // Giả sử bạn có biến bán kính va chạm cho việc kiểm tra đất
    public float collisionRadius = 0.3f; // Ví dụ bán kính CharacterController hoặc CapsuleCollider

    void MovePlayer()
    {
        if (moveDirection == Vector3.zero) return; // Không di chuyển nếu không có input

        // Tính toán vận tốc mục tiêu dựa trên input và hướng local của player
        // TransformDirection chuyển hướng từ local sang world space
        Vector3 targetVelocity = transform.TransformDirection(moveDirection) * currentSpeed;

        // Tính toán sự thay đổi vận tốc cần thiết
        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = (targetVelocity - velocity);

        // Giới hạn sự thay đổi vận tốc (chỉ theo phương ngang)
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0; // Không dùng lực này để ảnh hưởng trọng lực/nhảy

        // Áp dụng lực
        rb.AddForce(velocityChange, movementForceMode);
    }

    void HandleFootstepSounds(bool isRunning)
    {
        if (isGrounded && moveDirection.magnitude > 0.1f && rb.linearVelocity.magnitude > 0.1f) // Chỉ phát tiếng khi thực sự di chuyển trên mặt đất
        {
            stepTimer += Time.deltaTime;
            // Điều chỉnh stepInterval dựa trên tốc độ chạy/đi bộ
            float currentStepInterval = isRunning ? stepInterval / (runSpeed / walkSpeed) : stepInterval;
            if (stepTimer >= currentStepInterval)
            {
                stepTimer = 0f;
                PlayFootstepSound(isRunning);
            }
        }
        else
        {
            stepTimer = 0f; // Reset timer nếu không di chuyển
        }
    }

    private void UpdateAnimation(float horizontalInput, float verticalInput)
    {
        if (animator == null) return;

        // Cập nhật parameter Horizontal và Vertical (dùng cho blend tree chẳng hạn)
        animator.SetFloat(_horizontalHash, horizontalInput);
        animator.SetFloat(_verticalHash, verticalInput);

        // Cập nhật parameter MoveSpeed (độ lớn vận tốc ngang) và IsGrounded
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        animator.SetFloat(_moveSpeedHash, horizontalVelocity.magnitude);
        // animator.SetBool(_isGroundedHash, isGrounded); // Đã chuyển vào CheckGrounded()
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayFootstepSound(bool isRunning)
    {
        AudioClip clipToPlay = isRunning ? runSound : walkSound;
        PlaySound(clipToPlay);
    }

    // Hàm OnCollisionEnter không còn cần thiết để phát âm thanh tiếp đất nếu dùng CheckGrounded()
    // void OnCollisionEnter(Collision collision) { ... }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // Vẽ SphereCast kiểm tra đất để dễ debug
        Vector3 sphereCastOrigin = transform.position + Vector3.up * (collisionRadius * 1.1f);
        float sphereCastRadius = collisionRadius;
        float maxDistance = groundCheckDistance + (collisionRadius * 1.1f) - collisionRadius;
        Gizmos.DrawWireSphere(sphereCastOrigin + Vector3.down * maxDistance, sphereCastRadius);

        // Vẽ hướng di chuyển mong muốn
        Gizmos.color = Color.blue;
        Vector3 targetWorldDirection = transform.TransformDirection(moveDirection);
        Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, transform.position + Vector3.up * 0.1f + targetWorldDirection * 2f);
    }
}