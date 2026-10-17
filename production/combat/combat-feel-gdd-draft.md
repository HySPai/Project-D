# Combat Game Feel - Bản nháp đề xuất (Phase 1)

Trạng thái: DRAFT, chờ người dùng duyệt. Chưa sửa code/prefab/asset nào.
Phạm vi: soulslike isometric kiểu Tunic. Mọi thông số feel nằm trong ScriptableObject, phản hồi dùng Feel (MMF_Player).
Nguồn: code đã đọc ở phiên trước + kiểm nhanh Assets/Feel (xem mục 2).

---

## 1. Đánh giá hiện trạng

Mức ưu tiên: Cao = ảnh hưởng trực tiếp cảm giác đánh, Trung = nên làm, Thấp = để sau.

| Khu vực | Hiện có | Vấn đề | Ưu tiên |
|---|---|---|---|
| Hit stop | MMF_FreezeFrame "Hit Stop" trong MMF_Player hit/kill trên Player.prefab | Giá trị nằm trong prefab, không theo vũ khí/đòn; chỉ chỉnh được bằng cách mở prefab. Chưa kiểm: duration thực tế đang đặt bao nhiêu | Cao |
| Screen shake | MMF_CinemachineImpulse "Camera Shake" (hit, kill, damage-taken) | Cùng lý do: nằm trong prefab, cường độ chỉ scale theo AttackData.impact. Không có hướng (shake không theo hướng đòn) | Cao |
| Hit flash | MMF_Flicker "Flash" trên enemy và player | Có sẵn, đủ dùng. Chưa kiểm: màu/thời lượng | Thấp |
| VFX | Enemy: ParticlesInstantiation "Hit Particles"; sword: TrailRenderer | Một hiệu ứng cho mọi đòn, không phân biệt đòn thường/chí mạng/giết | Trung |
| SFX | Không có SFX chiến đấu (AudioSfxID chỉ có âm UI) | Đòn đánh "câm" - thiếu lớn nhất về cảm giác va chạm | Cao |
| Knockback theo vũ khí | Một bộ số chung trong SO_EnemyStats (đẩy 1.5m/0.25s, stun 0.5s); coroutine trong EnemyMovement | Không phụ thuộc vũ khí/đòn; vi phạm luật no-coroutine (story 005 sẽ xử lý) | Cao |
| Input buffer | attackBufferDuration 0.4s (serialized trong PlayerCombat) | GDD nói vũ khí chậm không được buffer dài hơn kiếm - hiện chưa theo vũ khí. Chưa kiểm: có buffer cho roll không | Trung |
| Cancel window | Cửa sổ combo là animation event trong .fbx.meta (EnableCanDoCombo); roll bị từ chối khi IsAttacking | Không roll-cancel được recovery; khó chỉnh vì baked trong FBX. Cần state machine (story 002) | Cao (sau 002) |
| Độ trễ phản hồi | GDD: đòn nhẹ bắt đầu trong 100ms | Chưa kiểm (chưa đo từ input tới frame animation đầu) | Trung |
| Xoay hướng khi đánh | Aim assist + lunge DOTween (0.14s, dừng cách 0.9m); DisableCanRotate/EnableCanRotate bằng event | Tốt. Lunge dùng AttackData.lungeDistance nhưng lungeDuration/stopDistance là field riêng | Thấp |
| Roll | Stamina 15, i-frame bằng event trong FBX | Giá trị i-frame nằm trong FBX meta, không chỉnh qua Inspector. Chưa kiểm: độ dài i-frame thực | Trung |
| Startup/recovery (commitment) | animationSpeed trong AttackData; frame dữ liệu nằm trong animation | Không có số startup/recovery nào hiển thị để tune; chỉ chỉnh được animationSpeed | Trung |
| Stamina | Trừ giữa animation (DrainStaminaBasedOnAttack event); stamina mỗi đòn trong AttackData | Trừ muộn nên spam đòn đầu vẫn "miễn phí" cảm giác; có thể là chủ ý. Cần quyết định | Thấp |
| Enemy telegraph | Không có; tấn công ngay khi cooldown 1.5s hết | Soulslike cần đọc được đòn; hiện không né/parry có ý nghĩa. Vấn đề lớn nhất ở phía enemy | Cao |
| Phản ứng trúng đòn enemy | Mọi đòn đều TakeDamage + Chase + stun 0.5s + knockback | Không phân biệt đòn nhẹ/nặng; sẽ có poise ở story 005 | Cao (gộp 005) |
| Độ dễ đọc enemy | Seed lao tới vị trí khóa rồi đánh; Spider kite rồi đánh tại chỗ | Chưa kiểm: có dấu hiệu thị giác nào trước đòn không (nhiều khả năng chỉ có animation) | Trung |
| Camera rung | Chỉ qua Impulse trong MMF_Player | Chưa kiểm: Impulse listener có trên camera không (có vẻ có vì đang dùng) | Trung |
| Camera zoom | CameraOffsetController: ortho 4 -> 8 khi mục tiêu gần mép (ngưỡng 0.6) | Số nằm trong component, không SO | Thấp |
| Camera offset lock-on | free-look 0.15, lock-on 0.3, smooth speeds, serialized | Như trên; chuyển sang SO để tune dễ | Thấp |
| Profile theo vũ khí | WeaponItem có WeaponClass + AttackData; không có feel data | Chưa có khái niệm "feel profile"; GDD đã mô tả khác biệt Sword/Spear/Hammer/Dual | Cao |

---

## 2. Kiến trúc dữ liệu đề xuất

### 2.1 Cách SO điều khiển Feel (chọn một)

Chọn: SO giữ số, một component trung tâm `CombatFeelPlayer` ghi số vào feedback của MMF_Player ngay trước khi Play.
- Mỗi MMF_Player vẫn giữ nguyên feedback (FreezeFrame, Impulse, Flicker, Particles, Sound), nhưng giá trị chính bị ghi đè lúc chạy từ SO: `GetFeedbackOfType<MMF_FreezeFrame>("Hit Stop").FreezeFrameDuration = x`, `Velocity` của MMF_CinemachineImpulse, rồi `FeedbacksIntensity`, rồi `PlayFeedbacks()`.
- Lý do chọn: tránh nhân bản MMF_Player theo từng tier/vũ khí (4 vũ khí x 3 loại = 12 player là khó bảo trì); user chỉ tune ở một chỗ (SO). Nhược điểm: ghi đè giá trị lúc chạy nên số trong prefab chỉ là mặc định - cần ghi chú trong asset.
- Tham chiếu đến `CombatFeelPlayer` đi qua controller trung tâm của nhân vật (đúng luật [SerializeField], không Find/GetComponent).

API Feel đã xác nhận có trong Assets/Feel:
- `MMF_FreezeFrame.FreezeFrameDuration` (float, public, mặc định 0.02) - đã thấy trong MMF_FreezeFrame.cs.
- `MMF_Player.GetFeedbackOfType<T>(string searchedLabel)` và `GetFeedbackOfType<T>()` - đã thấy trong MMF_Player.cs.
- `MMF_Player.FeedbacksIntensity` - đã thấy.
- `MMF_CinemachineImpulse.Velocity` (Vector3, public) - đã thấy.

Cần kiểm trên Feel version trong project (chưa xác nhận):
- Ghi đè giá trị feedback lúc chạy có cần gọi lại `Initialization`/`ForceInitialValue` hay không, và có an toàn khi 2 lần Play chồng nhau.
- Việc MMF_FreezeFrame dùng timescale 0 sẽ đóng băng cả enemy lẫn player (xem câu hỏi mở 2).
- Tên chính xác API của MMF_TimescaleModifier (có file) cho slow-mo parry sau này.
- MMF_Sound / MMF_Flicker field cần ghi đè (chưa đọc).

Nếu việc ghi đè không ổn định, phương án dự phòng: một MMF_Player theo tier (light/heavy/kill) và SO chỉ chọn tier + scale `FeedbacksIntensity`.

### 2.2 Các ScriptableObject

Tất cả đặt dưới `Assets/SO/Combat/`, script ở `Assets/Scripts/CoreGame/Combat/Feel/`.

**WeaponFeelProfile** (gắn vào WeaponItem, một field mới `feelProfile`; AttackData có thể override bằng `AttackFeelOverride` về sau)

| Field | Mặc định | Khoảng an toàn | Ghi chú |
|---|---|---|---|
| hitStopLight | 0.05s | 0 - 0.15 | Ghi vào FreezeFrameDuration |
| hitStopHeavy | 0.09s | 0 - 0.2 | Đòn heavy/running |
| hitStopKill | 0.12s | 0 - 0.25 | Giết enemy |
| shakeLight | 0.35 | 0 - 2 | Nhân lên Impulse Velocity |
| shakeHeavy | 0.7 | 0 - 3 | |
| shakeKill | 0.9 | 0 - 3 | |
| shakeDirectional | true | - | Impulse theo hướng đòn thay vì ngẫu nhiên |
| hitFeedbackIntensityScale | 1 | 0.5 - 2 | Nhân với AttackData.impact |
| sfxHitLight / sfxHitHeavy / sfxKill / sfxSwing | null | - | AudioClip hoặc AudioSfxID, tùy quyết định câu hỏi 1 |
| inputBufferDuration | 0.4s | 0.1 - 0.5 | GDD: vũ khí chậm không vượt kiếm |
| enemyKnockbackDistance | 1.5m | 0 - 4 | Dùng ở slice 4 |
| enemyKnockbackDuration | 0.25s | 0.1 - 0.5 | |
| enemyStunDuration | 0.5s | 0 - 1.5 | |
| enemyHitReactionStrength | 1 | 0 - 3 | Hệ số cho poise ở story 005 |

Giá trị mặc định của asset sword phải bằng đúng hành vi hiện tại, để slice 1 không đổi cảm giác (kiểm chứng bằng playtest).
Chưa kiểm: số hiện tại trong prefab; slice 1 bước đầu tiên là đọc chúng và điền vào SO.

**CameraFeelProfile** (thay các field serialized trong CameraOffsetController)

| Field | Mặc định | Khoảng an toàn |
|---|---|---|
| freeLookOffset | 0.15 | 0 - 0.4 |
| lockOnOffset | 0.3 | 0 - 0.6 |
| lockOnZoomMin / lockOnZoomMax (ortho) | 4 / 8 | 3 - 12 |
| edgeThreshold (viewport) | 0.6 | 0.4 - 0.9 |
| offsetSmoothSpeed / zoomSmoothSpeed | giữ theo giá trị hiện tại | 1 - 20 |
| hitKickDistance | 0.0 (tắt) | 0 - 0.3 |
| hitKickDuration | 0.08s | 0.03 - 0.2 |

Camera không xoay (luật thiết kế); hitKick là dịch camera nhẹ theo hướng đòn bằng DOTween, không phải rotate.

**EnemyCombatFeelProfile** (gắn vào SO_EnemyStats hoặc tham chiếu từ đó)

| Field | Mặc định | Khoảng an toàn |
|---|---|---|
| telegraphDuration | 0.35s | 0.15 - 1.0 |
| telegraphFlashColor | vàng/cam | - |
| telegraphSfx | null | - |
| attackSpeedDuringWindup | 1 | 0.3 - 1 |
| recoveryDuration | 0.5s | 0.2 - 1.5 |
| deathHitStop / deathShake | 0.12s / 0.5 | theo player profile |

Enemy damage hiện 25 (1 tim), cooldown 1.5s: telegraph phải nằm TRONG cooldown, không kéo dài chu kỳ tấn công trừ khi user muốn.

---

## 3. Các lát cắt (slice)

Quy ước: mỗi slice để game vẫn chạy được, giá trị mặc định = hành vi cũ trừ khi slice nói khác. Mỗi slice dưới ~6 file. Mọi slice cần: compile, kiểm console, chạy play mode, chụp screenshot vào `production/qa/evidence/` (luật dự án).

### Slice 1 - WeaponFeelProfile: dời hit stop và shake vào SO (rủi ro thấp)

- Thay đổi: tạo `WeaponFeelProfile` SO với hit stop/shake/intensity scale; `CombatFeelPlayer` ghi số vào MMF_Player hit/kill trước khi Play. PlayerCombat gọi `CombatFeelPlayer` thay vì gọi trực tiếp MMF_Player.
- File: `WeaponFeelProfile.cs` (mới), `CombatFeelPlayer.cs` (mới), `PlayerCombat.cs` (sửa nhỏ), `WeaponItem.cs` (thêm field), `SO_WeaponItem.asset` (gán), `Player.prefab` (gán tham chiếu).
- Field SO thêm: hitStopLight/Heavy/Kill, shakeLight/Heavy/Kill, hitFeedbackIntensityScale.
- Phụ thuộc: story 001 phải đóng (dùng `OnHitLanded`/DamageInfo nếu có, để PlayerCombat không bị sửa chồng chéo). Không phụ thuộc 002/005.
- Cảm giác khi test: GIỐNG HỆT hiện tại với mặc định; sau đó tăng hit stop lên 0.1 thấy đòn "nặng" hơn ngay, không cần mở prefab.
- Playtest checklist:
  1. Chơi với giá trị mặc định, cảm giác không đổi so với trước.
  2. Đổi hitStopLight 0 -> 0.1 ngay trong Inspector khi đang chạy, đòn thường đổi theo.
  3. Đòn giết enemy có hitStopKill/shake riêng.
  4. Không lỗi console khi đánh liên tiếp nhanh (Play chồng nhau).

### Slice 2 - Feel profile theo loại vũ khí + SFX placeholder

- Thay đổi: tạo thêm vài asset profile (Sword mặc định, Greatsword nặng hơn) cho WeaponClass có sẵn; thêm MMF_Sound vào MMF_Player hit/kill; phát swing SFX khi mở hit window.
- File: asset profile x2, `Player.prefab` (thêm MMF_Sound), `CombatFeelPlayer.cs`, `PlayerCombat.cs` (gọi swing SFX), `AudioSfxID`/AudioManager nếu chọn dùng ID.
- Field SO thêm: sfxHitLight/Heavy/Kill, sfxSwing, inputBufferDuration (đọc từ profile).
- Phụ thuộc: slice 1; quyết định câu hỏi mở 1 (nguồn SFX).
- Cảm giác: có tiếng va chạm, đổi vũ khí đổi cảm giác (Greatsword hit stop dài, buffer ngắn hơn).
- Checklist:
  1. Nghe được swing, hit, kill khác nhau.
  2. Greatsword hit stop/shake rõ hơn Sword.
  3. Buffer Greatsword không dài hơn Sword.
  4. Tắt SFX (để null) không lỗi.

### Slice 3 - Enemy telegraph (khả năng đọc đòn)

- Thay đổi: trước khi vào animation tấn công, enemy chạy windup `telegraphDuration`: flash bằng MMF_Flicker (hoặc glow, tùy câu hỏi 3) + SFX cảnh báo. Tạo `EnemyCombatFeelProfile`.
- File: `EnemyCombatFeelProfile.cs` (mới), `SO_EnemyStats.cs` (thêm tham chiếu), `EnemyCombat.cs` (sửa), 3 enemy prefab (thêm MMF_Player "Telegraph") - gộp thành 1-2 lần làm tùy số prefab, hoặc chỉ Seed trước.
- Field SO thêm: telegraphDuration, telegraphFlashColor, telegraphSfx.
- Phụ thuộc: story 005 sửa EnemyState/EnemyCombat -> làm sau 005 hoặc phối hợp cùng lúc để không xung đột; dùng UniTask cho windup (không coroutine).
- Cảm giác: biết trước đòn sắp tới, có thể né (roll) hợp lý; đòn không còn "bất ngờ".
- Checklist:
  1. Seed: thấy dấu hiệu rõ trước khi đánh.
  2. Roll theo dấu hiệu thì tránh được; đứng yên thì bị trúng.
  3. telegraphDuration = 0 trả về hành vi cũ.
  4. Spider kite rồi đánh vẫn mượt, không khựng lạ.

### Slice 4 - Enemy hit reaction theo vũ khí (chỉ dữ liệu, gộp story 005)

- Thay đổi: đọc enemyKnockbackDistance/Duration/StunDuration/ReactionStrength từ WeaponFeelProfile (kèm AttackData.impact) thay vì SO_EnemyStats. Triển khai cơ học knockback (DOTween/UniTask thay coroutine) KHÔNG làm ở đây: thuộc story 005.
- File: `WeaponFeelProfile.cs` (thêm field), `EnemyController`/`EnemyCombat` (đọc từ DamageInfo - phần này phải do 005 làm), asset profile.
- Phạm vi nếu 005 chưa xong: chỉ thêm field vào SO và truyền qua DamageInfo, chưa tiêu thụ -> không có thay đổi cảm giác.
- Phụ thuộc: story 001 (DamageInfo) và story 005 bắt buộc.
- Cảm giác: Greatsword đẩy enemy xa hơn và stun lâu hơn Sword.
- Checklist:
  1. Đòn Sword và Greatsword đẩy enemy khoảng cách khác nhau.
  2. Giá trị mặc định = hành vi cũ khi dùng Sword.
  3. Không còn coroutine trong knockback (kiểm khi 005 đóng).

### Slice 5 - CameraFeelProfile + hit kick

- Thay đổi: chuyển các field của CameraOffsetController sang SO; thêm hit kick DOTween nhẹ theo hướng đòn (mặc định 0 = tắt).
- File: `CameraFeelProfile.cs` (mới), `CameraOffsetController.cs` (sửa), asset camera profile, scene (gán), 1 file khác nếu cần event hit.
- Field SO thêm: tất cả field ở mục 2.2 CameraFeelProfile.
- Phụ thuộc: slice 1 (để lấy hướng đòn); không phụ thuộc 002/005.
- Cảm giác: camera "đón" cú đánh; mặc định giống hiện tại.
- Checklist:
  1. Lock-on/free-look offset và zoom giống trước khi dùng giá trị mặc định.
  2. Tăng hitKickDistance lên 0.1 thấy camera dịch theo hướng đòn rồi trở lại.
  3. Hit kick không làm camera trôi khi đánh liên tục.

### Slice 6 - Cancel window, roll-cancel, i-frame trong SO

- Thay đổi: đưa thời điểm combo window, i-frame, cancel-roll vào SO thay vì animation event trong FBX; cho phép roll-cancel recovery.
- File: SO của player/attack (field mới), PlayerCombat/State (do story 002 sở hữu), có thể bỏ events trong .fbx.meta.
- Phụ thuộc: BẮT BUỘC sau story 002 (action state machine); không làm trước. Vì vậy slice này chỉ là đề xuất, chưa lên lịch cụ thể.
- Cảm giác: có thể hủy recovery bằng roll; thời gian i-frame chỉnh được trong Inspector.
- Checklist:
  1. Roll trong recovery đòn đánh được (nếu user muốn).
  2. Đổi i-frame và thấy khác biệt khi né đòn enemy.
  3. Combo vẫn nối bình thường.

### Ngoài phạm vi các slice trên (chỉ ghi nhận)

Parry/slow-mo (MMF_TimescaleModifier có sẵn), hold/release heavy attack, block - đều dựa trên input hold của story 002; sẽ thêm slice riêng sau khi 002 xong.

---

## 4. Câu hỏi mở (cần người dùng quyết)

1. Nguồn SFX: dùng placeholder (âm tự tạo/free) tạm thời, hay có asset pack cụ thể? Ảnh hưởng slice 2.
2. Hit stop: chỉ đóng băng enemy + player (qua animator speed) hay dừng toàn bộ timescale (MMF_FreezeFrame hiện tại)? Cái sau đơn giản, nhưng đóng băng cả enemy khác ngoài mục tiêu.
3. Telegraph enemy: chỉ animation windup (chi phí thấp, không cần VFX), hay thêm glow/flash VFX (dễ đọc hơn, tốn thêm asset)? Ảnh hưởng slice 3.
4. Thứ tự: làm slice 1 và 2 trước rồi mới tới telegraph, hay ưu tiên telegraph (vấn đề đọc đòn) sớm hơn?
5. Stamina: giữ trừ giữa animation (hiện tại) hay chuyển sang trừ lúc bắt đầu đòn? Ảnh hưởng cảm giác commitment, và chạm vào animation event nên chỉ làm cùng story 002.
