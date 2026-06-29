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

    // Nhện đánh tại chỗ từ vòng kite, KHÔNG lao vào. Vì vậy hãy đặt 'attackRange'
    // (kế thừa từ SO_EnemyStats) >= preferredDistance để nó đánh được từ khoảng cách đang giữ.
    // Đòn đánh cũng nên là clip tại chỗ / tầm xa (không có root motion lao tới).
}