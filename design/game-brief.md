# Game Brief: Project-D (tên tạm)

**Pitch một câu:** Một hiệp sĩ chó lang thang qua tàn tích của loài người đã diệt vong, chém bầy côn trùng quái vật trong những trận đấu khắc nghiệt kiểu Souls, rồi dùng kỹ năng mới và máy móc người xưa bỏ lại để mở đường vào những vùng đất cũ.

**Thể loại:** soulslike + metroidvania + giải đố + phiêu lưu, camera isometric.

**Bối cảnh:** Loài người đã diệt vong sau một thảm họa, chỉ còn tàn tích của nền văn minh cũ. Các loài động vật tiến hóa (mang chút hình dáng con người) đã lập nên nền văn minh riêng. Côn trùng tiến hóa thất bại, không có trí khôn, trở thành quái vật. Động vật và quái côn trùng đối đầu nhau.

## Vòng lặp chính
- Rời điểm nghỉ đi khám phá → gặp quái côn trùng: khóa mục tiêu, lăn, chém, giữ stamina.
- Hạ quái nhặt tiền → về điểm nghỉ hồi máu và lưu tiến độ (quái hồi sinh), về làng nâng cấp.
- Gặp đường chặn: giải đố bằng máy móc tàn tích, đẩy khối/môi trường hoặc lợi dụng hành vi quái. Đường nào cần kỹ năng chưa có thì để đó quay lại sau.
- Hạ boss → nhận kỹ năng di chuyển mới → mở lối tắt và vùng mới.

## Mục tiêu & thất bại — thế nào là "chạy được"
- Mục tiêu (MVP): đi xuyên 2 vùng, hạ boss của mỗi vùng, dùng kỹ năng mới mở được lối đã thấy từ trước.
- Thất bại: hết tim → hồi sinh ở điểm nghỉ gần nhất, quái hồi sinh, tiền rơi tại chỗ chết. Chết lần nữa trước khi nhặt lại thì mất hẳn số tiền đó.

## MVP — phải có thì mới là game
1. Combat Souls: lock-on, lăn, đánh, stamina, tim, hit feel *(đã có code)*; Khiên (chặn, parry) và Cung phụ nhận ở Làng Mái Vá (xem `design/gdd/weapons.md`).
2. Điểm nghỉ và cái chết kiểu Souls: hồi sinh, quái hồi sinh, rơi/nhặt lại tiền.
3. Tiền tệ và nâng cấp ở làng (tối thiểu: tim tối đa, stamina).
4. Một kỹ năng di chuyển (ví dụ lướt) mở ít nhất một lối tắt hoặc đường bị chặn.
5. Ba dạng giải đố, mỗi dạng ít nhất 1 câu: máy móc tàn tích, đẩy khối/môi trường, lợi dụng quái.
6. Quái côn trùng: Seed, Spider, Turret *(đã có code)*, mỗi vùng 1 boss (vùng 1: Spider King).
7. Hai vùng liên thông và một làng động vật (NPC, cửa hàng nâng cấp).

## Ngoài phạm vi — không làm
- Không multiplayer, không chế tạo, không cây kỹ năng hay build phức tạp.
- Không cốt truyện phân nhánh: câu chuyện kể qua tàn tích và vài lời NPC trong làng.
- Không có nhân vật chơi nào khác ngoài hiệp sĩ chó.
- Không có vùng thứ ba và không có kỹ năng di chuyển thứ hai trong MVP.
- Không có dạng giải đố nào ngoài 3 dạng trên trong MVP (mở rộng sau).
- Không có bản đồ thế giới hay lưu ở bất cứ đâu: chỉ lưu ở điểm nghỉ.

## Thứ tự xây
1. Điểm nghỉ và cái chết kiểu Souls (hồi sinh, rơi/nhặt tiền), xây tiếp trên luồng chết → restart đã có.
2. Kỹ năng di chuyển và một lối tắt bị chặn, để thử sớm nhất xem vòng metroidvania có hoạt động ở góc nhìn isometric không.
3. Vùng 1 liên thông: 3 câu đố, Seed/Spider/Turret, boss Spider King.
4. Làng động vật: NPC, cửa hàng, nâng cấp bằng tiền.
5. Vùng 2 và boss 2 (quái côn trùng mới đầu tiên).
6. Đánh bóng âm thanh, VFX và hit feel.

## Định hướng khám phá (cảm hứng từ Tunic)
Khám phá là chính. Kể chuyện gián tiếp qua môi trường; người chơi tự ghép thông tin.

**Làm:**
- Lối đi và cửa ẩn sau góc camera (tường, mái, tán cây). Camera giữ góc isometric cố định, không xoay.
- **Luật hiển thị:** quái và nhân vật bị che thì chỉ hiện viền bóng. **Tường không bao giờ trong suốt**, để lối đi ẩn không bị lộ.
- Lối tắt mở từ phía bên kia (thang, cổng then cài, cửa cuốn của loài người). Mỗi vùng có 2–3 lối tắt nối về Ngưỡng hoặc làng.
- **Dạy cơ chế bằng NPC và thiết kế màn, không có popup hướng dẫn.** NPC ở làng chỉ cách; màn chơi đặt tình huống buộc dùng cơ chế lần đầu. Ví dụ:
  - Lão Mật dạy chặn và parry;
  - thực thể giữ nguyên tố dạy combo qua thử thách.
- Không có điểm đánh dấu nhiệm vụ. Bản đồ là **bản vẽ tay từng vùng, mua ở Chị Mỏ**. Bản đồ không hiện lối ẩn hay bí mật.
- Bí mật thưởng cho người tò mò: rương sau thác, tường giả, lối chỉ thấy từ trên cao. Phần thưởng gồm tàn ý, Mảnh Sắt Cũ và mũi tên.
- Tùy chọn trợ năng: giảm sát thương nhận vào hoặc bất tử, cho người muốn khám phá mà không cần độ khó souls.

**Không làm** (để không sao chép Tunic):
- Không có trang sách hướng dẫn hay vật phẩm kiểu "sổ tay" để sưu tầm.
- Không có ngôn ngữ giả cho toàn bộ chữ. NPC động vật nói tiếng Việt bình thường.
- Không có câu đố meta nhập chuỗi hướng.
- Không có thẻ chỉ số hay hệ cúng vật phẩm lên cấp.
- Không có kết thúc cần giải mật mã.

---
**Dành cho ai / cảm giác gì:** Fan Souls và metroidvania thích trận đấu khó phải đọc chiêu. Cảm giác căng thẳng khi đấu, thỏa mãn khi mở được lối tắt, tò mò khi lần ra dấu vết loài người.

**Hướng hình ảnh & âm thanh:** Low-poly stylized. Tàn tích bê tông và máy móc gỉ của loài người bị cây cỏ phủ kín. Động vật nhân hóa dễ thương đối lập với côn trùng đáng sợ, quái nhìn thấy được xuyên tường. Hit feedback mạnh và có haptics. Nhân vật chính là chó nhỏ dáng chó rừng, tai to dựng nhọn, khăn quàng và kiếm đeo sau lưng, tính cách tò mò và giản dị (xem `design/narrative/characters/hiep-si-cho.md`).

**Game tham chiếu:** Tunic: cũng isometric, nhân vật là một con vật nhỏ, combat kiểu Souls với lock-on, lăn và stamina, thế giới mở dần. MVP giữ khoảng 10% của Tunic: 2 vùng và 1 kỹ năng mở lối tắt, không có hệ thống bí ẩn hay sách hướng dẫn.
