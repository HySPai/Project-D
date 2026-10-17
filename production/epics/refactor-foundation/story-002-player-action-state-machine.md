# Story 002: Máy trạng thái hành động Player

> **Epic**: refactor-foundation
> **Status**: Ready
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: —

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 1 (combat Souls), điểm gắn cho `weapons.md` states (Chặn, Parry, Nhắm, Đòn nặng, Vỡ thế) và `elemental-magic.md` states (Cast, Đang chọn)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.4.1f1 | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ code review R2.*

- [ ] Player có máy trạng thái hành động thay cho các cờ bool `isAttacking`/`isRolling`: tối thiểu Idle, Attack, Roll, Hit, Dead. Có chỗ khai báo trạng thái mới (Block, Parry, Aim, Cast, Stagger) mà chưa có hành vi.
- [ ] Mỗi trạng thái khai báo rõ: có cho di chuyển, xoay, root motion hay không, và chuyển được sang trạng thái nào. Chuyển trạng thái không hợp lệ bị từ chối.
- [ ] `ResetActionFlag` không còn reset hàng loạt cờ; hết clip thì báo máy trạng thái về Idle.
- [ ] Enemy không bị ảnh hưởng: `IsAttacking`/`IsRolling` vẫn dùng được cho enemy và các FSM Seed/Spider.
- [ ] `PlayerInputHandler` nhận `started`/`performed`/`canceled`, đo được thời gian giữ nút đánh và phát sự kiện khi thả nút.
- [ ] `PlayerController.inputactions` có thêm action Block, Aim, Cast, SwitchElement. Chưa gắn hành vi; nhấn không làm gì.
- [ ] Giữ nguyên luật hiện tại: không lăn khi đang đánh, combo buffer 0.4s, đòn chạy, đòn lăn, aim assist, lunge.
- [ ] Không đổi gameplay: vòng lặp Kiếm/lăn/lock-on/hit reaction/chết như cũ, ảnh chụp lưu ở `production/qa/evidence/`.

---

## Implementation Notes

- Máy trạng thái là class thuần, do `PlayerController` giữ và truyền cho các component qua `Initialize`, theo kiểu Controller trung tâm.
- Không dùng GetComponent/Find; Animator callback đi qua `StateMachineBehaviour` như `ResetActionFlag` hiện tại (ngoại lệ đã ghi nhận trong review).
- Sửa `.inputactions` và prefab Player qua MCP, trình kế hoạch trước.

---

## Out of Scope

- Hành vi thật của Chặn, Parry, Nhắm, Cast, Đòn nặng (thuộc epic vũ khí và nguyên tố).
- Story 004: tách lớp.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

*Governed by `qa.level: minimal`: test tự động được miễn, ảnh chụp thì không.*

**Story Type**: Integration
**Required evidence**: playtest ngắn + ảnh chụp trong `production/qa/evidence/` (đánh combo, lăn, đánh khi chạy, bị đánh giữa chừng).

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: 001
- Unlocks: 004
