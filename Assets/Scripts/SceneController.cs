using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc phải có để LoadScene

public class SceneController : MonoBehaviour
{
    // Biến static để giữ duy nhất 1 bộ Essentials xuyên suốt 6 Map
    public static SceneController Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại nhân vật và túi đồ
        }
        else
        {
            Destroy(gameObject); // Xóa bản sao thừa
        }   
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Debug.Log("Đang chuyển đến Map: " + sceneName);
    }
}