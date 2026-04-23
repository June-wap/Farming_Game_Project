using UnityEngine;
using TMPro;

// GoldUI: Hiển thị số Gold của Player lên UI (Dạng từng ô số)
// Gắn script này vào GameObject "Gold" trong Canvas
public class GoldUI : MonoBehaviour
{
    [Header("Hiển Thị Số Vàng (Từng Ô)")]
    [Tooltip("Kéo các Digit_Slot vào đây theo thứ tự từ TRÁI qua PHẢI (Hàng triệu -> Hàng đơn vị)")]
    public TextMeshProUGUI[] digitTexts;

    [Header("Tuỳ Chỉnh")]
    [Tooltip("Thời gian cập nhật UI (giây). 0.5 = cập nhật 2 lần mỗi giây)")]
    public float refreshInterval = 0.5f;

    private float _timer = 0f;
    private int _lastGold = -1; // Lưu giá trị cũ để tránh cập nhật thừa

    private void Start()
    {
        // Hiển thị ngay lần đầu khi vào game
        UpdateGoldDisplay();
    }

    private void Update()
    {
        // Cập nhật theo chu kỳ để không tốn performance
        _timer += Time.deltaTime;
        if (_timer >= refreshInterval)
        {
            _timer = 0f;
            UpdateGoldDisplay();
        }
    }

    private void UpdateGoldDisplay()
    {
        if (UserDataManager.Instance == null) return;

        // Chỉ cập nhật UI khi số liệu thực sự thay đổi (tối ưu hiệu năng)
        int currentGold = UserDataManager.Instance.Gold;
        if (currentGold != _lastGold)
        {
            _lastGold = currentGold;

            if (digitTexts != null && digitTexts.Length > 0)
            {
                // Ép số vàng thành chuỗi có độ dài bằng chính số lượng ô. 
                // Ví dụ: có 7 ô, vàng = 150 -> Chuỗi format sẽ là "0000150"
                string format = "D" + digitTexts.Length;
                string goldString = currentGold.ToString(format); 

                // Xử lý tràn số: Nếu số vàng thực tế lớn hơn sức chứa của các ô (Ví dụ: 10,000,000)
                // thì giới hạn hiển thị ở mức tối đa (Ví dụ: 9999999)
                if (goldString.Length > digitTexts.Length)
                {
                    goldString = new string('9', digitTexts.Length);
                }

                // Phân phát từng chữ số vào từng ô tương ứng (Từ trái qua phải)
                for (int i = 0; i < digitTexts.Length; i++)
                {
                    if (digitTexts[i] != null)
                    {
                        digitTexts[i].text = goldString[i].ToString();
                    }
                }
            }
        }
    }

    // Hàm public để các script khác gọi cập nhật ngay lập tức
    public void ForceRefresh()
    {
        _lastGold = -1;
        UpdateGoldDisplay();
    }
}
