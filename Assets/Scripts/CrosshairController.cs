using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public Image crosshairImage; // Kéo Image crosshair vào đây
    public Color normalColor = Color.white;
    public Color aimingColor = Color.green;
    public float normalSize = 20f;
    public float aimingSize = 30f;
    // Thêm các màu/kích thước khác nếu cần (vd: khi cooldown)

    void Start()
    {
        if (crosshairImage == null)
        {
            Debug.LogError("Chưa gán Crosshair Image vào CrosshairController!");
            enabled = false;
            return;
        }
        // Đặt trạng thái mặc định khi bắt đầu
        SetAimingState(false);
    }

    // Hàm này sẽ được gọi bởi script khác (ví dụ: BowShooting)
    public void SetAimingState(bool isAiming)
    {
        if (crosshairImage == null) return;

        if (isAiming)
        {
            crosshairImage.color = aimingColor;
            if (crosshairImage.rectTransform != null) // Kiểm tra null RectTransform
            {
                crosshairImage.rectTransform.sizeDelta = new Vector2(aimingSize, aimingSize);
            }
        }
        else
        {
            crosshairImage.color = normalColor;
            if (crosshairImage.rectTransform != null)
            {
                crosshairImage.rectTransform.sizeDelta = new Vector2(normalSize, normalSize);
            }
        }
    }

    // Ví dụ: Thêm hàm để hiển thị trạng thái cooldown
    public void SetCooldownState(bool isOnCooldown)
    {
        if (crosshairImage == null) return;
        // if (isOnCooldown) {
        //     crosshairImage.color = Color.red; // Hoặc màu khác
        // } else {
        //     // Quay về trạng thái normal hoặc aiming tùy logic
        // }
    }
}