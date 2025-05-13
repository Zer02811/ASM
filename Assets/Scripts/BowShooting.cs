using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // Cần cho Slider

public class BowShooting : MonoBehaviour
{
    [Header("Arrow - Normal")]
    public GameObject arrowPrefab;
    public float normalArrowSpeed = 20f;

    [Header("Arrow - Special")]
    public GameObject specialArrowPrefab;
    public float specialArrowSpeed = 30f;
    public float specialArrowCooldown = 2f;
    private float specialArrowTimer = 0f;

    [Header("Spawn Point")]
    public Transform arrowSpawnPoint;

    [Header("Aiming")]
    public Camera mainCamera; // Nên kéo Main Camera vào đây
    [SerializeField] private CrosshairController crosshairController; // Kéo CrosshairController vào đây

    [Header("Object Pooling")]
    public int normalPoolSize = 10;
    public int specialPoolSize = 5;
    private List<GameObject> normalArrowPool;
    private List<GameObject> specialArrowPool;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip specialShootSound;
    private AudioSource audioSource;

    [Header("Animation")]
    public Animator animator; // Kéo Animator của nhân vật vào đây (nếu có)
    public string releaseBowAnimationName = "ReleaseBow"; // Tên animation bắn cung
    private readonly int _releaseBowHash = Animator.StringToHash("ReleaseBowTrigger"); // Ví dụ dùng Trigger

    [Header("UI")]
    public Slider arrowCountSlider; // Slider hiển thị số mũi tên thường còn lại

    // Trạng thái aiming
    private bool isAiming = false;

    void Start()
    {
        // Kiểm tra và gán các prefab, transform
        if (arrowPrefab == null) Debug.LogError("Chưa gán Arrow Prefab!");
        if (specialArrowPrefab == null) Debug.LogError("Chưa gán Special Arrow Prefab!");
        if (arrowSpawnPoint == null) Debug.LogError("Chưa gán Arrow Spawn Point!");

        // Lấy Camera chính nếu chưa gán
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("Không tìm thấy Main Camera!");

        // Lấy hoặc thêm AudioSource
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        // Lấy Animator nếu chưa gán
        if (animator == null) animator = GetComponentInParent<Animator>(); // Thử tìm ở parent nếu cần
        if (animator == null) Debug.LogWarning("Chưa gán Animator cho BowShooting.");

        // Kiểm tra CrosshairController
        if (crosshairController == null) Debug.LogWarning("Chưa gán CrosshairController cho BowShooting!");

        // Khởi tạo Object Pooling
        normalArrowPool = InitializePool(arrowPrefab, normalPoolSize);
        specialArrowPool = InitializePool(specialArrowPrefab, specialPoolSize);

        // Khởi tạo Slider UI
        InitializeSlider();

        // Đặt trạng thái aiming và crosshair ban đầu
        SetAiming(false);
    }

    List<GameObject> InitializePool(GameObject prefab, int poolSize)
    {
        List<GameObject> pool = new List<GameObject>();
        if (prefab != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                // Đặt parent cho gọn gàng (tùy chọn)
                // obj.transform.SetParent(this.transform); // Hoặc một transform Pool Container riêng
                pool.Add(obj);
            }
        } else {
             Debug.LogError($"Prefab để tạo pool là null!");
        }
        return pool;
    }

    void InitializeSlider() {
         if (arrowCountSlider != null)
         {
             arrowCountSlider.maxValue = normalPoolSize;
             UpdateArrowCountSlider();
         }
         else
         {
             Debug.LogWarning("Chưa gán Arrow Count Slider!");
             // Không cần disable script nếu chỉ thiếu slider
         }
    }

    void Update()
    {
        // Cập nhật cooldown timer
        if (specialArrowTimer > 0)
        {
            specialArrowTimer -= Time.deltaTime;
             // Cập nhật UI cooldown nếu có
             // crosshairController?.SetCooldownState(true); // Ví dụ
        } else {
             // crosshairController?.SetCooldownState(false); // Ví dụ
        }


        // Xử lý Input ngắm bắn (ví dụ: giữ chuột phải)
        HandleAimingInput();


        // Bắn mũi tên thường (Chuột trái)
        if (Input.GetMouseButtonDown(0))
        {
            ShootNormalArrow();
            TriggerShootAnimation();
        }

        // Bắn mũi tên đặc biệt (Chuột phải khi cooldown xong - hoặc nút khác)
        // Nếu dùng chuột phải để ngắm, cân nhắc dùng nút khác (vd: Q, E, Middle Mouse) để bắn tên đặc biệt
        // Ví dụ: Dùng nút Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
             if (specialArrowTimer <= 0)
             {
                 ShootSpecialArrow();
                 specialArrowTimer = specialArrowCooldown; // Reset cooldown timer
                 TriggerShootAnimation();
             }
             else
             {
                 Debug.Log("Special arrow on cooldown!");
                 // Có thể phát âm thanh báo hiệu cooldown ở đây
             }
        }
    }

    void HandleAimingInput() {
         // Ví dụ dùng chuột phải để ngắm
         if (Input.GetMouseButtonDown(1)) {
             SetAiming(true);
         } else if (Input.GetMouseButtonUp(1)) {
             SetAiming(false);
         }
         // Nếu muốn ngắm kiểu toggle (nhấn 1 lần để bật/tắt) thì logic sẽ khác
    }

    void SetAiming(bool aim) {
         isAiming = aim;
         crosshairController?.SetAimingState(isAiming);
         // Có thể thêm logic khác khi ngắm (vd: đổi FOV camera, thay đổi animation player)
    }

    void ShootNormalArrow()
    {
        Vector3 shootDirection = GetShootDirection();
        GameObject arrow = GetPooledArrow(normalArrowPool);

        if (arrow != null)
        {
            ActivateAndShootArrow(arrow, shootDirection, normalArrowSpeed, shootSound, normalArrowPool); // Truyền pool vào
            UpdateArrowCountSlider();
        }
    }

    void ShootSpecialArrow()
    {
        Vector3 shootDirection = GetShootDirection();
        GameObject arrow = GetPooledArrow(specialArrowPool);

        if (arrow != null)
        {
            ActivateAndShootArrow(arrow, shootDirection, specialArrowSpeed, specialShootSound, specialArrowPool); // Truyền pool vào
            // Không cần cập nhật slider vì nó hiển thị tên thường
        }
    }

    Vector3 GetShootDirection()
    {
        if (mainCamera == null) return arrowSpawnPoint.forward; // Hướng mặc định nếu không có camera

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Tia từ giữa màn hình
        RaycastHit hit;
        Vector3 targetPoint;

        // Ưu tiên bắn trúng điểm raycast hit
        // Thêm LayerMask để chỉ raycast vào các đối tượng mong muốn (vd: Environment, Enemies)
        // LayerMask targetMask = LayerMask.GetMask("Default", "Enemy"); // Ví dụ
        if (Physics.Raycast(ray, out hit, 200f /*, targetMask*/)) // Giới hạn khoảng cách raycast
        {
            targetPoint = hit.point;
        }
        else
        {
            // Nếu không trúng gì, bắn về phía xa theo hướng nhìn của camera
            targetPoint = ray.GetPoint(200f); // Điểm xa trên đường ray
        }

        // Tính hướng từ điểm bắn đến điểm đích
        return (targetPoint - arrowSpawnPoint.position).normalized;
    }

    GameObject GetPooledArrow(List<GameObject> pool)
    {
        if (pool == null) return null;

        foreach (GameObject arrow in pool)
        {
            if (!arrow.activeInHierarchy)
            {
                return arrow;
            }
        }
        // Tùy chọn: Mở rộng pool nếu hết mũi tên
        // Debug.LogWarning("Không đủ mũi tên trong pool! Hãy tăng kích thước pool hoặc cân nhắc mở rộng.");
        // GameObject newArrow = Instantiate(pool == normalArrowPool ? arrowPrefab : specialArrowPrefab);
        // newArrow.SetActive(false);
        // pool.Add(newArrow);
        // return newArrow;

        Debug.LogWarning("Hết mũi tên trong pool!");
        return null; // Hoặc không cho bắn nếu hết
    }

    // Sửa hàm này để nhận List<GameObject> pool làm tham số
    void ActivateAndShootArrow(GameObject arrow, Vector3 shootDirection, float arrowSpeed, AudioClip sound, List<GameObject> pool)
    {
        arrow.transform.position = arrowSpawnPoint.position;
        arrow.transform.rotation = Quaternion.LookRotation(shootDirection);
        arrow.SetActive(true);

        Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();
        if (arrowRb != null)
        {
            arrowRb.linearVelocity = Vector3.zero; // Reset vận tốc cũ trước khi add force
            arrowRb.angularVelocity = Vector3.zero;
            arrowRb.AddForce(shootDirection * arrowSpeed, ForceMode.VelocityChange); // Dùng VelocityChange để bỏ qua khối lượng
        }
        else
        {
            Debug.LogWarning("Mũi tên không có Rigidbody!");
        }

        // Phát âm thanh
        if (sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sound);
        }

        // Bắt đầu coroutine trả mũi tên về pool tương ứng
        StartCoroutine(ReturnArrowToPool(arrow, pool, 5f)); // Trả về sau 5 giây
    }

    void TriggerShootAnimation()
    {
        if (animator != null)
        {
            // Có thể dùng Play theo tên hoặc Trigger theo Hash
            // animator.Play(releaseBowAnimationName);
            animator.SetTrigger(_releaseBowHash); // Ví dụ dùng trigger
        }
    }


    // Coroutine trả mũi tên về pool (đã nhận pool làm tham số)
    System.Collections.IEnumerator ReturnArrowToPool(GameObject arrow, List<GameObject> pool, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Kiểm tra xem mũi tên còn active không trước khi tắt (phòng trường hợp nó đã bị tắt bởi va chạm)
        if (arrow.activeInHierarchy)
        {
             arrow.SetActive(false);
             // Chỉ cập nhật slider nếu là mũi tên thường được trả về
             if (pool == normalArrowPool)
             {
                 UpdateArrowCountSlider();
             }
        }
    }

    // Cập nhật Slider dựa trên số lượng mũi tên thường đang không hoạt động trong pool
    void UpdateArrowCountSlider()
    {
        if (arrowCountSlider == null || normalArrowPool == null) return;

        int inactiveArrows = 0;
        foreach (GameObject arrow in normalArrowPool)
        {
            if (!arrow.activeInHierarchy)
            {
                inactiveArrows++;
            }
        }
        // Giá trị slider là số mũi tên còn lại có thể bắn
        arrowCountSlider.value = inactiveArrows;
    }
}