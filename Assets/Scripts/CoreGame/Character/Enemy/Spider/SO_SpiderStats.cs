using UnityEngine;

[CreateAssetMenu(fileName = "SO_SpiderStats", menuName = "Game/Stats/Spider Stats")]
public class SO_SpiderStats : SO_EnemyStats
{
    [Header("Kite (Giữ khoảng cách & lượn quanh player)")]
    /// <summary>bán kính lý tưởng muốn giữ với player</summary>
    public float preferredDistance = 5f;
    /// <summary>tốc độ khi lượn</summary>
    public float kiteSpeed = 5f;
    /// <summary>bước "nhìn trước" theo tiếp tuyến -&gt; tạo vòng lượn</summary>
    public float strafeLookAhead = 3f;
    /// <summary>đổi chiều lượn ngẫu nhiên: min</summary>
    public float strafeFlipMin = 1.5f;
    /// <summary>đổi chiều lượn ngẫu nhiên: max</summary>
    public float strafeFlipMax = 3.5f;
    /// <summary>tốc độ xoay thủ công để luôn ngoảnh về player</summary>
    public float turnSpeed = 10f;

}