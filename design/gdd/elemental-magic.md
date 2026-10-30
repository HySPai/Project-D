# Elemental Magic (Sức mạnh nguyên tố)

> **Status**: In Design
> **Author**: người dùng + Claude (solo, không có specialist)
> **Last Updated**: 2026-10-31
> **Last Verified**: 2026-10-28
> **Implements Pillar**: không có file pillar ở tier `minimal`; phục vụ thể loại trong `design/game-brief.md` (soulslike + metroidvania + giải đố)

> **Workflow**: `minimal`. GDD này là tự nguyện; brief vẫn là tài liệu thiết kế chính. Chỉ có 5 mục chuẩn và Summary. Không có mục Formulas: các con số được ghi thẳng trong Core Rules và đều là giá trị khởi điểm, đặt trong ScriptableObject.
> **Specialist**: `game-designer`, `systems-designer`, `ai-programmer`, `art-director` và `qa-lead` không được hỏi ý kiến (Solo mode). Cần review thủ công trước khi vào production.

## Summary

Hiệp sĩ chó thu thập 4 nguyên tố (Gió, Lửa, Nước, Đá) để chiến đấu, giải đố và mở lối kiểu metroidvania. Mỗi nguyên tố có 1 kỹ năng chiến đấu và 1 kiểu yểm lên vũ khí. Kỹ năng tốn 1 nấc thanh nguyên tố, còn yểm cần thanh đầy; thanh nạp bằng đòn đánh. Giải đố và mở lối dùng nút tương tác riêng, miễn phí.

> **Quick reference** — Layer: `Feature` · Priority: `Full Vision` (sau MVP; MVP chỉ có lướt thường) · Key deps: `Combat, Stamina, Lock-on, Lướt (MVP), Ngưỡng, Enemy AI, HUD`
> Category: Gameplay · Layer: Feature · Priority: sau MVP — cả ba suy ra từ brief, vì tier này không có systems index.

## Overview

Elemental Magic là hệ thống cho hiệp sĩ chó dùng 4 nguyên tố, tàn dư của Mẫu Địa (xem `design/narrative/lore/elements.md`), vào 3 việc:
- **Chiến đấu:** 1 nút phép. Nhấn để bắn kỹ năng (tốn 1 nấc thanh), giữ để yểm nguyên tố lên vũ khí (cần thanh đầy).
- **Giải đố:** nhấn nút tương tác để tác động lên vật trong môi trường.
- **Mở lối metroidvania:** có những khu chỉ vào được khi đã có nguyên tố phù hợp.

Người chơi nhận từng nguyên tố bằng cách hạ boss côn trùng đang chiếm nơi giữ nó, rồi vượt thử thách lòng trung thành của thực thể canh giữ. Gió luôn là nguyên tố đầu tiên; ba cái còn lại nhận theo thứ tự tùy ý. Thiếu hệ thống này, thế giới sẽ không có cổng chặn để tạo nhịp quay lại kiểu metroidvania, và combat chỉ còn mỗi kiếm.

## Detailed Design

### Core Rules

**Nhận nguyên tố**
1. Thứ tự: **Gió cố định là nguyên tố đầu tiên.** Sau khi có Gió, Lửa, Nước và Đá nhận được theo bất kỳ thứ tự nào.
2. Mỗi nguyên tố nhận qua 2 bước, ở nơi giữ nó:
   - (a) hạ boss côn trùng đang chiếm nơi đó;
   - (b) vượt thử thách phi chiến đấu của thực thể canh giữ.

   | Nguyên tố | Nơi | Thực thể | Thử thách (mặt trung thành) |
   |---|---|---|---|
   | Gió | Đỉnh Hạc | Tiếng Vọng | Được gọi thì quay lại |
   | Lửa | Thành Lũy Lửng | Lão Than | Giữ ấm |
   | Nước | Đầm Sen | Cụ Sen Cổ | Chờ đợi |
   | Đá | Thung Rùa | Đá Canh | Đứng vững |

   Thiết kế cụ thể cho boss và thử thách thuộc từng màn chơi (ngoài phạm vi GDD này).
3. Nguyên tố đã nhận thì giữ vĩnh viễn và được lưu ở Ngưỡng cùng tiến độ. Không bị mất khi chết.
4. Lướt (MVP) là mầm của Gió. Lướt dùng **nút lướt**, miễn phí với thanh nguyên tố (chỉ tốn stamina như lướt thường).
   - **Lướt thường** (trước khi có Gió, hoặc khi Lướt Gió đang hồi): **không** miễn nhiễm sát thương.
   - **Lướt Gió** (sau khi có Gió): miễn nhiễm sát thương (i-frame) suốt quãng lướt và đẩy lùi quái trên đường đi 2m, không gây sát thương. Mỗi lần dùng xong, Lướt Gió hồi lại sau **4s** (giá trị khởi điểm). Trong 4s đó, nhấn lướt ra lướt thường.

**Điều khiển**

5. Mỗi lúc chỉ trang bị **1 nguyên tố**. Có 1 nút phép (cast) và 1 thao tác đổi nguyên tố:
   - nhấn nhanh: đổi sang nguyên tố kế tiếp;
   - giữ: mở vòng chọn. Trong lúc mở, thời gian chậm còn 0.3× (giá trị khởi điểm).
6. **Nút phép** và **nút tương tác** là hai nút riêng. Cả hai dùng nguyên tố đang trang bị:
   - **nút phép, nhấn nhanh** (thả trước 0.35s): **bắn kỹ năng chiến đấu**, kể cả khi đang đứng trước vật giải đố;
   - **nút phép, giữ ≥ 0.35s rồi thả:** **yểm** nguyên tố lên vũ khí chính (xem luật 11b). Ngưỡng 0.35s lấy giống ngưỡng giữ của đòn nặng (`weapons.md` luật 5);
   - **nút tương tác:** nếu trước mặt có vật hợp với nguyên tố đó (đèn, tường tơ, nút nhấn, cánh quạt…) thì tác động lên vật. Nếu không có vật nguyên tố, nút dùng cho các tương tác thường (nói chuyện, Ngưỡng, nhặt đồ), do hệ Input/tương tác quyết định.

**Tài nguyên — thanh nguyên tố**

7. Có một thanh nguyên tố chung, riêng biệt với stamina. Giá trị khởi điểm:
   - tối đa 100;
   - mỗi đòn vũ khí trúng quái nạp **0.24 × sát thương gây ra** (làm tròn xuống, tối thiểu 1). Kiếm 25 sát thương nạp 6, tức khoảng 6 đòn cho một kỹ năng; Song kiếm 12 nạp 2; Búa 50 nạp 12. Sát thương từ kỹ năng nguyên tố không nạp. Hệ số giảm từ 0.48 xuống 0.24 (2026-10-28) vì mana hồi quá nhanh (xem `weapons.md` luật 17);
   - mỗi kỹ năng chiến đấu tốn 33, tức khoảng 3 lần dùng khi đầy;
   - mỗi kỹ năng có **thời gian hồi riêng** sau khi dùng: Lốc Xoáy 2s, Cầu Lửa 1s, Vòng Nước 1.5s, Đá Văng 1.5s. Đồng hồ hồi của từng nguyên tố chạy độc lập, kể cả khi đang trang bị nguyên tố khác. Yểm không có thời gian hồi (đã bị giới hạn bởi thanh đầy);
   - yểm chỉ dùng được khi thanh **đầy (100)** và tiêu hết cả thanh. Người chơi chọn giữa 3 kỹ năng hoặc 1 lần yểm;
   - nghỉ ở Ngưỡng thì hồi đầy.
8. Thanh nguyên tố **không** tự hồi theo thời gian, và **không** nạp khi đánh trúng vật vô tri.
9. Tương tác giải đố và mở lối (qua nút tương tác, hoặc Lướt Gió) tốn **0**: không cần thanh, và vẫn dùng được khi thanh rỗng.
10. Kỹ năng chiến đấu và yểm **không tốn stamina**, nhưng vẫn bị chặn khi đang lăn, đang bị choáng hoặc đã chết.

**Bộ kỹ năng**

11. Mỗi nguyên tố có 1 kỹ năng chiến đấu, các tương tác giải đố và các chướng ngại nó mở được:

| | Gió (Tiếng Vọng) | Lửa (Lão Than) | Nước (Cụ Sen Cổ) | Đá (Đá Canh) |
|---|---|---|---|---|
| **Kỹ năng chiến đấu** | *Lốc Xoáy*: tạo một cơn lốc phía trước, hút nhẹ quái vào tâm và giữ chúng không tấn công được | *Cầu Lửa*: bắn một quả cầu lửa bay thẳng về phía trước, nổ khi chạm quái hoặc vật, gây Bỏng | *Vòng Nước*: sóng quanh người, đẩy lùi và gây Chậm | *Đá Văng*: làm đất phía trước trồi lên thành các cột đá nhọn xếp theo vòng cung trước mặt, phá giáp và gây Choáng 1s |
| **Yểm lên vũ khí** (luật 11b) | *Gió*: tăng tốc đánh | *Than Hồng*: đòn trúng gây Bỏng (thiêu đốt) | *Nước*: đòn trúng gây Chậm | *Đá*: lớp đá bọc lưỡi; đòn đầu tiên trúng quái làm đá vỡ, thành một đòn sát thương lớn và tích thanh thế nhanh hơn |
| **Giải đố** | Xoay cánh quạt, cối xay; thổi vật nhẹ; đưa lửa lan theo gió | Thắp đèn, lò, đuốc; ánh lửa hút Ngài Đèn tới | Đổ đầy hoặc rút nước để nâng hạ vật nổi; tưới mầm cây mọc thành bậc | Dựng cột đá để đè nút nhấn hoặc làm bệ đứng |
| **Mở lối** | Lướt Gió qua vực rộng (nút lướt, luật 4); cưỡi luồng gió lên cao | Đốt tường tơ, rễ khô | Đi trên mặt nước (lá sen nổi dưới chân) | Phá tường nứt; dựng cột đá lên mỏm cao |

11a. Số khởi điểm của kỹ năng chiến đấu, ở mức không có duyên. Để so sánh: một đòn nhẹ Kiếm gây 25, đòn nặng gây 45. Sát thương ở đây là sát thương gốc, trước khi nhân yếu ×1.5 hay kháng ×0.5.

| Kỹ năng | Sát thương | Hình dạng / tầm | Hiệu ứng | Khác |
|---|---|---|---|---|
| Lốc Xoáy | 8 mỗi giây, tối đa 24 | Lốc bán kính 2m, đặt cách 3m phía trước, tồn tại 3s | Hút nhẹ quái vào tâm; quái trong lốc không tấn công được | Boss chỉ bị hút, vẫn tấn công được |
| Cầu Lửa | 30 | Đạn bay thẳng 15 m/s, tầm tối đa 10m. Nổ khi chạm quái hoặc vật, bán kính nổ 1.5m | Bỏng | Để lại vùng cháy bán kính 1.5m trong 3s (dùng cho combo 12, 13) |
| Vòng Nước | 20 | Vòng tròn quanh người, bán kính 3m | Chậm, đẩy lùi 2m | Để lại vùng ướt 5s (dùng cho combo 14, 15) |
| Đá Văng | 35 | Vùng đất hình vòng cung 90° phía trước, tầm 4m; đất trồi lên thành cột đá, không có đạn bay ra | Choáng 1s, phá giáp | Khởi động 0.35s. Sát thương tính ngay lúc đòn thả ra (sau khởi động), cho mọi quái trong vùng cung; các cột đá trồi lần lượt chỉ là phần nhìn, không làm sát thương trễ theo hàng |

Đạn Cầu Lửa nổ ngay khi chạm tường. Nếu bay hết 10m mà không chạm gì thì tự nổ tại chỗ.

Lướt Gió không phải kỹ năng của nút phép. Nó là bản nâng cấp miễn phí của nút lướt (luật 4).

**Yểm nguyên tố lên vũ khí**

11b. Giữ nút phép ≥ 0.35s rồi thả thì yểm nguyên tố đang trang bị lên **vũ khí chính**. Giá trị khởi điểm:
   - mọi lớp yểm hiện thành một lớp nguyên tố bao bọc lưỡi vũ khí (lửa, nước, gió hoặc đá);
   - cần thanh đầy (100), tiêu hết cả thanh về 0, không tốn stamina;
   - mỗi lúc chỉ có **1 lớp yểm**. Yểm lớp mới thì lớp cũ mất ngay, không hoàn thanh;
   - đổi nguyên tố đang trang bị không gỡ lớp yểm đang có;
   - Cung **không** nhận yểm; Cung dùng tên nguyên tố riêng (`weapons.md` luật 14);
   - yểm không phải tương tác giải đố: đòn vũ khí đang yểm không kích hoạt vật giải đố hay cổng chặn.

   | Yểm | Thời gian | Tác dụng |
   |---|---|---|
   | Than Hồng (Lửa) | 6s | Mỗi đòn vũ khí trúng gây Bỏng (thiêu đốt) |
   | Nước | 6s | Mỗi đòn vũ khí trúng gây Chậm |
   | Gió | 6s | Tốc độ đánh ×1.25: anim đòn nhẹ, đòn nặng và cửa sổ combo đều nhanh hơn. Stamina mỗi đòn không đổi |
   | Đá | Tới khi lớp đá vỡ, tối đa 6s | Lớp đá bọc lưỡi vũ khí. Đòn đầu tiên (nhẹ hoặc nặng) trúng quái làm lớp đá vỡ: đòn đó gây sát thương ×2 và tích thanh thế của quái ×3. Vung trượt hoặc trúng vật vô tri thì lớp đá còn nguyên |

11c. Yểm tôn trọng bảng yếu/kháng (luật 17):
   - quái **kháng** nguyên tố của lớp yểm thì không dính hiệu ứng. Riêng yểm Đá thì đòn chỉ gây sát thương và tích thế như đòn thường;
   - quái **yếu** nguyên tố đó thì vẫn chỉ dính hiệu ứng như bình thường. Yểm không cộng thêm sát thương nguyên tố, nên hệ số ×1.5 không áp vào yểm;
   - yểm Gió chỉ tác động lên người chơi, nên không xét yếu/kháng.

11d. Đòn vũ khí đang yểm vẫn nạp thanh theo luật 7, tính trên sát thương thực gây ra (đòn yểm Đá ×2 thì nạp gấp đôi). Sát thương Bỏng theo thời gian không nạp thanh.

11e. **Thanh thế (poise) của quái:** mỗi đòn trúng làm quái mất thế. Mất đủ ngưỡng thì quái bị Choáng; boss có ngưỡng cao hoặc miễn. Thanh này do Enemy AI sở hữu (story `refactor-foundation/005`). Yểm Đá ×3 và Búa ×2 (`weapons.md`) đều là hệ số nhân lên lượng mất thế này. Nếu cùng lúc có cả hai thì nhân dồn, thành ×6.

**Combo cố định** — chỉ có đúng 5 combo dưới đây, không có hệ trạng thái tự do:

12. Lửa + Gió: Lốc Xoáy chạm vùng cháy của Cầu Lửa hoặc vật đang cháy thì thành **lốc lửa**: quái trong lốc bị Bỏng, và lốc mang lửa theo, đốt các vật có cờ "cháy được" nó đi qua.
13. Nước + Lửa: Vòng Nước chạm vật hoặc vùng đang cháy thì dập lửa và tạo hơi nước 4s. Quái trong hơi nước mất dấu hiệp sĩ (thoát khỏi việc bị nhắm).
14. Nước + Đá: Đá Văng làm cột đá trồi lên trong vùng ướt thì tạo bùn 5s, gây Chậm cho quái đứng trong đó.
15. Gió + Nước: Lốc Xoáy chạm vùng nước (vùng ướt của Vòng Nước, vũng, mặt hồ) thì hút nước thành mưa bụi, dập mọi đèn và lửa trong bán kính 4m quanh lốc.
16. Lửa + Đá: cột đá dựng trong lúc vũ khí đang yểm Than Hồng thành đá nung 5s, gây Bỏng khi quái chạm vào.

**Cách người chơi học kỹ năng và combo** (theo brief: không có popup hướng dẫn, không có trang sách)

- Khi nhận một nguyên tố, thử thách của thực thể giữ nó buộc người chơi dùng kỹ năng chiến đấu và ít nhất một tương tác giải đố của nguyên tố đó.
- Mỗi combo được dạy bằng thiết kế màn: lần đầu combo cần thiết, màn đặt sẵn đủ hai yếu tố ngay trước chướng ngại. Ví dụ: vật đang cháy cạnh luồng gió, trước một vách tơ dày.
- NPC ở Làng Mái Vá có thể gợi ý mơ hồ về combo khi người chơi đã có đủ hai nguyên tố.

**Yếu và kháng của quái côn trùng**

17. Quy tắc: con quái nào khao khát nguyên tố nào thì kháng nguyên tố đó (khớp với lore). **Ngoại lệ duy nhất là Bọ Giáp**: nó khao khát Đá nhưng lại yếu Đá. Vỏ của nó gặm từ đá thường, nên đá nguyên tố nặng hơn làm vỏ nứt. Lore gọi đây là bản "đá chỉ nặng". Bọ Giáp kháng Gió.
    - Đánh vào điểm yếu: sát thương kỹ năng ×1.5 và luôn gây hiệu ứng.
    - Đánh vào kháng: sát thương ×0.5 và không gây hiệu ứng.
    - Giá trị khởi điểm.

| Quái | Yếu | Kháng | Ghi chú |
|---|---|---|---|
| Seed | Lửa | — | |
| Quái thường (tạm, chờ thiết kế quái thật) | Lửa | — | Dòng mẫu để test hệ yếu/kháng; quái demo hiện có trong Unity dùng dòng này |
| Boss vùng (tạm, chờ thiết kế quái thật) | Lửa | — | Dòng mẫu; mỗi boss thật sẽ có yếu/kháng riêng |
| Kiến Thợ | Nước | Lửa | |
| Ong Chúa Rỗng | Lửa | Nước | |
| Bọ Giáp | Đá | Gió | Ngoại lệ của luật 17 (xem trên). Giáp chỉ vỡ bởi Đá Văng (yểm Đá không phá giáp); đòn vũ khí thường vào giáp chỉ gây ×0.25 (Búa ×0.5, xem `weapons.md`) |
| Ngài Đèn | Đá | Gió | Bị lửa hút tới (dùng làm mồi) |
| Turret (máy) | — | — | Không yếu, không kháng nguyên tố nào |

### States and Transitions

| Trạng thái | Mô tả | Chuyển sang |
|---|---|---|
| Chưa có nguyên tố | Trước khi có Gió. Nút phép và vòng chọn bị ẩn; lướt là lướt thường, không i-frame | → Sẵn sàng, khi nhận Gió |
| Sẵn sàng | Đang trang bị 1 nguyên tố, có thể cast | → Đang cast (nhấn phép khi thanh ≥ 33; giữ phép khi thanh đầy; nhấn tương tác trước vật hợp nguyên tố) · → Đang chọn (giữ nút đổi) |
| Đang chọn | Vòng chọn mở, thời gian chậm 0.3× | → Sẵn sàng (thả nút) · → Sẵn sàng, đóng vòng (bị trúng đòn) |
| Đang cast | Đang ra kỹ năng, tương tác hoặc anim yểm. Không thể đổi nguyên tố | → Sẵn sàng (cast xong) · → Sẵn sàng (bị choáng, cast hủy, thanh không hoàn) |
| Bị khóa | Đang lăn, bị choáng, đã chết hoặc đang trong hội thoại | → Sẵn sàng (khi hết điều kiện khóa) |

Lớp yểm trên vũ khí chạy song song với các trạng thái trên. Lớp yểm có 2 trạng thái: **Không yểm** và **Đang yểm (nguyên tố X, còn t giây)**. Đang yểm trở về Không yểm khi hết giờ, khi lớp đá vỡ (đòn yểm Đá trúng quái), khi chết, khi nghỉ ở Ngưỡng hoặc khi yểm lớp mới (lớp mới thay ngay).

Hiệu ứng trên quái:

| Hiệu ứng | Thời gian | Tác dụng |
|---|---|---|
| Bỏng | 4s | 5 sát thương mỗi giây |
| Chậm | 3s | Tốc độ di chuyển và tấn công ×0.6 |
| Choáng | 1s | Không hành động được |

Các hiệu ứng **không cộng dồn** với chính nó: dính lại thì chỉ làm mới thời gian. Đây là giá trị khởi điểm.

### Interactions with Other Systems

| Hệ thống | Vào hệ thống này | Ra từ hệ thống này | Ai sở hữu giao diện |
|---|---|---|---|
| Combat (`PlayerCombat`) | Sự kiện đòn vũ khí trúng quái và sát thương gây ra (để nạp thanh); đòn đang diễn ra | Lệnh khóa cast khi đang tấn công; lớp yểm trên vũ khí (hiệu ứng mỗi đòn, tốc đánh ×1.25, đòn Đá ×2 sát thương và ×3 thế) | Combat |
| Weapons (`design/gdd/weapons.md`) | Vũ khí chính đang cầm (duyên nguyên tố); đang nhắm cung | Tăng cường kỹ năng có duyên (Song kiếm: yểm Than Hồng 9s); lớp yểm trên vũ khí chính (Cung không nhận); tên nguyên tố (1 nấc thanh khi chiến đấu, miễn phí khi giải đố) | Weapons cung cấp hệ số duyên; Elemental Magic giữ thanh và kỹ năng |
| Stamina (`PlayerState`) | — | Không tiêu stamina (luật 10) | Stamina |
| Lock-on | Mục tiêu đang khóa (hướng của Cầu Lửa, Đá Văng và vị trí đặt Lốc Xoáy) | — | Lock-on |
| Lướt (MVP) | Nút lướt; lướt thường không i-frame | Khi đã có Gió: Lướt Gió có i-frame và đẩy lùi, hồi 4s mỗi lần dùng | Lướt |
| Ngưỡng (điểm nghỉ) | Sự kiện nghỉ | Hồi đầy thanh; lưu danh sách nguyên tố đã có | Ngưỡng |
| Enemy AI / EnemyState | Bảng yếu và kháng của từng loài (data) | Sát thương nguyên tố và hiệu ứng (Bỏng, Chậm, Choáng); hệ số mất thế ×3 của yểm Đá | Elemental Magic gửi; Enemy nhận và tự xử lý, giữ thanh thế |
| Vật giải đố và cổng chặn | Loại nguyên tố mà vật cần | Sự kiện "đã tác động bằng nguyên tố X" | Elemental Magic |
| HUD (`GamePlayView`) | — | Giá trị thanh, nguyên tố đang chọn, vòng chọn | HUD |
| Input | Nút phép (nhấn / giữ), nút đổi, nút tương tác | — | Input |

Cách triển khai kỹ thuật (event hay controller trung tâm, ScriptableObject cho dữ liệu nguyên tố) sẽ thành một ADR. GDD này không quyết định.

## Edge Cases

- **Nếu thanh nguyên tố không đủ 33 mà nhấn phép:** không cast. Thanh rung nhẹ và phát âm báo; không trừ thanh.
- **Nếu nhấn phép khi kỹ năng của nguyên tố đang trang bị còn đang hồi:** không cast, không trừ thanh. Đổi sang nguyên tố khác đã hồi xong thì bắn được ngay.
- **Nếu nghỉ ở Ngưỡng hoặc hồi sinh:** mọi đồng hồ hồi về 0.
- **Nếu nhấn nút tương tác trước một vật nguyên tố nhưng nguyên tố đang trang bị không hợp với vật:** không có gì xảy ra với vật; vật chỉ phát một phản hồi nhỏ (âm "trượt", hoa văn trên vật lóe nhẹ). Người chơi tự đọc hoa văn màu trên vật để biết cần nguyên tố nào. Không dùng kỹ năng chiến đấu thay thế. Biểu tượng gợi ý chỉ hiện khi bật tùy chọn trợ năng.
- **Nếu đứng trước vật giải đố mà nhấn nút phép:** bắn kỹ năng như bình thường, tốn thanh. Kỹ năng không thay cho tương tác: Cầu Lửa nổ vào đèn không thắp đèn. Muốn tác động lên vật phải dùng nút tương tác (luật 6).
- **Nếu vật nguyên tố đứng cạnh Ngưỡng hoặc NPC:** nút tương tác ưu tiên vật nằm gần hướng nhìn nhất. Thiết kế màn không đặt vật nguyên tố sát Ngưỡng hay NPC.
- **Nếu bị trúng đòn khi đang mở vòng chọn:** vòng chọn đóng, giữ nguyên nguyên tố cũ, thời gian về 1×.
- **Nếu bị choáng khi đang cast:** cast hủy và thanh **không** được hoàn lại.
- **Nếu chết:** thanh nguyên tố giữ nguyên giá trị lúc chết. Ngưỡng hồi đầy khi hồi sinh. Nguyên tố đã có không bị mất. Thanh **không** rơi theo tàn ý.
- **Nếu đánh trúng quái đang kháng hoặc đang chết:** vẫn nạp theo sát thương thực gây ra (luật 7). Đánh trúng xác quái thì không nạp.
- **Nếu kỹ năng đánh vào quái vừa yếu vừa kháng cùng một nguyên tố:** không thể xảy ra, vì bảng ràng buộc mỗi loài chỉ có một ô cho mỗi nguyên tố. Nếu data sai, ưu tiên kháng.
- **Nếu 3 nguyên tố tự do được nhận theo thứ tự bất kỳ:** mỗi nơi giữ nguyên tố phải vào được và hoàn thành được **chỉ với Gió**. Không nơi nào được cần Lửa, Nước hay Đá để tới. Nếu không, người chơi có thể bị kẹt (softlock).
- **Nếu người chơi quay về khu cũ khi đã có nguyên tố mới:** các cổng và câu đố cũ phải giải được bằng nguyên tố mới nếu được thiết kế như vậy. Không có câu đố nào chỉ giải được trong một khoảng thời gian rồi khóa vĩnh viễn.
- **Nếu hơi nước (combo 13) che cả hiệp sĩ:** lock-on của người chơi vẫn giữ mục tiêu đang khóa nhưng không khóa được mục tiêu mới cho tới khi hơi tan.
- **Nếu Lửa + Gió lan tới đồ của dân làng hoặc vật cần cho câu đố:** lửa chỉ lan trên vật có gắn cờ "cháy được". Vật cần cho câu đố không bao giờ cháy hết.
- **Nếu nhấn lướt khi Lướt Gió đang hồi (chưa đủ 4s):** ra lướt thường, không i-frame, không đẩy lùi. Lướt thường không làm reset đồng hồ hồi của Lướt Gió.
- **Nếu Lướt Gió đang hồi mà gặp vực rộng cần Lướt Gió:** lướt thường không đủ xa để qua. Thiết kế màn đặt điểm đứng an toàn trước mỗi vực để người chơi chờ hồi.
- **Nếu Lốc Xoáy đặt vào tường hoặc vực:** lốc xuất hiện ở điểm gần nhất còn đứng được trên đường thẳng tới vị trí đặt.
- **Nếu quái đang bị Lốc Xoáy giữ mà bị Choáng hoặc bị đẩy lùi:** Choáng vẫn tính bình thường. Đẩy lùi bị bỏ qua cho tới khi lốc tan.
- **Nếu Lướt Gió đưa người chơi ra khỏi mép vực mà không có điểm đáp:** dùng luật rơi vực hiện có (chưa định nghĩa trong brief — xem Dependencies).
- **Nếu thời gian đang chậm vì vòng chọn mà có combo hoặc hiệu ứng đang đếm:** đồng hồ của Bỏng, Chậm, Choáng và combo chạy theo thời gian game, nên cũng chậm lại.
- **Nếu Turret bị kỹ năng nguyên tố đánh trúng:** nhận sát thương ×1.0 và không dính hiệu ứng. Turret là máy, không phải côn trùng (`world-foundation.md` mục 1.1), nên yểm Lửa và Nước cũng không gây hiệu ứng. Yểm Đá vẫn ×2 sát thương, vì đó là sát thương vật lý.
- **Nếu giữ nút phép khi thanh chưa đầy (dưới 100):** không yểm và không bắn kỹ năng thay thế. Có phản hồi thiếu thanh, không trừ thanh.
- **Nếu thả nút phép trước 0.35s:** là nhấn nhanh, bắn kỹ năng, không yểm. Thanh dưới 33 thì báo thiếu.
- **Nếu đang yểm mà thanh lại đầy:** được yểm lớp mới, lớp cũ mất ngay.
- **Nếu đang yểm Gió mà yểm Than Hồng:** tốc đánh về 1× ngay; Than Hồng tính 6s từ lúc yểm.
- **Nếu yểm Đá mà 6s không có đòn nào trúng quái:** lớp đá tan, không hoàn thanh. Vung trượt hay chém vào tường, thùng không làm vỡ lớp đá.
- **Nếu đòn yểm Đá là đòn nặng hoặc đòn có nhiều nhát (Song kiếm, Xoáy):** lớp đá vỡ ở nhát đầu tiên trúng quái, chỉ nhát đó được ×2 sát thương và ×3 thế. Nếu nhát đó trúng nhiều quái cùng lúc thì mỗi con đều được nhân.
- **Nếu đòn yểm Đá trúng quái đang bị chặn hoặc đang có giáp (Bọ Giáp):** lớp đá vẫn vỡ và nhân sát thương sau khi đã tính giáp. Yểm Đá không phá giáp.
- **Nếu đang yểm mà đổi vũ khí ở Ngưỡng:** nghỉ ở Ngưỡng gỡ mọi lớp yểm (thanh đầy lại nên không thiệt).
- **Nếu đang yểm mà nhắm Cung:** lớp yểm vẫn đếm giờ nhưng tên không mang yểm.

## Dependencies

| Hệ thống | Hướng | Loại | Ghi chú |
|---|---|---|---|
| Combat (lock-on, đòn kiếm) | Elemental Magic phụ thuộc vào | Cứng | Có code, chưa có GDD. Cần sự kiện "đòn trúng" để nạp thanh |
| Stamina | Liên quan | Mềm | Có code, chưa có GDD. Ràng buộc: phép không tốn stamina |
| Lướt (MVP, brief mục 4) | Elemental Magic phụ thuộc vào | Cứng | Chưa có code và GDD. Lướt thường không có i-frame; Lướt Gió thêm i-frame, đẩy lùi và hồi 4s. GDD Lướt cần ghi luật này |
| Ngưỡng (`design/gdd/rest-point.md`) | Elemental Magic phụ thuộc vào | Cứng | Nghỉ hoặc hồi sinh thì hồi đầy thanh |
| Lưu và tải (`design/gdd/save-load.md`) | Elemental Magic phụ thuộc vào | Mềm | Nguyên tố đã có và đang trang bị ghi ngay khi đổi; thanh nguyên tố không lưu (đầy lại khi tải ở Ngưỡng) |
| Enemy AI / EnemyState | Phụ thuộc vào Elemental Magic | Cứng | Cần thêm dữ liệu yếu/kháng và xử lý Bỏng, Chậm, Choáng |
| Vật giải đố và cổng chặn | Phụ thuộc vào Elemental Magic | Cứng | Chưa có hệ thống. Các màn chơi dựng trên nó |
| HUD (`GamePlayView`) | Phụ thuộc vào Elemental Magic | Mềm | Thêm thanh nguyên tố và vòng chọn |
| Input (`PlayerController.inputactions`) | Elemental Magic phụ thuộc vào | Cứng | Thêm hành động Cast (nhấn / giữ) và Switch; dùng chung hành động Interact cho vật nguyên tố |
| Rơi vực | Elemental Magic phụ thuộc vào | Mềm | Chưa định nghĩa ở đâu cả |
| Lore nguyên tố (`design/narrative/lore/elements.md`) | Elemental Magic phụ thuộc vào | Cứng | Nguồn của vị trí, thực thể và quan hệ với quái |
| Weapons (`design/gdd/weapons.md`) | Hai chiều | Cứng | Nạp thanh theo sát thương; duyên vũ khí–nguyên tố; tên nguyên tố của Cung |

Chiều ngược lại đã có ở `weapons.md`, `rest-point.md` và `save-load.md`. `world-state.md` không liên quan trực tiếp, vì nguyên tố đã có là mở khóa vĩnh viễn do save-load giữ. Story `refactor-foundation/005` dựng thanh thế, hiệu ứng và bảng yếu/kháng phía quái. Các hệ thống còn lại (Combat, Stamina, Lướt, Enemy AI, HUD, Input, vật giải đố) chưa có GDD, nên cần ghi "phụ thuộc vào elemental-magic" khi có GDD hoặc story.

## Acceptance Criteria

Ở tier `minimal` không có mục Formulas, nên các tiêu chí dưới đây kiểm các giá trị khởi điểm trong Core Rules.

- **GIVEN** người chơi chưa có nguyên tố nào, **WHEN** nhấn nút phép hoặc nút đổi, **THEN** không có gì xảy ra; nút phép và vòng chọn không hiện trên HUD.
- **GIVEN** người chơi vừa nhận Gió, **WHEN** mở HUD, **THEN** thanh nguyên tố hiện đầy (100) và biểu tượng Gió đang được trang bị.
- **GIVEN** người chơi đã có Gió và chưa có nguyên tố nào khác, **WHEN** đi tới từng nơi giữ Lửa, Nước, Đá, **THEN** mỗi nơi vào được và hoàn thành được mà không cần nguyên tố nào ngoài Gió.
- **GIVEN** thanh nguyên tố là 0, **WHEN** đánh trúng quái 3 lần bằng kiếm, **THEN** thanh là 18.
- **GIVEN** thanh là 50, **WHEN** đánh trúng một bức tường hoặc thùng, **THEN** thanh vẫn là 50.
- **GIVEN** thanh là 40, **WHEN** nhấn phép, **THEN** kỹ năng ra và thanh còn 7; stamina không đổi.
- **GIVEN** thanh là 20, **WHEN** nhấn phép, **THEN** kỹ năng không ra, thanh vẫn là 20 và có phản hồi báo thiếu.
- **GIVEN** thanh là 100 và trang bị Lửa, **WHEN** bắn Cầu Lửa rồi nhấn phép lần nữa trong 1s, **THEN** lần thứ hai không ra và thanh vẫn 67; **WHEN** đổi sang Nước và nhấn phép, **THEN** Vòng Nước ra và thanh còn 34.
- **GIVEN** thanh là 0, đang trang bị Lửa và đứng trước tường tơ, **WHEN** nhấn nút tương tác, **THEN** tường cháy và lối mở; thanh vẫn là 0.
- **GIVEN** thanh là 66, trang bị Lửa, đứng trước đèn, **WHEN** nhấn nút phép, **THEN** Cầu Lửa bắn ra, thanh còn 33 và đèn không sáng.
- **GIVEN** đang trang bị Nước và đứng trước tường tơ (cần Lửa), **WHEN** nhấn nút tương tác, **THEN** tường không đổi và chỉ có phản hồi "trượt", không có popup biểu tượng; **GIVEN** đã bật tùy chọn trợ năng gợi ý, **THEN** hiện biểu tượng Lửa.
- **GIVEN** đang giữ nút đổi, **WHEN** vòng chọn mở, **THEN** thời gian game chạy 0.3×; **WHEN** bị quái đánh trúng, **THEN** vòng đóng, nguyên tố giữ nguyên và thời gian về 1×.
- **GIVEN** thanh là 66 và đang cast Đá Văng, **WHEN** bị choáng giữa chừng, **THEN** cast hủy và thanh là 33.
- **GIVEN** người chơi chết khi thanh là 10, **WHEN** hồi sinh ở Ngưỡng, **THEN** thanh là 100 và mọi nguyên tố đã có vẫn còn.
- **GIVEN** một quái thường yếu Lửa (tạm, chờ thiết kế quái thật), **WHEN** trúng Cầu Lửa, **THEN** quái mất 45 máu (30 × 1.5) và bị Bỏng 4s, mất 5 máu mỗi giây.
- **GIVEN** Kiến Thợ (kháng Lửa), **WHEN** trúng Cầu Lửa, **THEN** mất 15 máu (30 × 0.5) và không bị Bỏng.
- **GIVEN** quái đứng cách 12m, **WHEN** bắn Cầu Lửa thẳng về phía nó, **THEN** cầu tự nổ ở 10m và quái không mất máu.
- **GIVEN** 2 quái thường trong bán kính 3m, **WHEN** dùng Vòng Nước, **THEN** mỗi con mất 20 máu, bị đẩy lùi 2m và bị Chậm 3s.
- **GIVEN** quái đứng trước mặt cách 3.5m, nằm trong vòng cung 90°, **WHEN** dùng Đá Văng, **THEN** quái mất 35 máu và bị Choáng 1s; **GIVEN** quái đứng lệch 60° so với hướng nhìn, **THEN** không trúng.
- **GIVEN** Bọ Giáp còn giáp, **WHEN** đánh bằng kiếm thường, **THEN** sát thương ×0.25; **WHEN** trúng Đá Văng, **THEN** giáp vỡ và Bọ Giáp bị Choáng 1s; **WHEN** trúng đòn yểm Đá, **THEN** giáp không vỡ.
- **GIVEN** thanh là 100 và trang bị Lửa, **WHEN** giữ nút phép 0.4s rồi thả, **THEN** vũ khí được yểm Than Hồng 6s, thanh về 0, stamina không đổi; **GIVEN** chỉ giữ 0.2s, **THEN** ra Cầu Lửa thay vì yểm và thanh còn 67.
- **GIVEN** thanh là 99, **WHEN** giữ nút phép 0.4s rồi thả, **THEN** không yểm, không bắn Cầu Lửa, thanh vẫn 99 và có phản hồi thiếu thanh.
- **GIVEN** Kiếm đang yểm Than Hồng, **WHEN** đòn nhẹ trúng một quái không kháng Lửa, **THEN** quái mất 25 máu và bị Bỏng 4s; **WHEN** trúng Kiến Thợ (kháng Lửa), **THEN** không bị Bỏng.
- **GIVEN** Kiếm đang yểm Nước, **WHEN** đòn trúng quái, **THEN** quái bị Chậm 3s.
- **GIVEN** Kiếm đang yểm Gió, **WHEN** ra combo 3 đòn nhẹ, **THEN** tổng thời gian combo ngắn hơn khoảng 20% so với không yểm (×1/1.25) và stamina tiêu như cũ; **WHEN** hết 6s, **THEN** tốc đánh về 1×.
- **GIVEN** Kiếm đang yểm Đá, **WHEN** đòn nhẹ kế tiếp trúng quái 100 máu, **THEN** quái mất 50 máu, lượng mất thế gấp 3 đòn thường, thanh nguyên tố tăng 12 và lớp yểm mất; **WHEN** đòn kế tiếp, **THEN** quái mất 25 như thường.
- **GIVEN** Kiếm đang yểm Đá, **WHEN** đòn kế tiếp vung trượt hoặc chém vào tường, **THEN** lớp đá còn nguyên; **WHEN** đòn sau đó trúng quái, **THEN** lớp đá vỡ và đòn đó gây ×2 sát thương, ×3 thế.
- **GIVEN** Kiếm đang yểm Đá, **WHEN** 6s trôi qua mà không đòn nào trúng quái, **THEN** lớp đá tan và thanh không được hoàn.
- **GIVEN** đang yểm Gió còn 4s và thanh đã đầy lại, **WHEN** yểm Than Hồng, **THEN** tốc đánh về 1× ngay, Than Hồng có hiệu lực 6s và thanh về 0.
- **GIVEN** đang yểm Nước, **WHEN** đổi nguyên tố trang bị sang Đá, **THEN** lớp yểm Nước vẫn còn tới hết giờ.
- **GIVEN** quái đang Bỏng còn 2s, **WHEN** dính Bỏng lần nữa, **THEN** thời gian Bỏng đặt lại về 4s, không cộng dồn sát thương.
- **GIVEN** vùng cháy của Cầu Lửa còn cháy, **WHEN** đặt Lốc Xoáy chạm vùng đó, **THEN** lốc thành lốc lửa, quái trong lốc bị Bỏng và tường tơ lốc chạm vào bị đốt (combo 12).
- **GIVEN** một vùng đang cháy, **WHEN** dùng Vòng Nước trên vùng đó, **THEN** lửa tắt và hơi nước xuất hiện 4s; quái trong hơi nước mất dấu người chơi (combo 13).
- **GIVEN** một vùng ướt, **WHEN** dùng Đá Văng và cột đá trồi lên trong vùng đó, **THEN** bùn xuất hiện 5s và quái trong bùn bị Chậm (combo 14).
- **GIVEN** một vũng nước có đèn đang sáng cách 3m, **WHEN** đặt Lốc Xoáy chạm vũng, **THEN** mưa bụi xuất hiện và mọi đèn, lửa trong bán kính 4m quanh lốc tắt (combo 15).
- **GIVEN** vũ khí đang yểm Than Hồng, **WHEN** dựng cột đá, **THEN** cột thành đá nung 5s và quái chạm vào bị Bỏng (combo 16).
- **GIVEN** chưa có Gió, **WHEN** lướt xuyên qua đòn quái, **THEN** người chơi vẫn nhận sát thương.
- **GIVEN** đã có Gió và Lướt Gió sẵn sàng, **WHEN** lướt xuyên qua đòn quái, **THEN** không nhận sát thương, quái trên đường bị đẩy lùi 2m, thanh nguyên tố không đổi.
- **GIVEN** vừa dùng Lướt Gió 2s trước, **WHEN** lướt lần nữa xuyên qua đòn quái, **THEN** ra lướt thường và nhận sát thương; **WHEN** đủ 4s kể từ Lướt Gió trước, **THEN** lướt lại có i-frame.
- **GIVEN** thanh 66, trang bị Gió, 2 quái thường đứng cách 3m phía trước, **WHEN** nhấn nút phép, **THEN** lốc xuất hiện 3s, 2 quái bị hút vào tâm, không tấn công được, mỗi con mất 24 máu sau 3s, thanh còn 33.
- **GIVEN** Turret, **WHEN** trúng bất kỳ kỹ năng nguyên tố nào hoặc đòn yểm Lửa, Nước, **THEN** nhận sát thương ×1.0 và không dính hiệu ứng.

## Visual/Audio Requirements

> `art-director` và `audio-director` không được hỏi ý kiến (Solo mode). Chưa có art bible. Review thủ công trước khi làm asset.

- **Màu đặc trưng của từng nguyên tố** (dùng cho VFX, HUD và gợi ý trên vật):
  - Gió: trắng ngà;
  - Lửa: cam than (ấm, không đỏ máu);
  - Nước: xanh ngọc;
  - Đá: nâu rêu.

  Theo brief, phong cách là low-poly stylized và tông ấm áp.
- **Mỗi kỹ năng chiến đấu cần:**
  - VFX khi cast;
  - VFX khi trúng;
  - VFX cho hiệu ứng trên quái: Bỏng là đốm than trên thân, Chậm là vệt nước nhỏ giọt, Choáng là vòng bụi đá quanh đầu.
- **VFX riêng của Đá Văng (đất trồi):** không có đạn đá bay ra. Đất phía trước nứt và trồi lên thành các cột đá nhọn xếp vòng cung, màu nâu rêu, lần lượt từ hàng gần ra hàng xa (hàng xa cao hơn, các cột nghiêng nhẹ ra ngoài), đứng một nhịp ngắn rồi lún lại xuống đất. Có bụi đất bắn lên ở chân cột lúc trồi và lúc lún. Hình hiện tại (15 cột, 3 hàng ở 35% / 68% / 100% tầm, trồi cách nhau 0.06s, bật lên 0.12s, đứng 0.5s) là hình tạm trên `StoneBurstView` của prefab `P_StoneBurst`, chưa phải VFX thật. Vùng cung hiển thị phải khớp vùng trúng thật (90°, 4m) để người chơi đọc được tầm.
- **Mỗi lớp yểm cần VFX bám lưỡi vũ khí trong suốt thời gian yểm:** Than Hồng là than hồng và tàn lửa, Nước là màng nước chảy, Gió là vệt gió quấn quanh lưỡi, Đá là lớp đá bọc lưỡi, vỡ tung thành mảnh khi đòn trúng quái. Khi lớp yểm còn 1s thì VFX nhấp nháy.
- **Mỗi combo cần VFX riêng:** lửa lan theo gió, hơi nước, bùn, mưa bụi, đá nung.
- **Vật giải đố và cổng chặn mang dấu nguyên tố cần dùng:** hoa văn màu của nguyên tố trên vật, đọc được từ góc nhìn isometric. Vật đã được tác động thì đổi trạng thái rõ ràng (đèn sáng, tường tơ cháy rụi, cột đá đứng yên).
- **Phản hồi đòn đánh:**
  - dùng Feel (MMFeedbacks) và haptics như combat hiện có;
  - trúng điểm yếu có feedback mạnh hơn: rung camera nhẹ, âm cao;
  - trúng kháng có feedback "bật lại": âm đục, tia xám.
- **Âm thanh:** mỗi nguyên tố có 1 âm cast, 1 âm trúng và 1 âm tương tác vật. Thanh nạp đầy một nấc (đủ 33) phát một âm báo nhỏ.

📌 **Asset Spec** — mục Visual/Audio đã có nội dung. Sau khi art bible được duyệt, chạy `/asset-spec system:elemental-magic` để có mô tả và prompt cho từng asset.

## Game Feel

- **Độ trễ input:** từ lúc **thả** nút phép tới frame đầu của kỹ năng ≤ 100ms. Vì kỹ năng ra khi thả nút (để phân biệt với giữ để yểm), kỹ năng trễ thêm đúng bằng thời gian bấm, thường 60–120ms, giống đòn nhẹ trong `weapons.md` luật 6b. Khi giữ đủ 0.35s và thanh đầy, tay cầm rung nhẹ báo sẵn sàng yểm. Nhấn nhanh nút đổi thì nguyên tố mới có hiệu lực ngay ở frame kế tiếp.
- **Tách cast khỏi lăn và đánh:**
  - cast không cắt ngang được đòn kiếm đang diễn ra;
  - được phép xếp hàng (buffer) cast trong 0.2s cuối của đòn kiếm;
  - đang lăn thì không cast; lăn được dùng để hủy phần hồi sau cast (recovery), không hủy được phần khởi động (startup).
- **Trọng lượng từng kỹ năng:**
  - Lốc Xoáy: dựng lên nhanh (≤ 0.2s), quay đều, tiếng gió hú tăng dần rồi tan nhẹ.
  - Lướt Gió (nút lướt): nhẹ, nhanh, có vệt gió trắng để người chơi biết lần này có i-frame. Lướt thường không có vệt này.
  - Cầu Lửa: phóng nhanh, có lực giật nhẹ khi bắn và tiếng nổ trầm khi trúng.
  - Vòng Nước: tỏa ra mềm.
  - Đá Văng: nặng, khởi động khoảng 0.35s, có dừng hình ngắn (hitstop) khi trúng. Lúc thả đòn có cảm giác đất rung và trồi lan ra từ chân nhân vật tới rìa tầm, từng hàng cột đá nối nhau như một đợt sóng; rung camera nhẹ đi cùng đợt trồi.
- **Yểm:**
  - anim yểm ngắn (khoảng 0.3s, vuốt tay dọc lưỡi vũ khí);
  - lăn hủy được phần hồi của anim yểm, giống luật cast;
  - đòn yểm Đá có hitstop dài hơn đòn thường (0.12s) để người chơi cảm nhận cú nặng.
- **Vòng chọn:**
  - mở trong ≤ 0.1s;
  - lúc mở, thời gian chậm 0.3× để giảm áp lực nhưng không dừng hẳn (vẫn căng thẳng kiểu souls);
  - đóng vòng là thời gian về 1× ngay, không làm mượt.
- **Cảm giác nạp thanh:** mỗi đòn trúng có một chớp nhỏ trên thanh. Người chơi phải cảm thấy đánh chủ động thì được thưởng phép.

## UI Requirements

- **Thanh nguyên tố trên HUD:**
  - đặt dưới thanh stamina hiện có trong `GamePlayView`;
  - chia 3 nấc, mỗi nấc 33;
  - luôn màu xanh lam, không đổi theo nguyên tố (quyết định 2026-10-30); màu nguyên tố chỉ hiện ở biểu tượng nguyên tố;
  - ẩn cho tới khi có Gió.
- **Lớp yểm:** biểu tượng nhỏ trên ô vũ khí chính, kèm vòng đếm ngược 6s. Với yểm Đá, biểu tượng vỡ ra khi lớp đá vỡ.
- **Lướt Gió:** biểu tượng nhỏ cạnh thanh stamina, kèm vòng hồi 4s. Hồi xong thì biểu tượng lóe một lần.
- **Hồi chiêu:** biểu tượng nguyên tố đang trang bị có lớp tối quay tròn, thu dần về 0 khi kỹ năng hồi xong.
- **Biểu tượng nguyên tố đang trang bị:** đặt cạnh thanh. Khi nhấn nhanh để đổi, biểu tượng chuyển bằng tween (DOTween).
- **Vòng chọn:**
  - giữa màn hình, 4 ô;
  - nguyên tố chưa có hiện ô khóa, không lộ tên (giữ bí ẩn);
  - chọn bằng hướng của joystick hoặc chuột.
- **Gợi ý trên vật:** mặc định **không có popup**. Hướng khám phá kiểu Tunic trong brief yêu cầu người chơi tự đọc hoa văn màu nguyên tố khắc trên vật (xem Visual/Audio). Popup biểu tượng nguyên tố chỉ là **tùy chọn trợ năng**: khi bật, đứng gần vật thì hiện biểu tượng nguyên tố cần dùng (nếu đã có) hoặc dấu "?" mờ (nếu chưa có).
- **Phản hồi thiếu thanh:** thanh rung, nấc trống nháy.
- **Thanh đầy:** thanh phát sáng viền màu nguyên tố để báo yểm đã sẵn sàng.

📌 **UX Flag — Elemental Magic**: hệ thống này có yêu cầu UI (thanh nguyên tố, vòng chọn, gợi ý trên vật). Trước khi viết story có UI, chạy `/ux-design` cho HUD và vòng chọn; story nên dẫn tới `design/ux/[screen].md` thay vì GDD.

## Cross-References

| Tài liệu | Mục | Liên hệ |
|---|---|---|
| `design/game-brief.md` | MVP mục 4 (kỹ năng di chuyển) | Lướt MVP là mầm của Gió; nâng thành Lướt Gió |
| `design/game-brief.md` | "Định hướng khám phá" | Không popup gợi ý (chỉ là tùy chọn trợ năng); dạy kỹ năng và combo qua thử thách thực thể và thiết kế màn |
| `design/game-brief.md` | MVP mục 2 (Ngưỡng, chết kiểu Souls) | Hồi đầy thanh và lưu nguyên tố khi nghỉ |
| `design/narrative/lore/elements.md` | Mục 2 (bốn nguyên tố), 3.5 (quái nhắm tới), 4.1 (thử thách) | Nguồn của nơi giữ, thực thể, thử thách và quy tắc yếu/kháng |
| `design/narrative/lore/world-foundation.md` | Mục 1 (luật thế giới) | Nguyên tố không phải phép lạ; tàn ý (tiền) không phải thanh nguyên tố |
| `design/gdd/weapons.md` | Luật 14 (tên nguyên tố), 16 (duyên), 17 (nạp thanh) | Luật 7 ở đây đã đổi theo luật 17 bên đó |
| `Assets/Scripts/CoreGame/Character/Player/PlayerCombat.cs` | Đòn trúng, `DrainStaminaBasedOnAttack` | Điểm gắn nạp thanh; phép không tốn stamina |
| `Assets/Scripts/CoreGame/Character/Player/SO_PlayerStats.cs` | Stamina | Nơi gợi ý đặt các giá trị khởi điểm của thanh nguyên tố (hoặc một SO riêng) |

## Open Questions

| Câu hỏi | Người quyết | Hạn |
|---|---|---|
| Thiết kế cụ thể boss côn trùng và thử thách thực thể ở mỗi nơi giữ nguyên tố | Người dùng + level-designer | Khi bắt đầu màn Đỉnh Hạc |
| Yếu/kháng nguyên tố của quái thật (dàn quái chưa thiết kế; quái hiện có trong Unity chỉ là demo, bảng yếu/kháng đang tạm dùng dòng "quái thường" và "boss vùng" yếu Lửa) | Người dùng | Trước khi viết story combat nguyên tố |
| Luật rơi vực (Lướt Gió ra ngoài mép) chưa được định nghĩa ở đâu | Người dùng | Trước khi làm lướt MVP |
| Các con số (thanh 100, nạp 0.24 × sát thương, giá 33; sát thương và tầm 4 kỹ năng ở luật 11a; ×1.5 / ×0.5 / ×0.25; thời gian hiệu ứng; yểm 6s, Gió ×1.25, Đá ×2 sát thương / ×3 thế; Lướt Gió hồi 4s; hồi chiêu kỹ năng 1–2s) chưa qua cân bằng. Riêng yểm tiêu cả 100 thanh mà chỉ được 6s, có thể quá yếu so với 3 kỹ năng: cần kiểm khi playtest | Người dùng, sau playtest | Sau bản lướt MVP chơi được |
| Thanh nguyên tố đặt trong `SO_PlayerStats` hay một ScriptableObject riêng | Lập trình (ADR) | Khi viết story đầu tiên của hệ thống |
| Có cho nâng cấp thanh nguyên tố ở Làng Mái Vá (brief chỉ nhắc tim tối đa và stamina) không | Người dùng | Khi thiết kế cửa hàng làng |
| Thứ tự gợi ý cho 3 nguyên tố tự do (để màn chơi tăng độ khó hợp lý) | Người dùng + level-designer | Khi lên bản đồ thế giới |
