# Combat Feel - Slice 1 (WeaponFeelProfile) - Phác thảo kiến trúc

Trạng thái: DRAFT cho Phase 2 /team-combat. Chưa sửa code/prefab/asset nào.
Phạm vi: chỉ Slice 1. Thứ tự đã chốt: 1 -> 3 -> 2 -> 5. SFX để sau. Hit stop giữ MMF_FreezeFrame toàn timescale. Telegraph enemy làm sau (MMF_Flicker + windup chậm).
Engine: Unity 6000.4.1f1 (Feel + Cinemachine có sẵn trong project, không dùng API Unity mới nào trong slice này).

---

## 1. Số hiện tại trong Player.prefab (đã đọc)

Ánh xạ MMF_Player (GameObject con của "Feedbacks"):

| MMF_Player | Ai dùng | Hit Stop (FreezeFrameDuration) | Camera Shake Velocity | ImpulseDuration | FeedbacksIntensity | CanPlayWhileAlreadyPlaying |
|---|---|---|---|---|---|---|
| AttackHitFeedback (fileID 1987881308303024513) | `PlayerCombat.hitFeedback` | 0.05 | (0.08, -0.1, 0) | 0.18 | 1 | 1 |
| AttackKillFeedback (fileID 4605510643443736066) | `PlayerCombat.killFeedback` | 0.10 | (0.16, -0.2, 0) | (cùng cấu trúc) | 1 | 1 |
| DamageFeedback (fileID 8912662728325990622) | `CharacterControllerBase.damageFeedback` (bị đánh) | 0.08 | (-0.2, 0.25, 0) | (cùng cấu trúc) | 1 | - |

- `deathFeedback` của Player = null (fileID 0).
- Cả 3 MMF_Player: ForceTimescaleMode = 1 (unscaled), Chance 100, Cooldown 0, InitialDelay 0, không có Pause.
- Camera Shake: AmplitudeGain 1, FrequencyGain 1, DissipationDistance 100; Timing.ConstantIntensity = 0 -> Velocity bị nhân với intensity.
- Intensity hiện tại = `AttackData.impact` truyền vào `PlayFeedbacks(position, impact)`. Giá trị trong SO_WeaponItem: light1 = 1, light2 = 1.35, running = 1.4, rolling = 1.3.
- DamageFeedback KHÔNG thuộc slice 1 (giữ nguyên trong prefab).

## 2. Cách Feel scale theo intensity (đã đọc code Assets/Feel)

- `MMF_FreezeFrame.CustomPlayFeedback` gọi `MMFreezeFrameEvent.Trigger(FeedbackDuration)`, với `FeedbackDuration = ApplyTimeMultiplier(FreezeFrameDuration)`. Tham số `feedbacksIntensity` BỊ BỎ QUA. Player có `DurationMultiplier = 1` và `RandomizeDuration = 0` nên thời lượng = FreezeFrameDuration nguyên. Hệ quả: `impact` hiện KHÔNG ảnh hưởng hit stop. SO phải ghi thẳng giây vào `FreezeFrameDuration`, không nhân impact.
- `MMF_CinemachineImpulse.CustomPlayFeedback`: `CreateEvent(position, Velocity * ComputeIntensity(feedbacksIntensity, position))`. `ComputeIntensity = intensity * ComputedRandomMultiplier * Owner.ComputeRangeIntensityMultiplier`. RandomizeOutput = 0 và OnlyPlayIfWithinRange = 0 nên hệ số = intensity thuần. Vậy shake thực = `Velocity * impact`.
- Công thức SO phải tái tạo (mặc định): `shake_thực = baseVelocity_prefab * shakeTier * (impact * hitFeedbackIntensityScale)`. Với shakeTier = 1 và scale = 1 thì đúng bằng hiện tại.

## 3. Kiểm chứng API (Task 2)

- Ghi `FreezeFrameDuration` / `Velocity` lúc chạy có hiệu lực ở lần Play kế tiếp, KHÔNG cần Initialization lại: VERIFIED bằng đọc code. Cả hai field được đọc trực tiếp bên trong `CustomPlayFeedback` tại thời điểm Play. `Initialization()` của MMF_Impulse không cache Velocity (chỉ `Reset` mới gán 5,5,5).
- Play đồng bộ: với `Time.frameCount >= 2`, InitialDelay = 0, không có Pause thì `PlayFeedbacksInternal -> PreparePlay -> PlayAllFeedbacks` gọi `Feedback.Play` ngay trong cùng call. Ghi field rồi Play liền nhau không bị đua với Play khác. VERIFIED.
- Play chồng nhau: cả hit và kill player có `CanPlayWhileAlreadyPlaying = 1` nên không bị chặn. `CreateEvent` của Cinemachine tiêu thụ Velocity ngay nên ghi đè lần sau không làm hỏng event cũ. Freeze chồng: `MMF_FreezeFrame` bỏ qua nếu `Time.timeScale < MinimumTimescaleThreshold (0.1)` nên đòn trúng trong lúc đang đóng băng không cộng dồn (hành vi y như hiện tại). VERIFIED bằng đọc code, CHƯA verified bằng chạy thực (xem checklist).
- `GetFeedbackOfType<T>(string label)` có sẵn (MMF_Player.cs dòng 1444): VERIFIED. Cache vào field lúc Initialize, không tra mỗi lần đánh.
- `MMF_CinemachineImpulse` nằm trong `#if MM_CINEMACHINE || MM_CINEMACHINE3`; prefab đang serialize nó nên define chắc đang bật, nhưng CHƯA kiểm define trong Player Settings. Compile sẽ báo ngay nếu thiếu.

## 4. Thiết kế

### 4.1 Lớp và chữ ký

Thư mục script: `Assets/Scripts/CoreGame/Combat/Feel/` (nằm cạnh DamageInfo/DamageResult của story 001). Không có asmdef trong Assets/Scripts, nên mọi thứ vào Assembly-CSharp.

`WeaponFeelProfile.cs`
- `public enum HitFeelTier { Light, Heavy, Kill }`
- `[CreateAssetMenu(menuName = "Game/Combat/Weapon Feel Profile")] public class WeaponFeelProfile : ScriptableObject`
- Field `[SerializeField] private`, có `[Range]`/`[Min]` và `[Header]`, không `[Tooltip]`, không `//`:

| Field | Mặc định (tái tạo hiện tại) | Khoảng |
|---|---|---|
| hitStopLight | 0.05 | 0 - 0.15 |
| hitStopHeavy | 0.05 | 0 - 0.2 |
| hitStopKill | 0.10 | 0 - 0.25 |
| shakeLight | 1 | 0 - 3 |
| shakeHeavy | 1 | 0 - 3 |
| shakeKill | 1 | 0 - 3 |
| hitFeedbackIntensityScale | 1 | 0.5 - 2 |
| shakeDirectional | false | - |

- shakeLight/Heavy/Kill là HỆ SỐ nhân lên Velocity gốc của chính MMF_Player tương ứng (hit player cho Light/Heavy, kill player cho Kill). Do đó Kill = 1 vẫn cho (0.16, -0.2, 0) vì đó là Velocity gốc của kill player.
- API: `public float GetHitStop(HitFeelTier tier)`, `public float GetShake(HitFeelTier tier)`, `public float IntensityScale { get; }`, `public bool ShakeDirectional { get; }`.
- Lệch so với bảng draft mục 2.2 (0.09/0.7/0.9...): draft đưa số "đề xuất" để cảm giác nặng hơn; yêu cầu hiện tại là mặc định phải bằng đúng hôm nay, nên Heavy = Light cho đến khi người dùng tune. Cần người dùng biết điều này.

`CombatFeelPlayer.cs` (MonoBehaviour)
- Field: `[SerializeField] private MMF_Player hitFeedback; [SerializeField] private MMF_Player killFeedback;`
- Hằng private: nhãn "Hit Stop" và "Camera Shake" (khớp prefab hiện tại).
- Cache private: `MMF_FreezeFrame` và `MMF_CinemachineImpulse` của mỗi player, cộng `Vector3 baseVelocity` của mỗi impulse (đọc một lần khi Initialize, trước mọi lần ghi đè).
- `public void Initialize()` : tra feedback theo nhãn, cache base Velocity, log warning một lần nếu thiếu nhãn.
- `public void PlayHit(WeaponFeelProfile profile, HitFeelTier tier, float attackImpact, Vector3 position, Vector3 hitDirection)`:
  - chọn player (Kill -> killFeedback, còn lại -> hitFeedback);
  - nếu `profile != null`: `FreezeFrameDuration = profile.GetHitStop(tier)`; `Velocity = (directional ? hitDirection.normalized * base.magnitude : base) * profile.GetShake(tier)`; intensity = `attackImpact * profile.IntensityScale`;
  - nếu `profile == null`: không ghi đè gì, intensity = `attackImpact` (hành vi cũ, prefab tự chạy);
  - `player.PlayFeedbacks(position, intensity)`. Null-safe cho player/feedback thiếu.
- `public static HitFeelTier ResolveTier(WeaponItem weapon, AttackData attack, bool killed)` : Kill nếu killed; Heavy nếu `attack` là `weapon.runningAttack` hoặc `weapon.rollingAttack` (so sánh tham chiếu); còn lại Light. Static thuần để test được.
- Không update loop, không DOTween/Coroutine cần thiết (Feel tự xử lý thời gian).

Heavy trong slice này = running attack và rolling attack (chưa có heavy attack thật; heavy hold/release thuộc đợt sau story 002). Với mặc định Heavy = Light nên chưa đổi cảm giác.

shakeDirectional: tùy chọn, mặc định false. Khi true, hướng = `GetFlatForward()` của player (hướng đòn), giữ nguyên độ lớn Velocity gốc. Hệ trục Velocity của Cinemachine là world-space; camera isometric nên hướng thấy được trên màn hình sẽ bị xoay theo camera, cần playtest. Nếu chưa cần thì bỏ field này khỏi slice (ít rủi ro nhất).

### 4.2 Vị trí và nối dây

- `CombatFeelPlayer` gắn trên GameObject "Feedbacks" (cha của AttackHitFeedback / AttackKillFeedback / DamageFeedback) trong Player.prefab.
- `PlayerCombat` giữ `[SerializeField] private CombatFeelPlayer combatFeel;` và gọi `combatFeel.Initialize()` trong `PlayerCombat.Initialize(...)` (PlayerController đã gọi hàm này; không thêm GetComponent/Find). PlayerController không cần đổi.
- `PlayerCombat` bỏ hai field `hitFeedback`, `killFeedback` và alias `using MMF_Player`. Hai tham chiếu này chuyển sang `CombatFeelPlayer`.
- `WeaponItem` thêm `[Header("Feel")] public WeaponFeelProfile feelProfile;` (cùng style với `AttackData`/`WeaponItem` hiện tại: public field).

### 4.3 PlayerCombat chọn Light/Heavy/Kill

Chỉ sửa thân `PlayHitFeedback(bool killedAny)` (hiện ở dòng 362 trong file đang có) và khối field "Feedbacks (Feel)". Không đụng `SweepHitWindow`, `DamageInfo`, `ReceiveDamage`, `DamageResult` của story 001 (file hiện tại không dùng `OnHitLanded`).

Ý đồ: `tier = CombatFeelPlayer.ResolveTier(currentWeapon, currentAttack, killedAny)`; `combatFeel.PlayHit(currentWeapon.feelProfile, tier, currentAttack.impact, transform.position, GetFlatForward())`.

### 4.4 Danh sách file

Tạo:
1. `D:\Git\Project-D\Assets\Scripts\CoreGame\Combat\Feel\WeaponFeelProfile.cs`
2. `D:\Git\Project-D\Assets\Scripts\CoreGame\Combat\Feel\CombatFeelPlayer.cs`
3. `D:\Git\Project-D\Assets\SO\Combat\SO_WeaponFeel_Sword.asset` (thư mục `Assets/SO/Combat/` chưa tồn tại; giá trị mặc định ở bảng 4.1)

Sửa:
4. `D:\Git\Project-D\Assets\Scripts\CoreGame\Character\Player\PlayerCombat.cs`
5. `D:\Git\Project-D\Assets\Scripts\CoreGame\Item\Weapon\WeaponItem.cs`
6. `D:\Git\Project-D\Assets\SO\Weapon\SO_WeaponItem.asset` (gán `feelProfile`)
7. `D:\Git\Project-D\Assets\Prefabs\CoreGame\Player.prefab` (thêm CombatFeelPlayer, gán 3 tham chiếu)

### 4.5 Các bước qua Unity MCP (sau khi được duyệt)

1. Nạp skill `unity-mcp-skill`. Tạo 2 script, `read_console` đến khi compile sạch (kiểm cả define MM_CINEMACHINE).
2. Sửa WeaponItem.cs và PlayerCombat.cs, compile lại.
3. Tạo thư mục `Assets/SO/Combat/`, tạo asset `SO_WeaponFeel_Sword` từ WeaponFeelProfile, đặt giá trị mặc định đúng bảng 4.1.
4. Gán `feelProfile` vào `SO_WeaponItem`.
5. Mở Player.prefab: thêm `CombatFeelPlayer` vào GameObject "Feedbacks"; gán `hitFeedback` = AttackHitFeedback, `killFeedback` = AttackKillFeedback; gán `PlayerCombat.combatFeel` = component vừa thêm. Lưu prefab.
6. Chạy play mode. Xác nhận: không lỗi console; đọc lại (qua MCP) `FreezeFrameDuration`/`Velocity` của hit player sau một đòn = 0.05 / (0.08,-0.1,0) * impact-không-đổi. Đổi `hitStopLight` 0 -> 0.1 khi đang chạy, đòn kế tiếp đổi theo. Chụp screenshot vào `production/qa/evidence/` (ví dụ `story-combat-feel-s1/`).
7. Playtest checklist của Slice 1 (draft mục 3): mặc định giống cũ, đổi hitStop khi chạy có hiệu lực, đòn giết dùng thông số Kill, đánh liên tiếp nhanh không lỗi console.

## 5. Rủi ro

- Ghi đè lúc chạy làm số trong prefab chỉ còn là baseline (Velocity gốc vẫn lấy từ prefab; chỉ hit stop là tuyệt đối trong SO). Nếu muốn SO là nguồn duy nhất cho shake, đổi sang lưu biên độ tuyệt đối + hướng trong SO (vẫn đúng với 0.1281 hit / 0.2561 kill theo độ lớn).
- Chạy trong Editor khi tune: ghi đè xảy ra mỗi lần đánh nên chỉnh SO lúc play mode có hiệu lực ngay, nhưng NẾU Play Mode Changes ghi vào SO thì asset bị đổi thật (hành vi SO bình thường).
- PlayerCombat.cs đang có thay đổi chưa commit của story 001 (git status "M"); sửa chồng lên working copy sẽ lẫn diff của hai việc khi commit. Nên commit/đóng 001 trước, hoặc tách commit theo hunk.
- Không có thư mục Tests / asmdef nào trong Assets/Scripts, nên unit test EditMode cho `ResolveTier`/`GetHitStop` cần tạo asmdef cho Scripts (thay đổi rộng, ngoài phạm vi slice). Đề xuất bằng chứng thay thế: đọc lại giá trị bằng MCP khi play mode + screenshot, và hỏi người dùng có muốn tạo asmdef không.
