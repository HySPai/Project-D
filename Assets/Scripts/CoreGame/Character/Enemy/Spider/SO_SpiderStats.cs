using UnityEngine;

// Stats riêng cho Spider: kế thừa toàn bộ stats enemy (vision/chase/attack/knockback)
// và bổ sung tham số cho lối chơi "giữ khoảng cách + lượn quanh".
[CreateAssetMenu(fileName = "SO_SpiderStats", menuName = "Game/Stats/Spider Stats")]
public class SO_SpiderStats : SO_EnemyStats
{
    [Header("Kite (Giữ khoảng cách & lượn quanh player)")]
    public float preferredDistance = 5f;   // bán kính lý tưởng muốn giữ với player
    public float kiteSpeed = 5f;           // tốc độ khi lượn
    public float strafeLookAhead = 3f;     // bước "nhìn trước" theo tiếp tuyến -> tạo vòng lượn
    public float strafeFlipMin = 1.5f;     // đổi chiều lượn ngẫu nhiên: min
    public float strafeFlipMax = 3.5f;     // đổi chiều lượn ngẫu nhiên: max
    public float turnSpeed = 10f;          // tốc độ xoay thủ công để luôn ngoảnh về player

    // Nhịp chơi: đang cooldown -> lượn giữ khoảng cách ở 'preferredDistance';
    // hết cooldown -> tiến vào tới 'attackRange' (kế thừa từ SO_EnemyStats) rồi đánh,
    // sau đó lại lùi ra lượn tiếp. Nên đặt attackRange < preferredDistance
    // (tầm đánh gần, vòng lượn xa) để thấy rõ chu kỳ "lượn -> lao vào -> lùi ra".
}