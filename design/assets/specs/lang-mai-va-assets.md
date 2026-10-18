# Asset Specs — Level: Làng Mái Vá

> **Source**: design/narrative/lore/world-foundation.md §4.0, design/art/art-bible.md §2 và §6 (phác thảo Làng Mái Vá) — chưa có `design/levels/lang-mai-va.md`
> **Art Bible**: design/art/art-bible.md
> **Generated**: 2026-10-18
> **Status**: 13 assets specced / 0 approved / 0 in production / 0 done
> **Review**: art-director và technical-artist không được gọi (Solo mode). Spec soạn từ luật art bible; ràng buộc kỹ thuật chưa được technical-artist kiểm. Chức năng NPC (cửa hàng, nâng cấp) chờ game-designer.

Chung: 2700–3200K, nhiều nguồn sáng nhỏ, bóng mềm; mật độ prop cao nhất game nhưng lối đi chính sàn sạch; mọi thứ cư dân thêm vào cong, lệch, ấm; texture vẽ tay; tái dùng tileable và trim của Vùng 1 (ASSET-014, 015).

## ASSET-034 — Sân ga và mái ga gãy

| Field | Value |
|-------|-------|
| Category | 3D (bộ module) |
| Naming | `SM_Village_Platform_4m`, `SM_Village_PlatformEdge_4m`, `SM_Village_StationRoof_Broken_01..02`, `SM_Village_Pillar_01` |
| Polycount | 200–1500 mỗi module |

**Visual Description:** Sân ga dài của người xưa, cột thép đỡ mái ga; mái gãy một phần, lỗ hổng được căng vải nhiều màu. Đường ray chạy dọc hai bên, ra khỏi làng thì lạnh dần.

**Art Bible Anchors:** §6 phác thảo làng; §4 nhiệt làng; rìa làng lạnh dần đánh dấu ranh giới an toàn.

**Generation Prompt:** `isometric cozy refugee village inside an old train station, long platform, steel pillars, partly broken station roof patched with stretched colorful fabric, warm lantern light, hand-painted` — negative: `modern station, crowds of humans, readable signs`

**Status:** Needed

## ASSET-035 — Toa nhà

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Village_RailcarHome_01..03`, `T_Village_RailcarHome_A` 1024² |
| Polycount | 1500–5000 |

**Visual Description:** Toa tàu đỗ song song làm nhà; cửa tròn cắt vào thành toa, cửa sổ vá ván, ống khói lò nhỏ thò ra mái. Ánh lửa hắt ra cửa toa; mỗi đầu toa treo một đèn dầu.

**Art Bible Anchors:** §3 làng nghiêng cong, cửa tròn cắt từ toa; §6 lớp cư dân dùng lại khung người xưa.

**Generation Prompt:** `old railway carriages converted into cozy homes, round doors cut into the metal sides, windows patched with planks, tiny chimney pipes, oil lamp at each end, warm firelight spilling out, hand-painted isometric` — negative: `new paint, neon, text`

**Status:** Needed

## ASSET-036 — Mái vá vải (prop anh hùng)

| Field | Value |
|-------|-------|
| Category | 3D + Texture |
| Naming | `SM_Hero_PatchRoof_01..04`, `T_Hero_PatchRoof_A` 1024² (vải vá là ngoại lệ được họa tiết) |
| Polycount | 1500–5000 |

**Visual Description:** Mái võng ghép từ nhiều mảng vải, ván và lá; mảng vá không thẳng hàng, đường khâu lớn nhìn rõ từ trên xuống — đây là hình ảnh nhận diện của làng ở camera isometric. Màu vải ấm, bão hòa vừa, không lấn nhân vật.

**Art Bible Anchors:** §6 vật anh hùng, vải vá là ngoại lệ có họa tiết; §2 Làng Mái Vá.

**Generation Prompt:** `top-down view of sagging patchwork roofs made of many fabric pieces, planks and leaves, big visible stitches, warm muted colors, hand-painted` — negative: `neat quilt, bright saturated rainbow, plastic`

**Status:** Needed

## ASSET-037 — Bàn thờ chung kiêm Ngưỡng (prop anh hùng)

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Hero_VillageAltar_01`, `T_Hero_VillageAltar_A` 1024², prefab `P_Threshold_Village` |
| Polycount | 2–5k |
| Ánh sáng | nguồn amber lớn nhất làng, ổn định; nến dùng `vfx_village_candle` |

**Visual Description:** Bàn thờ giữa sân ga ghép từ quầy bán vé cũ; trên đó là vật thờ của nhiều loài (sừng dê, quả sồi, búa nhỏ, bó thảo dược, đồ nhặt, lông vịt, móng gấu) và nến nhiều cỡ. Ở giữa là đèn lồng Người Gác Ngưỡng làm điểm nghỉ. Tâm bố cục của làng.

**Art Bible Anchors:** §6 bàn thờ kiêm Ngưỡng; §2 bàn thờ chung có nến của nhiều loài; lore §4.0.

**Generation Prompt:** `communal shrine built from an old ticket counter at the center of a train platform, offerings from many animal species, many candles of different sizes, central old lantern glowing steady warm amber, cozy, hand-painted isometric hero prop` — negative: `religious symbols of real religions, gold altar, skulls`

**Status:** Needed

## ASSET-038 — Prop làng và VFX nhỏ

| Field | Value |
|-------|-------|
| Category | 3D + VFX |
| Naming | `SM_Village_OilLamp_01`, `SM_Village_Clothesline_01..02`, `SM_Village_Pot_01..03`, `SM_Village_Scrap_Pile_01..03`; VFX `vfx_village_chimney_smoke`, `vfx_village_candle`, `vfx_village_door_glow` |
| Polycount | 50–800 |
| Texture | atlas `T_Prop_Village_Atlas_A` 1024² |

**Visual Description:** Đèn dầu treo, dây phơi đồ vá nối các toa, chậu cây trồng trong xô và lốp xe, đống đồ nhặt (nắp lon, mảnh kính, dây). Khói bếp mảnh, lửa nến nhỏ chập chờn nhẹ (nhịp chậm, khác Ngưỡng ổn định).

**Art Bible Anchors:** §6 mật độ prop làng; §8 particle ≤ 50 mỗi hiệu ứng.

**Generation Prompt:** `prop sheet: hanging oil lamps, clotheslines with patched laundry, plants growing in buckets and old tires, piles of scavenged junk, thin chimney smoke, small candle flames, hand-painted` — negative: `plastic bright, modern`

**Status:** Needed

## ASSET-039 — Điểm chức năng: quầy, lò rèn, cổng làng

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Village_Stall_BaHat_01`, `SM_Village_Forge_BacGai_01`, `SM_Village_Gate_LaoMat_01` |
| Polycount | 1500–5000 |

**Visual Description:** Quầy của Bà Hạt dựng từ xe đẩy hàng, treo túi hạt; lò rèn của Bác Gai làm từ thùng phuy và đường ống, ánh lửa đỏ cam ấm (lửa thật, không phải cam báo); cổng làng là hai toa chắn ngang có cửa trượt, chòi gác của Lão Mật.

**Art Bible Anchors:** §6 cư dân dùng lại đồ người xưa; §4 lửa lò rèn là amber/ấm, không dùng cam #FF5A1F.

**Generation Prompt:** `market stall made from an old shopping cart with seed bags; blacksmith forge built from oil drums and pipes with warm firelight; village gate made of two railcars with a sliding door and a watch hut, hand-painted isometric` — negative: `danger orange glow, neon, text signs`

**Status:** Needed

## ASSET-040..046 — Bảy NPC

Chung cho NPC:

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SK_Npc_[Name]`, `T_Npc_[Name]_A`, `M_Npc_[Name]`, prefab `P_Npc_[Name]` |
| Polycount | 3–6k |
| Texture Res | 512² |
| Rig | ≤ 40 bone; 2–4 clip: `Idle`, `Idle_Signature`, `Talk`, `React` |
| Viền xuyên tường | không (§5) |

Luật hình (§3, §5): thân bầu tròn/oval, không góc nhọn; ít nhất một mảng vải vá ≥ 6–8 px trên màn hình; mỗi loài một tư thế chữ ký đọc được bằng bóng đen; cường điệu nhẹ, hài; ấm toàn thân.

| ID | NPC | Loài | Phụ kiện vá | Tư thế chữ ký | Dấu "đã mất" |
|---|---|---|---|---|---|
| ASSET-040 | Ông Râu (trưởng làng, kể chuyện) | Dê | khăn choàng vải vá trùm vai | chống gậy, râu dài, ngồi trên rương | chuông đàn dê đeo ở gậy |
| ASSET-041 | Bà Hạt (bán hàng) | Sóc | tạp dề nhiều túi vá | ôm túi hạt, đuôi cong cao | một nhánh sồi khô cắm túi |
| ASSET-042 | Bác Gai (thợ rèn) | Nhím | tạp dề da vá, găng to | cầm búa, gù lưng, gai nhô qua áo | khăn của người học việc buộc tay |
| ASSET-043 | Cô Thảo (y sĩ) | Thỏ | túi đeo chéo vá, bó thảo dược | nhón chân, tai rủ một bên | ba dải ruy băng nhỏ trên tai |
| ASSET-044 | Tí Lục (nhặt đồ, rành Mạch Ngầm) | Chuột | ba lô vá to hơn người, kính bảo hộ cũ | khom, lục lọi, cuộn cáp trên vai | không có (không nhớ mình mất gì) |
| ASSET-045 | Chị Mỏ (đưa tin, bán bản đồ) | Vịt | túi thư vá, cuộn giấy bản đồ | đứng thẳng, cánh chỉ đường | lọ nước đầm đeo cổ |
| ASSET-046 | Lão Mật (canh cổng, dạy chặn/parry) | Gấu | áo choàng vá, nắp nồi làm khiên | đứng dựa chân gỗ thay chân sau | chân sau bằng gỗ |

**Visual Description (chung):** Động vật hơi giống người, đứng hai chân, tay khéo, vẫn giữ tai, đuôi, mõm, lông. Mỗi người mang một dấu vết của thứ đã mất. Hình tròn mềm, khác hẳn côn trùng nhọn; không ai đeo kiếm chéo lưng hay khăn cùng màu hiệp sĩ.

**Art Bible Anchors:** §3 NPC thân bầu, phụ kiện vải vá; §5 vải vá là chữ ký phe sống, LOD ~4 mảng màu; lore §1 hình dáng "hơi giống người", §4.0 cư dân.

**Generation Prompt (mẫu, thay [loài/phụ kiện/tư thế]):** `character concept, round chubby anthropomorphic [goat elder] villager, standing on two legs, keeps animal ears tail and muzzle, wearing [a patched cloth shawl], signature pose [leaning on a staff with a herd bell], warm cozy colors, hand-painted texture, readable silhouette for isometric game` — negative: `human face, sharp angular shapes, armor, weapons on back, dark scarf, anime`

**Status:** Needed (7 NPC)
