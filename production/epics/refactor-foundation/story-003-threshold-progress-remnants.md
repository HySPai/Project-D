# Story 003: Ngưỡng, tiến độ và tàn ý

> **Epic**: refactor-foundation
> **Status**: Ready
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: XL
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: —

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 2 (điểm nghỉ và cái chết kiểu Souls); mục "Thứ tự xây" bước 1; lore 1.2 (Ngưỡng, tàn ý)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.4.1f1 | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ code review R3 và brief.*

- [ ] Chết không tải lại scene. Người chơi hồi sinh ở Ngưỡng gần nhất đã nghỉ: tim đầy, stamina đầy, vị trí đúng điểm nghỉ.
- [ ] Có Ngưỡng tương tác được trong scene. Nghỉ ở đó hồi đầy tim và stamina, đặt điểm hồi sinh, và lưu tiến độ.
- [ ] Quái hồi sinh về `HomePosition` với máu đầy khi người chơi nghỉ ở Ngưỡng hoặc hồi sinh sau khi chết.
- [ ] Quái chết rơi tàn ý (số lượng cấu hình trong ScriptableObject). Người chơi nhặt được; số tàn ý hiện trên HUD.
- [ ] Chết thì tàn ý đang có rơi tại chỗ chết. Quay lại nhặt được; chết lần nữa trước khi nhặt thì số tàn ý cũ mất hẳn.
- [ ] Tiến độ lưu khi nghỉ (tàn ý, Ngưỡng đã nghỉ, tàn ý đang rơi nếu có) và tải lại khi mở game.
- [ ] Màn Game Over hiện có đổi thành luồng "gục → hồi sinh"; bấm nút thì hồi sinh thay vì tải lại scene.
- [ ] Dữ liệu tiến độ là một class thuần dễ mở rộng cho nguyên tố đã nhận, vũ khí và cấp nâng cấp sau này; chưa thêm các trường đó.
- [ ] Không đổi gameplay ngoài luồng chết: combat, lock-on, lăn như cũ. Ảnh chụp lưu ở `production/qa/evidence/`.

---

## Implementation Notes

- Bỏ `SceneManager.LoadSceneAsync` trong `GameManager.HandleRestartRequested` khi có Ngưỡng; giữ làm phương án dự phòng nếu chưa có Ngưỡng nào được nghỉ.
- Hồi sinh quái dùng `HomePosition`/`HomeRotation` đã có trong `EnemyState`. Cần `Revive()` trên `CharacterStateBase`/`EnemyState` (bật lại CharacterController, LegsAnimator, collider, FSM).
- Giá trị tàn ý nằm trong ScriptableObject, không viết cứng.
- Dùng UniTask nếu cần đợi; tween dùng DOTween.
- Sửa scene/prefab/HUD qua MCP, trình kế hoạch trước; UI cần ảnh chụp từng màn đụng tới.

---

## Out of Scope

- Đổi vũ khí, nâng cấp, mua bán ở làng (menu nghỉ chỉ cần nút nghỉ).
- Thanh nguyên tố hồi đầy khi nghỉ (khi hệ nguyên tố tồn tại).
- Bản đồ, Làng Mái Vá, NPC.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

*Governed by `qa.level: minimal`: test tự động được miễn, ảnh chụp thì không.*

**Story Type**: Integration
**Required evidence**: playtest các kịch bản (chết rồi nhặt lại, chết hai lần liên tiếp, nghỉ rồi quái hồi sinh, thoát game rồi mở lại) + ảnh chụp trong `production/qa/evidence/`, kèm ảnh HUD tàn ý và màn gục.

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None (nên làm sau 001)
- Unlocks: epic nguyên tố và vũ khí (hồi thanh, lưu đồ ở Ngưỡng)
