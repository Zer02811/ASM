using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Cần để truy cập các thành phần UI
using System.Collections;

public class MainMenu : MonoBehaviour
{
    // Không cần public string gameSceneName nữa, dùng SceneNames.GameScene

    [Header("UI Elements")]
    [SerializeField] private Button playButton;      // Kéo Button Play vào đây
    [SerializeField] private Button settingsButton;  // Kéo Button Settings vào đây
    [SerializeField] private Button quitButton;      // Kéo Button Quit vào đây

    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioSource audioSource;

    [Header("Load Delay")]
    public float loadSceneDelay = 0.5f; // Thời gian chờ trước khi chuyển scene (tùy chọn)


    private void Start()
    {
        // Lấy hoặc thêm AudioSource
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Gán sự kiện cho các Button
        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        else Debug.LogError("Chưa gán PlayButton trong Inspector!");

        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        else Debug.LogError("Chưa gán SettingsButton trong Inspector!");

        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        else Debug.LogError("Chưa gán QuitButton trong Inspector!");

        // Đảm bảo chuột được mở khóa và hiển thị ở Main Menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void PlayButtonClickSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    public void PlayGame()
    {
        PlayButtonClickSound();
        StartCoroutine(LoadSceneAfterDelay(SceneNames.GameScene, loadSceneDelay)); // Sử dụng hằng số
    }

    public void OpenSettings()
    {
        PlayButtonClickSound();
        Debug.Log("Mở màn hình cài đặt (chưa triển khai)");
        // TODO: Thêm code để mở màn hình cài đặt (ví dụ: bật một Panel khác)
    }

    public void QuitGame()
    {
        PlayButtonClickSound();
        Debug.Log("Thoát trò chơi");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Thoát trong Editor
#else
        Application.Quit(); // Thoát trong bản build
#endif
    }

    // Coroutine để chuyển cảnh sau một khoảng trễ
    IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}