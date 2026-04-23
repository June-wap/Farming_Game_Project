using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Quản lý việc Bật/Tắt Menu Cài Đặt (Settings)
public class SettingsUIManager : MonoBehaviour
{
    [Header("Giao diện UI")]
    [Tooltip("Kéo Panel chứa giao diện Settings vào đây")]
    public GameObject settingsPanel;

    private void Start()
    {
        // Ẩn Menu Cài đặt khi mới vào game
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Cho phép người chơi bấm phím ESC để bật/tắt nhanh Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsMenu();
        }
    }

    // Hàm này sẽ được gọi khi bấm vào Nút Menu (góc trái) hoặc Nút X (đóng menu)
    public void ToggleSettingsMenu()
    {
        if (settingsPanel != null)
        {
            // Nếu đang bật thì tắt, nếu đang tắt thì bật
            bool isActive = !settingsPanel.activeSelf;
            settingsPanel.SetActive(isActive);

            // Dừng thời gian trong game khi mở Menu (Tuỳ chọn)
            if (isActive)
            {
                Time.timeScale = 0f; // Tạm dừng game
                // Nếu bạn có AudioManager thì gọi: AudioManager.Instance.PauseBGM();
            }
            else
            {
                Time.timeScale = 1f; // Tiếp tục game
            }
        }
    }

    // Nút Thoát Game (Về màn hình Đăng Nhập)
    public void QuitGame()
    {
        Debug.Log("Đang thoát về màn hình Đăng Nhập...");
        
        // 1. Tạm thời lưu UserProfile (sẽ cập nhật thêm tính năng lưu toàn bộ sau)
        if (UserDataManager.Instance != null)
        {
            UserDataManager.Instance.SaveUserProfile();
        }
        
        // 2. Bắt buộc xả đông thời gian trước khi chuyển Scene
        Time.timeScale = 1f;
        
        // 3. Tải màn hình đăng nhập
        SceneManager.LoadScene("LoginScreen");
    }

    // ─── MẠNG XÃ HỘI (SOCIAL LINKS) ──────────────────────────────────────────

    public void OpenFacebook()
    {
        Application.OpenURL("https://web.facebook.com/profile.php?id=100007506727190");
        Debug.Log("Mở Facebook...");
    }

    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.com/channels/1342873908621082658/1342873910495940691");
        Debug.Log("Mở Discord...");
    }

    public void OpenGithub()
    {
        Application.OpenURL("https://github.com/June-wap");
        Debug.Log("Mở Github...");
    }
}
