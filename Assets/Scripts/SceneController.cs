using UnityEngine;
using UnityEngine.SceneManagement;

// SceneController: Singleton quản lý việc chuyển đổi Scene (Map).
// Đặt script này lên GameObject trong Scene đầu tiên và tick DontDestroyOnLoad.
// Nhờ Singleton, bất kỳ Script nào cũng có thể gọi SceneController.Instance.LoadScene(...)
// mà không cần tìm kiếm object trong Scene.
public class SceneController : MonoBehaviour
{
    // Biến static — trỏ đến instance duy nhất trong toàn bộ game
    // Bất kỳ script nào cũng có thể gọi: SceneController.Instance.LoadScene("Graveyard")
    public static SceneController Instance;

    void Awake()
    {
        // Kiểm tra xem đã có Instance chưa
        if (Instance == null)
        {
            // Chưa có → đây là bản đầu tiên, giữ lại xuyên suốt mọi Scene
            Instance = this;
            DontDestroyOnLoad(gameObject); // GameObject này sẽ không bị xóa khi LoadScene
        }
        else
        {
            // Đã có rồi (do Unity tạo lại khi quay lại Scene đầu) → xóa bản thừa đi
            Destroy(gameObject);
        }   
    }

    // Hàm chuyển Scene — nhận tên Scene làm tham số
    // Ví dụ: SceneController.Instance.LoadScene("Village")
    public void LoadScene(string sceneName)
    {
        // LoadScene của Unity sẽ xóa toàn bộ GameObject trong Scene hiện tại
        // (trừ những object được DontDestroyOnLoad)
        SceneManager.LoadScene(sceneName);
        Debug.Log("Đang chuyển đến Map: " + sceneName);
    }
}