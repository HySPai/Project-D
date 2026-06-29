using UnityEngine;

// SpiderKing: tạm thời dùng nguyên hành vi mặc định của EnemyController
// (tiếp cận thẳng -> tấn công khi vào tầm) + chịu đẩy lùi khi trúng đòn.
//
// Để riêng thành một class (thay vì gắn thẳng EnemyController) nhằm:
//  - phân biệt rõ loại prefab / component trên scene,
//  - có sẵn chỗ để sau này thêm chiêu riêng cho "vua nhện" (override BuildStates,
//    thêm state đặc biệt, đỡ đòn theo phase máu...).
//
// Vì không override gì, nó kế thừa BuildStates() mặc định:
//   Idle / Chase / Return / Attack = các Enemy*State gốc.
public class SpiderKingController : EnemyController
{
    // Chưa cần thêm logic. Mọi thứ (chase, attack, knockback) đã có ở lớp cha.
}
