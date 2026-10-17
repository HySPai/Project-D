# Epic: refactor-foundation

> **Status**: Ready
> **Workflow**: minimal (epic tổng hợp từ code review, không qua /create-epics)
> **Created**: 2026-10-17

## Mục tiêu

Dựng nền code cho vũ khí nhiều loại, 4 nguyên tố và metroidvania gating mà không làm hỏng vòng lặp combat đang chạy (Kiếm đánh Seed/Spider/Turret, lăn, lock-on, stamina, tim).

## Nguồn

- Kết quả `/code-review Assets/Scripts` (2026-10-17), thứ tự R1→R6 đã được duyệt.
- `design/game-brief.md`: MVP 1 (combat), 2 (Ngưỡng, chết kiểu Souls), 4 (kỹ năng di chuyển), 6 (quái); mục "Thứ tự xây" bước 1.
- `design/gdd/weapons.md`, `design/gdd/elemental-magic.md`: các điểm gắn cần có sẵn.

## Phạm vi

Chỉ refactor và dựng khung. Không thêm vũ khí, nguyên tố hay kỹ năng mới. Đơn vị sát thương đã chốt: mọi đòn tính bằng điểm, chỉ quy ra tim khi trừ máu người chơi (25 điểm = 1 tim).

## Quy tắc chung cho mọi story

- Vòng lặp hiện có phải chạy y như cũ sau mỗi story: Kiếm chém Seed/Spider/Turret, lăn, lock-on, tim, stamina, chết.
- Sau mỗi thay đổi: refresh và compile qua UnityMCP, đọc console không lỗi, chạy play mode, chụp ảnh vào `production/qa/evidence/`.
- Tham chiếu gán qua `[SerializeField]`; không GetComponent/Find, không Coroutine, không `//`, tween dùng DOTween, async dùng UniTask.
- Sửa scene/prefab/asset thì trình kế hoạch và xin duyệt trước, theo Collaboration Protocol.

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | Kênh sát thương chung | Integration | Ready | N/A |
| 002 | Máy trạng thái hành động Player | Integration | Ready | N/A |
| 003 | Ngưỡng, tiến độ và tàn ý | Integration | Ready | N/A |
| 004 | Tách class lớn | Integration | Ready | N/A |
| 005 | Enemy: mất thăng bằng, hiệu ứng, yếu/kháng | Integration | Ready | N/A |
| 006 | Dọn Coroutine và tween | Integration | Ready | N/A |
| 007 | Dọn GetComponent/Find và hiệu năng | Integration | Ready | N/A |
| 008 | Dọn comment và phần sót lại | Integration | Blocked | N/A |

## Thứ tự

001 → 002 → 004 → 005 là chuỗi chính. 003 độc lập, làm sau 001 theo build order của brief. 006 và 007 làm xen kẽ bất kỳ lúc nào. 008 chờ quyết quy ước comment.
