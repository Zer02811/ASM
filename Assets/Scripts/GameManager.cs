using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Singleton Pattern ---
    public static GameManager Instance { get; private set; }

    void Awake() // Sử dụng Awake cho Singleton
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Bỏ comment nếu muốn GameManager tồn tại qua các scene
        }
        else
        {
            Debug.LogWarning("Phát hiện GameManager instance khác. Hủy bỏ cái mới.");
            Destroy(gameObject);
            return; // Dừng thực thi Awake/Start nếu bị hủy
        }
    }
    // --- End Singleton

    [Header("UI Panels")]
    public GameObject gameCompletePanel;
    public GameObject gameOverPanel;

    [Header("Game Objective")]
    public int enemiesToDefeat = 10;
    public int enemiesDefeatedCount = 0;
    public Text enemiesDefeatedText; // Hoặc TMPro.TextMeshProUGUI nếu dùng TextMeshPro

    [Header("Game State")]
    private bool isGameOver = false; // Thêm cờ để tránh gọi GameOver/GameComplete nhiều lần

    void Start()
    {
        // Ẩn các panel khi bắt đầu
        if (gameCompletePanel != null) gameCompletePanel.SetActive(false);
        else Debug.LogError("Chưa gán Game Complete Panel!");

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        else Debug.LogError("Chưa gán Game Over Panel!");

        if (enemiesDefeatedText == null)
        {
            Debug.LogWarning("Chưa gán Text hiển thị số địch đã hạ!");
        }

        // Reset trạng thái và UI khi bắt đầu game
        isGameOver = false;
        enemiesDefeatedCount = 0;
        Time.timeScale = 1f; // Đảm bảo thời gian chạy bình thường
        UpdateEnemiesDefeatedText();
        UnlockCursor(false); // Khóa chuột khi bắt đầu game
    }

    public void EnemyDefeated()
    {
        if (isGameOver) return; // Không làm gì nếu game đã kết thúc

        enemiesDefeatedCount++;
        UpdateEnemiesDefeatedText();

        if (enemiesDefeatedCount >= enemiesToDefeat)
        {
            GameComplete();
        }
    }

    public void PlayerDied()
    {
        if (isGameOver) return; // Không làm gì nếu game đã kết thúc
        GameOver();
    }

    void GameComplete()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Game Complete!");
        if (gameCompletePanel != null) gameCompletePanel.SetActive(true);
        PauseGame(true); // Tạm dừng game và mở khóa chuột
    }

    void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Game Over!");
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        PauseGame(true); // Tạm dừng game và mở khóa chuột
    }

    void UpdateEnemiesDefeatedText()
    {
        if (enemiesDefeatedText != null)
        {
            enemiesDefeatedText.text = "Enemies Defeated: " + enemiesDefeatedCount + "/" + enemiesToDefeat;
        }
    }

    // Hàm tiện ích để tạm dừng/tiếp tục game và quản lý chuột
    void PauseGame(bool pause)
    {
         Time.timeScale = pause ? 0f : 1f;
         UnlockCursor(pause);
    }

    // Hàm tiện ích để khóa/mở khóa chuột
    void UnlockCursor(bool unlock)
    {
         Cursor.lockState = unlock ? CursorLockMode.None : CursorLockMode.Locked;
         Cursor.visible = unlock;
    }


    // Các hàm cho Button (UI)
    public void RestartGame()
    {
        Time.timeScale = 1f; // Đảm bảo thời gian không bị dừng
        // Load lại scene hiện tại (GameScene)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Đảm bảo thời gian không bị dừng
        SceneManager.LoadScene(SceneNames.MainMenu); // Sử dụng hằng số từ Constants.cs
    }
}