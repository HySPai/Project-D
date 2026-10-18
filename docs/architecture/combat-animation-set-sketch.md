# Combat Animation Set — Architecture Sketch

> Đây là bản ghi kiến trúc, chưa phải ADR. Không có gì trong repo đọc file này.
> Ngày: 2026-10-19. Phạm vi: đòn nặng giữ nút, chặn/vỡ thế, parry/phản kích, lùi bước và đòn lùi bước, cho bộ Straight Sword (main, off, th, dw).
> Engine validation: NOT ASSESSED — không spawn `unity-specialist` (team.size = individual, và subagent không có công cụ Unity MCP). Kiểm tra bằng biên dịch, 14 test EditMode và chạy play mode.

## Luồng dữ liệu

```
PlayerInputHandler (Fire pressed/released, Guard held/pressed)
        │
PlayerController.Update ──► PlayerGuard.Tick(held, pressed)
        │                        │  CrossFadeGuard() ──► PlayerAnimation ──► layer "Guard"
        ├─► PlayerCombat.AttackPressed / AttackReleased / TickHeavyHold
        │        ├─ light  : TryStartAttack (cũ)
        │        ├─ heavy  : TryBeginHeavy → ReleaseHeavy (HeavyAttackData)
        │        └─ backstep attack : cờ canPerformBackstepAttack (anim event)
        └─► PlayerMovement.Roll ──► Backstep() khi không có hướng đi

Đòn tới ──► CharacterStateBase.ReceiveDamage
              └─► AbsorbIncoming (hook) ──► PlayerState ──► PlayerGuard.TryAbsorb
                                                     └─► GuardResolver.Resolve (hàm thuần)
```

## File

| File | Vai trò |
|---|---|
| `Combat/GuardResolver.cs` | Hàm thuần: chặn / parry / vỡ thế, cung chặn. Có test. |
| `Player/PlayerGuard.cs` | Trạng thái chặn, cửa sổ parry, vỡ thế, slow-mo bằng DOTween, bất tử ngắn bằng UniTask |
| `Item/Weapon/WeaponItem.cs` | Thêm `HeavyAttackData`, `GuardData`, `backstepAttack`, `counterMultiplier` |
| `CharacterStateBase.cs` | Hook `AbsorbIncoming`, `IsOffBalance`, `OnParried` |
| `EnemyState/EnemyController` | `OnParried`: dừng đòn, choáng, đẩy lùi nhẹ |
| `Editor/CombatClipEventBuilder.cs` | Đo tốc độ bàn tay trong clip để đặt anim event (mở/đóng vùng đánh, combo, i-frame) |
| `Editor/CombatAnimatorBuilder.cs` | Dựng state trong `Humanoid.controller`, thêm layer `Guard` |
| `Editor/CombatWeaponBuilder.cs` | Ghi số liệu 3 `WeaponItem` |

## Quyết định

- Layer `Guard` nằm **dưới** `ActionOverride`, nên anim hành động (đánh, lăn, bị đánh) tự đè lên tư thế chặn mà không cần tắt layer.
- Anim chặn nhẹ/vừa/nặng và parry chạy trong layer `Guard`; vỡ thế và choáng khiên chạy trong `ActionOverride` vì cần khóa hành động.
- Mọi số liệu nằm trong `WeaponItem` (ScriptableObject). Tên state dựng theo `Guard_{animationSet}_{Up|Down|Loco|Parry|BlockLight|BlockMedium|BlockHeavy|Break|Stun}`.
- `AbsorbIncoming` là hook ở lớp gốc nên quái sau này có thể chặn mà không sửa luồng `ReceiveDamage`.
- Phần chặn thành công mà còn sát thương dư (Búa, Song kiếm) đi qua `ApplyChipDamage`, không phát anim bị đánh và không gây bất tử 1s.

## Rủi ro đã biết

- Đòn nhẹ ra khi thả nút nên trễ bằng thời gian bấm (GDD yêu cầu ≤ 100ms, thường đạt nhưng không đảm bảo).
- Locomotion nền vẫn là bộ 1 tay; Đại kiếm và Song kiếm chưa có locomotion riêng.
- Đổi vũ khí khi đang giữ chặn: layer `Guard` giữ tư thế của vũ khí cũ cho đến khi thả nút. Chỉ xảy ra với nút debug, vì đổi vũ khí thật chỉ ở Ngưỡng.
- Cửa sổ đánh trúng do đo tự động; chưa được tinh chỉnh bằng mắt.
- Chưa có VFX tia lửa chặn/parry và chưa có âm riêng; dùng lại feedback đánh trúng của vũ khí.
