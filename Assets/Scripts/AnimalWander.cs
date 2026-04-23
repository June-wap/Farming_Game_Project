using UnityEngine;
using System.Collections;

// AnimalAI: Dieu khien con vat di chuyen ngau nhien va tranh vat can.
// Gan script nay vao GameObject con vat (Ga, Bo, Lon...).
// Yeu cau: Rigidbody2D (Dynamic, Gravity=0), Collider2D.
// Child "EmojiDisplay" can co SpriteRenderer de hien emoji.
public class AnimalAI : MonoBehaviour
{
    [Header("Cau hinh di chuyen")]
    public float maxSpeed          = 1.5f;   // Van toc toi da
    public float steerForce        = 0.3f;   // Do nhay khi doi huong
    public float wallAvoidDistance = 0.6f;   // Khoang cach phat hien vat can
    public LayerMask obstacleLayer;          // Layer cua tuong, cay, hang rao

    [Header("San pham")]
    public string productName  = "Egg";
    public string productDesc  = "Trung ga tuoi.";
    [Header("Thời gian (Tính bằng ngày Ingame)")]
    public int daysToProduce = 3; // Cần 3 ngày no bụng để ra sản phẩm
    public int daysToHungry = 1;  // Mất 1 ngày để đói lại (Mỗi ngày phải cho ăn 1 lần)
    public float emojiDuration = 2.5f; // Thời gian hiện mặt cười khi cho ăn

    [Header("Emoji Sprites")]
    public Sprite emojiHappy;      // Mat cuoi khi duoc cho an
    public Sprite emojiHungry;     // Mat buon khi doi
    public Sprite emojiHeart;      // Tim khi co san pham san sang
    public Sprite emojiSleep;      // Zzz khi nghi

    // ─── INTERNAL ─────────────────────────────────────────────────────────────
    private Vector2        currentVelocity;
    private Rigidbody2D    rb;
    private SpriteRenderer emojiRenderer;   // SpriteRenderer cua child EmojiDisplay

    private int _daysFedCount = 0;   // Đếm số ngày đã được cho ăn
    private bool  _hasProduct = false;

    private bool  _isHungry = false;

    // ─── VONG DOI ─────────────────────────────────────────────────────────────

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"[AnimalAI] {gameObject.name} thieu Rigidbody2D!");
            enabled = false;
            return;
        }

        // Tim SpriteRenderer cua child "EmojiDisplay"
        Transform emojiObj = transform.Find("EmojiDisplay");
        if (emojiObj != null)
            emojiRenderer = emojiObj.GetComponent<SpriteRenderer>();
        else
            Debug.LogWarning($"[AnimalAI] {gameObject.name} khong co child 'EmojiDisplay'!");

        // An emoji luc dau
        HideEmoji();

        Physics2D.queriesStartInColliders = false;
        currentVelocity = Random.insideUnitCircle.normalized * maxSpeed;
    }

    void Start()
    {
        // Đăng ký nhận sự kiện mỗi khi sang ngày mới từ TimeManager
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnNewDayStarted += HandleNewDay;
        }
    }

    void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnNewDayStarted -= HandleNewDay;
        }
    }

    // Xử lý logic khi sang ngày mới
    private void HandleNewDay()
    {
        // 1. Kiểm tra tiến độ đẻ trứng/vắt sữa
        // Chỉ tính ngày đó là "đã cho ăn" nếu nó đang không đói
        if (!_isHungry && !_hasProduct)
        {
            _daysFedCount++;
            Debug.Log($"[AnimalAI] {gameObject.name} đã no bụng { _daysFedCount}/{daysToProduce} ngày.");

            // Đủ 3 ngày no bụng -> Ra sản phẩm
            if (_daysFedCount >= daysToProduce)
            {
                _hasProduct = true;
                _daysFedCount = 0; // Reset lại tiến độ
                ShowEmoji(emojiHeart, 0f); // Hiện tim liên tục
                Debug.Log($"[AnimalAI] {gameObject.name} đã tạo ra sản phẩm: {productName}!");
            }
        }

        // 2. Sang ngày mới thì con vật sẽ bị đói lại (bắt người chơi mỗi ngày phải cho ăn)
        if (!_isHungry)
        {
            _isHungry = true;
            rb.linearVelocity = Vector2.zero; // Dừng lại đòi ăn
            
            // Nếu chưa có sản phẩm (chưa có tim) thì mới hiện mặt đói
            if (!_hasProduct)
            {
                ShowEmoji(emojiHungry, 0f); 
            }
            Debug.Log($"[AnimalAI] {gameObject.name} đang đói ăn vào ngày mới!");
        }
    }

    void FixedUpdate()
    {
        // Khong di chuyen khi doi an (dung lai cho an)
        if (_isHungry) return;

        // Di chuyen ngau nhien
        Vector2 steerDirection = Random.insideUnitCircle * steerForce;
        Vector2 targetVelocity = currentVelocity + steerDirection;
        targetVelocity = AvoidObstacles(targetVelocity);
        targetVelocity = Vector2.ClampMagnitude(targetVelocity, maxSpeed);

        rb.MovePosition(rb.position + targetVelocity * Time.fixedDeltaTime);
        currentVelocity = targetVelocity;

        if (Mathf.Abs(currentVelocity.x) > 0.01f)
            transform.localScale = new Vector3(Mathf.Sign(currentVelocity.x), 1f, 1f);
    }

    void Update()
    {
        // Không cần Update đếm giây nữa, mọi thứ đã chuyển sang đếm ngày trong HandleNewDay()
    }

    // ─── TUONG TAC VOI PLAYER ─────────────────────────────────────────────────

    // Goi tu PlayerFarmControler khi Player nhan [E] gan con vat.
    // Tra ve true neu con vat dang doi an.
    public bool FeedAnimal()
    {
        if (!_isHungry) return false;

        _isHungry = false;
        currentVelocity = Random.insideUnitCircle.normalized * maxSpeed;

        // Hien emoji mat cuoi trong emojiDuration giay
        ShowEmoji(emojiHappy, emojiDuration);
        Debug.Log($"[AnimalAI] {gameObject.name} da duoc cho an!");
        return true;
    }

    // Goi khi Player lay san pham [E].
    public bool CollectProduct()
    {
        if (!_hasProduct) return false;
        _hasProduct = false;
        HideEmoji();
        return true;
    }

    // Tra ve ten san pham (Ket noi vao Inventory moi cua ban sau)
    // TODO: Thay the bang: MyInventory.AddItem(productName, 1);
    public string GetProductName()
    {
        return productName;
    }

    // ─── EMOJI ────────────────────────────────────────────────────────────────

    // Hien emoji len dau con vat.
    // duration = 0 → hien mai; duration > 0 → tu an sau N giay.
    private void ShowEmoji(Sprite sprite, float duration)
    {
        if (emojiRenderer == null || sprite == null) return;

        StopAllCoroutines();
        emojiRenderer.sprite  = sprite;
        emojiRenderer.enabled = true;

        if (duration > 0f)
            StartCoroutine(HideEmojiAfter(duration));
    }

    private void HideEmoji()
    {
        if (emojiRenderer == null) return;
        emojiRenderer.enabled = false;
        emojiRenderer.sprite  = null;
    }

    private IEnumerator HideEmojiAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideEmoji();
    }

    // ─── TRANH VAT CAN ────────────────────────────────────────────────────────

    private Vector2 AvoidObstacles(Vector2 direction)
    {
        Vector2 dir = direction.normalized;
        Vector2[] checkDirs = new Vector2[]
        {
            dir,
            (Quaternion.Euler(0, 0,  45) * dir).normalized,
            (Quaternion.Euler(0, 0, -45) * dir).normalized
        };

        foreach (var checkDir in checkDirs)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, checkDir, wallAvoidDistance, obstacleLayer);

            if (hit.collider != null)
            {
                Debug.DrawRay(transform.position, checkDir * wallAvoidDistance, Color.red);
                Vector2 avoidDir = Vector2.Perpendicular(hit.normal).normalized;
                if (Vector2.Dot(avoidDir, direction) < 0)
                    avoidDir = -avoidDir;
                return avoidDir * maxSpeed;
            }

            Debug.DrawRay(transform.position, checkDir * wallAvoidDistance, Color.green);
        }

        return direction;
    }
}