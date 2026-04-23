using UnityEngine;

// Gắn Script này vào bất kỳ ROOT GameObject nào mà bạn muốn
// nó SỐNG SÓT khi nhảy qua Map (Scene) mới.
// LƯU Ý: Chỉ gắn vào GameObject ở cấp ROOT (Không có cha) thì mới có hiệu lực!
public class PersistentObject : MonoBehaviour
{
    private void Awake()
    {
        // Tìm tất cả các object cùng tên trong scene
        // (Phòng trường hợp quay lại map cũ và đẻ thêm bản sao)
        string objName = gameObject.name;

        // Tìm xem đã có bản sao nào tồn tại từ scene trước chưa
        PersistentObject[] allPersistent = FindObjectsByType<PersistentObject>(FindObjectsSortMode.None);
        foreach (var other in allPersistent)
        {
            if (other != this && other.gameObject.name == objName)
            {
                // Đã có bản gốc rồi → Xóa cái mới sinh ra để tránh nhân bản
                Destroy(gameObject);
                return;
            }
        }

        // Tuyên bố bất tử — không bao giờ bị xóa khi chuyển scene
        DontDestroyOnLoad(gameObject);
        Debug.Log($"[Persistent] ✅ {objName} sẽ tồn tại xuyên suốt tất cả Scene.");
    }
}
