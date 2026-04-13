using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// FireBaseLoginManager: Quản lý Đăng ký / Đăng nhập bằng Firebase Authentication.
//
// Luồng Đăng ký:
//   1. Tạo tài khoản Firebase Auth
//   2. UserDataManager.CreateAndSaveNewProfile() → lưu profile mặc định lên Firebase
//   3. LoadScene("Home")
//
// Luồng Đăng nhập:
//   1. Xác thực email + password với Firebase Auth
//   2. UserDataManager.LoadUserProfile(uid) → tải profile từ Firebase
//   3. Khi load xong → LoadScene("Home")
public class FireBaseLoginManager : MonoBehaviour
{
    // ─── UI ĐĂNG KÝ ──────────────────────────────────────────────────────────
    [Header("Đăng ký")]
    public InputField ipRegisterEmail;
    public InputField ipRegisterPassword;
    public InputField ipRegisterName;       // Tên hiển thị người chơi
    public Button    buttonRegister;

    // ─── UI ĐĂNG NHẬP ────────────────────────────────────────────────────────
    [Header("Đăng nhập")]
    public InputField ipLoginEmail;
    public InputField ipLoginPassword;
    public Button    buttonLogin;

    // ─── CHUYỂN ĐỔI FORM ─────────────────────────────────────────────────────
    [Header("Switch Forms")]
    public Button     buttonMoveToLogin;
    public Button     buttonMoveToRegister;
    public GameObject LoginForm;
    public GameObject RegisterForm;

    // ─── THÔNG BÁO TRẠNG THÁI ────────────────────────────────────────────────
    [Header("Thông báo (tuỳ chọn)")]
    public Text txtStatus;  // Label hiển thị "Đang đăng nhập...", "Sai mật khẩu", v.v.

    private FirebaseAuth _auth;

    // ─── VÒNG ĐỜI ────────────────────────────────────────────────────────────

    private void Start()
    {
        _auth = FirebaseAuth.DefaultInstance;

        buttonRegister.onClick.AddListener(RegisterAccountWithFireBase);
        buttonLogin.onClick.AddListener(LoginAccountWithFireBase);
        buttonMoveToRegister.onClick.AddListener(SwitchForm);
        buttonMoveToLogin.onClick.AddListener(SwitchForm);
    }

    // ─── ĐĂNG KÝ ─────────────────────────────────────────────────────────────

    public void RegisterAccountWithFireBase()
    {
        string email    = ipRegisterEmail.text.Trim();
        string password = ipRegisterPassword.text;
        string name     = ipRegisterName != null ? ipRegisterName.text.Trim() : "Farmer";

        // Kiểm tra đầu vào cơ bản
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetStatus("❌ Vui lòng nhập đầy đủ email và mật khẩu.");
            return;
        }

        SetStatus("⏳ Đang đăng ký...");

        _auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    string error = task.Exception?.GetBaseException().Message ?? "Lỗi không xác định";
                    SetStatus("❌ Đăng ký thất bại: " + error);
                    Debug.LogError("[Login] Đăng ký thất bại: " + task.Exception);
                    return;
                }

                // ✅ Đăng ký thành công → tạo profile mặc định và lưu Firebase
                FirebaseUser newUser = task.Result.User;
                Debug.Log($"[Login] Đăng ký thành công: {newUser.Email} | UID: {newUser.UserId}");

                if (UserDataManager.Instance != null)
                {
                    // Tạo và lưu profile lên Firebase cho user mới
                    UserDataManager.Instance.CreateAndSaveNewProfile(newUser.UserId, name);
                }
                else
                {
                    Debug.LogWarning("[Login] UserDataManager chưa tồn tại!");
                }

                SetStatus("✅ Đăng ký thành công! Đang vào game...");
                // ▶ Bật timer — chỉ bắt đầu đếm sau khi đăng ký/đăng nhập thành công
                TimeManager.Instance?.StartTicking();
                SceneManager.LoadScene("Home");
            });
    }

    // ─── ĐĂNG NHẬP ───────────────────────────────────────────────────────────

    public void LoginAccountWithFireBase()
    {
        string email    = ipLoginEmail.text.Trim();
        string password = ipLoginPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetStatus("❌ Vui lòng nhập đầy đủ email và mật khẩu.");
            return;
        }

        SetStatus("⏳ Đang đăng nhập...");

        _auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    string error = task.Exception?.GetBaseException().Message ?? "Lỗi không xác định";
                    SetStatus("❌ Đăng nhập thất bại: " + error);
                    Debug.LogError("[Login] Đăng nhập thất bại: " + task.Exception);
                    return;
                }

                // ✅ Đăng nhập thành công → load profile từ Firebase TRƯỚC khi LoadScene
                FirebaseUser user = task.Result.User;
                Debug.Log($"[Login] Đăng nhập thành công: {user.Email} | UID: {user.UserId}");
                SetStatus("⏳ Đang tải dữ liệu của bạn...");

                if (UserDataManager.Instance != null)
                {
                    // Load dữ liệu người chơi từ Firebase, sau đó mới chuyển scene
                    UserDataManager.Instance.LoadUserProfile(user.UserId, onLoaded: (success) =>
                    {
                        if (success)
                        {
                            SetStatus("✅ Đăng nhập thành công! Đang vào game...");
                        }
                        else
                        {
                            // Lỗi tải data nhưng vẫn vào game với profile mặc định
                            SetStatus("⚠ Không tải được dữ liệu, vào game với profile mặc định...");
                            Debug.LogWarning("[Login] Tải profile thất bại — dùng profile mặc định.");
                        }
                        // ▶ Bật timer — chỉ bắt đầu đếm sau khi đăng nhập thành công
                        TimeManager.Instance?.StartTicking();
                        // Luôn chuyển scene dù thành công hay thất bại
                        SceneManager.LoadScene("Home");
                    });
                }
                else
                {
                    // Không có UserDataManager → vào thẳng
                    Debug.LogWarning("[Login] UserDataManager chưa tồn tại! Vào game không có data.");
                    TimeManager.Instance?.StartTicking();
                    SceneManager.LoadScene("Home");
                }
            });
    }

    // ─── CHUYỂN ĐỔI FORM ─────────────────────────────────────────────────────

    public void SwitchForm()
    {
        bool loginActive = LoginForm.activeSelf;
        LoginForm.SetActive(!loginActive);
        RegisterForm.SetActive(loginActive);
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    private void SetStatus(string message)
    {
        if (txtStatus != null)
            txtStatus.text = message;
        Debug.Log("[Login] " + message);
    }
}
