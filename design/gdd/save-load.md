# Lưu và tải (Save / Load)

> **Status**: In Design
> **Author**: người dùng + Claude (solo, không có specialist)
> **Last Updated**: 2026-10-18
> **Last Verified**: 2026-10-18
> **Implements Pillar**: không có file pillar ở tier `minimal`; phục vụ "Điểm nghỉ và cái chết kiểu Souls" trong `design/game-brief.md`

> **Workflow**: `minimal`. GDD này là tự nguyện; brief vẫn là tài liệu thiết kế chính. Chỉ có Summary và 5 mục chuẩn. Không có mục Formulas: các con số nằm thẳng trong Core Rules.
> **Specialist**: `systems-designer`, `gameplay-programmer`, `engine-programmer`, `qa-lead` không được hỏi ý kiến (Solo mode). Cần review thủ công trước production.
> **Khả thi kỹ thuật**: NOT SOURCEABLE — không có `docs/engine-reference/unity/modules/core.md`; chưa kiểm với Unity 6000.4.1f1. Cách ghi file sẽ thành ADR.

## Summary

Lưu và tải giữ tiến độ của 3 slot chơi riêng biệt. Mở khóa vĩnh viễn và tài nguyên đang mang được ghi ngay khi xảy ra; vị trí, máu và quái chỉ chốt khi nghỉ ở Ngưỡng. Thoát game ở đâu thì vào lại cũng đứng ở Ngưỡng cuối.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None`
> Category: Persistence (từ `design/gdd/systems-index.md`)

## Overview

Hệ nền (Persistence) ghi và đọc dữ liệu của một lần chơi. Người chơi chỉ chạm tới nó qua menu chọn slot và khi nghỉ ở Ngưỡng. Còn lại nó chạy ngầm, để thoát game hay crash không xóa công sức đã bỏ ra. Thiếu nó thì không có Ngưỡng, không có tàn ý rơi và không có metroidvania quay lại.

## Detailed Design

### Core Rules

1. **3 slot**, chọn ở menu chính. Mỗi slot là một lần chơi độc lập. Cài đặt (âm lượng, haptic, trợ năng) không nằm trong slot mà dùng chung cho mọi slot.
2. **4 loại dữ liệu trong một slot:**
   - **(A) Tiến độ vĩnh viễn:** vũ khí đã có và cấp nâng cấp, vũ khí đang trang bị, nguyên tố đã có, nâng cấp tim tối đa và stamina, kỹ năng di chuyển đã mở. **Ghi ngay khi thay đổi.**
   - **(B) Trạng thái thế giới:** boss đã hạ, lối tắt đã mở, câu đố đã giải, rương đã mở, bí mật đã tìm. Hệ Trạng thái thế giới sở hữu nội dung; hệ này chỉ ghi. **Ghi ngay khi thay đổi.**
   - **(C) Tài nguyên đang mang:** tàn ý, mũi tên, Mảnh Sắt Cũ. **Ghi chậm nhất 1 giây** sau lần thay đổi cuối.
   - **(D) Snapshot Ngưỡng:** Ngưỡng nghỉ gần nhất. **Chỉ ghi khi nghỉ ở Ngưỡng.**
3. **Tàn ý rơi:** khi chết, ghi ngay vị trí và số tàn ý rơi, thay cho bản rơi cũ (bản cũ mất). Nhặt lại thì ghi ngay: xóa bản rơi và cộng vào tàn ý đang mang trong cùng một lần ghi.
4. **Không lưu:** vị trí chính xác của người chơi ngoài Ngưỡng, máu hiện tại, quái đang sống hay đã chết, trạng thái giữa chừng của một trận đấu.
5. **Tải slot:** người chơi xuất hiện ở Ngưỡng trong snapshot (D), như vừa nghỉ xong: đầy tim, đầy thanh nguyên tố, quái hồi sinh. Áp (A), (B), (C) và đặt lại đống tàn ý rơi nếu có.
6. **Slot mới:** bắt đầu ở Ngưỡng khởi đầu, chưa có tiến độ.
7. **Ghi an toàn:** một lần ghi bị ngắt (mất điện, crash) không được làm hỏng bản đang có. Luôn giữ thêm một bản dự phòng của lần ghi trước. Cách làm cụ thể → ADR.
8. **Thông tin hiển thị của slot:** tên Ngưỡng cuối, tổng thời gian chơi, số tàn ý đang mang, ngày giờ lưu.
9. **Phiên bản dữ liệu:** mỗi slot ghi số phiên bản. Bản cũ hơn thì nâng cấp tự động khi tải; bản mới hơn phiên bản game thì không tải và báo cho người chơi.

### States and Transitions

| Trạng thái | Chuyển sang |
|---|---|
| Chọn slot (menu chính) | → Đang tải (chọn slot có dữ liệu) · → Đang chơi (slot trống, chơi mới) · → Xác nhận xóa (chọn xóa) |
| Đang tải | → Đang chơi (thành công) · → Slot lỗi (cả bản chính lẫn bản dự phòng không đọc được) |
| Đang chơi | Ghi (A), (B), (C), tàn ý rơi theo sự kiện, không làm khựng game · → Chọn slot (thoát ra menu) |
| Slot lỗi | → Chọn slot. Slot hiện "Hỏng"; chỉ xóa được khi người chơi xác nhận, không bao giờ tự ghi đè |
| Xác nhận xóa | → Chọn slot (đồng ý thì slot thành trống) |

### Interactions with Other Systems

| Hệ thống | Đưa vào | Nhận ra |
|---|---|---|
| Trạng thái thế giới | Các cờ (B) khi đổi | Cờ (B) khi tải |
| Ngưỡng | Sự kiện nghỉ, gọi ghi snapshot (D) | Ngưỡng để hồi sinh |
| Tàn ý rơi/nhặt | Bản rơi lúc chết; tàn ý đang mang | Bản rơi và tàn ý khi tải |
| Vũ khí (`weapons.md`) | Vũ khí, cấp, đang trang bị, số tên | Như trên khi tải |
| Nguyên tố (`elemental-magic.md`) | Nguyên tố đã có, đang trang bị | Như trên khi tải |
| Nâng cấp ở làng | Tim tối đa, stamina | Như trên khi tải |
| Menu chính / tạm dừng | Chọn slot, xóa slot, thoát | Danh sách slot và thông tin hiển thị |

## Edge Cases

- **Nếu hạ boss rồi chết trước khi nghỉ:** boss vẫn đã hạ (B ghi ngay); hồi sinh ở Ngưỡng cũ.
- **Nếu mở lối tắt rồi thoát game:** lối tắt vẫn mở; vào lại ở Ngưỡng cuối.
- **Nếu mở rương rồi thoát trong vòng 1 giây:** khi thoát có chủ động, ghi ngay phần (C) còn chờ. Nếu crash, chấp nhận mất tối đa 1 giây tài nguyên, nhưng cờ rương và đồ bên trong luôn khớp nhau (cùng một lần ghi).
- **Nếu chết khi đã có đống tàn ý rơi chưa nhặt:** đống cũ bị xóa, đống mới ghi đè, đúng luật brief.
- **Nếu nhặt lại tàn ý rồi crash:** xóa bản rơi và cộng tàn ý nằm trong cùng một lần ghi, nên không thể nhân đôi hay mất.
- **Nếu ghi thất bại** (đĩa đầy, không có quyền): game vẫn chạy, báo một lần, thử lại ở lần ghi kế tiếp; bản cũ giữ nguyên.
- **Nếu bản chính hỏng, bản dự phòng đọc được:** tải bản dự phòng, báo người chơi là có thể mất tiến độ gần nhất.
- **Nếu cả 3 slot đã có dữ liệu mà chọn chơi mới:** phải xóa một slot (có xác nhận) trước.
- **Nếu thoát giữa trận boss:** vào lại ở Ngưỡng cuối, boss đầy máu như chưa đánh.

## Dependencies

| Hệ thống | Hướng | Loại | Ghi chú |
|---|---|---|---|
| (không có) | Lưu và tải phụ thuộc vào | — | Tầng Foundation |
| Trạng thái thế giới | Phụ thuộc vào Lưu và tải | Cứng | Chưa có GDD |
| Ngưỡng | Phụ thuộc vào Lưu và tải | Cứng | Chưa có GDD |
| Tàn ý rơi/nhặt | Phụ thuộc vào Lưu và tải | Cứng | Chưa có GDD |
| Vũ khí (`design/gdd/weapons.md`) | Phụ thuộc vào Lưu và tải | Mềm | `weapons.md` hiện chỉ ghi "lưu ở Ngưỡng"; cần thêm dòng phụ thuộc |
| Nguyên tố (`design/gdd/elemental-magic.md`) | Phụ thuộc vào Lưu và tải | Mềm | Tương tự, cần thêm dòng phụ thuộc |
| Nâng cấp ở làng | Phụ thuộc vào Lưu và tải | Mềm | Chưa có GDD |
| Menu chính / tạm dừng | Phụ thuộc vào Lưu và tải | Mềm | Chưa có GDD |

## Acceptance Criteria

Không có mục Formulas ở tier `minimal`; các tiêu chí dưới đây kiểm luật trong Core Rules.

- **GIVEN** menu chính, **WHEN** mở danh sách slot, **THEN** thấy đúng 3 slot, mỗi slot có dữ liệu hiện tên Ngưỡng, thời gian chơi, tàn ý và ngày giờ.
- **GIVEN** đã hạ Spider King, chưa nghỉ, **WHEN** thoát game rồi tải lại slot, **THEN** Spider King vẫn đã hạ và người chơi đứng ở Ngưỡng cuối.
- **GIVEN** đã mở một lối tắt, **WHEN** tắt hẳn tiến trình game rồi tải lại, **THEN** lối tắt vẫn mở.
- **GIVEN** mở rương có 50 tàn ý, **WHEN** chờ 2 giây rồi tắt tiến trình, **THEN** tải lại thấy rương đã mở và tàn ý đã cộng 50.
- **GIVEN** chết khi mang 300 tàn ý, **WHEN** thoát rồi tải lại, **THEN** đống 300 tàn ý vẫn nằm ở chỗ chết và tàn ý đang mang là 0.
- **GIVEN** đang có đống tàn ý rơi, **WHEN** chết lần nữa ở chỗ khác, **THEN** chỉ còn đống mới.
- **GIVEN** đang chơi ở giữa vùng với 1 tim, **WHEN** thoát rồi tải lại, **THEN** đứng ở Ngưỡng cuối, đầy tim, quái đã hồi sinh.
- **GIVEN** file slot chính bị làm hỏng thủ công, **WHEN** tải slot đó, **THEN** game tải bản dự phòng và báo có thể mất tiến độ gần nhất.
- **GIVEN** cả file chính và dự phòng bị hỏng, **WHEN** mở menu slot, **THEN** slot hiện "Hỏng", không tải được và không bị ghi đè trừ khi người chơi xác nhận xóa.
- **GIVEN** 3 slot đều có dữ liệu, **WHEN** chọn chơi mới, **THEN** game yêu cầu xóa một slot (có xác nhận) trước.
- **GIVEN** đang chơi, **WHEN** một lần ghi xảy ra, **THEN** không có khung hình nào khựng quá 1 frame (đo bằng profiler).
- **GIVEN** slot ghi bằng phiên bản dữ liệu cao hơn bản game, **WHEN** tải, **THEN** game từ chối tải và báo cần cập nhật.
