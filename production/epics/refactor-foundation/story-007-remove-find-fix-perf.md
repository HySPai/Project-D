# Story 007: Dọn GetComponent/Find và hiệu năng

> **Epic**: refactor-foundation
> **Status**: Ready
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: M
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: —

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Quy ước CLAUDE.md "Không dùng GetComponent / Find*; gán tham chiếu qua Inspector"; code review R6b (hiệu năng Turret, Grass)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.4.1f1 | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

- [ ] Code gameplay không còn `GetComponent*`, `Find*`, `Camera.main`: `Turret`, `Bullet`, `Grass`/`GrassTrigger`, `EnemyController`, `CharacterAnimationBase`, `CharacterControllerBase.Reset`, `PlayerCamera`, `CharacterCombatBase`. Tham chiếu gán qua `[SerializeField]`. `DamageCollider` đã xử lý ở story 001.
- [ ] Ngoại lệ được giữ và ghi rõ: `ResetActionFlag` (StateMachineBehaviour không serialize được tham chiếu tới scene).
- [ ] `Turret` không còn `Physics.OverlapSphere` cấp phát mảng mỗi frame: dùng `OverlapSphereNonAlloc` với buffer cố định, quét theo chu kỳ.
- [ ] `Turret` chọn mục tiêu gần nhất trong tầm thay vì `hits[0]`, và kiểm tra tầm nhìn bằng raycast tới thân mục tiêu.
- [ ] `GrassTrigger` dùng `OnTriggerEnter`/`Exit` hoặc giới hạn tần suất, không gọi `Bend` mỗi bước physics; tween của `Grass` có `SetLink`.
- [ ] `hitLayer` của `PlayerCombat` không còn viết cứng `1 << 8`, gán qua Inspector.
- [ ] `SCR_Pool`/`Bullet`: pool không tìm kiếm tuyến tính khi có nhiều loại prefab (hoặc ghi lại là chấp nhận được vì số loại ít).
- [ ] Không đổi gameplay: turret vẫn thức, ngắm, bắn loạt 3 viên và chỉ bắn khi có người trong tầm; cỏ vẫn nghiêng khi đi qua. Ảnh chụp lưu ở `production/qa/evidence/`.

---

## Implementation Notes

- Gán tham chiếu mới trong prefab và scene qua MCP, trình kế hoạch trước.
- Phần View/Ulti (CanvasScaler, SafeArea, Button...) làm ở story này nếu thuộc nhóm gameplay; phần UI thuần xin hỏi trước khi đụng.
- Đo lại bằng profiler nếu có nhiều turret trong scene.

---

## Out of Scope

- Khoá chọn mục tiêu bằng lock-on và mọi thứ của story 004.
- Thư viện bên thứ ba (PlayerPrefsEditor).

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

*Governed by `qa.level: minimal`: test tự động được miễn, ảnh chụp thì không.*

**Story Type**: Integration
**Required evidence**: playtest + ảnh chụp trong `production/qa/evidence/` (turret bắn khi vào tầm và dừng khi có tường chắn, cỏ nghiêng khi đi qua).

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: 008
