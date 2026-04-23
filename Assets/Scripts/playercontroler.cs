using UnityEngine;
using UnityEngine.InputSystem;

// playercontroler: Điều khiển di chuyển nhân vật bằng bàn phím WASD / Arrow Keys.
// Đặt script này lên GameObject Player, cần có Rigidbody2D và Animator.
// DontDestroyOnLoad → nhân vật tồn tại xuyên suốt tất cả các Scene/Map.
public class playercontroler : MonoBehaviour
{
    [SerializeField] private float speed = 5f; // Tốc độ di chuyển — chỉnh trong Inspector
    private Rigidbody2D rb;                    // Component vật lý 2D của nhân vật
    private Vector2 moveInput;                 // Hướng di chuyển hiện tại (x, y) từ -1 đến 1
    private Vector2 lastMoveInput = new Vector2(0, -1); // Hướng nhìn cuối cùng (mặc định nhìn xuống)
    public Animator animator;                  // Animator điều khiển hoạt ảnh nhân vật
    private PlayerFarmControler farmController;

    private void Awake()
    {
        // Lấy Rigidbody2D trên cùng GameObject — phải có component này
        rb = GetComponent<Rigidbody2D>();
        farmController = GetComponent<PlayerFarmControler>();

        // Giữ nhân vật tồn tại khi chuyển Scene
        DontDestroyOnLoad(gameObject);
    }

    // FixedUpdate chạy theo chu kỳ vật lý cố định (mặc định 50Hz)
    // Nên xử lý di chuyển vật lý ở đây thay vì Update để tránh giật
    private void FixedUpdate()
    {
        // Gán vận tốc thẳng cho Rigidbody2D dựa trên input + tốc độ
        rb.linearVelocity = new Vector2(moveInput.x * speed, moveInput.y * speed);
    }

    // Update chạy mỗi frame (thường 60fps)
    private void Update()
    {
        // Nếu đang cuốc đất/trồng cây thì khoá phím di chuyển
        if (farmController != null && farmController.IsBusy)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            // 1. Đọc phím bấm để tính hướng đi
            HandleInput();
        }

        // 2. Cập nhật Animator DUY NHẤT 1 lần/frame tại đây
        if (animator != null)
        {
            // Nhớ hướng nhìn cuối cùng khi dừng lại
            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastMoveInput = moveInput;
            }

            // Gửi toạ độ nhớ được cho Animator, lúc dừng lại nó vẫn biết đang quay mặt hướng nào!
            animator.SetFloat("Horizontal", lastMoveInput.x);
            animator.SetFloat("Vertical", lastMoveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude); 
        }
    }

    // Đọc phím bấm từ bàn phím và tính vector hướng di chuyển
    private void HandleInput()
    {
        // Nếu không có bàn phím kết nối thì bỏ qua
        if (Keyboard.current == null) return;

        float x = 0;
        float y = 0;

        // Kiểm tra phím W/Up → đi lên
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1;
        // Phím S/Down → đi xuống (else if để không thể cùng lúc nhấn cả 2)
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1;

        // Phím A/Left → đi trái
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1;
        // Phím D/Right → đi phải
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1;

        moveInput = new Vector2(x, y);

        // Normalize để tránh đi chéo nhanh hơn đi thẳng
        // Ví dụ: (1,1).magnitude ≈ 1.41 → Normalize về (0.7, 0.7) để tốc độ đều nhau
        if (moveInput.magnitude > 1)
            moveInput.Normalize();
    }
}