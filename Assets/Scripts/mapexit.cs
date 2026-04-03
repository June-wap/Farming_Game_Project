using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện để chuyển cảnh

public class SceneChanger : MonoBehaviour
{
    public string sceneToLoad; // Tên của cảnh muốn chuyển đến

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem có phải nhân vật chạm vào không
        if (collision.CompareTag("Player"))
        {
            // Gọi hàm chuyển cảnh
            SceneManager.LoadScene(sceneToLoad);
        }
    }
    
}