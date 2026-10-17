# Story 005: Enemy: mất thăng bằng, hiệu ứng, yếu/kháng

> **Epic**: refactor-foundation
> **Status**: Ready
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: —

## Context

**GDD**: `design/gdd/elemental-magic.md`
**Requirement**: Luật 17 (yếu/kháng), bảng hiệu ứng Bỏng/Chậm/Choáng; `weapons.md` luật 9 (mất thăng bằng 1.5s), Búa mất thế ×2, edge case Bọ Giáp

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.4.1f1 | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ code review R5. Chưa có nguyên tố hay Khiên nào; kiểm chứng bằng nút debug.*

- [ ] `EnemyState` có thanh thế (poise). Lượng mất thế lấy từ `DamageInfo`. Đủ ngưỡng thì vào trạng thái Stagger; boss cấu hình được miễn hoặc ngưỡng cao.
- [ ] FSM có slot Stagger. Với cấu hình mặc định (chưa đặt ngưỡng) phản ứng khi trúng đòn giống hiện tại: đẩy lùi và choáng 0.5s.
- [ ] Component hiệu ứng trạng thái cho enemy: Bỏng (4s, 5 sát thương/giây), Chậm (3s, tốc độ di chuyển và tấn công ×0.6), Choáng (1s, không hành động). Không cộng dồn: dính lại chỉ làm mới thời gian. Đếm theo thời gian game.
- [ ] Hệ số Chậm áp vào tốc độ `NavMeshAgent` và tốc độ animator.
- [ ] `SO_EnemyStats` có bảng yếu/kháng theo `ElementType`. Seed, Spider, Spider King yếu Lửa; Turret không yếu không kháng. Yếu ×1.5 và luôn gây hiệu ứng; kháng ×0.5 và không hiệu ứng; nếu data ghi cả hai thì kháng thắng. Áp dụng trong `ResolveIncoming` của story 001.
- [ ] Đẩy lùi dùng DOTween/UniTask, không còn Coroutine trong `EnemyMovement`; tween bị hủy khi quái chết hoặc bị hủy object.
- [ ] Nút `[Button]` debug áp Bỏng/Chậm/Choáng/sát thương Lửa lên một quái để kiểm chứng.
- [ ] Không đổi gameplay mặc định: quái bị kiếm chém, bị đẩy lùi, choáng, rồi đuổi tiếp như cũ. Ảnh chụp lưu ở `production/qa/evidence/`.

---

## Implementation Notes

- Số liệu mặc định lấy đúng từ GDD, đặt trong ScriptableObject.
- Dùng `DamageInfo.element`/`staggerAmount` từ story 001; `ElementType` đã có.
- Tách logic knockback thành hàm UniTask với `CancellationToken` gắn `GetCancellationTokenOnDestroy`.
- Sửa asset `SO_*Stats` qua MCP, trình kế hoạch trước.

---

## Out of Scope

- Nguyên tố thật và kỹ năng phép (chỉ có đường nhận hiệu ứng).
- Bọ Giáp, Kiến Thợ và quái mới.
- Hành vi boss nhiều phase của Spider King.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

*Governed by `qa.level: minimal`: test tự động được miễn, ảnh chụp thì không.*

**Story Type**: Integration
**Required evidence**: playtest + ảnh chụp trong `production/qa/evidence/` (đẩy lùi như cũ, Bỏng/Chậm/Choáng bằng nút debug, Spider nhận ×1.5 từ Lửa).

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: 001, 004
- Unlocks: epic nguyên tố
