using UnityEngine;

// Gắn cái Script này vào một Hình vuông rỗng (Empty GameObject) ở trên Map
public class SpawnPoint : MonoBehaviour
{
    [Header("Tên Định Danh của Cửa Này (Vd: CuaChinh, CuaSau)")]
    public string spawnID;

    private void Start()
    {
        // Khi map load xong, cục đá này sẽ kiểm tra xem Tên của nó
        // có khớp với Tấm vé (NextSpawnID) mà cổng dịch chuyển truyền sang không.
        if (SceneChanger.NextSpawnID == spawnID)
        {
            // Nếu khớp, Lập tức lôi thằng bé Player tới đúng chỗ này!
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = this.transform.position;
                Debug.Log($"[Spawn] ✅ Player đã được kéo thành công tới Cửa: {spawnID}");
            }
        }
    }
}
