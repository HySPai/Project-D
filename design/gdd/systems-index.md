# Systems Index: Project-D

> **Status**: Approved
> **Created**: 2026-10-18
> **Last Updated**: 2026-10-18
> **Source Concept**: design/game-brief.md (cùng `design/gdd/weapons.md`, `design/gdd/elemental-magic.md`, `design/narrative/characters/hiep-si-cho.md`)
> **Workflow**: `minimal` — index này là tự nguyện; brief vẫn là tài liệu thiết kế chính.
> **Gates**: TD-SYSTEM-BOUNDARY, PR-SCOPE, CD-SYSTEMS skipped — Solo mode.

---

## Overview

Project-D là soulslike + metroidvania + giải đố, camera isometric cố định. Vòng chính: rời Ngưỡng đi khám phá, đánh quái côn trùng (lock-on, lăn, chém, giữ stamina), nhặt tàn ý, về Ngưỡng hồi máu và lưu, về làng nâng cấp; gặp đường chặn thì giải đố hoặc quay lại khi có kỹ năng mới từ boss. Combat Souls đã có code; phần còn thiếu cho MVP là vòng chết kiểu Souls, một kỹ năng di chuyển mở lối tắt, ba dạng giải đố, khung boss, làng (NPC, cửa hàng, nâng cấp) và lưu trạng thái thế giới. Thứ tự ưu tiên bám mục "Thứ tự xây" của brief.

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|-------------|----------|----------|--------|------------|------------|
| 1 | Điều khiển nhân vật | Core | MVP | Implemented | — | — |
| 2 | Camera isometric | Core | MVP | Implemented | — | — |
| 3 | Luật hiển thị (inferred) | Core | MVP | Not Started | — | Camera isometric |
| 4 | Tim và kênh sát thương | Core | MVP | Implemented | — | — |
| 5 | Vùng và chuyển vùng (inferred) | Core | MVP | Not Started | — | — |
| 6 | Combat cận chiến | Gameplay | MVP | Implemented | — | Điều khiển nhân vật, Tim và kênh sát thương, Stamina, Lock-on |
| 7 | Stamina | Gameplay | MVP | Implemented | — | Điều khiển nhân vật |
| 8 | Lock-on | Gameplay | MVP | Implemented | — | Điều khiển nhân vật, Camera isometric |
| 9 | Vũ khí (Kiếm, Khiên, Cung) | Gameplay | MVP | In Design | design/gdd/weapons.md | Combat cận chiến, Stamina, Lock-on |
| 10 | AI quái côn trùng | Gameplay | MVP | Implemented | — | Tim và kênh sát thương, Combat cận chiến |
| 11 | Khung boss (inferred) | Gameplay | MVP | Not Started | — | Combat cận chiến, AI quái côn trùng |
| 12 | Kỹ năng di chuyển (Lướt) | Gameplay | MVP | Not Started | — | Điều khiển nhân vật, Stamina |
| 13 | Khóa khả năng metroidvania (inferred) | Gameplay | MVP | Not Started | — | Kỹ năng di chuyển (Lướt), Trạng thái thế giới |
| 14 | Tương tác và công tắc (inferred) | Gameplay | MVP | Not Started | — | — |
| 15 | Giải đố máy móc (Mạch Ngầm) | Gameplay | MVP | Not Started | — | Tương tác và công tắc |
| 16 | Giải đố đẩy khối/môi trường | Gameplay | MVP | Not Started | — | Tương tác và công tắc |
| 17 | Giải đố lợi dụng quái | Gameplay | MVP | Not Started | — | Tương tác và công tắc, AI quái côn trùng |
| 18 | Lối tắt và cổng một chiều | Gameplay | MVP | Not Started | — | Tương tác và công tắc, Trạng thái thế giới |
| 19 | Bí mật và tường giả | Gameplay | MVP | Not Started | — | Camera isometric, Luật hiển thị, Trạng thái thế giới |
| 20 | Tín hiệu đánh hơi | Gameplay | Vertical Slice | Not Started | design/narrative/characters/hiep-si-cho.md | Điều khiển nhân vật, Tim và kênh sát thương, Bí mật và tường giả |
| 21 | Tàn ý rơi và nhặt khi chết | Economy | MVP | Not Started | — | AI quái côn trùng, Ngưỡng, Trạng thái thế giới |
| 22 | Cửa hàng và kho vật phẩm | Economy | MVP | Not Started | — | Tàn ý rơi và nhặt khi chết |
| 23 | Rương và phần thưởng | Economy | MVP | Not Started | — | Tương tác và công tắc, Tàn ý rơi và nhặt khi chết, Trạng thái thế giới |
| 24 | Nâng cấp ở làng | Progression | MVP | Not Started | — | Tim và kênh sát thương, Stamina, Vũ khí, Tàn ý rơi và nhặt khi chết, Cửa hàng và kho vật phẩm |
| 25 | Nguyên tố | Progression | Full Vision | In Design | design/gdd/elemental-magic.md | Combat cận chiến, Kỹ năng di chuyển (Lướt), Ngưỡng |
| 26 | Ngưỡng (điểm nghỉ) | Persistence | MVP | Designed | design/gdd/rest-point.md | Tim và kênh sát thương, AI quái côn trùng, Lưu và tải, Trạng thái thế giới |
| 27 | Lưu và tải (inferred) | Persistence | MVP | Designed | design/gdd/save-load.md | — |
| 28 | Trạng thái thế giới (inferred) | Persistence | MVP | Designed | design/gdd/world-state.md | Vùng và chuyển vùng, Lưu và tải |
| 29 | HUD | UI | MVP | Not Started | — | Tim và kênh sát thương, Stamina, Vũ khí, Tàn ý rơi và nhặt khi chết |
| 30 | Menu Ngưỡng | UI | MVP | Not Started | — | Vũ khí, Ngưỡng |
| 31 | UI cửa hàng và nâng cấp | UI | MVP | Not Started | — | Cửa hàng và kho vật phẩm, Nâng cấp ở làng |
| 32 | Bản đồ vẽ tay | UI | Vertical Slice | Not Started | — | Vùng và chuyển vùng, Cửa hàng và kho vật phẩm |
| 33 | Hội thoại NPC | Narrative | MVP | Not Started | — | Tương tác và công tắc |
| 34 | Menu tạm dừng và cài đặt (inferred) | UI | MVP | Not Started | — | Điều khiển nhân vật |
| 35 | Chết và hồi sinh | UI | MVP | Not Started | — | Tim và kênh sát thương, Tàn ý rơi và nhặt khi chết, Ngưỡng |
| 36 | Âm thanh và nhạc | Audio | Vertical Slice | Implemented | — | — |
| 37 | Feedback combat (Feel/VFX) | Audio | MVP | Implemented | — | Combat cận chiến, AI quái côn trùng |
| 38 | Trợ năng | Meta | Alpha | Not Started | — | Tim và kênh sát thương |
| 39 | Ngày đêm | Core | MVP | Implemented | design/gdd/day-night-cycle.md | Ngưỡng, Lưu và tải |

Ghi chú trạng thái: "Implemented" = đã có code nhưng chưa có GDD (viết ngược bằng `/reverse-document` nếu cần). #35 hiện có màn Game Over/Restart, chưa có vòng hồi sinh ở Ngưỡng.

---

## Categories

| Category | Description | Typical Systems |
|----------|-------------|-----------------|
| **Core** | Nền mọi hệ khác dựa vào | Điều khiển, camera, luật hiển thị, tim, vùng |
| **Gameplay** | Thứ làm game vui | Combat, quái, boss, lướt, giải đố, lối tắt, bí mật |
| **Progression** | Người chơi mạnh lên | Nâng cấp ở làng, nguyên tố |
| **Economy** | Tàn ý, cửa hàng, phần thưởng | Tàn ý rơi/nhặt, cửa hàng, rương |
| **Persistence** | Lưu và liên tục | Ngưỡng, lưu/tải, trạng thái thế giới |
| **UI** | Thông tin cho người chơi | HUD, menu Ngưỡng, cửa hàng, bản đồ, tạm dừng, chết |
| **Audio** | Âm thanh và feedback | Âm thanh/nhạc, Feel/VFX |
| **Narrative** | Kể chuyện | Hội thoại NPC |
| **Meta** | Ngoài vòng chính | Trợ năng |

---

## Priority Tiers

| Tier | Definition | Target Milestone | Design Urgency |
|------|------------|------------------|----------------|
| **MVP** | Brief mục "MVP — phải có thì mới là game", xây theo "Thứ tự xây" bước 1–5 | Bản chơi được 2 vùng | Design FIRST |
| **Vertical Slice** | Bước 6 (đánh bóng) và các mục khám phá ngoài danh sách MVP | Bản demo | Design SECOND |
| **Alpha** | Tính năng hỗ trợ ngoài vòng chính | Alpha | Design THIRD |
| **Full Vision** | Brief để sau MVP | Beta / Release | Design as needed |

Theo "Thứ tự xây" của brief:
- **Bước 1** (Ngưỡng và cái chết): 27, 28, 26, 21, 35, 29.
- **Bước 2** (Lướt và lối tắt): 14, 12, 13, 18.
- **Bước 3** (Vùng 1): 5, 3, 15, 16, 17, 11, 19, 9.
- **Bước 4** (Làng): 33, 22, 24, 23, 30, 31, 34.
- **Bước 5** (Vùng 2): không có hệ mới, dùng lại 10 và 11 với nội dung mới.
- **Bước 6** (đánh bóng): 36, 37 cùng các hệ Vertical Slice.

---

## Dependency Map

### Foundation Layer (no dependencies)

1. Điều khiển nhân vật — mọi hành động của hiệp sĩ đi qua đây
2. Camera isometric — góc cố định là luật nền của khám phá
3. Tim và kênh sát thương — mọi đòn đánh và cái chết đi qua đây
4. Vùng và chuyển vùng — nền cho 2 vùng liên thông và làng
5. Tương tác và công tắc — nền chung cho 3 dạng giải đố, lối tắt, rương, NPC
6. Lưu và tải — chỉ lưu ở Ngưỡng nhưng mọi tiến độ cần nó
7. Âm thanh và nhạc — đã có AudioManager

### Core Layer (depends on foundation)

1. Luật hiển thị — depends on: Camera isometric
2. Stamina — depends on: Điều khiển nhân vật
3. Lock-on — depends on: Điều khiển nhân vật, Camera isometric
4. Combat cận chiến — depends on: Điều khiển nhân vật, Tim và kênh sát thương, Stamina, Lock-on
5. AI quái côn trùng — depends on: Tim và kênh sát thương, Combat cận chiến
6. Trạng thái thế giới — depends on: Vùng và chuyển vùng, Lưu và tải

### Feature Layer (depends on core)

1. Vũ khí — depends on: Combat cận chiến, Stamina, Lock-on
2. Khung boss — depends on: Combat cận chiến, AI quái côn trùng
3. Kỹ năng di chuyển (Lướt) — depends on: Điều khiển nhân vật, Stamina
4. Khóa khả năng metroidvania — depends on: Lướt, Trạng thái thế giới
5. Ngưỡng — depends on: Tim và kênh sát thương, AI quái côn trùng, Lưu và tải, Trạng thái thế giới
6. Giải đố máy móc, Giải đố đẩy khối — depends on: Tương tác và công tắc
7. Giải đố lợi dụng quái — depends on: Tương tác và công tắc, AI quái côn trùng
8. Lối tắt và cổng một chiều — depends on: Tương tác và công tắc, Trạng thái thế giới
9. Bí mật và tường giả — depends on: Camera isometric, Luật hiển thị, Trạng thái thế giới
10. Tín hiệu đánh hơi — depends on: Điều khiển nhân vật, Tim và kênh sát thương, Bí mật và tường giả
11. Tàn ý rơi và nhặt khi chết — depends on: AI quái côn trùng, Ngưỡng, Trạng thái thế giới
12. Cửa hàng và kho vật phẩm — depends on: Tàn ý rơi và nhặt khi chết
13. Rương và phần thưởng — depends on: Tương tác và công tắc, Tàn ý, Trạng thái thế giới
14. Nâng cấp ở làng — depends on: Tim, Stamina, Vũ khí, Tàn ý, Cửa hàng
15. Nguyên tố — depends on: Combat cận chiến, Lướt, Ngưỡng

### Presentation Layer (depends on features)

1. HUD — depends on: Tim, Stamina, Vũ khí, Tàn ý
2. Menu Ngưỡng — depends on: Vũ khí, Ngưỡng
3. UI cửa hàng và nâng cấp — depends on: Cửa hàng, Nâng cấp ở làng
4. Bản đồ vẽ tay — depends on: Vùng và chuyển vùng, Cửa hàng
5. Hội thoại NPC — depends on: Tương tác và công tắc
6. Menu tạm dừng và cài đặt — depends on: Điều khiển nhân vật
7. Chết và hồi sinh — depends on: Tim, Tàn ý, Ngưỡng
8. Feedback combat (Feel/VFX) — depends on: Combat cận chiến, AI quái côn trùng

### Polish Layer (depends on everything)

1. Trợ năng — depends on: Tim và kênh sát thương

---

## Recommended Design Order

| Order | System | Priority | Layer | Agent(s) | Est. Effort |
|-------|--------|----------|-------|----------|-------------|
| 1 | Lưu và tải | MVP | Foundation | game-designer, lead-programmer | S |
| 2 | Trạng thái thế giới | MVP | Core | game-designer, lead-programmer | M |
| 3 | Ngưỡng (điểm nghỉ) | MVP | Feature | game-designer | M |
| 4 | Tàn ý rơi và nhặt khi chết | MVP | Feature | game-designer, economy-designer | S |
| 5 | Chết và hồi sinh | MVP | Presentation | game-designer, ux-designer | S |
| 6 | HUD | MVP | Presentation | ux-designer | S |
| 7 | Tương tác và công tắc | MVP | Foundation | game-designer | M |
| 8 | Kỹ năng di chuyển (Lướt) | MVP | Feature | game-designer | M |
| 9 | Khóa khả năng metroidvania | MVP | Feature | game-designer, level-designer | S |
| 10 | Lối tắt và cổng một chiều | MVP | Feature | level-designer | S |
| 11 | Vùng và chuyển vùng | MVP | Foundation | lead-programmer, level-designer | M |
| 12 | Luật hiển thị | MVP | Core | technical-artist | M |
| 13 | Giải đố máy móc (Mạch Ngầm) | MVP | Feature | game-designer, level-designer | M |
| 14 | Giải đố đẩy khối/môi trường | MVP | Feature | game-designer | S |
| 15 | Giải đố lợi dụng quái | MVP | Feature | game-designer, ai-programmer | M |
| 16 | Khung boss | MVP | Feature | game-designer, ai-programmer | L |
| 17 | Bí mật và tường giả | MVP | Feature | level-designer | S |
| 18 | Vũ khí — chỉ `/design-review` GDD đã có | MVP | Feature | game-designer | S |
| 19 | Hội thoại NPC | MVP | Presentation | narrative-director, ux-designer | S |
| 20 | Cửa hàng và kho vật phẩm | MVP | Feature | economy-designer | S |
| 21 | Nâng cấp ở làng | MVP | Feature | economy-designer, systems-designer | M |
| 22 | Rương và phần thưởng | MVP | Feature | economy-designer | S |
| 23 | Menu Ngưỡng | MVP | Presentation | ux-designer | S |
| 24 | UI cửa hàng và nâng cấp | MVP | Presentation | ux-designer | S |
| 25 | Menu tạm dừng và cài đặt | MVP | Presentation | ux-designer | S |
| 26 | Tín hiệu đánh hơi | Vertical Slice | Feature | game-designer, technical-artist | S |
| 27 | Bản đồ vẽ tay | Vertical Slice | Presentation | ux-designer | S |
| 28 | Âm thanh và nhạc | Vertical Slice | Foundation | audio-director | M |
| 29 | Trợ năng | Alpha | Polish | accessibility-specialist | S |
| 30 | Nguyên tố — chỉ `/design-review` GDD đã có | Full Vision | Feature | game-designer | S |

Các hệ "Implemented" (1, 2, 4, 6, 7, 8, 10, 37) không nằm trong thứ tự trên; viết ngược GDD bằng `/reverse-document` khi cần.

---

## Circular Dependencies

- Không có. Cặp dễ thành vòng: Ngưỡng ↔ Trạng thái thế giới. Cách tách: Trạng thái thế giới chỉ giữ dữ liệu (lối tắt đã mở, câu đố đã giải, boss đã hạ, tàn ý đang rơi); Ngưỡng là nơi gọi lưu và đọc dữ liệu đó để hồi sinh quái.

---

## High-Risk Systems

| System | Risk Type | Risk Description | Mitigation |
|--------|-----------|-----------------|------------|
| Luật hiển thị | Technical | Viền bóng xuyên tường mà tường không trong suốt, lại không được làm lộ lối ẩn | Prototype shader viền sớm ở bước 3, thử với một lối ẩn thật |
| Lướt + Khóa khả năng | Design | Chưa biết vòng metroidvania có đọc được ở góc isometric cố định không | Đúng ý brief: xây ở bước 2, một lối tắt bị chặn, chơi thử trước khi làm vùng |
| Trạng thái thế giới | Scope | Điểm nghẽn của 6 hệ; đổi cấu trúc muộn sẽ phải sửa nhiều chỗ | Thiết kế ngay sau Lưu và tải, chốt định dạng dữ liệu trước khi làm lối tắt và rương |
| Khung boss | Scope | Mỗi boss cần phase và đấu trường riêng | Khung chung (phase, đấu trường, rơi kỹ năng), nội dung từng boss tách riêng |
| Giải đố lợi dụng quái | Design | Gắn chặt với AI quái, dễ vỡ khi AI đổi | Chốt 1 hành vi quái làm "API" cho câu đố, không phụ thuộc chi tiết FSM |

---

## Progress Tracker

| Metric | Count |
|--------|-------|
| Total systems identified | 38 |
| Design docs started | 5 (weapons, elemental-magic, save-load, world-state, rest-point) |
| Design docs reviewed | 0 |
| Design docs approved | 0 |
| MVP systems designed | 4/33 (save-load, world-state, rest-point Designed; weapons In Design) |
| Vertical Slice systems designed | 0/3 |

---

## Next Steps

- [ ] Design MVP-tier systems theo thứ tự trên (`/design-system [system-name]`), bắt đầu với Lưu và tải
- [ ] Run `/design-review design/gdd/weapons.md`
- [ ] Prototype sớm: Luật hiển thị, Lướt + một lối tắt
- [ ] Run `/gate-check technical-setup` khi các GDD MVP xong
