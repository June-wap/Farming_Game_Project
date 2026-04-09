using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

// FiseBaseDatabaseManager: Lớp kết nối và giao tiếp với Firebase Realtime Database.
// Cung cấp 2 thao tác cơ bản: GHI (WriteDatabase) và ĐỌC (ReadDatabase) dữ liệu.
//
// Lưu ý quan trọng về Bất đồng bộ (Async):
// Firebase không trả kết quả ngay lập tức (cần thời gian kết nối internet).
// Do đó ta dùng ContinueWithOnMainThread() — nó sẽ chạy code trong block { }
// KHI NÀO Firebase trả về kết quả, không làm đơ (freeze) game trong lúc chờ.
//
// Cấu trúc dữ liệu trên Firebase:
//   Users/
//     └── [deviceId]/
//           └── [dữ liệu của người chơi này]
public class FiseBaseDatabaseManager : MonoBehaviour
{
    // Reference đến gốc của cây dữ liệu Firebase
    // Từ đây ta có thể đi xuống từng nhánh: .Child("Users").Child(id)...
    private DatabaseReference _reference;

    // ID người dùng độc nhất — lấy từ thiết bị (không cần đăng nhập)
    // SystemInfo.deviceUniqueIdentifier tự động khác nhau trên mỗi máy
    // Tránh tình trạng mọi người cùng ghi đè vào 1 node "User1"
    private string _userId;

    private void Awake()
    {
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

    public void Start()
    {
        // Ví dụ demo: ghi và đọc dữ liệu TilemapDetail khi bắt đầu
        // Sau này sẽ thay bằng logic save/load thực sự của game
        TilemapDetail tilemapDetail = new TilemapDetail(0, 0, TilemapState.Ground);
        WriteDatabase(_userId, tilemapDetail.ToString());
        ReadDatabase(_userId);
    }

    // WriteDatabase: Ghi (cập nhật) giá trị vào node Users/{id} trên Firebase
    // id      = ID người dùng (thường là _userId)
    // message = chuỗi dữ liệu cần lưu (thường là JSON)
    public void WriteDatabase(string id, string message)
    {
        // SetValueAsync: ghi dữ liệu bất đồng bộ — không chặn game thread
        // ContinueWithOnMainThread: callback chạy trên Main Thread khi Firebase phản hồi
        _reference.Child("Users").Child(id).SetValueAsync(message)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    Debug.Log("[Firebase] Ghi dữ liệu thành công.");
                else
                    // task.Exception chứa thông tin lỗi chi tiết
                    Debug.LogError("[Firebase] Lỗi ghi dữ liệu: " + task.Exception);
            });
    }

    public void WriteDatabaseToPath(string path, string message)
{
    FirebaseDatabase.DefaultInstance.GetReference(path).SetValueAsync(message)
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("[Firebase] Ghi dữ liệu thành công vào path: " + path);
            else
                Debug.LogError("[Firebase] Lỗi ghi dữ liệu: " + task.Exception);
        });
}

    // ReadDatabase: Đọc dữ liệu từ node Users/{id} trên Firebase
    // id = ID người dùng cần đọc dữ liệu
    public void ReadDatabase(string id)
    {
        // GetValueAsync: tải dữ liệu bất đồng bộ
        _reference.Child("Users").Child(id).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    // task.Result là DataSnapshot — chứa toàn bộ dữ liệu của node
                    Debug.Log("[Firebase] Dữ liệu đọc được: " + task.Result.Value);
                }
                else if (task.IsCompleted)
                {
                    // IsCompleted = true nhưng Exists = false → node chưa có dữ liệu
                    Debug.Log("[Firebase] Không có dữ liệu cho user: " + id);
                }
                else
                {
                    Debug.LogError("[Firebase] Lỗi đọc dữ liệu: " + task.Exception);
                }
            });
    }
}
