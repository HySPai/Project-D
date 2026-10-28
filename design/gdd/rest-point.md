# Ngưỡng — điểm nghỉ (Rest Point)

> **Status**: In Design
> **Author**: người dùng + Claude (solo, không có specialist)
> **Last Updated**: 2026-10-18
> **Last Verified**: 2026-10-18
> **Implements Pillar**: không có file pillar ở tier `minimal`; phục vụ "Điểm nghỉ và cái chết kiểu Souls" trong `design/game-brief.md`

> **Workflow**: `minimal`. GDD này là tự nguyện; brief vẫn là tài liệu thiết kế chính. Chỉ có Summary và 5 mục chuẩn. Các con số nằm thẳng trong Core Rules.
> **Specialist**: `systems-designer`, `gameplay-programmer`, `engine-programmer`, `qa-lead` không được hỏi ý kiến (Solo mode). Cần review thủ công trước production.
> **Khả thi kỹ thuật**: NOT SOURCEABLE — không có `docs/engine-reference/unity/modules/core.md`; chưa kiểm với Unity 6000.4.1f1.
> **Phạm vi**: dịch chuyển giữa các Ngưỡng là phần thêm so với danh sách MVP của brief (người dùng chọn 2026-10-18).

## Summary

Ngưỡng là điểm nghỉ: nghỉ thì đầy tim và thanh nguyên tố, quái thường hồi sinh, tiến độ được lưu, và Ngưỡng đó thành nơi hồi sinh khi chết. Từ một Ngưỡng có thể đổi vũ khí và dịch chuyển tới các Ngưỡng đã thắp khác.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Lưu và tải, Trạng thái thế giới, Tim và kênh sát thương, AI quái côn trùng`
> Category: Persistence (từ `design/gdd/systems-index.md`)

## Overview

Hệ Feature, là nhịp thở của vòng Souls: đi xa khỏi Ngưỡng là rủi ro, về được Ngưỡng là an toàn. Người chơi tương tác trực tiếp để nghỉ, đổi vũ khí và dịch chuyển. Hệ này điều phối các hệ khác vào đúng một khoảnh khắc "nghỉ": hồi phục, reset thế giới, lưu. Trong lore, Ngưỡng là nơi Người Gác Ngưỡng còn để lại hơi ấm (`design/narrative/lore/world-foundation.md` mục 1.2).

## Detailed Design

### Core Rules

1. **Thắp** (bấm lần đầu, sửa 2026-10-26): Ngưỡng là một đống lửa âm ỉ; bấm lần đầu thì lửa bùng cháy, Ngưỡng thành **Ngưỡng nghỉ gần nhất** và game lưu ngay. Thắp **không** nghỉ: không hồi phục, không reset quái, không tua giờ. Cờ "đã thắp" lưu trong Trạng thái thế giới, xếp vào loại Lối tắt (Ngưỡng là điểm nối của thế giới), một chiều.
2. **Nghỉ** (mỗi lần tương tác với Ngưỡng đã thắp) là chuỗi tua giờ trong `day-night-cycle.md` luật 8: trong 2 giây màn hình tối dần tới 00:00, đen hẳn, sáng dần tới 09:00. Lúc màn hình đen:
   - (a) đầy tim, đầy stamina, đầy thanh nguyên tố; xóa hiệu ứng bất lợi; gỡ lớp yểm nguyên tố trên vũ khí (`elemental-magic.md` luật 11b);
   - (b) quái thường hồi sinh và đố dở reset (lệnh gửi Trạng thái thế giới);
   - (c) người chơi đứng ở chỗ cạnh đống lửa; Ngưỡng này thành **Ngưỡng nghỉ gần nhất**;
   - (d) khi sáng hẳn lúc 09:00, Lưu và tải ghi snapshot (D) cùng giờ 09:00.

   Mũi tên **không** hồi, theo `weapons.md`.
3. **Không nghỉ được** khi có quái đang truy đuổi người chơi trong bán kính 10m. Ngưỡng hiện trạng thái "Lạnh" và không phản hồi.
4. **Hồi sinh khi chết:** xuất hiện ở Ngưỡng nghỉ gần nhất, coi như một lần nghỉ (luật 2a–2d). Slot mới thì hồi sinh ở Ngưỡng khởi đầu.
5. **Menu Ngưỡng** (khi đang nghỉ): Đổi vũ khí (theo `weapons.md`) · Dịch chuyển · Rời đi.
6. **Dịch chuyển:**
   - chỉ tới Ngưỡng đã thắp, liệt kê theo vùng; miễn phí;
   - chỉ thực hiện được khi đang nghỉ ở một Ngưỡng;
   - đến nơi coi như nghỉ ở Ngưỡng đích (luật 2), nên Ngưỡng đích thành Ngưỡng nghỉ gần nhất;
   - tàn ý đang rơi giữ nguyên chỗ cũ.
7. **Ngưỡng trong đấu trường boss:** bị che và không tương tác được cho tới khi boss đã hạ.
8. **Bố trí (gợi ý cho level design):** mỗi vùng có ít nhất 1 Ngưỡng; Làng Mái Vá có 1 Ngưỡng; lối tắt dẫn về Ngưỡng theo brief.

### States and Transitions

Cho mỗi Ngưỡng:

| Trạng thái | Chuyển sang |
|---|---|
| Chưa thắp | → Đang nghỉ (tương tác lần đầu: thắp và nghỉ) |
| Đã thắp | → Đang nghỉ (tương tác) · → Lạnh (có quái truy đuổi trong 10m) |
| Lạnh | → Đã thắp (hết quái truy đuổi) |
| Đang nghỉ | → Đã thắp (Rời đi) · → Đang dịch chuyển (chọn đích) |
| Đang dịch chuyển | → Đang nghỉ ở Ngưỡng đích |
| Bị che (đấu trường boss) | → Chưa thắp (boss đã hạ) |

### Interactions with Other Systems

| Hệ thống | Đưa vào | Nhận ra |
|---|---|---|
| Lưu và tải (`save-load.md`) | Ngưỡng nghỉ gần nhất khi tải | Lệnh ghi snapshot (D) |
| Trạng thái thế giới (`world-state.md`) | Cờ đã thắp, boss đã hạ | Cờ thắp; lệnh reset khi nghỉ |
| Tim, Stamina | — | Hồi đầy |
| Nguyên tố (`elemental-magic.md`) | — | Hồi đầy thanh |
| AI quái | Có quái truy đuổi trong 10m | Hồi sinh quái thường |
| Vũ khí (`weapons.md`) | Danh sách vũ khí | Mở chức năng đổi vũ khí |
| Chết và hồi sinh | Sự kiện chết | Ngưỡng để hồi sinh |
| Menu Ngưỡng | Lựa chọn người chơi | Danh sách Ngưỡng đã thắp |

## Edge Cases

- **Nếu chết trước khi nghỉ ở bất kỳ Ngưỡng nào trong slot:** hồi sinh ở Ngưỡng khởi đầu.
- **Nếu Ngưỡng nghỉ gần nhất nằm ở vùng khác vùng đang đứng:** vẫn hồi sinh ở đó; nạp vùng tương ứng.
- **Nếu dịch chuyển khi tàn ý đang rơi ở vùng khác:** đống tàn ý giữ nguyên; chết lần nữa ở bất cứ đâu thì mất, đúng luật brief.
- **Nếu tương tác với Ngưỡng khi đang bị khóa hành động (choáng, đang lăn):** chỉ tương tác được khi đứng yên và không bị khóa hành động.
- **Nếu quái truy đuổi rời khỏi bán kính 10m:** Ngưỡng hết Lạnh ngay.
- **Nếu nghỉ ngay cạnh đống tàn ý của mình:** nghỉ không tự nhặt; phải đi tới nhặt.
- **Nếu mở Menu Ngưỡng rồi thoát game:** đã nghỉ xong (luật 2 chạy khi tương tác), nên tải lại ở Ngưỡng này.

## Dependencies

| Hệ thống | Hướng | Loại | Ghi chú |
|---|---|---|---|
| Lưu và tải (`design/gdd/save-load.md`) | Ngưỡng phụ thuộc vào | Cứng | Đã ghi chiều ngược lại |
| Trạng thái thế giới (`design/gdd/world-state.md`) | Ngưỡng phụ thuộc vào | Cứng | Đã ghi chiều ngược lại |
| Tim và kênh sát thương | Ngưỡng phụ thuộc vào | Cứng | Có code, chưa có GDD |
| AI quái côn trùng | Ngưỡng phụ thuộc vào | Cứng | Có code, chưa có GDD |
| Stamina | Ngưỡng phụ thuộc vào | Mềm | Có code, chưa có GDD |
| Khung boss | Ngưỡng phụ thuộc vào | Mềm | Chưa có GDD |
| Tàn ý rơi và nhặt khi chết | Phụ thuộc vào Ngưỡng | Cứng | Chưa có GDD |
| Chết và hồi sinh | Phụ thuộc vào Ngưỡng | Cứng | Chưa có GDD |
| Menu Ngưỡng | Phụ thuộc vào Ngưỡng | Cứng | Chưa có GDD |
| Vũ khí (`design/gdd/weapons.md`) | Phụ thuộc vào Ngưỡng | Cứng | Đổi vũ khí chỉ ở Ngưỡng |
| Nguyên tố (`design/gdd/elemental-magic.md`) | Phụ thuộc vào Ngưỡng | Cứng | Hồi đầy thanh và gỡ lớp yểm khi nghỉ |

## Acceptance Criteria

Không có mục Formulas ở tier `minimal`; các tiêu chí dưới đây kiểm luật trong Core Rules.

- **GIVEN** Ngưỡng chưa thắp, **WHEN** tương tác, **THEN** Ngưỡng sáng lên, người chơi nghỉ và Ngưỡng này thành nơi hồi sinh.
- **GIVEN** 1 tim, thanh nguyên tố 10, đang Bỏng, **WHEN** nghỉ, **THEN** đầy tim, thanh 100, hết Bỏng, số tên không đổi.
- **GIVEN** đã giết 5 quái thường, **WHEN** nghỉ rồi quay lại, **THEN** 5 quái đó đã hồi sinh.
- **GIVEN** nghỉ ở Ngưỡng A rồi đi qua Ngưỡng B (đã thắp, không nghỉ), **WHEN** chết, **THEN** hồi sinh ở A.
- **GIVEN** một quái thường đang đuổi trong 10m, **WHEN** tương tác với Ngưỡng, **THEN** không nghỉ được và Ngưỡng hiện trạng thái Lạnh.
- **GIVEN** đã thắp Ngưỡng A và B, **WHEN** nghỉ ở A và chọn dịch chuyển tới B, **THEN** xuất hiện ở B đã nghỉ xong, và chết sau đó thì hồi sinh ở B.
- **GIVEN** Ngưỡng C chưa thắp, **WHEN** mở danh sách dịch chuyển, **THEN** C không có trong danh sách.
- **GIVEN** boss vùng còn sống, **WHEN** vào đấu trường, **THEN** không thấy Ngưỡng; **WHEN** hạ boss, **THEN** Ngưỡng hiện ra và thắp được.
- **GIVEN** slot mới chưa nghỉ lần nào, **WHEN** chết, **THEN** hồi sinh ở Ngưỡng khởi đầu.
