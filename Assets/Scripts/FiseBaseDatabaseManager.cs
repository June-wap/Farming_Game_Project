using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

// FiseBaseDatabaseManager: Lớp kết nối và giao tiếp với Firebase Realtime Database.
// Cung cấp các thao tác: GHI (Write) và ĐỌC (Read) dữ liệu theo path tùy ý.
//
// Cấu trúc dữ liệu trên Firebase:
//   Users/
//     └── [deviceId]/
//           └── Maps/
//                 └── [mapName]/   ← JSON của Map
public class FiseBaseDatabaseManager : MonoBehaviour
{
    // ─── SINGLETON ─────────────────────────────────────────────────────────────
    // Truy cập từ bất kỳ script nào: FiseBaseDatabaseManager.Instance.WriteDatabaseToPath(...)
    public static FiseBaseDatabaseManager Instance;

    private DatabaseReference _reference;

    // ID người dùng độc nhất — lấy từ thiết bị (không cần đăng nhập)
    private string _userId;

    // Getter để các script khác (TileMapManager) có thể đọc userId
    public string UserId => _userId;

    private void Awake()
    {
        // Singleton pattern + DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (PlayerPrefs.HasKey("USER_ID"))
        {
            _userId = PlayerPrefs.GetString("USER_ID");
        }
        else
        {
            _userId = SystemInfo.deviceUniqueIdentifier;
            PlayerPrefs.SetString("USER_ID", _userId);
        }

        _reference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("[Firebase] Đã kết nối. UserID: " + _userId);
    }

    // ─── GHI dữ liệu vào path bất kỳ ────────────────────────────────────────
    // path    = đường dẫn Firebase, vd: "Users/abc123/Maps/Home"
    // message = chuỗi JSON cần lưu
    public void WriteDatabaseToPath(string path, string message)
    {
        FirebaseDatabase.DefaultInstance.GetReference(path).SetValueAsync(message)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                    Debug.Log("[Firebase] ✅ Ghi thành công vào: " + path);
                else
                    Debug.LogError("[Firebase] ❌ Lỗi ghi dữ liệu: " + task.Exception);
            });
    }

    // ─── ĐỌC dữ liệu từ path bất kỳ ─────────────────────────────────────────
    // path            = đường dẫn Firebase cần đọc
    // onDataReceived  = callback nhận về chuỗi JSON (hoặc null nếu không có data / lỗi)
    public void ReadDatabaseToPath(string path, Action<string> onDataReceived)
    {
        FirebaseDatabase.DefaultInstance.GetReference(path).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                {
                    string data = task.Result.Value.ToString();
                    Debug.Log("[Firebase] ✅ Đọc được từ: " + path);
                    onDataReceived?.Invoke(data);
                }
                else if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log("[Firebase] Không có dữ liệu tại: " + path);
                    onDataReceived?.Invoke(null);
                }
                else
                {
                    Debug.LogError("[Firebase] ❌ Lỗi đọc dữ liệu: " + task.Exception);
                    onDataReceived?.Invoke(null);
                }
            });
    }

    // ─── XÓA node tại path bất kỳ ────────────────────────────────────────────
    // Dùng khi dev muốn reset dữ liệu 1 map hoặc 1 layer cụ thể.
    // Ví dụ: DeleteDatabasePath("Users/abc123/Maps/Home") → xóa toàn bộ map Home
    public void DeleteDatabasePath(string path)
    {
        FirebaseDatabase.DefaultInstance.GetReference(path).RemoveValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                    Debug.Log("[Firebase] 🗑 Đã xóa node tại: " + path);
                else
                    Debug.LogError("[Firebase] ❌ Lỗi xóa dữ liệu: " + task.Exception);
            });
    }

    // ─── Ghi vào node Users/{userId} (legacy, giữ lại để tương thích) ────────
    public void WriteDatabase(string id, string message)
    {
        _reference.Child("Users").Child(id).SetValueAsync(message)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                    Debug.Log("[Firebase] ✅ Ghi dữ liệu thành công.");
                else
                    Debug.LogError("[Firebase] ❌ Lỗi ghi dữ liệu: " + task.Exception);
            });
    }

    // ─── Đọc từ node Users/{userId} (legacy, giữ lại để tương thích) ─────────
    public void ReadDatabase(string id)
    {
        _reference.Child("Users").Child(id).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                    Debug.Log("[Firebase] Dữ liệu đọc được: " + task.Result.Value);
                else if (task.IsCompleted && !task.IsFaulted)
                    Debug.Log("[Firebase] Không có dữ liệu cho user: " + id);
                else
                    Debug.LogError("[Firebase] ❌ Lỗi đọc dữ liệu: " + task.Exception);
            });
    }
}
