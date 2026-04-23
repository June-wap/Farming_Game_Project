using UnityEngine;

// Player_Movment_Mouse: Điều khiển nhân vật di chuyển bằng chuột (click chuột để đi tới điểm đó).
// Đây là phương án thay thế cho playercontroler.cs (điều khiển bàn phím).
// Chỉ nên dùng MỘT trong hai script này trên Player.
public class Player_Movment_Mouse : MonoBehaviour
{
    [SerializeField] private float speed = 5f; // Tốc độ di chuyển
    private Rigidbody2D rb;

    public Animator animator;
    private Vector2 targetPosition; // Tọa độ đích (nơi người chơi click chuột)
    private Vector2 moveInput;      // Hướng đang di chuyển mỗi frame
    private Vector2 lastMoveInput = new Vector2(0, -1); // Hướng nhìn cuối cùng
    private bool hasTarget;         // Cờ: đang có đích để đến không?
    private PlayerFarmControler farmController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        farmController = GetComponent<PlayerFarmControler>();
    }

    private void Update()
    {
        // Khi nhấn chuột trái (0) hoặc chuột phải (1) → cập nhật điểm đích
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // Chuyển vị trí chuột trên màn hình (pixel) sang tọa độ thế giới 2D
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
            hasTarget = true; // Đánh dấu là đang có đích
        }

        if (hasTarget)
        {
            Vector2 currentPosition = transform.position;
            Vector2 direction = targetPosition - currentPosition; // Vector từ vị trí hiện tại đến đích

            // Nếu khoảng cách đến đích nhỏ hơn ngưỡng → coi như đã đến nơi
            if (direction.sqrMagnitude < 0.01f)
            {
                moveInput = Vector2.zero; // Đứng im
                hasTarget = false;        // Xóa đích
            }
            else
            {
                // Normalize hướng để tốc độ không phụ thuộc vào khoảng cách
                moveInput = direction.normalized;
            }
        }
        else
        {
            moveInput = Vector2.zero; // Không có đích → đứng im
        }

        // Nếu đang cuốc đất thì khoá di chuyển
        if (farmController != null && farmController.IsBusy)
        {
            moveInput = Vector2.zero;
            hasTarget = false;
        }

        // Cập nhật Animator
        if (animator != null)
        {
            // Nhớ hướng nhìn
            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastMoveInput = moveInput;
            }

            animator.SetFloat("Horizontal", lastMoveInput.x);
            animator.SetFloat("Vertical", lastMoveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
        }
    }

    // Xử lý di chuyển vật lý trong FixedUpdate để mượt mà hơn
    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = moveInput * speed;
        }
    }
}
