using UnityEngine;

public class VFXOncollision : MonoBehaviour
{
    // Thay vì prefab, xác định loại VFX cần tạo
    // public VFX_Type vfxType = VFX_Type.GenericHit; // Ví dụ: Dùng enum hoặc string ID
    public GameObject vfxPrefabForThisObject; // Tạm thời vẫn dùng prefab trực tiếp nếu chưa có Pool Manager phức tạp

    [Header("Audio")]
    public AudioClip hitSound;
    private AudioSource audioSource;

    // Tham chiếu đến VFX Pool (cần được gán hoặc tìm thấy)
    // private VFXPool vfxPool;

    void Start()
    {
        // Lấy hoặc thêm AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Lấy tham chiếu đến VFXPool
        // vfxPool = FindFirstObjectByType<VFXPool>(); // Hoặc dùng Singleton: VFXPool.Instance
        // if (vfxPool == null) Debug.LogError("Không tìm thấy VFXPool!");
    }

    private void OnCollisionEnter(Collision collision)
    {
        // --- Logic dùng VFX Pool (ví dụ) ---
        /*
        if (vfxPool != null)
        {
            // Lấy prefab dựa trên loại VFX hoặc logic khác
            // GameObject prefabToUse = vfxPool.GetPrefabForType(vfxType); // Ví dụ

            GameObject vfxInstance = vfxPool.GetPooledVFX(vfxPrefabForThisObject); // Lấy từ pool

            if (vfxInstance != null)
            {
                // Lấy điểm va chạm đầu tiên
                ContactPoint contact = collision.contacts[0];
                vfxInstance.transform.position = contact.point;
                vfxInstance.transform.rotation = Quaternion.LookRotation(contact.normal); // Hướng hiệu ứng ra ngoài bề mặt
                vfxInstance.SetActive(true); // Kích hoạt VFX (script trên VFX sẽ tự trả về pool)
            }
        }
        */

        // --- Logic Instantiate trực tiếp (nếu chưa có Pool) ---
        if (vfxPrefabForThisObject != null)
        {
            ContactPoint contact = collision.contacts[0];
            GameObject vfxInstance = Instantiate(vfxPrefabForThisObject, contact.point, Quaternion.LookRotation(contact.normal));

            // Tự hủy sau một thời gian nếu không dùng pooling và không có script tự hủy trên prefab
            // ParticleSystem ps = vfxInstance.GetComponent<ParticleSystem>();
            // if (ps != null) Destroy(vfxInstance, ps.main.duration + ps.main.startLifetime.constantMax);
            // else Destroy(vfxInstance, 2f); // Hủy sau 2 giây nếu không có ParticleSystem
             // *** Tốt nhất là Prefab VFX nên có script tự hủy hoặc script trả về Pool ***
        }


        // Phát âm thanh va chạm
        if (hitSound != null && audioSource != null)
        {
            // Phát tại vị trí va chạm để có hiệu ứng âm thanh 3D tốt hơn
            // AudioSource.PlayClipAtPoint(hitSound, contact.point); // Cách này tạo AudioSource tạm thời
            // Hoặc nếu AudioSource đã là 3D trên đối tượng này:
             audioSource.PlayOneShot(hitSound);
        }

         // Quan trọng: Nếu đối tượng này (ví dụ: mũi tên) cần bị hủy hoặc trả về pool sau va chạm,
         // logic đó nằm trong script của chính đối tượng đó (Arrow.cs), không phải ở đây.
         // Ví dụ: Trong Arrow.cs -> OnCollisionEnter -> gameObject.SetActive(false);
    }
}