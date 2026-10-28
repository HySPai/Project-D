# Weapons (Vũ khí)

> **Status**: In Design
> **Author**: người dùng + Claude (solo, không có specialist)
> **Last Updated**: 2026-10-28
> **Last Verified**: 2026-10-17
> **Implements Pillar**: không có file pillar ở tier `minimal`; phục vụ "Combat Souls" và metroidvania trong `design/game-brief.md`

> **Workflow**: `minimal`. GDD này là tự nguyện; brief vẫn là tài liệu thiết kế chính. Không có mục Formulas: các con số được ghi thẳng trong Core Rules, đều là giá trị khởi điểm và nằm trong ScriptableObject.
> **Specialist**: `game-designer`, `systems-designer`, `ai-programmer`, `art-director`, `economy-designer` và `qa-lead` không được hỏi ý kiến (Solo mode). Cần review thủ công trước khi vào production.

## Summary

Hiệp sĩ chó dùng một vũ khí chính (Kiếm, Giáo, Búa hoặc Song kiếm), mang Khiên nếu vũ khí đó cầm một tay, và luôn đeo Cung làm vũ khí phụ. Mỗi vũ khí có lối đánh riêng, có duyên với một nguyên tố, và mở một loại bí mật tùy chọn. Riêng Cung giải đố ở xa, kể cả trên đường chính.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` (Kiếm, Khiên, Cung) / `Full Vision` (Giáo, Búa, Song kiếm) · Key deps: `Combat, Stamina, Lock-on, Elemental Magic, Ngưỡng, Thợ rèn (Làng Mái Vá), Enemy AI`
> Category: Gameplay · Layer: Feature · Priority: như trên — cả ba suy ra từ brief, vì tier này không có systems index.

## Overview

Weapons là hệ thống trang bị chiến đấu. Người chơi mang **1 vũ khí chính**:
- Kiếm và Giáo cầm một tay, nên dùng kèm Khiên.
- Búa và Song kiếm cầm hai tay, không dùng Khiên; mỗi món có cách phòng thủ riêng.

Ngoài ra, người chơi luôn mang **Cung** làm vũ khí phụ.

Vũ khí chính chỉ đổi được ở Ngưỡng. Chọn vũ khí là chọn lối chơi cho cả chặng đi: tốc độ, tầm đánh, cách thủ, và nguyên tố được tăng sức nhờ duyên. Vũ khí nhận từ nhiều nguồn (có sẵn từ đầu, Làng Mái Vá, tàn tích, boss, các nền văn minh) và nâng cấp ở thợ rèn. Thiếu hệ thống này, combat chỉ có một thanh kiếm và không có chặn đòn hay parry, là hai trụ cột của thể loại soulslike.

## Detailed Design

### Core Rules

**Mang và đổi**
1. Mỗi lúc mang đúng **1 vũ khí chính**.
   - Kiếm và Giáo được mang thêm Khiên ở tay phụ.
   - Búa và Song kiếm không mang được Khiên.
2. **Cung luôn được mang** làm vũ khí phụ, bất kể vũ khí chính, kể từ khi nhận được.
3. Đổi vũ khí chính và Khiên **chỉ ở Ngưỡng**, qua menu nghỉ. Ngoài Ngưỡng thì không đổi được.
4. Vũ khí đã nhận được giữ vĩnh viễn (lưu ở Ngưỡng). Chết không làm mất vũ khí.

**Bộ vũ khí chính**

5. Bảng dưới là giá trị khởi điểm ở cấp +0. Kiếm lấy từ asset hiện có `SO_WeaponItem`.
   - Đòn nhẹ là nhấn nút đánh.
   - Đòn nặng là **giữ nút đánh ≥ 0.35s rồi thả**.
   - Stamina bị trừ khi đòn ra.

| | **Kiếm** | **Giáo** | **Búa** | **Song kiếm** |
|---|---|---|---|---|
| Cầm | 1 tay (+ Khiên) | 1 tay (+ Khiên) | 2 tay | 2 tay |
| Tốc độ | Vừa | Vừa | Chậm | Nhanh |
| Tầm / góc | 2.0m / 160° | 3.2m / 40° (đâm thẳng) | 2.2m / 200° | 1.6m / 180° |
| Đòn nhẹ (sát thương / stamina) | 25 / 10 | 22 / 12 | 50 / 24 | 2 nhát × 12 / 6 |
| Số đòn trong combo nhẹ | 3 | 3 (đâm, đâm, quét) | 2 | 5 |
| Đòn nặng (sát thương / stamina) | Chém bổ: 45 / 20 | Đâm lao tới, xuyên mọi mục tiêu trên đường: 40 / 22 | Nện đất, vùng tròn bán kính 2.5m: 90 / 40 | Xoáy: 6 nhát × 8 / 25 |
| Đòn đặc biệt | **Phản kích:** đòn ngay sau parry thành công ×3 sát thương | **Đâm qua khiên:** đánh được khi đang giữ chặn; chặn vẫn hiệu lực | **Gồng:** không bị ngắt khi vung đòn nặng; đòn trúng làm quái mất thế ×2 | **Né ngắn:** thay cho khiên; lùi hoặc né ngang 2m, i-frame 0.2s, 6 stamina |
| Phòng thủ | Khiên | Khiên | Chặn bằng thân búa: giảm 50% sát thương, mỗi điểm sát thương chặn tốn 1.0 stamina, không parry | Không chặn, chỉ né ngắn |
| Duyên nguyên tố | Nước | Gió | Đá | Lửa |
| Bí mật tùy chọn mở được | — | Cắm giáo vào vách đất mềm làm bậc leo | Đập vách gỗ mục | Chém xuyên bụi gai dày |
| Cách nhận | Có từ đầu (MVP) | Tìm trong tàn tích | Rơi từ boss | Tặng hoặc mua ở một nền văn minh |

6. Đòn khi chạy và đòn khi lăn (đã có trong code) giữ nguyên cho mọi vũ khí. Mỗi vũ khí có dữ liệu `runningAttack` và `rollingAttack` riêng.

**Đã làm trong code (bộ animation Straight Sword, 2026-10-19)**

6b. **Đòn nặng theo ngữ cảnh.** Giữ nút đánh ≥ `heavyHoldThreshold` (0.35s) thì vào gồng (anim charge, rồi hold lặp), thả nút thì ra đòn. Đòn nhẹ ra khi thả nút trước ngưỡng, nên đòn nhẹ trễ bằng thời gian bấm (thường 60–120ms).
   - Ba biến thể, chọn theo lúc bắt đầu gồng: đứng yên → bản 1, trong cửa sổ combo → bản 2, đang chạy → bản 3. Thiếu bản nào thì dùng bản đầu.
   - Gồng đủ `fullChargeTime` (1.2s) thì ra bản "đầy": sát thương ×1.5, tầm +15%, cung +20°, stamina như bản thường.
   - Lăn hoặc bị đánh trúng thì hủy gồng. Hủy bằng lăn không tính là thả nút.
6c. **Lùi bước.** Nhấn nút lăn khi không giữ hướng đi thì lùi bước thay vì lăn: tốn 10 stamina (`backstepStaminaCost`), i-frame từ 0.04s đến 0.38s, lùi khoảng 2m.
   - Nhấn đánh trong cửa sổ từ 0.30s đến hết anim thì ra **đòn lùi bước** (`backstepAttack`), tương tự đòn khi lăn.
   - Kiếm 30 sát thương / 15 stamina; Đại kiếm 55 / 24; Song kiếm 24 / 10.
6d. **Dữ liệu từng vũ khí.** Kiếm (bộ main + off), Đại kiếm (bộ th, chặn bằng thân kiếm, không parry) và Song kiếm (bộ dw, chặn 50%, không parry, không có đòn nặng) đều đã có `WeaponItem`. Búa trong luật bảng trên dùng bộ th làm xương anim tạm. Đổi vũ khí chỉ để thử qua nút `CycleDebugWeapon` trên `PlayerCombat`; luật 3 vẫn đúng với bản chơi thật.

**Khiên** (chỉ dùng với Kiếm và Giáo; có từ đầu, MVP)

7. **Chặn:** giữ nút chặn.
   - Đòn thường vào phía trước (cung 120°) bị chặn 100% sát thương.
   - Mỗi điểm sát thương bị chặn tốn 0.8 stamina.
   - Khi đang chặn, tốc độ đi ×0.5 và stamina không hồi.
8. **Vỡ thế:** nếu stamina về 0 khi đang chặn, người chơi bị vỡ thế: choáng 1s và không chặn được. Phần sát thương mà stamina không đủ chặn sẽ xuyên qua.
9. **Parry:** nhấn nút chặn (không giữ). Cửa sổ parry là 0.15s tính từ lúc nhấn, mỗi lần thử tốn 10 stamina.
   - Đỡ trúng đòn trong cửa sổ: không nhận sát thương, quái mất thăng bằng 1.5s.
   - Đòn đầu tiên trúng quái đang mất thăng bằng là **phản kích** (Kiếm ×3, Giáo ×2).
10. **Đòn không chặn được:** đòn có cờ `unblockable` (ví dụ đòn tóm hoặc đòn có viền đỏ) xuyên qua cả chặn lẫn parry. Quái nào có đòn này thì do thiết kế quái quyết định.
11. Không thể chặn hay parry khi đang lăn, đang cast nguyên tố, đang nhắm cung hoặc đang bị choáng.
11b. **Phản hồi chặn theo sức đòn** (đã làm): đòn dưới 15 sát thương diễn anim chặn nhẹ, từ 15 chặn vừa, từ 30 chặn nặng. Đòn có `staggerAmount` ≥ 1 làm khiên bị đẩy: choáng 0.6s dù stamina còn. Sau mỗi lần chặn hoặc parry có 0.12s bất tử ngắn để một nhát vung nhiều collider không bị tính hai lần.
11c. **Nút:** chuột phải, phím Q hoặc vai trái tay cầm. Đòn đánh khi đang giữ chặn vẫn ra, và khiên tự dựng lại khi đòn xong nếu còn giữ nút. Parry hồi chiêu 0.4s giữa hai lần nhấn.

**Cung** (vũ khí phụ, nhận sớm ở Làng Mái Vá, MVP)

12. **Nhắm:** giữ nút nhắm, khi đó tốc độ đi ×0.5.
    - Nhấn nút đánh trong lúc nhắm để bắn: 15 sát thương, 8 stamina.
    - Giữ nút đánh 0.6s rồi thả là phát căng: 35 sát thương, 14 stamina.
    - Tầm 12m.
    - Đang lock-on thì tên tự nhắm vào mục tiêu; không lock-on thì bắn theo hướng nhân vật, có hỗ trợ nhắm nhẹ.
13. **Mũi tên:**
    - mang tối đa 30;
    - tên bắn trúng tường hoặc nền thì nằm lại và nhặt được; trúng quái thì mất;
    - có thể nhận thêm từ quái rơi ra, rương trong tàn tích, hoặc mua ở Bà Hạt (Làng Mái Vá) bằng tàn ý.

    Hết tên thì không bắn được, nhưng vẫn nhắm được.
14. **Tên nguyên tố:** khi đang nhắm, nhấn nút phép để phủ nguyên tố đang trang bị lên mũi tên kế tiếp.
    - Dùng để chiến đấu thì tốn 33 thanh nguyên tố, tức 1 nấc.
    - Dùng để giải đố thì miễn phí, theo luật 9 của `elemental-magic.md`.

    | Tên | Chiến đấu | Giải đố ở xa |
    |---|---|---|
    | Tên Lửa | Bỏng | Thắp đèn, đuốc ở xa; đốt tường tơ ở xa |
    | Tên Nước | Chậm | Tưới mầm cây ở xa; dập lửa ở xa |
    | Tên Đá | Choáng 0.5s, không phá giáp | Kích nút nhấn ở xa |
    | Tên Gió | Xuyên tối đa 3 mục tiêu | Xoay cánh quạt ở xa |

15. **Giải đố của Cung (cả đường chính):** bắn công tắc trên cao hoặc ở xa, bắn đứt dây treo để thả cầu hay thả vật. Vì Cung luôn được mang, câu đố trên đường chính có thể cần Cung (khác với luật 18 cho vũ khí chính).

**Duyên nguyên tố**

16. Dùng kỹ năng của nguyên tố có duyên với vũ khí chính đang cầm thì kỹ năng mạnh hơn (giá trị khởi điểm):

| Vũ khí | Duyên | Tăng cường |
|---|---|---|
| Kiếm | Nước | Vòng Nước: bán kính +30%, Chậm kéo dài 4s thay vì 3s |
| Giáo | Gió | Lốc Xoáy: bán kính rộng hơn 30% và tồn tại 4s thay vì 3s |
| Búa | Đá | Đá Văng: vòng cung rộng hơn 30%, Choáng 1.5s thay vì 1s |
| Song kiếm | Lửa | Yểm Than Hồng kéo dài 9s thay vì 6s |

Cung không có duyên với nguyên tố nào.

16b. **Yểm nguyên tố** (luật 11b của `elemental-magic.md`): khi thanh nguyên tố đầy, giữ nút phép để yểm nguyên tố đang trang bị lên vũ khí chính; yểm tiêu hết cả thanh. Yểm áp cho mọi vũ khí chính, không áp cho Cung.
   - Than Hồng: đòn trúng gây Bỏng.
   - Nước: đòn trúng gây Chậm.
   - Gió: tốc đánh ×1.25.
   - Đá: lớp đá bọc lưỡi vũ khí; đòn đầu tiên trúng quái làm lớp đá vỡ, gây ×2 sát thương và ×3 mất thế. Vung trượt thì lớp đá còn nguyên.

   Trên đòn nhiều nhát (combo Song kiếm, Xoáy), lớp đá vỡ ở nhát đầu tiên trúng quái và chỉ nhát đó được nhân. Búa yểm Đá thì hệ số mất thế nhân dồn thành ×6.

**Nạp thanh nguyên tố** (thay luật 7 cũ của `elemental-magic.md`)

17. Mỗi đòn trúng quái nạp **0.24 × sát thương gây ra**, làm tròn xuống, tối thiểu 1 (giảm từ 0.48 ngày 2026-10-28). Ví dụ:
    - Kiếm 25 sát thương nạp 6;
    - một nhát Song kiếm 12 nạp 2;
    - Búa 50 nạp 12.

    Áp dụng cho mọi đòn vũ khí, kể cả mũi tên thường. Sát thương từ kỹ năng nguyên tố không nạp thanh.

**Vai trò metroidvania**

18. Vũ khí chính **chỉ** mở bí mật tùy chọn: phần thưởng, lối tắt phụ, vật liệu. **Không bao giờ chặn đường chính**, vì vũ khí chính chỉ đổi được ở Ngưỡng. Lửa vẫn giữ "đốt tường tơ", Đá vẫn giữ "phá tường nứt" (xem `elemental-magic.md`), và không vũ khí nào làm trùng hai việc này.

**Cách người chơi học cơ chế** (theo brief: không có popup hướng dẫn, không có trang sách)

18b. Đòn nặng, chặn, parry, đâm qua khiên, né ngắn của Song kiếm và nhắm Cung đều **có sẵn ngay khi có vũ khí**, nhưng không có tutorial bật lên. Người chơi học qua NPC và thiết kế màn:
   - **Lão Mật** (canh cổng Làng Mái Vá) chỉ cách chặn và parry bằng lời và bằng một trận đấu tập ở cổng làng. Không bắt buộc.
   - Người trao Cung cũng chỉ cách nhắm và căng dây. Ngay sau đó có một công tắc ở xa chỉ bắn tên mới mở được.
   - Mỗi vũ khí mới nhận được đặt trước một tình huống cần đúng đòn đặc biệt của nó. Ví dụ: ngay sau khi lấy Búa có một nhóm quái đông để thử đòn nện đất.
   - Phần điều khiển trong menu tạm dừng luôn liệt kê nút bấm. Đây là chỗ duy nhất có chữ hướng dẫn.

**Nâng cấp**

19. Thợ rèn Bác Gai ở Làng Mái Vá nâng vũ khí chính, Khiên và Cung từ +0 lên +5. Giá trị khởi điểm:
    - lên cấp n tốn **150 × n tàn ý** và **n Mảnh Sắt Cũ**. Mảnh Sắt Cũ nhặt trong tàn tích hoặc mở được từ bí mật tùy chọn;
    - vũ khí chính và Cung: mỗi cấp +10% sát thương mọi đòn;
    - Khiên: mỗi cấp giảm 5% stamina tốn khi chặn, tức từ 0.8 xuống 0.6 ở cấp +5.

    Nâng cấp không đổi tốc độ, tầm hay tốn stamina của đòn.
20. Cấp nâng cấp đi theo từng món. Đổi sang vũ khí khác thì cấp không chuyển theo.

**Dữ liệu (ScriptableObject)**

21. Mọi chỉ số ở trên nằm trong ScriptableObject, theo đúng cách `WeaponItem` / `AttackData` đang làm. Không viết cứng số trong code. Dữ liệu cần thêm:
    - **Vũ khí:** loại cầm (1 hay 2 tay); combo đòn nặng; đòn đặc biệt; kiểu phòng thủ; nguyên tố duyên; loại bí mật mở được; bảng nâng cấp.
    - **Khiên:** cung chặn; stamina tốn cho mỗi điểm sát thương; cửa sổ parry; stamina mỗi lần parry; thời gian quái mất thăng bằng.
    - **Cung:** sát thương thường và căng; stamina; tầm; thời gian căng.
    - **Mũi tên:** số mang tối đa; giá mua.

    `WeaponClass` hiện có (`StraightSword`, `Greatsword`) cần thêm các loại mới. Cấu trúc dữ liệu cụ thể sẽ thành một ADR.

### States and Transitions

| Trạng thái | Mô tả | Chuyển sang |
|---|---|---|
| Sẵn sàng | Cầm vũ khí chính, không có hành động | → Đánh nhẹ · → Gồng đòn nặng (giữ nút đánh) · → Chặn (giữ nút chặn) · → Parry (nhấn nút chặn) · → Nhắm (giữ nút nhắm) |
| Đánh nhẹ | Đang ra một đòn trong combo | → Đánh nhẹ kế tiếp (trong cửa sổ combo) · → Sẵn sàng (hết đòn) · → Bị choáng (bị đánh, trừ khi Búa đang gồng) |
| Gồng đòn nặng | Đang giữ nút đánh | → Đòn nặng (thả nút sau ≥ 0.35s) · → Đánh nhẹ (thả trước 0.35s) |
| Đòn nặng | Đang ra đòn nặng | → Sẵn sàng (hết đòn) · → Bị choáng (bị đánh, trừ Búa) |
| Chặn | Giữ khiên hoặc chặn bằng thân búa | → Sẵn sàng (thả) · → Vỡ thế (stamina = 0) · → Đâm qua khiên (Giáo, nhấn đánh) |
| Parry | Cửa sổ 0.15s rồi phần hồi | → Sẵn sàng (hết phần hồi) · → Sẵn sàng + quái mất thăng bằng (parry trúng) |
| Nhắm | Giữ nút nhắm, cung căng sẵn | → Bắn (nhấn hoặc giữ rồi thả nút đánh) · → Sẵn sàng (thả nút nhắm) |
| Vỡ thế | Choáng 1s | → Sẵn sàng |
| Bị choáng | Chung với combat hiện có | → Sẵn sàng |

### Interactions with Other Systems

| Hệ thống | Vào | Ra | Ai sở hữu giao diện |
|---|---|---|---|
| Combat (`PlayerCombat`, `CharacterCombatBase`) | Input đánh, nhắm, chặn; trạng thái tấn công | Dữ liệu đòn đánh (`AttackData`) của vũ khí đang cầm | Weapons cung cấp dữ liệu; Combat thực thi |
| Stamina (`PlayerState`) | Stamina hiện có | Trừ stamina khi ra đòn, chặn, parry, né ngắn, bắn | Stamina |
| Lock-on | Mục tiêu đang khóa | Tự nhắm cho Cung | Lock-on |
| Elemental Magic | Nguyên tố đang trang bị; thanh nguyên tố | Nạp thanh theo sát thương (luật 17); tăng cường do duyên (luật 16); tên nguyên tố (luật 14); vũ khí mang lớp yểm (luật 16b) | Elemental Magic giữ thanh và kỹ năng; Weapons cung cấp hệ số duyên |
| Ngưỡng | Sự kiện nghỉ, menu nghỉ | Đổi vũ khí chính và Khiên; lưu danh sách vũ khí, cấp nâng cấp, số tên | Ngưỡng |
| Thợ rèn (Bác Gai) | Tàn ý, Mảnh Sắt Cũ | Cấp vũ khí mới | Thợ rèn / kinh tế |
| Cửa hàng (Bà Hạt) | Tàn ý | Mũi tên | Kinh tế |
| Enemy AI / EnemyState | Cờ `unblockable` trên đòn quái | Sát thương; mất thăng bằng (parry); mất thế (Búa) | Weapons gửi; Enemy nhận |
| Vật giải đố / bí mật | Loại bí mật mà vật cần | Sự kiện "đã mở bằng vũ khí X" hoặc "trúng tên" | Weapons |
| HUD | — | Số mũi tên, biểu tượng vũ khí, tâm nhắm | HUD |

## Edge Cases

- **Nếu stamina về 0 khi đang chặn:** vỡ thế, choáng 1s. Phần sát thương vượt quá số stamina còn lại (tính theo 0.8 stamina mỗi điểm) xuyên qua người chơi.
- **Nếu nhấn chặn khi stamina < 10:** parry không ra, nhưng vẫn vào chặn giữ nếu còn giữ nút.
- **Nếu parry trúng một đòn `unblockable`:** parry thất bại, người chơi nhận đủ sát thương.
- **Nếu hai quái đánh cùng lúc trong cửa sổ parry:** chỉ đòn đầu tiên bị parry. Đòn thứ hai bị chặn bình thường nếu còn giữ nút, không thì trúng.
- **Nếu đòn đến từ phía sau hoặc ngoài cung 120° khi đang chặn:** không chặn được, nhận đủ sát thương.
- **Nếu Búa đang gồng đòn nặng mà bị đánh:** vẫn nhận sát thương nhưng không bị ngắt đòn. Nếu chết thì vẫn chết.
- **Nếu thả nút đánh trước 0.35s:** ra đòn nhẹ, không ra đòn nặng.
- **Nếu stamina không đủ cho đòn nặng:** dùng luật hiện có: không ra đòn khi `HasStamina = false`. Nếu stamina > 0 nhưng thiếu, đòn vẫn ra và stamina về 0.
- **Nếu bắn khi hết tên:** không bắn; tâm nhắm nháy và có âm "rỗng".
- **Nếu tên nằm lại ở chỗ không tới được (vực, nước sâu):** tên mất.
- **Nếu chết:** số tên đang mang giữ nguyên; tên nằm rải trên bản đồ biến mất khi hồi sinh.
- **Nếu nhấn nút phép khi đang nhắm nhưng thanh < 33 và mục tiêu không phải vật giải đố:** tên không được phủ; bắn tên thường.
- **Nếu tên nguyên tố trúng vật giải đố ở xa:** vật nhận tác động như khi tương tác gần (luật 9 elemental-magic), thanh không bị trừ. Chỉ khi tên trúng quái thì thanh mới bị trừ 33, trừ lúc bắn.
- **Nếu đang cầm Búa hoặc Song kiếm mà gặp câu đố cần Khiên:** không được có câu đố như vậy, vì Khiên không có vai trò giải đố.
- **Nếu người chơi chưa có Giáo, Búa hay Song kiếm:** mọi đường chính vẫn đi được. Bí mật của vũ khí đó đứng yên và có gợi ý hình dạng (vách gỗ mục, đất mềm, gai dày) để người chơi nhớ quay lại.
- **Nếu Búa đánh vào giáp Bọ Giáp:** ×0.5, thay vì ×0.25 như các vũ khí khác. Giáp vẫn chỉ vỡ bởi Đá Văng; yểm Đá không phá giáp.
- **Nếu Song kiếm né ngắn trùng lúc lăn:** không cho; né ngắn và lăn dùng chung khóa hành động.
- **Nếu đổi vũ khí ở Ngưỡng sang Búa hoặc Song kiếm khi đang có Khiên:** Khiên tự gỡ ra và nằm trong kho.
- **Nếu nâng cấp đã ở +5:** thợ rèn không cho nâng thêm; nút nâng mờ đi.
- **Nếu không đủ tàn ý hoặc Mảnh Sắt Cũ:** không nâng được; hiện số còn thiếu.

## Dependencies

| Hệ thống | Hướng | Loại | Ghi chú |
|---|---|---|---|
| Combat (`PlayerCombat`, `WeaponItem`, `AttackData`) | Weapons phụ thuộc vào | Cứng | Có code, chưa có GDD. Cần thêm đòn nặng, chặn, parry, nhắm |
| Stamina (`PlayerState`, `SO_PlayerStats`) | Weapons phụ thuộc vào | Cứng | Có code, chưa có GDD |
| Lock-on | Weapons phụ thuộc vào | Mềm | Tự nhắm cho Cung |
| Elemental Magic (`design/gdd/elemental-magic.md`) | Hai chiều | Cứng | Đã sửa luật 7 (nạp theo sát thương) và thêm duyên, tên nguyên tố, Búa vào giáp |
| Ngưỡng (`design/gdd/rest-point.md`) | Weapons phụ thuộc vào | Cứng | Đổi vũ khí qua Menu Ngưỡng khi đang nghỉ |
| Lưu và tải (`design/gdd/save-load.md`) | Weapons phụ thuộc vào | Mềm | Vũ khí đã có, cấp nâng cấp, đang trang bị ghi ngay khi đổi; số tên ghi chậm nhất 1 giây |
| Thợ rèn và cửa hàng Làng Mái Vá (brief mục 3) | Weapons phụ thuộc vào | Cứng | Chưa có. Nâng cấp và bán tên |
| Enemy AI / EnemyState | Phụ thuộc vào Weapons | Cứng | Cần cờ `unblockable`, trạng thái mất thăng bằng và mất thế |
| Vật giải đố và bí mật | Phụ thuộc vào Weapons | Mềm | Chưa có hệ thống |
| HUD (`GamePlayView`) | Phụ thuộc vào Weapons | Mềm | Số tên, tâm nhắm |
| Input (`PlayerController.inputactions`) | Weapons phụ thuộc vào | Cứng | Thêm hành động Chặn và Nhắm; giữ nút đánh để ra đòn nặng |

## Acceptance Criteria

Ở tier `minimal` không có mục Formulas, nên các tiêu chí dưới đây kiểm các giá trị khởi điểm trong Core Rules.

- **GIVEN** người chơi đang ở ngoài Ngưỡng, **WHEN** mở bất kỳ menu nào, **THEN** không có lựa chọn đổi vũ khí chính; **WHEN** nghỉ ở Ngưỡng, **THEN** đổi được giữa các vũ khí đã nhận.
- **GIVEN** đang cầm Búa, **WHEN** mở menu đổi ở Ngưỡng, **THEN** ô Khiên bị khóa; **WHEN** đổi từ Kiếm có Khiên sang Búa, **THEN** Khiên tự gỡ.
- **GIVEN** Kiếm +0 và quái 100 máu, **WHEN** ra 1 đòn nhẹ trúng, **THEN** quái còn 75 máu, stamina giảm 10, thanh nguyên tố tăng 6.
- **GIVEN** Song kiếm +0, **WHEN** 1 nhát trúng quái, **THEN** quái mất 12 máu và thanh nguyên tố tăng 2.
- **GIVEN** Búa +0, **WHEN** đòn nhẹ trúng Bọ Giáp còn giáp, **THEN** sát thương là 25 (×0.5) và giáp không vỡ.
- **GIVEN** giữ nút đánh 0.2s rồi thả, **WHEN** đang cầm Kiếm, **THEN** ra đòn nhẹ; **GIVEN** giữ 0.4s rồi thả, **THEN** ra đòn nặng gây 45 và tốn 20 stamina.
- **GIVEN** Búa đang vung đòn nặng, **WHEN** bị quái đánh trúng, **THEN** đòn không bị ngắt và người chơi vẫn nhận sát thương.
- **GIVEN** đang chặn bằng Khiên với 100 stamina, **WHEN** đỡ một đòn 25 sát thương từ phía trước, **THEN** máu không đổi và stamina còn 80.
- **GIVEN** đang chặn với 10 stamina, **WHEN** đỡ đòn 25 sát thương, **THEN** vỡ thế, choáng 1s và nhận phần sát thương xuyên qua là 12.5.
- **GIVEN** đang chặn, **WHEN** bị đánh từ phía sau, **THEN** nhận đủ sát thương.
- **GIVEN** nhấn chặn, **WHEN** đòn quái chạm trong 0.15s, **THEN** không nhận sát thương và quái mất thăng bằng 1.5s; **WHEN** Kiếm đánh trúng quái đó trong lúc mất thăng bằng, **THEN** sát thương ×3.
- **GIVEN** quái ra đòn có cờ `unblockable`, **WHEN** người chơi chặn hoặc parry, **THEN** nhận đủ sát thương.
- **GIVEN** cầm Giáo và đang chặn, **WHEN** nhấn đánh, **THEN** ra đòn đâm trong khi chặn vẫn còn hiệu lực.
- **GIVEN** cầm Song kiếm, **WHEN** nhấn nút chặn, **THEN** né ngắn 2m với i-frame 0.2s và tốn 6 stamina.
- **GIVEN** có Cung và 30 tên, **WHEN** giữ nhắm và nhấn đánh, **THEN** bắn 1 tên gây 15 sát thương, tốn 8 stamina, còn 29 tên.
- **GIVEN** bắn tên trúng tường, **WHEN** đi tới chỗ tên, **THEN** nhặt lại được, số tên +1; **GIVEN** tên trúng quái, **THEN** không nhặt lại được.
- **GIVEN** có 0 tên, **WHEN** bắn, **THEN** không có tên nào bay ra và có phản hồi "rỗng".
- **GIVEN** trang bị Lửa, thanh 40, đang nhắm vào quái, **WHEN** nhấn nút phép rồi bắn trúng, **THEN** quái bị Bỏng và thanh còn 7.
- **GIVEN** trang bị Lửa, thanh 0, nhắm vào đèn ở xa, **WHEN** phủ tên rồi bắn trúng đèn, **THEN** đèn sáng và thanh vẫn là 0.
- **GIVEN** công tắc ở trên cao chỉ chạm được bằng tên, **WHEN** bắn trúng, **THEN** công tắc kích hoạt; câu đố này giải được với mọi vũ khí chính.
- **GIVEN** cầm Búa, trang bị Đá, **WHEN** dùng Đá Văng, **THEN** vòng cung rộng hơn 30% và Choáng 1.5s; **GIVEN** cầm Kiếm, **THEN** vòng cung và Choáng như mặc định.
- **GIVEN** cầm Song kiếm, **WHEN** yểm Than Hồng, **THEN** lớp yểm kéo dài 9s; **GIVEN** cầm Kiếm, **THEN** 6s.
- **GIVEN** chưa có Búa, **WHEN** đi hết đường chính của mọi vùng, **THEN** không có chỗ nào bị chặn vì thiếu Búa (tương tự cho Giáo và Song kiếm).
- **GIVEN** Kiếm +2, 450 tàn ý và 3 Mảnh Sắt Cũ, **WHEN** nâng lên +3 ở Bác Gai, **THEN** tàn ý còn 0, Mảnh Sắt Cũ còn 0, đòn nhẹ gây 32 (25 × 1.3, làm tròn xuống).
- **GIVEN** vũ khí đã +5, **WHEN** mở menu thợ rèn, **THEN** không nâng thêm được.
- **GIVEN** người chơi vừa nhận một vũ khí hoặc Cung, **WHEN** quan sát màn hình, **THEN** không có popup hướng dẫn nào; các nút chỉ được liệt kê trong phần điều khiển của menu tạm dừng.
- **GIVEN** chết khi còn 12 tên, **WHEN** hồi sinh ở Ngưỡng, **THEN** vẫn còn 12 tên và mọi vũ khí cùng cấp nâng cấp vẫn còn.

## Visual/Audio Requirements

> `art-director` và `audio-director` không được hỏi ý kiến (Solo mode). Chưa có art bible.

- **Silhouette:** mỗi vũ khí có hình dáng rõ từ góc nhìn isometric. Giáo dài và mảnh, Búa đầu to, Song kiếm hai lưỡi ngắn, Khiên tròn đeo tay trái, Cung đeo sau lưng khi không dùng.
- **Vệt chém (swing trail):** mỗi vũ khí một kiểu. Kiếm cong vừa, Giáo thẳng, Búa rộng và dày, Song kiếm nhiều vệt mảnh.
- **Ánh duyên:** khi dùng kỹ năng có duyên, vũ khí lóe màu của nguyên tố đó (bảng màu theo `elemental-magic.md`).
- **Phản hồi:**
  - chặn: tia lửa và âm kim loại đục;
  - parry: chớp sáng trắng, dừng hình 0.08s, âm sắc cao;
  - vỡ thế: khiên bật ra, âm gãy;
  - Búa gồng: viền sáng quanh người.
  Tất cả dùng Feel (MMFeedbacks) và haptics.
- **Cung:**
  - dây căng có âm kéo tăng dần;
  - phát căng đủ có tiếng "tách";
  - tên nguyên tố có vệt màu bay theo.
- **Thợ rèn:** tiếng búa gõ và tia lửa khi nâng cấp xong.

📌 **Asset Spec** — mục Visual/Audio đã có nội dung. Sau khi art bible được duyệt, chạy `/asset-spec system:weapons`.

## Game Feel

- **Độ trễ input:** đòn nhẹ ra trong ≤ 100ms. Đòn nặng có ngưỡng giữ 0.35s và có rung tay cầm nhẹ báo đã sẵn sàng.
- **Trọng lượng:**
  - Kiếm cân bằng;
  - Giáo nhanh nhưng hẹp, phải nhắm đúng hướng;
  - Búa chậm và nặng, có dừng hình khi trúng (0.1s đòn nhẹ, 0.15s đòn nặng);
  - Song kiếm liên tục, mỗi nhát dừng hình rất ngắn (0.03s).
- **Parry:** cửa sổ 0.15s phải cảm thấy chặt nhưng công bằng. Khi parry trúng, thời gian chậm 0.2× trong 0.1s để người chơi kịp thấy.
- **Chặn:** giữ chặn có cảm giác "cắm chân". Đẩy lùi (knockback) nhẹ, tùy theo sát thương bị chặn.
- **Cung:** căng dây mượt; tâm nhắm thu hẹp dần trong 0.6s. Bắn có lực giật nhẹ.
- **Không được làm "mềm" vũ khí chậm:** Búa không có buffer combo dài hơn Kiếm. Người chơi phải cam kết khi vung.

## UI Requirements

- **HUD:**
  - biểu tượng vũ khí chính và Khiên (nếu có) ở góc dưới;
  - số mũi tên cạnh biểu tượng Cung;
  - tâm nhắm hiện khi giữ nút nhắm.
- **Menu Ngưỡng:**
  - ô vũ khí chính, ô Khiên (khóa khi vũ khí hai tay), danh sách vũ khí đã có;
  - hiện bảng so sánh chỉ số ngắn: tốc độ, tầm, sát thương, phòng thủ, duyên.
- **Menu thợ rèn (Bác Gai):** cấp hiện tại và cấp kế, sát thương trước và sau, giá tàn ý và Mảnh Sắt Cũ, số còn thiếu.
- **Cửa hàng Bà Hạt:** mua tên theo bó, hiện số đang có và số tối đa (30).

📌 **UX Flag — Weapons**: hệ thống này có yêu cầu UI (HUD vũ khí, tâm nhắm, menu Ngưỡng, menu thợ rèn, cửa hàng). Chạy `/ux-design` cho các màn hình này trước khi viết story có UI.

## Cross-References

| Tài liệu | Mục | Liên hệ |
|---|---|---|
| `design/gdd/elemental-magic.md` | Luật 7 (nạp thanh), 9 (giải đố miễn phí), 11 (kỹ năng), 17 (yếu/kháng, Bọ Giáp) | Đã sửa luật 7 theo luật 17 ở đây; duyên tăng cường kỹ năng; tên nguyên tố dùng luật 9; Búa vào giáp ×0.5 |
| `design/game-brief.md` | MVP mục 1 (combat), 3 (nâng cấp ở làng); "Định hướng khám phá" | Kiếm, Khiên, Cung thuộc MVP; nâng cấp vũ khí thêm vào thợ rèn; dạy cơ chế qua NPC và thiết kế màn, không popup (luật 18b) |
| `design/narrative/lore/world-foundation.md` | Mục 4.0 (Làng Mái Vá: Bác Gai, Bà Hạt), 1.2 (tàn ý) | Thợ rèn, cửa hàng tên, tiền tệ |
| `Assets/Scripts/CoreGame/Item/Weapon/WeaponItem.cs` | `WeaponItem`, `AttackData`, `WeaponClass` | Mở rộng dữ liệu cho đòn nặng, phòng thủ, duyên |
| `Assets/SO/Weapon/SO_WeaponItem.asset` | Kiếm | Nguồn số liệu khởi điểm của Kiếm |

## Open Questions

| Câu hỏi | Người quyết | Hạn |
|---|---|---|
| Ai trao Cung ở Làng Mái Vá (Lão Mật hay Chị Mỏ) và qua tình huống nào | Người dùng | Khi viết story Cung |
| Giáo nằm ở tàn tích nào; Búa rơi từ boss nào; Song kiếm thuộc nền văn minh nào | Người dùng + level-designer | Khi lên bản đồ thế giới |
| Quái nào có đòn `unblockable` (vd. đòn tóm của boss vùng; tạm, chờ thiết kế quái thật) | Người dùng + thiết kế quái | Trước story parry |
| Mọi con số (sát thương, stamina, parry 0.15s, giá nâng cấp 150×n) chưa qua cân bằng; economy-designer chưa xem giá | Người dùng, sau playtest | Sau bản combat Kiếm + Khiên chơi được |
| Có Khiên thứ hai (khiên nặng hoặc nhẹ) không | Người dùng | Sau MVP |
| Tên nằm lại trên bản đồ có giới hạn số lượng không (hiệu năng) | Lập trình | Khi làm story Cung |
| Brief mục 3 chỉ nói nâng tim và stamina; giờ thêm nâng vũ khí ở thợ rèn — đã ghi vào brief chưa | Người dùng | Khi cập nhật brief tiếp theo |
