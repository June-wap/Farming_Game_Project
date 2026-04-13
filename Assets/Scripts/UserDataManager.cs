using UnityEngine;
using Firebase.Auth;

// UserDataManager: Singleton quản lý toàn bộ dữ liệu người chơi trong phiên chơi.
// Lưu trữ UserProfile hiện tại trong RAM.
// Cung cấp API để các script khác đọc/ghi Gold, Money, Name, v.v.
// Tự động đồng bộ với Firebase khi có thay đổi.
//
// Cách dùng:
//   UserDataManager.Instance.AddGold(50);
//   UserDataManager.Instance.SpendMoney(100);
//   int gold = UserDataManager.Instance.Gold;
public class UserDataManager : MonoBehaviour
{
    // ─── SINGLETON ────────────────────────────────────────────────────────────
    public static UserDataManager Instance;

    // ─── DỮ LIỆU NGƯỜI CHƠI ─────────────────────────────────────────────────
    // Profile đang giữ trong RAM — luôn là nguồn dữ liệu duy nhất trong phiên
    private UserProfile _profile;

    // DB property: ưu tiên Singleton Instance, fallback sang FindObjectOfType nếu chưa kịp Awake
    private FiseBaseDatabaseManager DB
    {
        get
        {
            if (FiseBaseDatabaseManager.Instance != null)
                return FiseBaseDatabaseManager.Instance;
            return FindObjectOfType<FiseBaseDatabaseManager>(); // fallback
        }
    }

    // ─── GETTER TIỆN ÍCH (đọc từ RAM, không gọi Firebase) ───────────────────
    public string PlayerName => _profile?.name    ?? "Farmer";
    public int    Gold       => _profile?.gold    ?? 0;
    public int    Money      => _profile?.money   ?? 0;
    public int    CurrentDay => _profile?.currentDay ?? 1;
    public bool   IsLoaded   => _profile != null;

    // ─── VÒNG ĐỜI ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ─── LOAD DỮ LIỆU TỪ FIREBASE ───────────────────────────────────────────

    // Gọi hàm này SAU KHI đăng nhập thành công (từ FireBaseLoginManager)
    // onLoaded: callback báo khi data đã sẵn sàng → có thể LoadScene
    public void LoadUserProfile(string uid, System.Action<bool> onLoaded = null)
    {
        if (DB == null)
        {
            // Không tìm thấy Firebase DB → dùng profile mặc định và tiếp tục vào game
            // Trường hợp này xảy ra khi FiseBaseDatabaseManager chưa được đặt vào scene
            Debug.LogWarning("[UserDataManager] Không tìm thấy FiseBaseDatabaseManager. " +
                             "Đang dùng profile mặc định.");
            _profile = new UserProfile();
            onLoaded?.Invoke(true); // Vẫn tiếp tục vào game
            return;
        }

        string path = $"Users/{uid}/profile";
        Debug.Log("[UserDataManager] Đang tải profile từ: " + path);

        DB.ReadDatabaseToPath(path, (jsonData) =>
        {
            if (!string.IsNullOrEmpty(jsonData))
            {
                // Người chơi đã từng chơi → load profile từ Firebase
                _profile = JsonUtility.FromJson<UserProfile>(jsonData);
                Debug.Log($"[UserDataManager] ✅ Load thành công: {_profile.name} | Gold: {_profile.gold} | Money: {_profile.money}");
                onLoaded?.Invoke(true);
            }
            else
            {
                // Lần đầu đăng nhập → dùng profile mặc định
                Debug.Log("[UserDataManager] Không có profile cũ. Dùng profile mặc định.");
                _profile = new UserProfile();
                onLoaded?.Invoke(true);
            }
        });
    }

    // ─── LƯU DỮ LIỆU LÊN FIREBASE ───────────────────────────────────────────

    // Lưu toàn bộ profile hiện tại lên Firebase
    // Gọi khi: thoát map, khi Gold/Money thay đổi quan trọng
    public void SaveUserProfile()
    {
        if (_profile == null || DB == null) return;

        // Lấy UID của user đang đăng nhập từ Firebase Auth
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("[UserDataManager] Chưa đăng nhập, không thể lưu profile.");
            return;
        }

        string path = $"Users/{user.UserId}/profile";
        DB.WriteDatabaseToPath(path, _profile.ToString());
        Debug.Log($"[UserDataManager] Đang lưu profile lên: {path}");
    }

    // Tạo profile mới cho tài khoản vừa đăng ký và lưu lên Firebase ngay
    public void CreateAndSaveNewProfile(string uid, string displayName = "Farmer")
    {
        _profile = new UserProfile(displayName, gold: 0, money: 500);

        string path = $"Users/{uid}/profile";
        DB.WriteDatabaseToPath(path, _profile.ToString());
        Debug.Log($"[UserDataManager] ✅ Tạo profile mới cho uid: {uid}");
    }

    // ─── CÁC THAO TÁC CẬP NHẬT DỮ LIỆU ─────────────────────────────────────

    // Thêm vàng (khi thu hoạch, hoàn thành nhiệm vụ...)
    public void AddGold(int amount)
    {
        if (_profile == null) return;
        _profile.gold += amount;
        Debug.Log($"[UserDataManager] +{amount} Gold → Tổng: {_profile.gold}");
        SaveUserProfile(); // Auto-save ngay
    }

    // Tiêu vàng (mua hạt giống, công cụ...)
    // Trả về false nếu không đủ vàng
    public bool SpendGold(int amount)
    {
        if (_profile == null || _profile.gold < amount)
        {
            Debug.Log($"[UserDataManager] Không đủ Gold! Cần: {amount}, Có: {_profile?.gold ?? 0}");
            return false;
        }
        _profile.gold -= amount;
        Debug.Log($"[UserDataManager] -{amount} Gold → Còn: {_profile.gold}");
        SaveUserProfile();
        return true;
    }

    // Thêm tiền
    public void AddMoney(int amount)
    {
        if (_profile == null) return;
        _profile.money += amount;
        Debug.Log($"[UserDataManager] +{amount} Money → Tổng: {_profile.money}");
        SaveUserProfile();
    }

    // Tiêu tiền
    public bool SpendMoney(int amount)
    {
        if (_profile == null || _profile.money < amount)
        {
            Debug.Log($"[UserDataManager] Không đủ Money! Cần: {amount}, Có: {_profile?.money ?? 0}");
            return false;
        }
        _profile.money -= amount;
        Debug.Log($"[UserDataManager] -{amount} Money → Còn: {_profile.money}");
        SaveUserProfile();
        return true;
    }

    // Cập nhật ngày hiện tại (gọi từ TimeManager khi day tick)
    public void SetCurrentDay(int day)
    {
        if (_profile == null) return;
        _profile.currentDay = day;
        // Không auto-save ở đây vì gọi quá thường xuyên
        // TimeManager nên gọi trực tiếp SaveUserProfile() khi cần
    }

    // Đồng bộ currentDay lên Firebase (gọi từ TimeManager.OnNewDayStarted)
    public void SyncDay(int day)
    {
        SetCurrentDay(day);
        SaveUserProfile();
        Debug.Log($"[UserDataManager] 📅 Đồng bộ ngày {day} lên Firebase.");
    }
}
