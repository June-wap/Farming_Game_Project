using UnityEngine;

public class AnimalAI : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float maxSpeed = 2f;         // Vận tốc tối đa
    public float steerForce = 0.5f;     // Độ nhạy khi đổi hướng
    public float wallAvoidDistance = 1.5f; // Khoảng cách phát hiện vật cản
    public LayerMask obstacleLayer;     // Layer của tường, cây, đá

    private Vector2 currentVelocity;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Khởi tạo vận tốc ban đầu ngẫu nhiên
        currentVelocity = Random.insideUnitCircle.normalized * maxSpeed;
    }

    void FixedUpdate()
    {
        // B1 & B2: Chọn vectorA ngẫu nhiên và cộng vào vận tốc
        Vector2 steerDirection = Random.insideUnitCircle * steerForce;
        Vector2 targetVelocity = currentVelocity + steerDirection;

        // Cơ chế Raycasting phát hiện vật cản
        targetVelocity = AvoidObstacles(targetVelocity);

        // B3: Đảm bảo độ lớn không vượt quá vận tốc tối đa
        targetVelocity = Vector2.ClampMagnitude(targetVelocity, maxSpeed);

        // B4 & B5: Di chuyển và cập nhật vận tốc mới
        rb.MovePosition(rb.position + targetVelocity * Time.fixedDeltaTime);
        currentVelocity = targetVelocity;

        // Xoay hướng Sprite theo hướng di chuyển (nếu cần)
        if (currentVelocity.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(currentVelocity.x), 1, 1);
    }

    private Vector2 AvoidObstacles(Vector2 direction)
    {
        // Bắn Raycast về phía trước hướng đang đi
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, wallAvoidDistance, obstacleLayer);

        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, direction.normalized * wallAvoidDistance, Color.red);
            
            // Tính toán hướng né: Lấy vector phản xạ hoặc hướng vuông góc với tia va chạm
            Vector2 avoidanceForce = Vector2.Reflect(direction, hit.normal);
            return (direction + avoidanceForce).normalized * maxSpeed;
        }

        Debug.DrawRay(transform.position, direction.normalized * wallAvoidDistance, Color.green);
        return direction;
    }
}