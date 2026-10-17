# Story 004: Tách class lớn

> **Epic**: refactor-foundation
> **Status**: Ready
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: —

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 1 (combat Souls); chuẩn bị chỗ gắn cho Khiên, Cung, nguyên tố (`weapons.md` luật 21, `elemental-magic.md` mục Interactions)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.4.1f1 | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ code review R4. Refactor thuần, không đổi hành vi.*

- [ ] Ground check, gravity và edge guard tách khỏi `CharacterStateBase` thành component riêng, do controller giữ qua `[SerializeField]`.
- [ ] Máu, hồi phục và hit reaction tách thành phần riêng; `CharacterStateBase` chỉ còn cờ trạng thái, quyền (CanMove/CanRotate/root motion) và stats.
- [ ] Lock-on (`LockTarget`, `SwitchTarget`, auto unlock, `IsInView`) chuyển ra component riêng của Player; `CharacterCombatBase` và `EnemyCombat` không còn field lock-on.
- [ ] `CharacterCombatBase` không còn vũ khí/combo cho enemy; combo và `currentWeapon` thuộc phần của Player.
- [ ] Xóa field công khai không ai đọc: `canBlock`, `canPerformBackstepAttack`, `lastAttackAnimationPerformed` nếu không còn dùng.
- [ ] Không có `GetComponent`/`Find` mới sinh ra khi tách; mọi tham chiếu gán qua Inspector và truyền qua `Initialize` của controller.
- [ ] Prefab Player, Seed, Spider, SpiderKing được nối lại qua MCP, không mất tham chiếu nào (console không có missing reference).
- [ ] Không đổi gameplay: số liệu đi, chạy, lăn, mép vực, lock-on, aim assist giống cũ. Ảnh chụp lưu ở `production/qa/evidence/`.

---

## Implementation Notes

- Làm từng cụm một (ground → máu → lock-on → combat), compile và chạy play mode giữa các cụm.
- Giữ tên property công khai mà code khác đang dùng (`IsGrounded`, `YVelocity`, `LockOnTransform`...), hoặc cập nhật mọi chỗ gọi trong cùng story.
- Nếu một bước làm vỡ prefab, dừng và hỏi trước khi sửa tiếp.

---

## Out of Scope

- Hệ vũ khí/Khiên/Cung mới.
- Story 005: poise và hiệu ứng trạng thái cho enemy.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

*Governed by `qa.level: minimal`: test tự động được miễn, ảnh chụp thì không.*

**Story Type**: Integration
**Required evidence**: playtest đầy đủ vòng lặp hiện có + ảnh chụp trong `production/qa/evidence/` (đi gần mép vực, lock-on và đổi mục tiêu, chém, lăn, chết).

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: 001, 002
- Unlocks: 005
