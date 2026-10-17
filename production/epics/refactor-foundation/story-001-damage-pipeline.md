# Story 001: Kênh sát thương chung

> **Epic**: refactor-foundation
> **Status**: In Progress
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-10-17

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 1 (combat Souls), điểm gắn cho `weapons.md` luật 7-11, 17 và `elemental-magic.md` luật 7, 17

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.4.1f1 | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ code review R1, đã chốt đơn vị: điểm, quy ra tim.*

- [ ] Có `DamageInfo` (điểm, người đánh, vị trí nguồn, nguyên tố, loại nguồn: vũ khí/tên/phép/máy móc, cờ không chặn được, lượng làm mất thế), `DamageResult` (điểm thực trúng, bị chặn, bị parry, mục tiêu chết) và enum `ElementType`.
- [ ] Mọi sát thương đi qua một hàm `ReceiveDamage(in DamageInfo)` trên `CharacterStateBase`; không còn hàm `TakeDamage(float…)`/`TakeDamage(int…)` nào được gọi từ code gameplay. `IDamageable` dùng `DamageInfo`, `IAttacker` bị xóa.
- [ ] Có hàm ảo `ResolveIncoming` chạy trước khi trừ máu, mặc định trả nguyên. Đây là chỗ gắn chặn/parry/yếu-kháng ở các story sau.
- [ ] Người chơi: `SO_PlayerStats` có trường số điểm cho 1 tim (mặc định 25). Bộ đếm cộng dồn phần lẻ: hai đòn 12.5 điểm mất 1 tim; đòn dưới 25 điểm vẫn có phản ứng trúng đòn và i-frame nhưng chưa mất tim. Bộ đếm về 0 khi hồi máu.
- [ ] `CharacterCombatBase` phát sự kiện "đòn trúng" kèm `DamageResult` cho cả đòn vũ khí của Player lẫn đòn của enemy.
- [ ] `DamageCollider` không còn `GetComponentInParent`; tra mục tiêu qua `CombatTargetRegistry`. `CharacterControllerBase` có mảng hurtbox gán qua Inspector và đăng ký vào registry.
- [ ] `Bullet`/`Turret` gây sát thương loại máy móc, không người đánh; turret vẫn bắn cả động vật lẫn côn trùng.
- [ ] Dữ liệu: `attackDamage` của Seed/Spider/Enemy trong 4 asset `SO_*Stats` đổi từ 1 lên 25; `_damage` của Turret trong scene `URP scene complete` đổi từ 1 lên 25.
- [ ] Không đổi gameplay: Kiếm chém quái mất 25 máu; đòn quái mất đúng 1 tim; đạn turret mất đúng 1 tim; chết, i-frame và HUD tim như cũ. Ảnh chụp lưu ở `production/qa/evidence/`.

---

## Implementation Notes

- File mới trong `Assets/Scripts/CoreGame/Combat/`: `DamageInfo.cs`, `DamageResult.cs`, `ElementType.cs`.
- File sửa: `ICharacter.cs`, `CharacterStateBase.cs`, `PlayerState.cs`, `SO_PlayerStats.cs`, `CharacterCombatBase.cs`, `PlayerCombat.cs`, `EnemyCombat.cs`, `SO_EnemyStats.cs` (cờ không chặn được, mặc định tắt), `DamageCollider.cs`, `CharacterControllerBase.cs`, `Bullet.cs`, `Turret.cs`.
- `PlayerState` giữ một nút `[Button]` để debug sát thương.
- Nếu hitbox Seed/Spider nằm ở object con thì gán vào mảng hurtbox qua MCP.
- Code mới không có comment.

---

## Out of Scope

- Story 002: chặn/parry thật dùng `ResolveIncoming`.
- Story 005: yếu/kháng và hiệu ứng trạng thái.
- Thanh nguyên tố nạp theo sát thương (hệ nguyên tố, ngoài epic này).

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

*Governed by `qa.level: minimal`: test tự động được miễn, ảnh chụp thì không.*

**Story Type**: Integration
**Required evidence**: playtest ngắn + ảnh chụp trong `production/qa/evidence/` (chém quái, bị quái đánh, bị turret bắn, HUD tim).

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: 002, 004, 005
