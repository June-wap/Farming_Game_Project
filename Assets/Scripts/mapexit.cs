using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc phải có để LoadScene

public class SceneChanger : MonoBehaviour
{
    [Header("Tên Map tiếng Anh (vd: Graveyard)")]
    public string sceneToLoad; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Co va cham voi: " + collision.name); // Dòng này giúp bạn biết có va chạm hay chưa
    if (collision.CompareTag("Player"))
    {
        Debug.Log("Da nhan dien dung Player, dang tai map: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }   
        if (collision.CompareTag("Player"))
        {
            // Gọi thông qua Instance của SceneController
            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadScene(sceneToLoad);
            }
            else
            {
                // Phòng trường hợp bạn chưa tạo bộ Essentials
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            }
        }
        
    }
}