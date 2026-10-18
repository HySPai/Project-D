# Asset Specs — Character: Quái Vùng 1

> **Source**: design/narrative/lore/world-foundation.md (mục 1.1, 5), design/game-brief.md (MVP mục 6)
> **Art Bible**: design/art/art-bible.md
> **Generated**: 2026-10-18
> **Status**: 7 assets specced / 0 approved / 0 in production / 0 done
> **Review**: art-director và technical-artist không được gọi (Solo mode). Spec soạn từ luật art bible; ràng buộc kỹ thuật chưa được technical-artist kiểm.
> **Ghi chú**: thay hoàn toàn hình demo hiện có. Giữ tên và vai trò lore (Seed, Spider, Turret, Spider King). Hành vi combat thuộc game-designer; spec chỉ quy định hình và telegraph.

Luật chung (§3, §5): côn trùng chân/càng nhọn tỏa ra, phân đốt, khối nặng phía trên, lạnh toàn thân, đúng một điểm cam; không mặc vải; đứng yên thật rồi bật gấp. Máy khối vuông/trụ đối xứng, một mắt tròn, xám ngủ, cam thức. Phép thử: thu 25% + grayscale, bốn con phân biệt được bằng bóng đen.

## ASSET-007 — Seed

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SK_Enemy_Seed`, `T_Enemy_Seed_A`, `T_Enemy_Seed_E`, `M_Enemy_Seed`, prefab `P_Enemy_Seed` |
| Polycount | 2–3k |
| Texture Res | 512² |
| Rig | ≤ 25 bone (4 chân, vỏ trên, vỏ dưới) |
| Kích thước | ~0,4 m, nhỏ hơn hiệp sĩ |

**Visual Description:** Hạt ý chí nứt trước khi nở: khối bầu cỡ quả dừa, nhưng vỏ không trơn. Vỏ là các mảnh góc cạnh như vỏ hạt cứng tách đôi theo đường nứt ngoằn ngoèo; lõi cam #FF5A1F lộ qua vết nứt là điểm nóng duy nhất. Bốn chân ngắn nhọn thò ra từ khe dưới, chạy lạch bạch rồi lao; trước khi vỡ, vết nứt toác rộng và lõi sáng gắt.

**Art Bible Anchors:**
- §3: con trông vô hại phải lộ góc cạnh hoặc lõi cam.
- §4: vỏ bê tông lạnh #66747D pha xanh đêm; lõi cam HDR chỉ trên nền lạnh.
- §5: telegraph = vết nứt mở rộng (dáng) + lõi sáng lên (điểm cam).

**Generation Prompt:** `stylized creature concept, small cracked seed-pod insect monster, round body made of angular shell plates split by jagged cracks, glowing orange core visible through cracks, four short pointed legs, cold grey-blue shell, hand-painted texture, isometric game readability, top-left light` — negative: `cute face, eyes, fur, cloth, green plant, warm colors on shell, multiple glow points`

**Status:** Needed

## ASSET-008 — Spider

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SK_Enemy_Spider`, `T_Enemy_Spider_A`, `T_Enemy_Spider_E`, `M_Enemy_Spider`, prefab `P_Enemy_Spider` |
| Polycount | 3–5k |
| Texture Res | 512² |
| Rig | ≤ 40 bone (8 chân × 3, thân, bụng, càng) |
| Kích thước | sải chân ~1,2 m |

**Visual Description:** Kẻ dệt bẫy: bụng to nâng cao phía trên, đầu ngực nhỏ hạ thấp, tám chân dài phân đốt chĩa nhọn ra ngoài tạo silhouette hình sao. Một cụm mắt cam ở đầu là điểm nóng duy nhất; trên bụng có túi tơ màu kem đục (không phát sáng) làm điểm nhận diện. Telegraph: hai chân trước nhấc cao chỉ đúng hướng đòn, cụm mắt sáng lên.

**Art Bible Anchors:**
- §3: chân nhọn tỏa ra, phân đốt, khối nặng phía trên.
- §4: thân xanh đêm #22313F → bê tông lạnh; tơ kem đục để không lẫn tàn ý (kem-vàng bay lên).
- §5: bộ phận sắp đánh nhô khỏi silhouette, hướng nhọn chỉ hướng đòn.

**Generation Prompt:** `stylized spider monster concept for isometric action game, heavy raised abdomen with a pale silk sac, small lowered head, eight long segmented pointed legs spread in a star silhouette, single cluster of glowing orange eyes, cold dark navy and grey body, hand-painted texture, clean readable shapes` — negative: `realistic hairy spider, cute, cloth, multiple glowing spots, purple, gore`

**Status:** Needed

## ASSET-009 — Turret (máy quân sự)

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SK_Enemy_Turret` (rig cứng), `T_Enemy_Turret_A`, `T_Enemy_Turret_E`, `M_Enemy_Turret`, prefab `P_Enemy_Turret` |
| Polycount | 2–4k |
| Texture Res | 512² |
| Rig | ≤ 8 bone cứng (đế, thân xoay, nắp, nòng trượt, mắt) |
| Kích thước | cao ~1,2 m, cao hơn hiệp sĩ |

**Visual Description:** Khối trụ đối xứng bê tông-kim loại trên đế vuông bắt vít xuống sàn, vỏ chống mục sạch bất thường giữa rêu (máy tự sửa). Một mắt tròn đơn ở giữa thân: xám khi ngủ, cam khi thức. Ngủ thì nắp khép kín; thức thì nắp tách, nòng trượt ra, thân xoay đều rồi khóa cứng trước khi bắn. Trên vỏ còn vạch sọc cảnh báo bạc màu của người xưa (không chữ đọc được).

**Art Bible Anchors:**
- §3/§5: máy vuông/trụ đối xứng, cứng tuyệt đối, "biểu cảm" duy nhất là mắt.
- §6: quanh máy đang thức cỏ cháy xém thành vòng (ASSET-027).
- §5 telegraph: thân khóa, mắt xám → cam, bộ phận trượt ra; đòn càng mạnh khóa càng lâu.

**Generation Prompt:** `stylized abandoned military sentry turret concept, symmetrical cylindrical body on square bolted base, single round lens eye, closed armored lid in sleep mode and open lid with sliding barrel in active mode, faded diagonal warning stripes without text, weathered concrete and steel, hand-painted texture, moss around the base but clean shell` — negative: `sci-fi neon, multiple eyes, readable text, organic shapes, cute robot, rounded mascot`

**Status:** Needed

## ASSET-010 — Spider King (boss Vùng 1)

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SK_Boss_SpiderKing`, `T_Boss_SpiderKing_A`, `T_Boss_SpiderKing_E`, `M_Boss_SpiderKing` (+ slot 2 `M_Boss_SpiderKing_Silk`), prefab `P_Boss_SpiderKing` |
| Polycount | 10–20k |
| Texture Res | 1024² (+ 512² cho tơ) |
| Material | ≤ 3 slot |
| Rig | ≤ 100 bone |
| Kích thước | cao ~3 m khi đứng nửa thẳng |

**Visual Description:** Vua giả: con nhện khổng lồ đứng nửa thẳng trên bốn chân sau, bốn chân trước gập như tay đặt trên "tay vịn". Trên đầu "đội" vương miện là vành một chiếc đèn đường gãy của người xưa; sau lưng phủ "áo choàng" tơ dày rủ xuống như rèm triều đình. Trên vỏ lưng có sọc chéo kiểu vạch cảnh báo chiến tranh của người xưa (gợi mối nối với Hắc Tàn). Nguồn "giả ấm" riêng: túi phát quang ở ngực màu vàng nhạt ngả lục, giống ánh nến nhưng lạnh; phase sau chuyển dần sang cam báo.

**Art Bible Anchors:**
- §2 đấu boss: một nguồn "giả ấm" riêng (không phải amber thật); phase sau đẩy sang cam.
- §5: boss nhại hành vi/đồ của người nhưng sai tỉ lệ; nhiều điểm cam, mỗi lúc chỉ một điểm sáng nhất.
- §4: thân xanh đêm; "giả ấm" đề xuất #C9C46A (vàng-lục), cần duyệt.
- Lore (world-foundation §5, §7): bắt chước ngai vàng và triều đình; hoa văn chiến tranh trên lưng.

**Generation Prompt:** `boss creature concept, giant spider king standing half-upright like a monarch, front legs folded like arms resting on armrests, crown made from a broken old street lamp ring, thick silk cape draped like royal curtains, faded diagonal war-stripe markings on the carapace, sickly pale yellow-green glowing chest sac like fake candlelight, cold dark navy body, hand-painted texture, dramatic but readable silhouette` — negative: `gold crown jewels, warm amber glow, cute, realistic hair, gore, readable text, many glowing spots`

**Status:** Needed

## ASSET-011 — VFX telegraph dùng chung (điểm cam)

| Field | Value |
|-------|-------|
| Category | VFX |
| Naming | `vfx_enemy_telegraph_small`, `vfx_enemy_telegraph_large`; texture `T_Vfx_Glint_A` 128² |
| Ràng buộc | ≤ 20 hạt; gắn vào điểm cam của từng quái; Shuriken qua `EffectController` + `SO_Vfx` |

**Visual Description:** Lóe cam #FF5A1F HDR (> ngưỡng bloom 1.1) ngắn, sắc cạnh, phình 1,0 → 1,6 trong pha báo trước rồi giữ ở pha giữ; 2–4 tia nhọn tỏa ra theo hướng đòn. Luôn đi kèm thay đổi dáng của quái và âm thanh; không bao giờ là tín hiệu duy nhất.

**Art Bible Anchors:**
- §4: cam chỉ cho nguy hiểm, chỉ trên nền lạnh; nhỏ, sắc, nháy nhanh.
- §5: cấm telegraph chỉ bằng màu.
- §7: không nháy quá 3 lần/giây.

**Generation Prompt:** `game vfx sprite, sharp small orange glint with 3 pointed rays, grayscale with alpha for tinting, soft core, crisp edges` — negative: `round soft glow, warm amber, sparkles, lens flare`

**Status:** Needed

## ASSET-012 — VFX riêng của Spider King

| Field | Value |
|-------|-------|
| Category | VFX |
| Naming | `vfx_spiderking_silk_burst`, `vfx_spiderking_summon`, `vfx_spiderking_phase_shift`; texture ≤ 512² |
| Ràng buộc | ≤ 300 hạt mỗi hiệu ứng |

**Visual Description:**
- Silk burst: dải tơ kem đục bắn thành hình nón nhọn theo hướng đòn, telegraph bằng tơ căng rung trước 0,5 giây.
- Summon: túi trứng trên áo choàng nứt, Spider nhỏ rơi xuống kèm bụi.
- Phase shift: túi ngực chuyển vàng-lục → cam, nền đấu trường tối thêm một bậc.

**Art Bible Anchors:** §2 đấu boss; §5 boss telegraph cả thân, có thể đổi sáng nền.

**Generation Prompt:** `vfx concept sheet, pale silk strands shooting in a pointed cone, egg sacs cracking with dust, chest glow shifting from sickly yellow-green to orange, dark cold arena background` — negative: `fire, magic runes, rainbow, warm amber`

**Status:** Needed

## ASSET-013 — Material viền xuyên tường của quái

| Field | Value |
|-------|-------|
| Category | Material / Shader |
| Naming | `M_Fx_Occlusion_Enemy` |
| Ràng buộc | SRP Batcher compatible; ≤ 25 renderer có viền cùng lúc |

**Visual Description:** Silhouette cam đục, không HDR (khác telegraph), alpha ~50%, không nhấp nháy.

**Art Bible Anchors:** §5 viền quái cam đục; §8 trần renderer có viền.

**Generation Prompt:** không áp dụng (shader).

**Status:** Needed
