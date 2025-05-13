using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio; // Cần cho AudioMixer

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;

    // Không cần public string mainMenuSceneName nữa

    [Header("Audio")]
    public AudioMixer audioMixer; // Tham chiếu đến Audio Mixer (Tùy chọn)
    public string volumeParameter = "MasterVolume"; // Tên Parameter Volume trong Mixer

    private bool gameIsPaused = false;

    // Tham chiếu đến GameManager để quản lý trạng thái chuột (hoặc quản lý trực tiếp)
    // [SerializeField] private GameManager gameManager;

    void Start()
    {
        if (pauseMenuUI == null)
        {
            Debug.LogError("Chưa gán Pause Menu UI!");
            enabled = false;
            return;
        }
        pauseMenuUI.SetActive(false); // Đảm bảo menu ẩn khi bắt đầu

        // Lấy GameManager nếu cần
        // if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
        SetAudioMute(false); // Unmute audio
        UnlockCursor(false); // Khóa chuột lại khi chơi tiếp
    }

    void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Dừng thời gian
        gameIsPaused = true;
        SetAudioMute(true); // Mute audio (tùy chọn)
        UnlockCursor(true); // Mở khóa chuột khi pause
    }

    void SetAudioMute(bool mute)
    {
        if (audioMixer != null && !string.IsNullOrEmpty(volumeParameter))
        {
            // Giảm âm lượng xuống rất thấp (-80dB coi như mute)
            audioMixer.SetFloat(volumeParameter, mute ? -80f : 0f); // Giả sử 0dB là âm lượng gốc
        }
    }

    // Hàm tiện ích quản lý chuột (nếu không dùng GameManager)
    void UnlockCursor(bool unlock)
    {
         Cursor.lockState = unlock ? CursorLockMode.None : CursorLockMode.Locked;
         Cursor.visible = unlock;
    }

    // Các hàm cho Button
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Quan trọng: Reset timeScale trước khi chuyển scene
        SceneManager.LoadScene(SceneNames.MainMenu); // Sử dụng hằng số
    }

    public void OpenSettings()
    {
        Debug.Log("Mở màn hình cài đặt từ Pause Menu (chưa triển khai)");
        // TODO: Thêm code để mở màn hình cài đặt (có thể là một panel khác trong PauseMenuUI)
    }

    public void QuitGame()
    {
        Debug.Log("Thoát trò chơi từ Pause Menu");
        Time.timeScale = 1f; // Reset timeScale trước khi thoát

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}