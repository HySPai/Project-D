# Story 008: Dọn comment và phần sót lại

> **Epic**: refactor-foundation
> **Status**: Blocked
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: M
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: —

> **BLOCKED**: chờ quyết quy ước comment. CLAUDE.md cấm `//`, còn `.claude/docs/coding-standards.md` yêu cầu doc comment cho public API. Chốt một trong hai (hoặc ngoại lệ cho `///`), rồi đổi Status thành Ready.

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Quy ước CLAUDE.md "Không viết `//` comment"; code review R6c (phần sót lại từ template, trùng lặp, field thừa)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.4.1f1 | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

- [ ] Quy ước comment đã chốt và ghi vào CLAUDE.md hoặc `coding-standards.md` để hai nơi không mâu thuẫn.
- [ ] Mọi file gameplay tuân theo quy ước đó: không còn `//` ở `CoreGame/`, `View/`, `FeedbackController/`, `Audio/` (trừ code bên thứ ba và `Ulti/PlayerPrefsEditor`).
- [ ] Xóa file sót lại không ai dùng: `SpiderSilk.cs` (rỗng), `TilemapManager`/`MatrixData` nếu xác nhận không dùng, `TinySauceSDKController` và chỗ gọi trong `GameManager`, `IAttacker` nếu chưa xóa ở story 001.
- [ ] Sửa lỗi lặp: `EnemyChaseState.Tick` gọi `Chase(...)` hai lần liên tiếp.
- [ ] Hợp nhất kênh sự kiện: chọn một trong `GameEvent` static hoặc `EventDispatcher`, ghi lý do; không còn hai hệ chạy song song cho cùng mục đích.
- [ ] `PlayerController` không phụ thuộc trực tiếp vào `GUIViewManager.Instance`; HUD nhận sự kiện hoặc tham chiếu qua Inspector.
- [ ] Console không có warning mới; project compile sạch. Không đổi gameplay, ảnh chụp lưu ở `production/qa/evidence/`.

---

## Implementation Notes

- Xóa file trong Unity phải kèm `.meta`; làm qua MCP.
- Hỏi lại trước khi xóa TinySauce/Tilemap nếu có tham chiếu trong scene hoặc build settings.
- Làm sau 006 và 007 để tránh xung đột sửa cùng file.

---

## Out of Scope

- Viết lại UI template hay `Helper.cs`, `PlayerPrefsX.cs` (bên thứ ba).

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

*Governed by `qa.level: minimal`: test tự động được miễn, ảnh chụp thì không.*

**Story Type**: Integration
**Required evidence**: compile sạch + playtest ngắn + ảnh chụp trong `production/qa/evidence/` (vào game, chiến đấu, chết, HUD).

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: 006, 007, quyết định quy ước comment
- Unlocks: None
