using UnityEngine;
using UnityEngine.InputSystem;

public class playercontroler : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    public Animator animator;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        DontDestroyOnLoad(gameObject);
    }

     private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * speed, moveInput.y * speed);
    }
    private void Update()
    {
        HandleInput();
        if (animator != null)
        {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
        }
    }
    private void HandleInput()
    {
        if (Keyboard.current != null)
        {
            float x = 0;
            float y = 0;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1;

            moveInput = new Vector2(x, y);
            
            if (moveInput.magnitude > 1)
            {
                moveInput.Normalize();
            }
            if (animator != null)
            {
                animator.SetFloat("Horizontal", moveInput.x);
                animator.SetFloat("Vertical", moveInput.y);
                animator.SetFloat("Speed", moveInput.sqrMagnitude);
            }
        }
    }
}