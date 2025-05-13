using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;           // Tham chiếu đến UI Slider
    public Health health;           // Tham chiếu đến Health component
    public Gradient gradient;         // Gradient màu cho HP bar
    public Image fill;               // Image của phần "fill" của Slider

    void Start()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
            if (slider == null)
            {
                Debug.LogError("Không tìm thấy Slider component!");
            }
        }
        if (health == null)
        {
            Debug.LogError("Chưa gán Health component!");
            enabled = false;
            return;
        }

        // Khởi tạo giá trị lớn nhất của Slider
        slider.maxValue = health.maxHealth;
        //Set health hiện tại
        slider.value = health.currentHealth;
        //Set gradient
        if (gradient != null && fill != null)
        {
            fill.color = gradient.Evaluate(1f);
        }
    }

    void Update()
    {
        // Cập nhật giá trị của Slider dựa trên HP hiện tại
        slider.value = health.currentHealth;
        if (gradient != null && fill != null)
        {
            fill.color = gradient.Evaluate(slider.normalizedValue);
        }
    }
}