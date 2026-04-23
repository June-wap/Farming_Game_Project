using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// SceneController: Singleton quản lý việc chuyển đổi Scene (Map).
// Đặt script này lên GameObject trong Scene đầu tiên và tick DontDestroyOnLoad.
// Nhờ Singleton, bất kỳ Script nào cũng có thể gọi SceneController.Instance.LoadScene(...)
// mà không cần tìm kiếm object trong Scene.
public class SceneController : MonoBehaviour
{
    // Biến static — trỏ đến instance duy nhất trong toàn bộ game
    // Bất kỳ script nào cũng có thể gọi: SceneController.Instance.LoadScene("Graveyard")
    public static SceneController Instance;

    [Header("Hiệu ứng chuyển Map")]
    public float fadeDuration = 0.5f; // Thời gian đen dần (giây)
    private Image fadeImage;
    private bool isFading = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeCanvas(); // Tự động tạo bức màn đen
        }
        else
        {
            Destroy(gameObject);
        }   
    }

    // Tự động tạo UI Canvas phủ đen toàn màn hình (Dev không cần kéo thả bằng tay)
    private void CreateFadeCanvas()
    {
        GameObject canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform); // Gắn vào cục SceneController

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Luôn nằm trên cùng mọi UI khác

        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        
        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Ban đầu trong suốt
        fadeImage.raycastTarget = false; // Không chặn các nút bấm khác

        // Kéo dãn Image phủ kín màn hình
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
    }

    // Hàm gọi khi chạm vào MapExit
    public void LoadScene(string sceneName)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndLoadRoutine(sceneName));
        }
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName)
    {
        isFading = true;
        fadeImage.raycastTarget = true; // Chặn người chơi bấm nút linh tinh khi đang load

        // 1. TỐI DẦN (Fade Out)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 2. CHUYỂN SCENE
        Debug.Log("Đang chuyển đến Map: " + sceneName);
        SceneManager.LoadScene(sceneName);

        // Đợi 1 chút để Scene mới kịp vẽ lên
        yield return new WaitForSeconds(0.1f);

        // 3. SÁNG DẦN LÊN (Fade In)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;
        isFading = false;
    }

    // ─── HIỆU ỨNG NGỦ (Không chuyển Scene) ──────────────────────────────────
    public void StartSleepFade(System.Action onMidSleep)
    {
        if (!isFading)
        {
            StartCoroutine(SleepFadeRoutine(onMidSleep));
        }
    }

    private IEnumerator SleepFadeRoutine(System.Action onMidSleep)
    {
        isFading = true;
        fadeImage.raycastTarget = true;

        // 1. TỐI DẦN
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(timer / fadeDuration));
            yield return null;
        }

        // --- MÀN HÌNH ĐANG ĐEN THUI ---
        // Gọi hàm Callback (Hồi máu, Qua ngày mới) ở ngay lúc này
        onMidSleep?.Invoke();

        // Ngủ thêm 1 giây cho chân thực
        yield return new WaitForSeconds(1.5f);

        // 2. SÁNG DẦN LÊN
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - Mathf.Clamp01(timer / fadeDuration));
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;
        isFading = false;
    }
}