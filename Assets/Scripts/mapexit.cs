using UnityEngine;
using UnityEngine.SceneManagement;

// SceneChanger: Đặt script này lên một vùng va chạm vô hình (Collider 2D IsTrigger = true)
// ở rìa bản đồ. Khi Player bước vào vùng đó, game sẽ tự động chuyển sang Scene tiếp theo.
public class SceneChanger : MonoBehaviour
{
    [Header("Tên Map tiếng Anh (vd: Graveyard)")]
    // Tên Scene cần chuyển đến — nhập trong Inspector, phải trùng khớp tên trong Build Settings
    public string sceneToLoad; 

    // OnTriggerEnter2D được Unity gọi tự động khi có vật thể khác chạm vào Collider 2D của object này
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chỉ xử lý khi vật thể chạm vào là Player (kiểm tra Tag)
        // Nếu không phải Player (ví dụ NPC hoặc thú cưng chạm vào cổng) thì bỏ qua
        if (!collision.CompareTag("Player")) return;

        Debug.Log("Da nhan dien Player, dang tai map: " + sceneToLoad);

        // Ưu tiên gọi thông qua SceneController Singleton để có thể thêm logic sau này
        // (ví dụ: auto-save trước khi chuyển map)
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(sceneToLoad);
        }
        else
        {
            // Fallback: nếu chưa có Essentials/SceneController thì gọi thẳng Unity API
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}