# Hiệp sĩ chó — hồ sơ nhân vật chính

> **Cập nhật:** 2026-10-18
> **Nguồn:** ảnh tham chiếu người dùng cung cấp (truyện tranh 4 khung: chú chó nhỏ vào hang tối, trầm trồ trước kho vàng, rồi vui vẻ kéo về một bao xương). Ảnh chỉ là **tham chiếu tinh thần**: bảng màu, trang phục và chi tiết khuôn mặt phải khác đi để thành nhân vật riêng của Project D.
> **Liên quan:** `design/narrative/lore/world-foundation.md` mục 6 (động cơ, bí mật), `design/game-brief.md` (hướng hình ảnh), `design/gdd/weapons.md` (vũ khí mang trên người).

## 1. Ngoại hình — CANON (người dùng duyệt 2026-10-18)

- **Loài và tỉ lệ:** chó nhỏ dáng chó rừng. Đầu to so với thân, kiểu chibi, để đọc rõ ở camera isometric nhìn từ xa.
- **Tai:** rất to, dựng nhọn. Đây là nét nhận diện chính của silhouette.
- **Lông:** nâu vàng; mõm, má và ngực màu kem; mũi đen, mõm ngắn.
- **Mắt:** to tròn, màu đen, có đốm sáng; biểu cảm rõ (ngạc nhiên, hớn hở, sợ).
- **Trang phục:**
  - khăn quàng sẫm màu, dài, bay theo khi chạy và lăn;
  - áo chẽn màu xanh;
  - dây da chéo ngực để đeo vũ khí.
- **Vũ khí trên người:** kiếm đeo chéo sau lưng, cán quấn vải, chuôi đơn giản. Cung và Khiên (xem `weapons.md`) cũng đeo sau lưng hoặc bên hông khi không dùng.
- **Ràng buộc với art:** low-poly stylized theo brief; nhân vật "dễ thương" đối lập với côn trùng đáng sợ. Bảng màu cuối cùng do art-director chốt, không lấy nguyên màu ảnh tham chiếu.

## 2. Tính cách — CANON (người dùng duyệt 2026-10-18)

- **Tò mò:** luôn muốn biết sau góc tường có gì. Khớp với định hướng khám phá kiểu Tunic.
- **Rụt rè ở chỗ tối:** chần chừ trước hang tối và lối lạ, nhưng vẫn đi tiếp.
- **Dễ trầm trồ:** phản ứng mạnh, ngây thơ trước điều mới.
- **Nhu cầu giản dị:** không ham của cải. Trước một kho báu, nó chọn thứ một con chó thật sự thích (xương). Khớp với động cơ ở `world-foundation.md` mục 6: mong có một nơi để gọi là nhà, không chinh phục.
- **Hài hước nhẹ:** tạo khoảnh khắc vui giữa thế giới tàn tích, giữ "tông luôn ấm" của lore.
- **Lòng trung thành theo bản năng:** giữ nguyên đề xuất ở `world-foundation.md` mục 6.

## 3. Móc gameplay — ĐỀ XUẤT (người dùng chọn giữ; chờ game-designer thiết kế chi tiết)

Cả ba đều dạy người chơi bằng hình ảnh trên nhân vật, không dùng popup, đúng luật "dạy cơ chế bằng NPC và thiết kế màn" của brief.

1. **Tai làm tín hiệu**
   - Tai dựng lên khi ở gần bí mật hoặc khi quái chuẩn bị tấn công ngoài tầm nhìn.
   - Tai cụp xuống khi chỉ còn 1 tim.
   - Phụ thuộc: hệ animation nhân vật, hệ tim (`PlayerState`), hệ bí mật.
2. **Dấu "!" khi đánh hơi**
   - Hiện dấu "!" trên đầu khi đi gần một lối ẩn hoặc tường giả.
   - Không chỉ đường, chỉ báo "có gì đó ở đây"; người chơi vẫn phải tự tìm.
   - Cần quyết: bán kính phát hiện, có tắt được trong tùy chọn trợ năng/độ khó không.
3. **Xương là món quà vui**
   - Xương đặt ở các bí mật như một phần thưởng vui, **không phải tiền tệ** và không thay thế tàn ý.
   - Nhặt xương có phản ứng hớn hở (vẫy đuôi, khoảnh khắc ngắn).
   - Cần quyết: xương có tác dụng gì (sưu tầm, đổi quà ở NPC, chỉ để vui) — chưa chốt, không được thêm hệ thống kinh tế mới trái với brief.

## 4. Còn để ngỏ

- [CHƯA XÁC ĐỊNH — cần hỏi người dùng] Tên hiệp sĩ chó.
- [CHƯA XÁC ĐỊNH — cần hỏi người dùng] Bí mật thật về nguồn gốc (xem `world-foundation.md` mục 6 và 7).
- Bảng màu và model 3D cuối cùng: chờ art bible (`/art-bible`).
- Animation cần cho các móc gameplay (tai, đuôi, dấu "!"): chờ game-designer và technical-artist.
