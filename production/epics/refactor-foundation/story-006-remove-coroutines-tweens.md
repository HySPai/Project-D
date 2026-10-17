# Story 006: Dọn Coroutine và tween

> **Epic**: refactor-foundation
> **Status**: Ready
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: S
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: —

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Quy ước CLAUDE.md "Không dùng Coroutine — dùng UniTask" và "Mọi animation/tween dùng DOTween"; code review R6a

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.4.1f1 | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Knockback của enemy làm ở story 005, không lặp lại ở đây.*

- [ ] `EffectController`: bỏ `TurnOffFx` coroutine, tắt FX bằng UniTask có `CancellationToken`; không còn `new WaitForSeconds` mỗi lần spawn.
- [ ] `PressAndHoldButton`: bỏ `HoldCoroutine`/`WaitForSeconds`, dùng UniTask.
- [ ] `ParabolicMovement`: bỏ coroutine và Lerp thủ công, dùng DOTween.
- [ ] `Helper.AnimText` và `Helper.GetWait`/`waitDictionary`: chuyển sang UniTask/DOTween hoặc xóa nếu không còn nơi gọi.
- [ ] Mọi tween mới có `SetLink` hoặc bị kill khi object bị hủy; UniTask gắn token hủy theo object.
- [ ] Không còn `IEnumerator`, `StartCoroutine`, `WaitForSeconds` trong `Assets/Scripts` (trừ thư mục PlayerPrefsEditor của bên thứ ba).
- [ ] Không đổi gameplay: FX vẫn bật rồi tắt đúng thời gian, nút giữ vẫn lặp, console không lỗi. Ảnh chụp lưu ở `production/qa/evidence/`.

---

## Implementation Notes

- Tìm lại phạm vi bằng grep trước khi làm, vì code có thể đã đổi từ lúc review.
- Kiểm tra từng nơi gọi `EffectController.SpawnFx` còn chạy đúng qua play mode.

---

## Out of Scope

- Knockback enemy (story 005).
- `CameraFollow`/`CameraOffsetController` dùng Lerp làm mượt liên tục mỗi frame, giữ nguyên.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

*Governed by `qa.level: minimal`: test tự động được miễn, ảnh chụp thì không.*

**Story Type**: Integration
**Required evidence**: playtest + ảnh chụp trong `production/qa/evidence/` (FX trúng đòn bật/tắt, nút giữ nếu có trong scene).

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: 008
