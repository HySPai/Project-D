# Trạng thái thế giới (World State)

> **Status**: In Design
> **Author**: người dùng + Claude (solo, không có specialist)
> **Last Updated**: 2026-10-18
> **Last Verified**: 2026-10-18
> **Implements Pillar**: không có file pillar ở tier `minimal`; phục vụ vòng metroidvania và "Điểm nghỉ và cái chết kiểu Souls" trong `design/game-brief.md`

> **Workflow**: `minimal`. GDD này là tự nguyện; brief vẫn là tài liệu thiết kế chính. Chỉ có Summary và 5 mục chuẩn.
> **Specialist**: `systems-designer`, `gameplay-programmer`, `engine-programmer`, `qa-lead` không được hỏi ý kiến (Solo mode). Cần review thủ công trước production.
> **Khả thi kỹ thuật**: NOT SOURCEABLE — không có `docs/engine-reference/unity/modules/core.md`; chưa kiểm với Unity 6000.4.1f1. Cách gán ID và nạp vùng sẽ thành ADR.

## Summary

Trạng thái thế giới nhớ những gì người chơi đã thay đổi vĩnh viễn trong thế giới: boss đã hạ, lối tắt đã mở, câu đố đã giải, rương đã mở, bí mật đã tìm, sự kiện NPC đã xảy ra. Mỗi thay đổi là một cờ một chiều, ghi ngay qua Lưu và tải. Mọi thứ còn lại (quái thường, đố dở dang) reset khi nghỉ ở Ngưỡng.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Lưu và tải, Vùng và chuyển vùng`
> Category: Persistence (từ `design/gdd/systems-index.md`)

## Overview

Hệ Core (Persistence) giữ danh sách cờ và trả lời câu hỏi "vật này đã được kích hoạt chưa". Người chơi không thao tác trực tiếp với nó, nhưng thấy kết quả: lối tắt đã mở vẫn mở, boss đã hạ không quay lại. Đây là điều kiện để vòng metroidvania (quay lại và thấy thế giới đã đổi) có nghĩa.

## Detailed Design

### Core Rules

1. **ID cố định:** mỗi vật hoặc sự kiện bền vững có một ID duy nhất trong toàn game, gắn với vùng của nó. Di chuyển hay đổi tên vật trong màn không làm đổi ID.
2. **6 loại cờ:** Boss đã hạ · Lối tắt đã mở · Câu đố đã giải · Rương đã mở · Bí mật đã tìm (tường giả vỡ, lối ẩn lộ) · Sự kiện NPC (đã gặp, đã dạy cơ chế, đã trao vật).
3. **Một chiều:** cờ chỉ đi từ "chưa" sang "đã". Không bao giờ tự đặt lại; chỉ mất khi xóa slot.
4. **Đặt cờ** thì báo Lưu và tải ghi ngay (loại B trong `save-load.md`).
5. **Khi một vùng nạp** (vào vùng, tải slot, hồi sinh), mọi vật bền vững đọc cờ của mình và hiện **ngay ở trạng thái cuối**, không chạy lại hoạt cảnh mở: cửa đã mở, rương trống, tường giả đã vỡ, boss không xuất hiện.
6. **Không bền vững:** quái thường, câu đố đang giải dở (khối đẩy dở, cần gạt chưa đủ), vật phá được không thuộc bí mật (thùng, cỏ). Những thứ này reset khi nghỉ ở Ngưỡng, khi tải slot, và khi rời vùng rồi quay lại.
7. **Boss:** đặt cờ ngay khi boss chết. Boss đã hạ không bao giờ hồi sinh; cửa đấu trường giữ trạng thái mở.
8. **Câu đố:** chỉ đặt cờ khi câu đố hoàn tất. Kết quả của câu đố đã giải (cửa mở, cầu hạ, Mạch Ngầm nối) giữ vĩnh viễn.
9. **Hỏi và nghe:** hệ khác hỏi được "cờ X đã đặt chưa", và nhận được thông báo khi một cờ vừa đổi (để âm thanh, HUD, hội thoại phản ứng).
10. **Không thuộc hệ này:** tàn ý rơi và tài nguyên đang mang (Lưu và tải cùng Tàn ý rơi/nhặt quản lý); tiến độ vĩnh viễn của nhân vật như vũ khí, nguyên tố, kỹ năng (loại A trong `save-load.md`).

### States and Transitions

Cho mỗi vật bền vững:

| Trạng thái | Chuyển sang |
|---|---|
| Chưa kích hoạt | → Đã kích hoạt (người chơi hoàn tất hành động: hạ boss, mở lối tắt, giải xong đố, mở rương, tìm bí mật, xong sự kiện NPC) |
| Đã kích hoạt | Không chuyển. Chỉ mất khi xóa slot |

### Interactions with Other Systems

| Hệ thống | Đưa vào | Nhận ra |
|---|---|---|
| Lưu và tải (`save-load.md`) | Cờ khi tải slot | Cờ mới để ghi ngay |
| Vùng và chuyển vùng (hợp đồng tạm) | Sự kiện "vùng X vừa nạp" | Trạng thái cuối cho từng vật của vùng |
| Ngưỡng | Sự kiện "nghỉ" | Lệnh reset phần không bền vững |
| Khung boss | Boss chết | Boss đã hạ thì không sinh boss |
| Lối tắt, Câu đố, Rương, Bí mật | Hoàn tất hành động | Trạng thái cuối khi nạp vùng |
| Hội thoại NPC | Sự kiện NPC xảy ra | Cờ NPC để chọn lời thoại |

## Edge Cases

- **Nếu hai vật trùng ID** (lỗi dựng màn): editor báo lỗi; bản build không qua bước kiểm tra.
- **Nếu bản cập nhật xóa một vật đã có cờ:** cờ vẫn nằm trong slot, khi nạp vùng thì bỏ qua.
- **Nếu bản cập nhật thêm vật mới:** coi là "chưa kích hoạt" ở mọi slot cũ.
- **Nếu boss và người chơi chết cùng một frame:** boss vẫn tính là đã hạ (đặt cờ); người chơi hồi sinh ở Ngưỡng, không phải đánh lại.
- **Nếu đang giải đố dở thì chết:** hồi sinh là một lần nghỉ, nên đố reset về đầu.
- **Nếu rời vùng khi đố đang dở rồi quay lại:** đố reset về đầu.
- **Nếu mở lối tắt từ phía bên kia:** đi vòng tới phía trước sẽ thấy lối đã mở.
- **Nếu tường giả cần nhiều đòn, đánh dở rồi bỏ đi:** chưa tính là đã tìm, tường hồi nguyên khi reset.
- **Nếu tải slot đúng lúc vùng đang nạp:** cờ áp xong mới hiện vùng, người chơi không thấy vật "nhảy" trạng thái.

## Dependencies

| Hệ thống | Hướng | Loại | Ghi chú |
|---|---|---|---|
| Lưu và tải (`design/gdd/save-load.md`) | Trạng thái thế giới phụ thuộc vào | Cứng | `save-load.md` đã ghi chiều ngược lại |
| Vùng và chuyển vùng | Trạng thái thế giới phụ thuộc vào | Cứng | Chưa có GDD. Hợp đồng tạm: phát sự kiện "vùng X vừa nạp" kèm danh sách vật bền vững |
| Ngưỡng | Phụ thuộc vào Trạng thái thế giới | Cứng | Chưa có GDD |
| Khung boss | Phụ thuộc vào Trạng thái thế giới | Cứng | Chưa có GDD |
| Khóa khả năng metroidvania | Phụ thuộc vào Trạng thái thế giới | Cứng | Chưa có GDD |
| Lối tắt và cổng một chiều | Phụ thuộc vào Trạng thái thế giới | Cứng | Chưa có GDD |
| Bí mật và tường giả | Phụ thuộc vào Trạng thái thế giới | Cứng | Chưa có GDD |
| Rương và phần thưởng | Phụ thuộc vào Trạng thái thế giới | Cứng | Chưa có GDD |
| Tàn ý rơi và nhặt khi chết | Phụ thuộc vào Trạng thái thế giới | Cứng | Chưa có GDD |
| Hội thoại NPC | Phụ thuộc vào Trạng thái thế giới | Mềm | Chưa có GDD |
| Câu đố (3 dạng) | Phụ thuộc vào Trạng thái thế giới | Mềm | Chưa có GDD |

## Acceptance Criteria

Không có mục Formulas ở tier `minimal`; các tiêu chí dưới đây kiểm luật trong Core Rules.

- **GIVEN** đã mở cổng then cài từ phía sau, **WHEN** đi vòng ra phía trước, **THEN** cổng đang mở và không chạy lại hoạt cảnh mở.
- **GIVEN** đã hạ Spider King, **WHEN** nghỉ ở Ngưỡng rồi quay lại đấu trường, **THEN** không có boss và cửa đấu trường đang mở.
- **GIVEN** đã giết 5 Spider và mở 1 rương, **WHEN** nghỉ ở Ngưỡng rồi quay lại, **THEN** 5 Spider đã hồi sinh, rương vẫn mở và trống.
- **GIVEN** đã kéo 2 trên 3 cần gạt, **WHEN** nghỉ ở Ngưỡng hoặc rời vùng rồi quay lại, **THEN** cả 3 cần gạt về vị trí ban đầu.
- **GIVEN** đã giải xong đố Mạch Ngầm, **WHEN** tải lại slot, **THEN** cửa do đố mở vẫn mở.
- **GIVEN** Lão Mật đã dạy parry, **WHEN** nói chuyện lại, **THEN** Lão Mật không dạy lại từ đầu (hệ Hội thoại đọc cờ NPC).
- **GIVEN** hai vật trong một vùng có cùng ID, **WHEN** chạy bước kiểm tra build, **THEN** kiểm tra báo lỗi và nêu tên hai vật.
- **GIVEN** slot cũ có cờ của một rương đã bị xóa khỏi màn ở bản mới, **WHEN** tải slot, **THEN** không lỗi, mọi cờ khác vẫn đúng.
- **GIVEN** boss và người chơi chết cùng frame, **WHEN** hồi sinh, **THEN** boss đã hạ.
