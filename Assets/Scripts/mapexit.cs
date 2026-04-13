using UnityEngine;
using UnityEngine.SceneManagement;

// SceneChanger: Đặt script này lên một vùng va chạm vô hình (Collider 2D IsTrigger = true)
// ở rìa bản đồ. Khi Player bước vào vùng đó, game sẽ tự động lưu map và chuyển scene.
public class SceneChanger : MonoBehaviour
{
    [Header("Tên Map đích (vd: Graveyard)")]
    [Tooltip("Phải trùng khớp tên Scene trong File > Build Settings")]
    public string sceneToLoad;

    // OnTriggerEnter2D được Unity gọi khi Player chạm vào vùng trigger này
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chỉ xử lý khi vật thể chạm vào là Player
        if (!collision.CompareTag("Player")) return;

        Debug.Log("[SceneChanger] Player vào cổng → Đang lưu map và chuyển đến: " + sceneToLoad);

        // ─── AUTO-SAVE trước khi rời map ─────────────────────────────────────
        // Tìm TileMapManager trong scene hiện tại và lưu tất cả layer lên Firebase
        TileMapManager tileMapManager = FindObjectOfType<TileMapManager>();
        if (tileMapManager != null)
        {
            tileMapManager.SaveAllLayersToFirebase();
            Debug.Log("[SceneChanger] ✅ Đã lưu tilemap lên Firebase.");
        }
        else
        {
            Debug.LogWarning("[SceneChanger] Không tìm thấy TileMapManager trong scene. Bỏ qua auto-save.");
        }

        // ─── CHUYỂN SCENE ────────────────────────────────────────────────────
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}