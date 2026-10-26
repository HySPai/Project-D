# Ngày đêm (Day-Night Cycle)

> **Status**: Implemented (chưa design-review)
> **Author**: người dùng + Claude (solo, không có specialist)
> **Last Updated**: 2026-10-26
> **Implements Pillar**: không có file pillar ở tier `minimal`; phục vụ không khí và nhịp nghỉ "Điểm nghỉ và cái chết kiểu Souls" trong `design/game-brief.md`
> **Phạm vi**: hệ mới, thêm theo yêu cầu người dùng 2026-10-26. **Ánh sáng lấy theo demo `Assets/Demo/AssetsPixel`** (quyết định người dùng 2026-10-26, thay cho tông màu theo ảnh tham chiếu): logic và thông số của `DayNightCycleManager` và `CloudShadowManager` trong scene `ShaderTest`, cùng volume `SampleSceneProfile`. **Material của các GameObject giữ nguyên bản gốc** (người dùng chọn lại cùng ngày), không dùng `ToonLit`.

> **Workflow**: `minimal`. GDD này là tự nguyện; brief vẫn là tài liệu thiết kế chính. Có Summary, 5 mục chuẩn, Formulas và Tuning Knobs vì hệ có nhiều con số.
> **Specialist**: không hỏi ý kiến (Solo mode). Cần review thủ công.

## Summary

Thời gian trong game trôi liên tục từ sáng qua trưa, chiều, tối rồi lại sáng. Mặt trời và mặt trăng đổi hướng, màu và độ sáng theo giờ. Nghỉ ở Ngưỡng đã thắp thì giờ tua nhanh trong 2 giây: màn hình tối dần, đen hẳn đúng 00:00, rồi sáng dần tới 09:00 sáng hôm sau.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Ngưỡng, Lưu và tải`
> Code: `Assets/Scripts/CoreGame/World/DayNight/` (`DayNightMath`, `DayNightCycle`, `CloudShadows`, `BlackoutFade`); chuỗi nghỉ trong `SaveController.RestAsync`.

## Overview

Hệ Core điều khiển ánh sáng toàn cảnh theo một đồng hồ 24 giờ. Người chơi không chỉnh trực tiếp đồng hồ; họ thấy thời gian qua màu nắng, độ dài bóng và màu trời, và tác động lên nó duy nhất bằng cách nghỉ ở Ngưỡng. Hệ cho thế giới một nhịp sống, và cho việc nghỉ một khoảnh khắc "ngủ qua đêm" rõ ràng.

## Detailed Design

### Core Rules

1. **Đồng hồ:** giờ là số thực 0–24, quấn vòng. Một ngày đủ 24 giờ trôi trong `dayDurationSeconds` giây thật (mặc định 840 s, tức 1 giờ game = 35 s). Slot mới bắt đầu lúc **09:00**.
2. **Ngày và đêm:** mặt trời mọc 06:00, lặn 18:00. Ban ngày (06–18) mặt trời đi cung 0°→180°: 0° ở chân trời lúc mọc, cao nhất lúc 12:00, 180° lúc lặn. Ban đêm (18–06) góc đi tiếp 180°→360°, nửa đêm ở 270°. Mặt trăng luôn đối diện mặt trời (góc + 180°).
3. **Quỹ đạo và hướng nắng** (sửa 2026-10-26 theo ảnh tham chiếu ban ngày): mặt phẳng quỹ đạo xoay quanh trục Y theo `orbitAxisY` = 198° và nghiêng `orbitTilt` = 40°. Buổi trưa mặt trời cao khoảng 50°, chiếu từ phía trên bên phải màn hình, nên bóng đổ chéo xuống bên trái và không bao giờ biến mất.
4. **Độ sáng và màu:** gọi `sunDot` là độ cao của mặt trời (0 ở chân trời và cả đêm, 1 ở thiên đỉnh). Logic lấy từ demo; màu ban ngày chỉnh lại theo ảnh tham chiếu để hết ám cam:
   - cường độ mặt trời = 1.1 × đường cong(`sunDot`), tắt bóng khi dưới 0.01. Đường cong dạng ease-out (0 → 0.68 ở `sunDot` 0.2 → 0.92 ở 0.5 → 1), để nắng đủ mạnh ngay sau bình minh, buổi sớm không bị nhạt và tối như chạng vạng (sửa 2026-10-27);
   - cường độ mặt trăng = 0.75 × (1 − đường cong(`sunDot`)), đủ để ban đêm nhìn rõ địa hình; mặt trăng đổ bóng mềm khi đủ sáng;
   - màu nắng: cam (1, 0.5, 0.22) sát chân trời, vàng ấm (1, 0.76, 0.5) ở `sunDot` 0.15, (1, 0.88, 0.7) ở 0.4, (1, 0.95, 0.86) ở thiên đỉnh;
   - màu trăng: xanh ngọc (0.35, 0.62, 0.7) khi trăng thấp tới (0.55, 0.88, 0.9) khi trăng cao (sửa 2026-10-27 theo ảnh tham chiếu rừng đêm);
   - ambient (ambient mode Flat, cũng đưa vào `_GlobalAmbientColor`): đêm xanh navy-ngọc (0.05, 0.11, 0.16); chạng vạng/sáng sớm (0.44, 0.38, 0.56) ở 0.1; ban ngày oải hương (0.5, 0.47, 0.66) ở 0.4 và (0.52, 0.5, 0.68) ở thiên đỉnh. Ambient cao và nắng vừa phải giữ tương phản dịu: chỗ nắng không bạc màu, chỗ bóng không xám đen.
5. **Bóng mây:** mây là một light cookie trên mặt trời (`T_CloudCookie`, tạo từ `CloudNoise2` của demo, khoảng 38% diện tích có mây, trong mây còn 42% nắng), phủ 26 m và trôi 0.5 m/s theo hướng `cloudDirection`. Vì là cookie nên hiện trên mọi material. `CloudShadows` vẫn đưa thông số mây của demo vào shader toàn cục cho vật liệu `ToonLit` nếu sau này dùng.
6. **Vật liệu:** mọi GameObject giữ material gốc. Không đổi sang `ToonLit`.
7. **Hậu kỳ và nền:** volume `Global Volume Profile daynight` (bản sao `SampleSceneProfile`, đã chỉnh): tông tươi sáng theo Tunic (sửa 2026-10-27): Tonemapping Neutral, Bloom ngưỡng 0.95 cường độ 0.5 scatter 0.75, Vignette 0.22, Color Adjustments bão hòa +32 tương phản +8 phơi sáng 0, White Balance nhiệt độ −10 tint −28 (kéo nền vàng olive gốc về xanh lá), Split Toning bóng (0.5, 0.45, 0.66) sáng (1, 0.95, 0.85) cân bằng −15. Nền trời là skybox mặc định ở phòng ngoài trời; phòng kín (`AreaRoom.darkOutside`) nền đen.
7b. **Không khí ban đêm** (`NightAmbience`, thêm 2026-10-27 theo ảnh tham chiếu rừng đêm): `DayNightCycle.NightFactor` = 1 − đường cong(`sunDot`) (0 ban ngày, 1 sau khi mặt trời lặn) điều khiển hai lớp, cả hai đi theo người chơi (đom đóm đã thử rồi bỏ theo yêu cầu người dùng 2026-10-27):
   - **volume đêm** (`Night Volume Profile`, độ ưu tiên 1, weight = `NightFactor`): White Balance −18/0, Split Toning bóng navy (0.12, 0.22, 0.4) sáng (0.9, 0.88, 0.8) cân bằng −35, bão hòa +25, tương phản +14, Vignette 0.42 màu xanh đen, Bloom 1.0 ngưỡng 0.8. Vùng sáng giữ trung tính để lửa vẫn cam;
   - **cột sáng trăng**: **tạm tắt** (GameObject `MoonShafts` inactive, người chơi chọn bổ sung sau). Đã có sẵn 3 quad cộng sáng (`M_MoonShaft`, `T_MoonShaft`) xoay theo góc camera, nghiêng 30°, đặt trước khối hình; bật lại là chạy.
   - Màu lửa Ngưỡng đẩy sang đỏ cam (ngoài (3.6, 0.75, 0.08), trong (4.2, 2.0, 0.35)) để bù tint xanh của grade chung.
8. **Nghỉ tua giờ** (chỉ khi bấm lần 2 ở Ngưỡng đã thắp, xem `rest-point.md`). Cả chuỗi dài **2 giây**:
   - đồng hồ ngừng tự chạy, người chơi bị khóa điều khiển;
   - **Pha tối (0.9 s):** giờ tua từ giờ hiện tại tới 00:00, màn hình tối dần theo đường cong mượt, **đen hoàn toàn đúng 00:00**;
   - **Lúc đen (0.2 s):** hồi đầy, quái hồi sinh, người chơi đứng cạnh đống lửa;
   - **Pha sáng (0.9 s):** giờ tua 00:00 → 09:00, màn hình sáng dần, **sáng hẳn đúng 09:00**;
   - đồng hồ chạy lại từ 09:00, mở khóa điều khiển, lưu game.
9. **Nghỉ sau nửa đêm** (00:00–09:00): bỏ phần tua tới nửa đêm vì nó đã qua. Màn hình tối hẳn trong 0.9 s ở giờ hiện tại, rồi giờ tua tới 09:00 trong lúc sáng dần.
10. **Lưu:** giờ hiện tại ghi vào slot ở mọi lần lưu (`SaveData.hourOfDay`). Tải slot thì đồng hồ tiếp tục từ giờ đã lưu. Nghỉ xong luôn lưu 09:00. Slot cũ thiếu trường này thì dùng 09:00.

### Formulas

| Công thức | Biểu thức | Biến |
|---|---|---|
| Giờ trôi mỗi khung | `hour += Δt × 24 / dayDurationSeconds` rồi quấn về 0–24 | `Δt`: giây thật của khung |
| Góc mặt trời ban ngày | `angle = 180 × (hour − 6) / 12` | ví dụ 09:00 → 45°, 12:00 → 90° |
| Góc ban đêm | `angle = 180 + 180 × giờ_kể_từ_18 / 12` | ví dụ 21:00 → 225°, 03:00 → 315° |
| Kế hoạch nghỉ | giờ ≥ 09: `tối = 24 − giờ`, `sáng = 9`; giờ < 09: `tối = 0`, `sáng = 9 − giờ` | ví dụ 20:00 → 4 h tối + 9 h sáng |
| Thời lượng pha | `(restDurationSeconds − blackHoldSeconds) / 2` | (2 − 0.2) / 2 = 0.9 s, không phụ thuộc số giờ cần tua |
| Độ đen theo tiến độ pha `t` | pha tối `smooth(t)`, pha sáng `1 − smooth(t)`; `smooth(t) = t²(3 − 2t)` | `t` 0→1 |
| Volume đen | `weight = −log2(1 − độ_đen) / 10` | volume mang post exposure −10 EV, nên ánh sáng còn lại giảm tuyến tính theo độ đen |

### States and Transitions

| Trạng thái | Chuyển sang |
|---|---|
| Đang trôi | → Đang tua (bắt đầu nghỉ ở Ngưỡng đã thắp) |
| Đang tua — pha tối | → Đen (tới 00:00 sau 0.9 s) |
| Đen | → Đang tua — pha sáng (sau 0.2 s) |
| Đang tua — pha sáng | → Đang trôi (tới 09:00 sau 0.9 s, lưu game) |

### Interactions with Other Systems

| Hệ thống | Đưa vào | Nhận ra |
|---|---|---|
| Ngưỡng (`rest-point.md`) | Lệnh nghỉ | Thời điểm "đen" để hồi phục và reset thế giới |
| Lưu và tải (`save-load.md`) | `hourOfDay` khi tải | `hourOfDay` khi lưu |
| Vùng và chuyển vùng | Phòng kín hay ngoài trời | Nền skybox hay đen |
| Điều khiển nhân vật | — | Khóa điều khiển khi đang tua |
| Chết và hồi sinh | — | Không đổi giờ: chết không tua thời gian |

## Edge Cases

- **Nếu nghỉ lúc 12:00:** tua 12 giờ tới nửa đêm trong 0.9 s, rồi 9 giờ tới sáng trong 0.9 s; mặt trời quét nhanh qua hoàng hôn.
- **Nếu nghỉ lúc 23:50:** pha tối vẫn 0.9 s dù chỉ tua 10 phút game, để màn hình tối dần chứ không cắt.
- **Nếu nghỉ lúc 02:00:** không tua qua nửa đêm. Màn hình tối hẳn trong 0.9 s ở 02:00, rồi tua 02:00 → 09:00 trong lúc sáng dần.
- **Nếu nghỉ đúng 09:00:** coi như đã qua giờ thức, tua trọn 15 giờ tới nửa đêm rồi 9 giờ tới sáng hôm sau.
- **Nếu chết:** giờ không đổi, hồi sinh ở Ngưỡng với giờ hiện tại; chỉ nghỉ mới tua giờ.
- **Nếu thoát game giữa lúc tua:** chuỗi nghỉ chưa lưu; tải lại ở lần lưu trước (giờ của lần lưu đó).
- **Nếu có quái bị tắt (inactive) gần Ngưỡng:** không tính là đang truy đuổi.
- **Nếu ở phòng kín (`darkOutside`):** ánh sáng vẫn theo giờ, nền ngoài tường luôn đen.
- **Nếu `Time.timeScale` bị đổi (hit stop):** đồng hồ thường dùng `deltaTime` nên dừng theo; chuỗi nghỉ dùng thời gian thật (`unscaledDeltaTime`) nên không bị kẹt.

## Dependencies

| Hệ thống | Hướng | Loại | Ghi chú |
|---|---|---|---|
| Ngưỡng (`design/gdd/rest-point.md`) | Ngày đêm phục vụ | Cứng | Chuỗi nghỉ gọi tua giờ |
| Lưu và tải (`design/gdd/save-load.md`) | Ngày đêm phụ thuộc vào | Mềm | Ghi và đọc `hourOfDay` |
| Vùng và chuyển vùng | Ngày đêm phụ thuộc vào | Mềm | Chọn nền theo phòng |
| Điều khiển nhân vật | Phụ thuộc vào Ngày đêm | Mềm | Khóa khi đang tua |

## Tuning Knobs

| Núm | Mặc định | Khoảng an toàn | Ảnh hưởng |
|---|---|---|---|
| `startHour` | 9 | 6–12 | Giờ khi bắt đầu slot mới |
| `dayDurationSeconds` | 840 | 300–2400 | Độ dài một ngày; ngắn quá thì màu đổi lộ khi đang đánh |
| `sunriseHour` / `sunsetHour` | 6 / 18 | 5–7 / 17–20 | Độ dài ngày và đêm |
| `orbitAxisY` | 198 | theo góc camera | Hướng nắng và bóng trên màn hình |
| `orbitTilt` | 40 | 20–50 | Độ cao mặt trời buổi trưa (90 − tilt); lớn thì bóng dài |
| `maxSunIntensity` | 1.1 | 0.9–1.6 | Độ chói ban ngày; cao quá thì nền sáng màu bị bạc |
| `lightIntensityCurve` | ease-out (0.68 ở 0.2) | — | Nắng buổi sớm mạnh nhanh hay chậm |
| Bão hòa / tint (volume daynight) | +32 / −28 | 0–35 / −40–0 | Độ tươi và độ xanh của cả cảnh; ảnh hưởng cả đêm (đã kiểm 2026-10-27: hoàng hôn tím xám, đêm xanh dương thẫm, không ám xanh lá) |
| `cookieSize` / `cookieDriftSpeed` | 26 m / 0.5 m/s | 15–60 / 0.2–1.5 | Cỡ đám mây và tốc độ mây trôi |
| `maxMoonIntensity` | 0.75 | 0.5–1 | Độ sáng đêm; thấp quá thì không nhìn thấy, cao quá thì mất cảm giác đêm |
| `shaftIntensity` / `shaftColor` | 0.7 / (0.6, 0.88, 0.88) | 0.3–1.2 | Độ rõ và màu cột sáng trăng (đang tắt) |
| `wakeHour` | 9 | 6–10 | Giờ thức dậy sau khi nghỉ |
| `restDurationSeconds` | 2 | 1.5–4 | Tổng thời gian chuỗi nghỉ |
| `blackHoldSeconds` | 0.2 | 0–0.6 | Thời gian giữ màn đen (nằm trong tổng thời gian) |
| Gradient nắng, ambient, trăng | luật 4 | — | Tông màu theo giờ |

## Acceptance Criteria

- **GIVEN** slot mới, **WHEN** vào game, **THEN** đồng hồ ở 09:00, nắng trắng ấm không ám cam, bóng màu oải hương, có bóng mây trôi trên nền.
- **GIVEN** đang chơi, **WHEN** để 840 giây trôi qua, **THEN** đồng hồ quay về đúng giờ cũ.
- **GIVEN** 18:00, **WHEN** nhìn cảnh, **THEN** mặt trời ở chân trời, nắng cam đỏ gần tắt, mặt trăng bắt đầu sáng.
- **GIVEN** Ngưỡng đã thắp lúc 20:00, **WHEN** bấm Tương tác, **THEN** trong khoảng 2 giây màn hình tối dần, đen hẳn khi đồng hồ chạm 00:00, rồi sáng dần và sáng hẳn đúng 09:00.
- **GIVEN** đang tua, **WHEN** nhấn di chuyển hoặc tấn công, **THEN** nhân vật không phản hồi.
- **GIVEN** nghỉ xong, **WHEN** thoát và tải lại, **THEN** đồng hồ ở 09:00.
- **GIVEN** nghỉ lúc 02:00, **WHEN** chuỗi nghỉ chạy, **THEN** đồng hồ không qua 00:00 lần nữa và kết thúc ở 09:00 cùng ngày.
- **GIVEN** đang ở 15:00, **WHEN** chết và hồi sinh, **THEN** đồng hồ vẫn khoảng 15:00.
- Logic thuần (`DayNightMath`) có test tự động: `Assets/Tests/EditMode/Editor/DayNightMathTests.cs`.
