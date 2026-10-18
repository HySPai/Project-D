# Test Cases — Combat Animation Set

Ngày: 2026-10-19. Nguồn tiêu chí: `design/gdd/weapons.md` (luật 6b, 6c, 7–11c).
Cột Kết quả: **PASS** là đã quan sát ở lần chạy 2026-10-19; **CHƯA** là chưa chạy.

## Tự động (EditMode, `Assets/Tests/EditMode/Editor/GuardResolverTests.cs`)

14 test, 14 pass. Phủ: chặn 25 sát thương tốn 20 stamina, vỡ thế còn 12.5 sát thương xuyên, stamina 0, đòn từ phía sau, đòn `unblockable`, parry, vũ khí không parry, chặn 50% của Búa, khiên bị đẩy, cung chặn 120°.

## Thủ công (play mode, gọi trực tiếp vào code)

| ID | Kịch bản | Mong đợi | Kết quả |
|---|---|---|---|
| H-01 | Giữ nút đánh ≥ 0.35s trên Kiếm | Vào `Main_Heavy1_Charge` rồi `_Hold`, `IsCharging = true` | PASS |
| H-02 | Thả nút sau khi gồng < 1.2s | Ra `Main_Heavy1_Release` | PASS |
| H-03 | Thả nút sau khi gồng ≥ 1.2s | Ra `Main_Heavy1_ReleaseFull` | PASS |
| H-04 | Đòn nặng trúng quái đang mất thăng bằng | Sát thương 45 × 3 = 135, cờ mất thăng bằng bị xóa | PASS |
| H-05 | Thả nút trước 0.35s | Ra đòn nhẹ | CHƯA (chỉ kiểm đường `Attack()`) |
| H-06 | Bấm lăn khi đang gồng | Hủy gồng và lăn; thả nút sau đó không ra đòn | CHƯA |
| H-07 | Gồng khi đang chạy / trong cửa sổ combo | Ra bản 3 / bản 2 | CHƯA |
| B-01 | Chặn đòn 25 từ phía trước, stamina 100 | Máu không đổi, stamina còn 80 | PASS |
| B-02 | Chặn với stamina 10, đòn 25 | Vỡ thế, nhận 12.5, anim `guard_break` | PASS |
| B-03 | Đòn `unblockable` hoặc từ phía sau | Nhận đủ sát thương | CHƯA (đã có unit test) |
| B-04 | Đang chặn: tốc độ đi | ×0.5, stamina không hồi | PASS (hệ số 0.5); hồi stamina CHƯA |
| B-05 | Giữ chặn | Layer `Guard` chạy `guard_up` rồi blend tree `guard_idle` | PASS (ảnh `combat-guard-idle.png`) |
| P-01 | Nhấn chặn rồi bị đánh trong 0.15s | Không nhận sát thương, stamina −10, slow-mo 0.2× | PASS |
| P-02 | Parry đòn của Seed | Seed choáng và mất thăng bằng | PASS |
| P-03 | Parry khi stamina < 10 | Không mở cửa sổ parry, vẫn chặn thường | CHƯA |
| K-01 | Bấm lăn khi không giữ hướng | Chạy `Backstep_Main`, stamina −10, lùi ≈ 2m | PASS |
| K-02 | Đánh trong cửa sổ lùi bước | Ra `Main_Backstep_Attack_01` | PASS (bật cờ bằng tay; event kiểm bằng danh sách event của clip) |
| K-03 | Lùi bước tạo i-frame | `EnableIsInvulnerable` ở 0.04s, tắt ở 0.38s | CHƯA (chỉ kiểm sự có mặt của event) |
| W-01 | `CycleDebugWeapon` sang Đại kiếm | Đòn nhẹ `Th_Light_01`, đòn nặng `Th_Heavy1_*` chạy | PASS |
| W-02 | Sang Song kiếm | Đòn nhẹ `Dw_Light_01`; không có đòn nặng | CHƯA |

## Chưa phủ (cần người chơi thật)

- Cảm giác trễ khi bấm nhẹ và độ chặt của cửa sổ parry 0.15s.
- Cửa sổ đánh trúng tự đo có khớp mắt nhìn hay không.
- Hiệu năng (chưa đo).
- Input thật: chuột phải, phím Q, vai trái tay cầm. Mọi kiểm thử trên đi qua code, không qua thiết bị nhập.
