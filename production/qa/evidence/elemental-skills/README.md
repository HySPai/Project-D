# Evidence: 3 kỹ năng nguyên tố + thanh thế (2026-10-30)

Kiểm trong Play Mode qua UnityMCP, Area A. Player bất tử khi test. Chưa có VFX nên ảnh chỉ cho thấy bố cục; số liệu đo bằng code.

| Kiểm | Kết quả |
|---|---|
| Vòng Nước, 2 quái trong 3m | Mỗi con −20 máu, bị Chậm: agent 2→1.2, 4.5→2.7; animator ×0.6; mana 100→67 |
| Đá Văng, Seed thẳng trước 3.5m, Spider lệch 60° 3m | Seed −35, Spider không trúng |
| Lốc Xoáy, 3 quái quanh tâm | Mỗi con −24 (3 nhát × 8); trong 0.47s Seed bị kéo 1.20→0.28m (2 m/s); Spider King (boss) bị hút, không bị dừng FSM |
| Thanh thế Seed (50), 2 đòn Kiếm 25 | Đòn 1: thế 25, choáng 0.5s như cũ. Đòn 2: vỡ thế, choáng 1.5s, mất thăng bằng, thế đầy lại |
| Đòn phép | Không trừ thế |
| EditMode | 83/83 (thêm PoiseMeterTests 8, SpellTargetingTests 6) |

Ảnh: `01-wind-vortex-pull.png`, `02-poise-break-area-a.png`.

Ghi chú: Spider mất thêm 50 máu do đòn Spider King trúng đồng đội (lỗi cũ, chưa sửa).
