using System.Collections;
using UnityEngine;

public class AnimalWander : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float moveRadius = 2f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;

    [Header("Sleep")]
    [Range(0f, 1f)]
    public float sleepChance = 0.25f; 
    public float minSleepTime = 2f;
    public float maxSleepTime = 5f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private bool isWalking = false;
    private bool isSleeping = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        startPosition = transform.position;

        
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (animator == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Không tìm thấy Animator.");
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Không tìm thấy SpriteRenderer.");
        }

        StartCoroutine(WanderRoutine());
    }

    private void Update()
    {
        if (isWalking && !isSleeping)
        {
            MoveToTarget();
        }
    }

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            // 1. Idle
            isWalking = false;
            isSleeping = false;
            UpdateAnimation();

            float idleTime = Random.Range(minIdleTime, maxIdleTime);
            yield return new WaitForSeconds(idleTime);

            // 2. Random ngủ
            if (Random.value < sleepChance)
            {
                isWalking = false;
                isSleeping = true;
                UpdateAnimation();

                float sleepTime = Random.Range(minSleepTime, maxSleepTime);
                yield return new WaitForSeconds(sleepTime);

                isSleeping = false;
                UpdateAnimation();

        
                yield return new WaitForSeconds(0.3f);
            }

          
            Vector2 randomOffset = Random.insideUnitCircle * moveRadius;
            targetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);

            isSleeping = false;
            isWalking = true;
            UpdateAnimation();

            // 4. Đợi tới khi đi xong
            while (isWalking)
            {
                yield return null;
            }
        }
    }

    private void MoveToTarget()
    {
        Vector3 direction = targetPosition - transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

       
        if (spriteRenderer != null)
        {
            if (direction.x > 0.05f)
                spriteRenderer.flipX = false;
            else if (direction.x < -0.05f)
                spriteRenderer.flipX = true;
        }

        // Nếu tới gần mục tiêu thì dừng
        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            isWalking = false;
            UpdateAnimation();
        }
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isSleeping", isSleeping);
        }
    }

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, moveRadius);
    }
}