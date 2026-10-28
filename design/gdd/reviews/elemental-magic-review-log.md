# Review Log — Elemental Magic

## Review — 2026-10-28 — Verdict: NEEDS REVISION
Scope signal: L
Specialists: none (solo)
Blocking items: 4 | Recommended: 8
Summary: Bảng yếu/kháng mâu thuẫn với luật 17 ở Bọ Giáp. 4 kỹ năng chiến đấu chưa có số nên tiêu chí ×1.5 không kiểm được. Giữ nút phép trước vật tương tác chưa có luật. Bảng luật 11 còn mô tả yểm Đá kiểu cũ.
Prior verdict resolved: First review
Findings:
- [BLOCKING] Core Rules 17: Bọ Giáp khao khát Đá nhưng yếu Đá, trái luật 17
- [BLOCKING] Core Rules 11: kỹ năng thiếu sát thương, tầm, hình dạng
- [BLOCKING] Core Rules 6: giữ nút phép trước vật tương tác chưa định nghĩa
- [BLOCKING] Core Rules 11: dòng Yểm Đá "đòn kế tiếp" lệch với luật 11b
- [RECOMMENDED] Game Feel: độ trễ 100ms cần tính từ lúc thả nút
- [RECOMMENDED] Core Rules 17: Turret miễn hiệu ứng nhưng không có cột trong bảng
- [RECOMMENDED] Core Rules 12: combo Lửa + Gió với lửa trên vũ khí chưa rõ cách kích
- [RECOMMENDED] Core Rules 16: combo đá nung cần dựng cột đá (chỉ có ở câu đố)
- [RECOMMENDED] Core Rules 11: Lướt Gió dùng nút nào, va quái khi thanh < 33 thì sao
- [RECOMMENDED] Dependencies: rest-point ghi Mềm, save-load dòng 86 còn cũ
- [RECOMMENDED] Interactions: dòng Weapons thiếu yểm
- [RECOMMENDED] Core Rules 11b: yểm Gió rút ngắn cửa sổ combo có chủ ý không

### Sửa sau review — 2026-10-28
- 4 lỗi chặn đã sửa. Người dùng chốt: nút phép và nút tương tác tách riêng (nhấn phép = kỹ năng, giữ = yểm, nút tương tác = giải đố); yểm cần thanh đầy và tiêu cả 100; Bọ Giáp là ngoại lệ của luật 17; số kỹ năng ở luật 11a.
- Đề xuất đã sửa: độ trễ tính từ lúc thả nút; dòng Weapons trong Interactions; độ cứng phụ thuộc ở rest-point; save-load dòng 82–86.
- Đề xuất còn mở: Turret miễn hiệu ứng (cột dữ liệu), combo 12 và 16, nút của Lướt Gió, yểm Gió và cửa sổ combo.
- Không ghi receipt hash: verdict trên thuộc bản trước khi sửa. Cần `/design-review` lại để có verdict cho bản hiện tại.

## Review — 2026-10-28 (lần 2) — Verdict: NEEDS REVISION
Scope signal: L
Specialists: none (solo)
Blocking items: 1 | Recommended: 7
Summary: 4 lỗi chặn của lần 1 đã được giải quyết. Còn 1 lỗi chặn mới lộ ra sau khi tách nút: Lướt Gió có hai đường kích hoạt (nút lướt miễn phí kèm đẩy lùi, và nút phép tốn 33) mâu thuẫn nhau. Các đề xuất chủ yếu là combo, Cầu Lửa với vật cháy được, và trạng thái giữ nút.
Prior verdict resolved: Yes (4/4 lỗi chặn lần 1)
Findings:
- [BLOCKING] Core Rules 4/11/AC: Lướt Gió kích hoạt bằng nút lướt (miễn phí, có đẩy lùi theo AC) hay nút phép (tốn 33)
- [RECOMMENDED] Edge Cases: Cầu Lửa không đốt tường tơ/đèn trong khi tên Lửa thì đốt được, ngược kỳ vọng người chơi
- [RECOMMENDED] Core Rules 12: combo Lửa + Gió với lửa trên vũ khí chưa rõ cách kích
- [RECOMMENDED] Core Rules 16: combo đá nung chỉ xảy ra ở chỗ có câu đố dựng cột
- [RECOMMENDED] States: thiếu trạng thái "đang giữ nút phép" (đi lại, bị đánh, lăn khi đang giữ)
- [RECOMMENDED] Edge Cases: giữ nút phép khi đang nhắm Cung thì phủ tên hay yểm
- [RECOMMENDED] Core Rules 11a: Choáng 1s của Đá Văng và yểm có áp lên boss không
- [RECOMMENDED] Core Rules 17: Turret miễn hiệu ứng chưa có cột dữ liệu
Reviewed-Content-Hash: design/gdd/elemental-magic.md f1e0073942a0db6ed2fdf0101d9b3e34d6f7de05
Reviewed-Content-Hash: design/registry/entities.yaml 8fd06f86b857465be8bac82bd8ead307311d4131
