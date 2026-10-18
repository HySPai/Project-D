# Elemental Magic (Sức mạnh nguyên tố)

> **Status**: In Design
> **Author**: người dùng + Claude (solo, không có specialist)
> **Last Updated**: 2026-10-17
> **Last Verified**: 2026-10-17
> **Implements Pillar**: không có file pillar ở tier `minimal`; phục vụ thể loại trong `design/game-brief.md` (soulslike + metroidvania + giải đố)

> **Workflow**: `minimal`. GDD này là tự nguyện; brief vẫn là tài liệu thiết kế chính. Chỉ có 5 mục chuẩn và Summary. Không có mục Formulas: các con số được ghi thẳng trong Core Rules và đều là giá trị khởi điểm, đặt trong ScriptableObject.
> **Specialist**: `game-designer`, `systems-designer`, `ai-programmer`, `art-director` và `qa-lead` không được hỏi ý kiến (Solo mode). Cần review thủ công trước khi vào production.

## Summary

Hiệp sĩ chó thu thập 4 nguyên tố (Gió, Lửa, Nước, Đá) để chiến đấu, giải đố và mở lối kiểu metroidvania. Mỗi nguyên tố có 1 kỹ năng chiến đấu, tốn thanh nguyên tố nạp bằng đòn đánh. Dùng để giải đố hay mở lối thì miễn phí.

> **Quick reference** — Layer: `Feature` · Priority: `Full Vision` (sau MVP; MVP chỉ có lướt thường) · Key deps: `Combat, Stamina, Lock-on, Lướt (MVP), Ngưỡng, Enemy AI, HUD`
> Category: Gameplay · Layer: Feature · Priority: sau MVP — cả ba suy ra từ brief, vì tier này không có systems index.

## Overview

Elemental Magic là hệ thống cho hiệp sĩ chó dùng 4 nguyên tố, tàn dư của Mẫu Địa (xem `design/narrative/lore/elements.md`), vào 3 việc:
- **Chiến đấu:** 1 nút phép, tốn thanh nguyên tố.
- **Giải đố:** tác động lên vật trong môi trường.
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
4. Lướt (MVP) là mầm của Gió. Trước khi có Gió, lướt là lướt thường. Sau khi có Gió, lướt nâng cấp thành Lướt Gió.

**Điều khiển**

5. Mỗi lúc chỉ trang bị **1 nguyên tố**. Có 1 nút phép (cast) và 1 thao tác đổi nguyên tố:
   - nhấn nhanh: đổi sang nguyên tố kế tiếp;
   - giữ: mở vòng chọn. Trong lúc mở, thời gian chậm còn 0.3× (giá trị khởi điểm).
6. Nút phép dùng nguyên tố đang trang bị. Nếu trước mặt có vật tương tác được với nguyên tố đó (đèn, tường tơ, nút nhấn, cánh quạt…), nút phép **tương tác với vật**. Nếu không có, nút phép **dùng kỹ năng chiến đấu**.

**Tài nguyên — thanh nguyên tố**

7. Có một thanh nguyên tố chung, riêng biệt với stamina. Giá trị khởi điểm:
   - tối đa 100;
   - mỗi đòn vũ khí trúng quái nạp **0.48 × sát thương gây ra** (làm tròn xuống, tối thiểu 1). Kiếm 25 sát thương nạp 12; Song kiếm 12 nạp 5; Búa 50 nạp 24. Sát thương từ kỹ năng nguyên tố không nạp. Luật này thay cho "12 mỗi đòn" cũ để các vũ khí nạp công bằng (xem `weapons.md` luật 17);
   - mỗi kỹ năng chiến đấu tốn 33, tức khoảng 3 lần dùng khi đầy;
   - nghỉ ở Ngưỡng thì hồi đầy.
8. Thanh nguyên tố **không** tự hồi theo thời gian, và **không** nạp khi đánh trúng vật vô tri.
9. Tương tác giải đố và mở lối tốn **0**: không cần thanh, và vẫn dùng được khi thanh rỗng.
10. Kỹ năng chiến đấu **không tốn stamina**, nhưng vẫn bị chặn khi đang lăn, đang bị choáng hoặc đã chết.

**Bộ kỹ năng**

11. Mỗi nguyên tố có 1 kỹ năng chiến đấu, các tương tác giải đố và các chướng ngại nó mở được:

| | Gió (Tiếng Vọng) | Lửa (Lão Than) | Nước (Cụ Sen Cổ) | Đá (Đá Canh) |
|---|---|---|---|---|
| **Kỹ năng chiến đấu** | *Lướt Gió*: lướt có i-frame, đẩy lùi quái trên đường đi | *Than Hồng*: phủ lửa lên kiếm 6s, đòn trúng gây Bỏng | *Vòng Nước*: sóng quanh người, đẩy lùi và gây Chậm | *Giậm Đất*: chấn động hình nón phía trước, phá giáp và gây Choáng 1s |
| **Giải đố** | Xoay cánh quạt, cối xay; thổi vật nhẹ; đưa lửa lan theo gió | Thắp đèn, lò, đuốc; ánh lửa hút Ngài Đèn tới | Đổ đầy hoặc rút nước để nâng hạ vật nổi; tưới mầm cây mọc thành bậc | Dựng cột đá để đè nút nhấn hoặc làm bệ đứng |
| **Mở lối** | Lướt qua vực rộng; cưỡi luồng gió lên cao | Đốt tường tơ nhện, rễ khô | Đi trên mặt nước (lá sen nổi dưới chân) | Phá tường nứt; dựng cột đá lên mỏm cao |

Với Lướt Gió:
- **dùng để di chuyển** (vực, luồng gió) thì miễn phí, theo luật 9;
- **dùng để tấn công** (va vào quái khi đang khóa mục tiêu) thì tốn thanh như các kỹ năng chiến đấu khác.

**Combo cố định** — chỉ có đúng 5 combo dưới đây, không có hệ trạng thái tự do:

12. Lửa + Gió: đang có ngọn lửa (Than Hồng hoặc vật đang cháy) mà Gió thổi qua thì lửa lan theo hướng gió, gây Bỏng diện rộng.
13. Nước + Lửa: Vòng Nước chạm vật hoặc vùng đang cháy thì dập lửa và tạo hơi nước 4s. Quái trong hơi nước mất dấu hiệp sĩ (thoát khỏi việc bị nhắm).
14. Nước + Đá: Giậm Đất trên vùng ướt tạo bùn 5s, gây Chậm cho quái đứng trong đó.
15. Gió + Nước: Lướt Gió xuyên qua vùng nước tạo mưa bụi, dập mọi đèn và lửa trong vùng.
16. Lửa + Đá: cột đá dựng trong lúc đang có Than Hồng thành đá nung 5s, gây Bỏng khi quái chạm vào.

**Cách người chơi học kỹ năng và combo** (theo brief: không có popup hướng dẫn, không có trang sách)

- Khi nhận một nguyên tố, thử thách của thực thể giữ nó buộc người chơi dùng kỹ năng chiến đấu và ít nhất một tương tác giải đố của nguyên tố đó.
- Mỗi combo được dạy bằng thiết kế màn: lần đầu combo cần thiết, màn đặt sẵn đủ hai yếu tố ngay trước chướng ngại. Ví dụ: vật đang cháy cạnh luồng gió, trước một vách tơ dày.
- NPC ở Làng Mái Vá có thể gợi ý mơ hồ về combo khi người chơi đã có đủ hai nguyên tố.

**Yếu và kháng của quái côn trùng**

17. Quy tắc: con quái nào khao khát nguyên tố nào thì kháng nguyên tố đó (khớp với lore).
    - Đánh vào điểm yếu: sát thương kỹ năng ×1.5 và luôn gây hiệu ứng.
    - Đánh vào kháng: sát thương ×0.5 và không gây hiệu ứng.
    - Giá trị khởi điểm.

| Quái | Yếu | Kháng | Ghi chú |
|---|---|---|---|
| Seed | Lửa | — | |
| Spider | Lửa | — | Lửa đốt cả tơ của nó |
| Spider King | Lửa | — | Đề xuất, cần game-designer xác nhận |
| Kiến Thợ | Nước | Lửa | |
| Ong Chúa Rỗng | Lửa | Nước | |
| Bọ Giáp | Đá | Gió | Giáp chỉ vỡ bởi Giậm Đất; đòn vũ khí thường vào giáp chỉ gây ×0.25 (Búa ×0.5, xem `weapons.md`) |
| Ngài Đèn | Đá | Gió | Bị lửa hút tới (dùng làm mồi) |
| Turret (máy) | — | — | Không yếu, không kháng nguyên tố nào |

### States and Transitions

| Trạng thái | Mô tả | Chuyển sang |
|---|---|---|
| Chưa có nguyên tố | Trước khi có Gió. Nút phép và vòng chọn bị ẩn; lướt là lướt thường | → Sẵn sàng, khi nhận Gió |
| Sẵn sàng | Đang trang bị 1 nguyên tố, có thể cast | → Đang cast (nhấn phép, đủ thanh hoặc đang tương tác) · → Đang chọn (giữ nút đổi) |
| Đang chọn | Vòng chọn mở, thời gian chậm 0.3× | → Sẵn sàng (thả nút) · → Sẵn sàng, đóng vòng (bị trúng đòn) |
| Đang cast | Đang ra kỹ năng hoặc tương tác. Không thể đổi nguyên tố | → Sẵn sàng (cast xong) · → Sẵn sàng (bị choáng, cast hủy, thanh không hoàn) |
| Bị khóa | Đang lăn, bị choáng, đã chết hoặc đang trong hội thoại | → Sẵn sàng (khi hết điều kiện khóa) |

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
| Combat (`PlayerCombat`) | Sự kiện đòn vũ khí trúng quái và sát thương gây ra (để nạp thanh); đòn đang diễn ra | Lệnh khóa cast khi đang tấn công; phủ Than Hồng lên vũ khí đang cầm | Combat |
| Weapons (`design/gdd/weapons.md`) | Vũ khí chính đang cầm (duyên nguyên tố); đang nhắm cung | Tăng cường kỹ năng có duyên; tên nguyên tố (1 nấc thanh khi chiến đấu, miễn phí khi giải đố) | Weapons cung cấp hệ số duyên; Elemental Magic giữ thanh và kỹ năng |
| Stamina (`PlayerState`) | — | Không tiêu stamina (luật 10) | Stamina |
| Lock-on | Mục tiêu đang khóa (hướng của Giậm Đất và Lướt Gió) | — | Lock-on |
| Lướt (MVP) | Lướt thường | Nâng lên Lướt Gió khi đã có Gió | Lướt |
| Ngưỡng (điểm nghỉ) | Sự kiện nghỉ | Hồi đầy thanh; lưu danh sách nguyên tố đã có | Ngưỡng |
| Enemy AI / EnemyState | Bảng yếu và kháng của từng loài (data) | Sát thương nguyên tố và hiệu ứng (Bỏng, Chậm, Choáng) | Elemental Magic gửi; Enemy nhận và tự xử lý |
| Vật giải đố và cổng chặn | Loại nguyên tố mà vật cần | Sự kiện "đã tác động bằng nguyên tố X" | Elemental Magic |
| HUD (`GamePlayView`) | — | Giá trị thanh, nguyên tố đang chọn, vòng chọn | HUD |
| Input | Nút phép, nút đổi | — | Input |

Cách triển khai kỹ thuật (event hay controller trung tâm, ScriptableObject cho dữ liệu nguyên tố) sẽ thành một ADR. GDD này không quyết định.

## Edge Cases

- **Nếu thanh nguyên tố không đủ 33 mà nhấn phép không có vật tương tác:** không cast. Thanh rung nhẹ và phát âm báo; không trừ thanh.
- **Nếu nhấn phép trước một vật tương tác được nhưng nguyên tố đang trang bị không hợp với vật:** không có gì xảy ra với vật; vật chỉ phát một phản hồi nhỏ (âm "trượt", hoa văn trên vật lóe nhẹ). Người chơi tự đọc hoa văn màu trên vật để biết cần nguyên tố nào. Không dùng kỹ năng chiến đấu thay thế. Biểu tượng gợi ý chỉ hiện khi bật tùy chọn trợ năng.
- **Nếu vừa có vật tương tác vừa có quái trong tầm:** ưu tiên tương tác với vật (luật 6).
- **Nếu bị trúng đòn khi đang mở vòng chọn:** vòng chọn đóng, giữ nguyên nguyên tố cũ, thời gian về 1×.
- **Nếu bị choáng khi đang cast:** cast hủy và thanh **không** được hoàn lại.
- **Nếu chết:** thanh nguyên tố giữ nguyên giá trị lúc chết. Ngưỡng hồi đầy khi hồi sinh. Nguyên tố đã có không bị mất. Thanh **không** rơi theo tàn ý.
- **Nếu đánh trúng quái đang kháng hoặc đang chết:** vẫn nạp theo sát thương thực gây ra (luật 7). Đánh trúng xác quái thì không nạp.
- **Nếu kỹ năng đánh vào quái vừa yếu vừa kháng cùng một nguyên tố:** không thể xảy ra, vì bảng ràng buộc mỗi loài chỉ có một ô cho mỗi nguyên tố. Nếu data sai, ưu tiên kháng.
- **Nếu 3 nguyên tố tự do được nhận theo thứ tự bất kỳ:** mỗi nơi giữ nguyên tố phải vào được và hoàn thành được **chỉ với Gió**. Không nơi nào được cần Lửa, Nước hay Đá để tới. Nếu không, người chơi có thể bị kẹt (softlock).
- **Nếu người chơi quay về khu cũ khi đã có nguyên tố mới:** các cổng và câu đố cũ phải giải được bằng nguyên tố mới nếu được thiết kế như vậy. Không có câu đố nào chỉ giải được trong một khoảng thời gian rồi khóa vĩnh viễn.
- **Nếu hơi nước (combo 13) che cả hiệp sĩ:** lock-on của người chơi vẫn giữ mục tiêu đang khóa nhưng không khóa được mục tiêu mới cho tới khi hơi tan.
- **Nếu Lửa + Gió lan tới đồ của dân làng hoặc vật cần cho câu đố:** lửa chỉ lan trên vật có gắn cờ "cháy được". Vật cần cho câu đố không bao giờ cháy hết.
- **Nếu Lướt Gió đưa người chơi ra khỏi mép vực mà không có điểm đáp:** dùng luật rơi vực hiện có (chưa định nghĩa trong brief — xem Dependencies).
- **Nếu thời gian đang chậm vì vòng chọn mà có combo hoặc hiệu ứng đang đếm:** đồng hồ của Bỏng, Chậm, Choáng và combo chạy theo thời gian game, nên cũng chậm lại.
- **Nếu Turret bị kỹ năng nguyên tố đánh trúng:** nhận sát thương ×1.0 và không dính hiệu ứng.

## Dependencies

| Hệ thống | Hướng | Loại | Ghi chú |
|---|---|---|---|
| Combat (lock-on, đòn kiếm) | Elemental Magic phụ thuộc vào | Cứng | Có code, chưa có GDD. Cần sự kiện "đòn trúng" để nạp thanh |
| Stamina | Liên quan | Mềm | Có code, chưa có GDD. Ràng buộc: phép không tốn stamina |
| Lướt (MVP, brief mục 4) | Elemental Magic phụ thuộc vào | Cứng | Chưa có code và GDD. Lướt Gió nâng cấp trên nó |
| Ngưỡng (`design/gdd/rest-point.md`) | Elemental Magic phụ thuộc vào | Cứng | Nghỉ hoặc hồi sinh thì hồi đầy thanh |
| Lưu và tải (`design/gdd/save-load.md`) | Elemental Magic phụ thuộc vào | Mềm | Nguyên tố đã có và đang trang bị ghi ngay khi đổi; thanh nguyên tố không lưu (đầy lại khi tải ở Ngưỡng) |
| Enemy AI / EnemyState | Phụ thuộc vào Elemental Magic | Cứng | Cần thêm dữ liệu yếu/kháng và xử lý Bỏng, Chậm, Choáng |
| Vật giải đố và cổng chặn | Phụ thuộc vào Elemental Magic | Cứng | Chưa có hệ thống. Các màn chơi dựng trên nó |
| HUD (`GamePlayView`) | Phụ thuộc vào Elemental Magic | Mềm | Thêm thanh nguyên tố và vòng chọn |
| Input (`PlayerController.inputactions`) | Elemental Magic phụ thuộc vào | Cứng | Thêm hành động Cast và Switch |
| Rơi vực | Elemental Magic phụ thuộc vào | Mềm | Chưa định nghĩa ở đâu cả |
| Lore nguyên tố (`design/narrative/lore/elements.md`) | Elemental Magic phụ thuộc vào | Cứng | Nguồn của vị trí, thực thể và quan hệ với quái |
| Weapons (`design/gdd/weapons.md`) | Hai chiều | Cứng | Nạp thanh theo sát thương; duyên vũ khí–nguyên tố; tên nguyên tố của Cung |

Ngoài `weapons.md` (đã ghi chiều ngược lại), chưa có GDD nào khác trong `design/gdd/`, nên chưa thể kiểm tra chiều ngược lại. Các hệ thống trên cần ghi "phụ thuộc vào elemental-magic" khi có GDD hoặc story.

## Acceptance Criteria

Ở tier `minimal` không có mục Formulas, nên các tiêu chí dưới đây kiểm các giá trị khởi điểm trong Core Rules.

- **GIVEN** người chơi chưa có nguyên tố nào, **WHEN** nhấn nút phép hoặc nút đổi, **THEN** không có gì xảy ra; nút phép và vòng chọn không hiện trên HUD.
- **GIVEN** người chơi vừa nhận Gió, **WHEN** mở HUD, **THEN** thanh nguyên tố hiện đầy (100) và biểu tượng Gió đang được trang bị.
- **GIVEN** người chơi đã có Gió và chưa có nguyên tố nào khác, **WHEN** đi tới từng nơi giữ Lửa, Nước, Đá, **THEN** mỗi nơi vào được và hoàn thành được mà không cần nguyên tố nào ngoài Gió.
- **GIVEN** thanh nguyên tố là 0, **WHEN** đánh trúng quái 3 lần bằng kiếm, **THEN** thanh là 36.
- **GIVEN** thanh là 50, **WHEN** đánh trúng một bức tường hoặc thùng, **THEN** thanh vẫn là 50.
- **GIVEN** thanh là 40 và không có vật tương tác, **WHEN** nhấn phép, **THEN** kỹ năng ra và thanh còn 7; stamina không đổi.
- **GIVEN** thanh là 20 và không có vật tương tác, **WHEN** nhấn phép, **THEN** kỹ năng không ra, thanh vẫn là 20 và có phản hồi báo thiếu.
- **GIVEN** thanh là 0, đang trang bị Lửa và đứng trước tường tơ nhện, **WHEN** nhấn phép, **THEN** tường cháy và lối mở; thanh vẫn là 0.
- **GIVEN** đang trang bị Nước và đứng trước tường tơ (cần Lửa), **WHEN** nhấn phép, **THEN** tường không đổi và chỉ có phản hồi "trượt", không có popup biểu tượng; **GIVEN** đã bật tùy chọn trợ năng gợi ý, **THEN** hiện biểu tượng Lửa.
- **GIVEN** đang giữ nút đổi, **WHEN** vòng chọn mở, **THEN** thời gian game chạy 0.3×; **WHEN** bị quái đánh trúng, **THEN** vòng đóng, nguyên tố giữ nguyên và thời gian về 1×.
- **GIVEN** thanh là 66 và đang cast Giậm Đất, **WHEN** bị choáng giữa chừng, **THEN** cast hủy và thanh là 33.
- **GIVEN** người chơi chết khi thanh là 10, **WHEN** hồi sinh ở Ngưỡng, **THEN** thanh là 100 và mọi nguyên tố đã có vẫn còn.
- **GIVEN** quái Spider (yếu Lửa), **WHEN** trúng Than Hồng, **THEN** sát thương kỹ năng ×1.5 và Spider bị Bỏng 4s, mất 5 máu mỗi giây.
- **GIVEN** Kiến Thợ (kháng Lửa), **WHEN** trúng Than Hồng, **THEN** sát thương ×0.5 và không bị Bỏng.
- **GIVEN** Bọ Giáp còn giáp, **WHEN** đánh bằng kiếm thường, **THEN** sát thương ×0.25; **WHEN** trúng Giậm Đất, **THEN** giáp vỡ và Bọ Giáp bị Choáng 1s.
- **GIVEN** quái đang Bỏng còn 2s, **WHEN** dính Bỏng lần nữa, **THEN** thời gian Bỏng đặt lại về 4s, không cộng dồn sát thương.
- **GIVEN** một vật đang cháy, **WHEN** Lướt Gió xuyên qua, **THEN** lửa lan theo hướng lướt và quái trong vùng lan bị Bỏng (combo 12).
- **GIVEN** một vùng đang cháy, **WHEN** dùng Vòng Nước trên vùng đó, **THEN** lửa tắt và hơi nước xuất hiện 4s; quái trong hơi nước mất dấu người chơi (combo 13).
- **GIVEN** một vùng ướt, **WHEN** dùng Giậm Đất trên đó, **THEN** bùn xuất hiện 5s và quái trong bùn bị Chậm (combo 14).
- **GIVEN** một vùng nước có đèn đang sáng, **WHEN** Lướt Gió xuyên qua, **THEN** mọi đèn và lửa trong vùng tắt (combo 15).
- **GIVEN** Than Hồng đang có hiệu lực, **WHEN** dựng cột đá, **THEN** cột thành đá nung 5s và quái chạm vào bị Bỏng (combo 16).
- **GIVEN** chưa có Gió, **WHEN** lướt, **THEN** là lướt thường; **GIVEN** đã có Gió, **WHEN** lướt qua quái, **THEN** quái bị đẩy lùi và người chơi có i-frame.
- **GIVEN** Turret, **WHEN** trúng bất kỳ kỹ năng nguyên tố nào, **THEN** nhận sát thương ×1.0 và không dính hiệu ứng.

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
- **Mỗi combo cần VFX riêng:** lửa lan theo gió, hơi nước, bùn, mưa bụi, đá nung.
- **Vật giải đố và cổng chặn mang dấu nguyên tố cần dùng:** hoa văn màu của nguyên tố trên vật, đọc được từ góc nhìn isometric. Vật đã được tác động thì đổi trạng thái rõ ràng (đèn sáng, tường tơ cháy rụi, cột đá đứng yên).
- **Phản hồi đòn đánh:**
  - dùng Feel (MMFeedbacks) và haptics như combat hiện có;
  - trúng điểm yếu có feedback mạnh hơn: rung camera nhẹ, âm cao;
  - trúng kháng có feedback "bật lại": âm đục, tia xám.
- **Âm thanh:** mỗi nguyên tố có 1 âm cast, 1 âm trúng và 1 âm tương tác vật. Thanh nạp đầy một nấc (đủ 33) phát một âm báo nhỏ.

📌 **Asset Spec** — mục Visual/Audio đã có nội dung. Sau khi art bible được duyệt, chạy `/asset-spec system:elemental-magic` để có mô tả và prompt cho từng asset.

## Game Feel

- **Độ trễ input:** nhấn phép tới frame đầu của kỹ năng ≤ 100ms. Nhấn nhanh nút đổi thì nguyên tố mới có hiệu lực ngay ở frame kế tiếp.
- **Tách cast khỏi lăn và đánh:**
  - cast không cắt ngang được đòn kiếm đang diễn ra;
  - được phép xếp hàng (buffer) cast trong 0.2s cuối của đòn kiếm;
  - đang lăn thì không cast; lăn được dùng để hủy phần hồi sau cast (recovery), không hủy được phần khởi động (startup).
- **Trọng lượng từng kỹ năng:**
  - Lướt Gió: nhẹ, nhanh.
  - Than Hồng: tức thì, chỉ bùng lên trên lưỡi kiếm.
  - Vòng Nước: tỏa ra mềm.
  - Giậm Đất: nặng, khởi động khoảng 0.35s, có dừng hình ngắn (hitstop) khi trúng.
- **Vòng chọn:**
  - mở trong ≤ 0.1s;
  - lúc mở, thời gian chậm 0.3× để giảm áp lực nhưng không dừng hẳn (vẫn căng thẳng kiểu souls);
  - đóng vòng là thời gian về 1× ngay, không làm mượt.
- **Cảm giác nạp thanh:** mỗi đòn trúng có một chớp nhỏ trên thanh. Người chơi phải cảm thấy đánh chủ động thì được thưởng phép.

## UI Requirements

- **Thanh nguyên tố trên HUD:**
  - đặt dưới thanh stamina hiện có trong `GamePlayView`;
  - chia 3 nấc, mỗi nấc 33;
  - mang màu của nguyên tố đang trang bị;
  - ẩn cho tới khi có Gió.
- **Biểu tượng nguyên tố đang trang bị:** đặt cạnh thanh. Khi nhấn nhanh để đổi, biểu tượng chuyển bằng tween (DOTween).
- **Vòng chọn:**
  - giữa màn hình, 4 ô;
  - nguyên tố chưa có hiện ô khóa, không lộ tên (giữ bí ẩn);
  - chọn bằng hướng của joystick hoặc chuột.
- **Gợi ý trên vật:** mặc định **không có popup**. Hướng khám phá kiểu Tunic trong brief yêu cầu người chơi tự đọc hoa văn màu nguyên tố khắc trên vật (xem Visual/Audio). Popup biểu tượng nguyên tố chỉ là **tùy chọn trợ năng**: khi bật, đứng gần vật thì hiện biểu tượng nguyên tố cần dùng (nếu đã có) hoặc dấu "?" mờ (nếu chưa có).
- **Phản hồi thiếu thanh:** thanh rung, nấc trống nháy.

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
| Seed, Spider, Spider King có liên quan nguyên tố trong lore không (bảng yếu/kháng đang tạm cho chúng yếu Lửa) | Người dùng | Trước khi viết story combat nguyên tố |
| Luật rơi vực (Lướt Gió ra ngoài mép) chưa được định nghĩa ở đâu | Người dùng | Trước khi làm lướt MVP |
| Các con số (100 / 12 / 33, ×1.5 / ×0.5 / ×0.25, thời gian hiệu ứng) chưa qua cân bằng | Người dùng, sau playtest | Sau bản lướt MVP chơi được |
| Thanh nguyên tố đặt trong `SO_PlayerStats` hay một ScriptableObject riêng | Lập trình (ADR) | Khi viết story đầu tiên của hệ thống |
| Có cho nâng cấp thanh nguyên tố ở Làng Mái Vá (brief chỉ nhắc tim tối đa và stamina) không | Người dùng | Khi thiết kế cửa hàng làng |
| Thứ tự gợi ý cho 3 nguyên tố tự do (để màn chơi tăng độ khó hợp lý) | Người dùng + level-designer | Khi lên bản đồ thế giới |
