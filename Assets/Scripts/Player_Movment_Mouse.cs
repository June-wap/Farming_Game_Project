using UnityEngine;

public class Player_Movment_Mouse : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Rigidbody2D rb;

    public Animator animator;
    private Vector2 targetPosition;
    private Vector2 moveInput;
    private bool hasTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Chuột trái hoặc chuột phải để di chuyển
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
            hasTarget = true;
        }

        if (hasTarget)
        {
            Vector2 currentPosition = transform.position;
            Vector2 direction = targetPosition - currentPosition;

            if (direction.sqrMagnitude < 0.01f)
            {
                // Đến đích
                moveInput = Vector2.zero;
                hasTarget = false;
            }
            else
            {
                moveInput = direction.normalized;
            }
        }
        else
        {
            moveInput = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = moveInput * speed;
        }
    }
}
