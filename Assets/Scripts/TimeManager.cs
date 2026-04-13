using UnityEngine;
using System;

// TimeManager: Singleton quản lý thời gian ingame.
// Tỷ lệ: 1 giây thực = 1 phút ingame.
// Sau mỗi phút ingame → kích hoạt OnGameMinuteTick (cây lớn lên).
// Sau 24 phút ingame (24 giây thực) = 1 ngày game → kích hoạt OnNewDayStarted (check héo cây).
//
// Cách kết nối:
//   CropManager lắng nghe cả 2 event này để xử lý sinh trưởng và héo cây.
public class TimeManager : MonoBehaviour
{
    // ─── SINGLETON ────────────────────────────────────────────────────────────
    public static TimeManager Instance;

    // ─── SỰ KIỆN (Events) ────────────────────────────────────────────────────
    // Script khác đăng ký lắng nghe: TimeManager.Instance.OnGameMinuteTick += MyMethod;
    public event Action OnGameMinuteTick;   // Mỗi phút ingame (1 giây thực)
    public event Action OnNewDayStarted;    // Mỗi ngày mới (24 phút ingame)

    // ─── TRẠNG THÁI THỜI GIAN ────────────────────────────────────────────────
    [Header("Thông tin thời gian (Read-only)")]
    [SerializeField] private int _currentMinute = 0;  // Phút hiện tại trong ngày (0–23)
    [SerializeField] private int _currentDay = 1;     // Ngày hiện tại

    // Số phút ingame trong 1 ngày (= số giây thực trong 1 ngày game)
    private const int MINUTES_PER_DAY = 24;

    // Accumulator để đếm thời gian thực tế
    private float _timer = 0f;
    private const float SECONDS_PER_GAME_MINUTE = 1f; // 1 giây thực = 1 phút ingame

    // Getter để script khác đọc ngày/phút hiện tại
    public int  CurrentDay    => _currentDay;
    public int  CurrentMinute => _currentMinute;

    // ─── TRẠNG THÁI PAUSE ────────────────────────────────────────────────────
    // Timer MẶC ĐỊNH TẮT — chỉ bật sau khi người chơi đăng nhập thành công.
    // FireBaseLoginManager gọi TimeManager.Instance.StartTicking() trước LoadScene.
    private bool _isPaused = true;

    // Getter để UI hoặc debug đọc trạng thái
    public bool IsRunning => !_isPaused;

    // ─── VÒNG ĐỜI ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Tồn tại xuyên suốt tất cả scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Đăng ký CropManager lắng nghe event ngay khi bắt đầu
        RegisterCropManager();
    }

    private void Update()
    {
        // Không đếm thời gian khi chưa đăng nhập (isPaused = true)
        if (_isPaused) return;

        _timer += Time.deltaTime;

        if (_timer >= SECONDS_PER_GAME_MINUTE)
        {
            _timer -= SECONDS_PER_GAME_MINUTE;
            TickMinute();
        }
    }

    // ─── ĐIỀU KHIỂN TIMER ─────────────────────────────────────────────────────

    // Gọi sau khi đăng nhập thành công (từ FireBaseLoginManager trước LoadScene)
    public void StartTicking()
    {
        _isPaused = false;
        Debug.Log("[TimeManager] ▶ Timer bắt đầu chạy.");
    }

    // Tạm dừng (tuỳ chọn: dùng khi mở menu pause, hay cut-scene)
    public void StopTicking()
    {
        _isPaused = true;
        Debug.Log("[TimeManager] ⏸ Timer tạm dừng.");
    }

    // ─── XỬ LÝ TICK ──────────────────────────────────────────────────────────

    private void TickMinute()
    {
        _currentMinute++;

        // Bắn sự kiện phút — CropManager dùng để tăng tuổi cây
        OnGameMinuteTick?.Invoke();
        Debug.Log($"[TimeManager] Phút ingame: {_currentMinute} | Ngày: {_currentDay}");

        // Sau 24 phút ingame → kết thúc ngày, bắt đầu ngày mới
        if (_currentMinute >= MINUTES_PER_DAY)
        {
            _currentMinute = 0;
            _currentDay++;
            OnNewDayStarted?.Invoke();

            // Đồng bộ ngày mới lên Firebase qua UserDataManager
            UserDataManager.Instance?.SyncDay(_currentDay);

            Debug.Log($"[TimeManager] ☀ NGÀY MỚI BẮT ĐẦU! Ngày {_currentDay}");
        }
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    // Đăng ký CropManager vào event (gọi lại mỗi khi scene mới load)
    private void RegisterCropManager()
    {
        if (CropManager.Instance != null)
        {
            // Tránh đăng ký trùng lặp: hủy trước rồi đăng ký lại
            OnGameMinuteTick -= CropManager.Instance.OnGameMinuteTick;
            OnNewDayStarted  -= CropManager.Instance.OnNewDayStarted;

            OnGameMinuteTick += CropManager.Instance.OnGameMinuteTick;
            OnNewDayStarted  += CropManager.Instance.OnNewDayStarted;

            Debug.Log("[TimeManager] Đã kết nối CropManager vào event.");
        }
        else
        {
            Debug.LogWarning("[TimeManager] CropManager chưa tồn tại khi TimeManager.Start() chạy. Thử lại sau 0.5 giây.");
            Invoke(nameof(RegisterCropManager), 0.5f);
        }
    }
}
