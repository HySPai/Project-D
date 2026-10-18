# Asset Specs — Level: Vùng 1 (rừng trên tuyến đường sắt cũ)

> **Source**: design/art/art-bible.md §6 (phác thảo Vùng 1), design/narrative/lore/world-foundation.md (mục 1.1, 4.0), design/game-brief.md — chưa có `design/levels/vung-1.md`
> **Art Bible**: design/art/art-bible.md
> **Generated**: 2026-10-18
> **Status**: 20 assets specced / 0 approved / 0 in production / 0 done
> **Review**: art-director và technical-artist không được gọi (Solo mode). Spec soạn từ luật art bible; ràng buộc kỹ thuật chưa được technical-artist kiểm. Bố cục màn (vị trí lối tắt, câu đố, Ngưỡng) chờ level-designer.

Chung cho mọi asset môi trường: texel 256 px/m; albedo vẽ tay nét cọ lớn, không PBR, không normal chi tiết; 1 material slot; dải giá trị trung, bão hòa thấp-vừa (§6); nguồn ở `SourceArt/Environment/Zone01/`; pivot module ở góc khớp lưới 0,5 m, prop ở tâm đáy.

## ASSET-014 — Bộ tileable vẽ tay

| Field | Value |
|-------|-------|
| Category | Texture |
| Naming | `T_Tile_Concrete_A`, `T_Tile_Wood_A`, `T_Tile_Soil_A`, `T_Tile_Moss_A`; material `M_Tile_[Name]` |
| Texture Res | bê tông, gỗ, đất 1024² lặp mỗi 4 m; rêu 512² lặp mỗi 2 m |
| Format | BC7 (BC1 cho đất nếu so mắt không vỡ gradient), Repeat, mip Kaiser, aniso 2–4× |

**Visual Description:** Bê tông lạnh #66747D có vết nứt lớn và vệt gỉ chảy dọc; gỗ nâu mật xẻ thô; đất ấm nâu đỏ có sỏi lớn; rêu #5F8F4E ngả ấm, mảng mềm. Mỗi tấm có một swatch gốc. Không họa tiết 4–8 px (lẫn với chân quái và máu).

**Art Bible Anchors:** §4 bảng màu; §6 kể chuyện ba lớp; §8 bậc tileable; §9 kỹ thuật kiểu Blizzard.

**Generation Prompt:** `seamless hand-painted tileable texture, [weathered cold grey concrete with large cracks and vertical rust streaks | rough-sawn warm brown planks | warm reddish soil with large pebbles | soft warm green moss], broad brush strokes, mid values, low saturation, top-down flat` — negative: `photo, small noise, high contrast, normal map look, seams`

**Status:** Needed

## ASSET-015 — Trim sheet

| Field | Value |
|-------|-------|
| Category | Texture |
| Naming | `T_Trim_ConcreteMetal_A`, `T_Trim_WoodCloth_A` |
| Texture Res | 1024², dải theo bội 32 px, padding ≥ 8–16 px |

**Visual Description:** Bê tông/kim loại: mép bê tông vỡ, dầm thép gỉ, bu lông, ống, vạch sọc cảnh báo bạc màu (không chữ). Gỗ/vải: ván, gờ gỗ mòn, mép vải vá có đường khâu, dây buộc.

**Art Bible Anchors:** §6, §8 trim sheet; §6 biển hiệu không ký tự đọc được.

**Generation Prompt:** `hand-painted trim sheet, horizontal strips: broken concrete edges, rusty steel beams, bolts, pipes, faded warning stripes without text, worn wooden ledges, patched fabric edge with stitches, rope` — negative: `letters, numbers, logos, photo`

**Status:** Needed

## ASSET-016 — Đường ray

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Env_Rail_Straight_4m`, `SM_Env_Rail_Curve_4m`, `SM_Env_Rail_RootBent_4m`, `SM_Env_Rail_End_01` |
| Polycount | 200–1000 mỗi đoạn 4 m |
| Texture | trim `T_Trim_ConcreteMetal_A` + `T_Tile_Wood_A` |

**Visual Description:** Đường ray quá khổ so với cư dân (tà vẹt cao ngang đầu gối hiệp sĩ), ray gỉ, tà vẹt gỗ mục. Biến thể "RootBent": rễ trồi lên nâng và uốn cong ray. Là đường dẫn mắt chính xuyên rừng.

**Art Bible Anchors:** §6 lớp người xưa (quá khổ), lớp thời gian (rễ nâng); phác thảo Vùng 1.

**Generation Prompt:** `isometric game asset, oversized old railway track segment, rusty rails, rotten wooden sleepers, thick tree roots lifting and bending the rails, moss on horizontal surfaces, hand-painted` — negative: `new metal, train, photo`

**Status:** Needed

## ASSET-017 — Toa tàu

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Env_Railcar_01`, `SM_Env_Railcar_Tilted_01` |
| Polycount | 1500–5000 (prop anh hùng cho bản Tilted) |
| Texture | trim + 1024² riêng cho bản Tilted |

**Visual Description:** Toa chở khách của người xưa, khung vuông thép, cửa sổ vỡ không kính. Bản Tilted bị rễ cổ thụ nâng nghiêng ~20° giữa rừng, dây leo phủ một bên, bên trong có một dấu vết người (ghế đơn còn nguyên). Vật nhận diện của Vùng 1.

**Art Bible Anchors:** §3 khung góc cạnh bị đường cong phủ; §6 vật nhận diện Vùng 1.

**Generation Prompt:** `isometric game asset, abandoned passenger railcar lifted and tilted by giant tree roots, broken windows, vines covering one side, moss on roof, warm sunlight shaft falling on it, hand-painted` — negative: `intact glass, graffiti text, sci-fi`

**Status:** Needed

## ASSET-018 — Cột điện đổ (cầu)

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Env_PowerPole_Fallen_01` |
| Polycount | 300–1500 |

**Visual Description:** Cột điện bê tông đổ ngang vắt qua khe vực, dây cáp chùng còn treo, sứ cách điện vỡ. Mặt trên đủ rộng cho hiệp sĩ đi; rêu dẫm thành vệt khi là lối đi.

**Art Bible Anchors:** §6 vật nhận diện; §6 từ vựng "rêu bị dẫm = có lối".

**Generation Prompt:** `fallen concrete power pole lying across a gap as a bridge, sagging cables, broken insulators, moss with a worn trampled path on top, hand-painted isometric asset` — negative: `wood pole, sparks, photo`

**Status:** Needed

## ASSET-019 — Sân bê tông và cầu cạn

| Field | Value |
|-------|-------|
| Category | 3D (bộ module) |
| Naming | `SM_Env_Platform_Floor_4m`, `SM_Env_Platform_Edge_4m`, `SM_Env_Viaduct_Span_8m`, `SM_Env_Viaduct_Pillar_01`, `SM_Env_Stair_Big_01` |
| Polycount | sàn 2–100; mép, cột, nhịp 200–1000 |

**Visual Description:** Vùng rừng thưa dần: sân bê tông lạnh lớn, nứt, cỏ mọc theo khe; cầu cạn nhịp lớn với cột vuông. Bậc thang người xưa cao ngang đầu hiệp sĩ (không leo được, phải tìm lối khác). Nhiệt chuyển 4500K → ~6000K.

**Art Bible Anchors:** §6 tỉ lệ người thật gấp 2–3 lần cư dân; §4 nhiệt tàn tích; §6 mật độ prop thấp-vừa.

**Generation Prompt:** `isometric modular kit, cracked concrete plaza, raised concrete viaduct with square pillars, oversized steps taller than a small animal, grass in cracks, cool daylight, hand-painted` — negative: `people, cars, text`

**Status:** Needed

## ASSET-020 — Tường đặc

| Field | Value |
|-------|-------|
| Category | 3D (bộ module) |
| Naming | `SM_Env_Wall_Concrete_4m`, `SM_Env_Wall_Concrete_Corner`, `SM_Env_Wall_Brick_4m`, `SM_Env_Wall_Fake_4m` (tường giả, cùng hình) |
| Polycount | 200–1000 mỗi đoạn 4 m |

**Visual Description:** Khối đặc, cạnh sạch, đỉnh tường sáng lên một bậc để tách khối dưới camera cao. Không bao giờ trong suốt. Tường giả giống hệt tường thật, chỉ khác một gợi ý tinh tế (vết nứt xuyên, rêu không đọng).

**Art Bible Anchors:** §1 nguyên tắc 1 và 3; §3 tường khối đặc; §9 mép trên sáng.

**Generation Prompt:** `isometric modular solid concrete wall segment, clean silhouette, lighter top edge, moss on top, rust streaks, hand-painted` — negative: `transparent, holes, windows, graffiti`

**Status:** Needed

## ASSET-021 — Cây lớn và tán lá

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Veg_Tree_Big_01..03`, `SM_Veg_Canopy_01..02` |
| Polycount | thân 1500–3000; tán 800–2000 (card lá lớn) |
| Texture | `T_Veg_Foliage_A` 1024² atlas |

**Visual Description:** Cổ thụ thân to, rễ bạnh trồi lên bám tàn tích; tán lá là mảng lớn, ít chi tiết lá nhỏ. Tán cao chỉ làm khung ở mép khung hình, không che lối đi; tia nắng xuyên khe tán.

**Art Bible Anchors:** §6 tán lá cao là khung; §9 Ghibli/Oga — tránh lá nhỏ li ti; §4 rêu lá ngả ấm.

**Generation Prompt:** `stylized hand-painted ancient tree with buttress roots gripping concrete ruins, canopy in large simple clumps, warm green, light shafts through gaps, isometric game asset` — negative: `tiny leaves noise, realistic foliage, cel shading`

**Status:** Needed

## ASSET-022 — Rễ và dây leo

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Veg_Root_01..04`, `SM_Veg_Vine_Hang_01..03`, `SM_Veg_Vine_Wall_01..02` |
| Polycount | 100–800 |

**Visual Description:** Rễ cong mềm vắt qua khối vuông, dây leo rủ xuống từ mép cầu và tường. Cong phủ lên vuông là ngôn ngữ chính của vùng.

**Art Bible Anchors:** §3, §6 lớp thời gian.

**Generation Prompt:** `hand-painted organic roots and hanging vines draping over square concrete blocks, soft curves, isometric asset sheet` — negative: `thorns, purple, photo`

**Status:** Needed

## ASSET-023 — Cỏ thấp và rêu phủ

| Field | Value |
|-------|-------|
| Category | 3D / Decal |
| Naming | `SM_Veg_Grass_Low_01..04`, `SM_Veg_Flower_01..02`; decal `M_Decal_MossPatch` |
| Polycount | ≤ 100 mỗi cụm, chung material, GPU instancing, tắt Cast Shadows |

**Visual Description:** Cỏ thấp không che chân nhân vật trên sàn giao tranh; hoa nhỏ ấm làm dấu sống. Rêu phủ dạng decal trên mặt ngang.

**Art Bible Anchors:** §6 cỏ thấp trên sàn giao tranh; §8 mảnh vỡ/cỏ ≤ 100.

**Generation Prompt:** `low stylized grass tufts and small warm wildflowers, hand-painted cards, short so they never hide feet` — negative: `tall grass, dense noise`

**Status:** Needed

## ASSET-024 — Ngưỡng: trạm chờ mái võng (prop anh hùng)

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Hero_Threshold_Shelter_01`, `T_Hero_Threshold_Shelter_A` 1024² (2048² chỉ nếu test menu thấy mờ), `M_Hero_Threshold`, prefab `P_Threshold_Zone01` |
| Polycount | 2–5k |
| Ánh sáng | 1 point light amber #E9A23B ổn định, không nhấp nháy |

**Visual Description:** Trạm chờ xe buýt/tàu nhỏ của người xưa, mái võng xuống, cư dân vá bằng vải. Ở giữa là đèn lồng của Người Gác Ngưỡng, lõi kem, tỏa vòng amber tròn; quanh đó vòng sàn trống, 2–3 vật nhỏ của cư dân từng nghỉ (vải gấp, chén). Trạng thái chưa thắp: đèn tắt, xám lạnh; "Lạnh" (bị truy đuổi): ánh co lại, ngả xanh.

**Art Bible Anchors:** §2 nghỉ ở Ngưỡng; §4 amber + lõi kem; §6 mật độ prop Ngưỡng rất thấp; rest-point.md (thắp, Lạnh).

**Generation Prompt:** `small abandoned bus shelter with sagging roof patched with cloth, a single old lantern glowing steady warm amber with cream core, circle of light on the floor, a folded cloth and a small cup left behind, dark cool surroundings, hand-painted isometric hero prop` — negative: `fire pit, bonfire sword, flicker, magic runes`

**Status:** Needed

## ASSET-025 — Prop kể chuyện cỡ người

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Prop_Chair_01`, `SM_Prop_Cup_01`, `SM_Prop_Boot_01`, `SM_Prop_Umbrella_01`, `SM_Prop_Stroller_01`, `SM_Prop_Helmet_Nest_01` |
| Polycount | 50–300 (nhỏ), 300–1500 (xe đẩy) |
| Texture | atlas prop chung `T_Prop_Atlas_A` 1024² |

**Visual Description:** Vật quen thuộc của người xưa, cỡ người thật nên to so với hiệp sĩ. Mỗi vật có một dấu sống: hoa mọc trong cốc, tổ chim trong mũ, rêu trên giày.

**Art Bible Anchors:** §6 vật cụ thể cỡ người, mất mát kèm dấu sống; §9 Ghibli.

**Generation Prompt:** `prop sheet of oversized human objects in ruins: wooden chair, mug with a flower growing inside, old boot with moss, broken umbrella, rusty baby stroller, helmet with a bird nest, hand-painted` — negative: `skeletons, blood, text labels`

**Status:** Needed

## ASSET-026 — Dấu côn trùng

| Field | Value |
|-------|-------|
| Category | 3D / Decal |
| Naming | `SM_Env_Web_Corner_01..03`, `SM_Env_Molt_Shell_01..02`, `SM_Env_Nest_Swollen_01`; decal `M_Decal_WebFloor` |
| Polycount | 100–800 |

**Visual Description:** Tơ dày đục giăng ở góc tối và khe; vỏ lột rỗng phân đốt; tổ phồng bám tường. Tông lạnh, không phát sáng. Báo "côn trùng ở gần".

**Art Bible Anchors:** §6 dấu kẻ thù và từ vựng lặp lại.

**Generation Prompt:** `thick pale insect webs in dark corners, empty segmented molted shells, swollen nest on a wall, cold tones, hand-painted, no glow` — negative: `gore, glowing, cartoon cute`

**Status:** Needed

## ASSET-027 — Dấu máy

| Field | Value |
|-------|-------|
| Category | Decal |
| Naming | `M_Decal_BurntGrassRing`, `M_Decal_ScorchLine`; texture `T_Decal_Burnt_A` 512² |
| Ràng buộc | decal unlit hoặc tô sẵn sáng tối (§8) |

**Visual Description:** Vòng cỏ chết cháy xém sạch bất thường quanh Turret đang thức; vệt xém thẳng trên tường theo đường đạn.

**Art Bible Anchors:** §6 dấu máy; §8 decal.

**Generation Prompt:** `top-down decal, ring of burnt dead grass with clean scorched center, straight scorch streaks, hand-painted, alpha` — negative: `fire, embers, glow`

**Status:** Needed

## ASSET-028 — Prop giải đố Mạch Ngầm

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Puzzle_Cable_Broken_01`, `SM_Puzzle_Cable_Joined_01`, `SM_Puzzle_Lever_01`, `SM_Puzzle_Junction_01` (trạm chuyển), prefab `P_Puzzle_[Name]` |
| Polycount | 300–1500 |
| Emission | `_E` mask: dòng điện chảy amber nhạt khi nối (không phải cam) |

**Visual Description:** Cáp dày người xưa bị rễ và nước làm đứt; cần gạt lớn quá khổ (hiệp sĩ phải dùng cả người kéo); trạm chuyển là hộp kim loại vuông có nắp. Khi nối: dòng sáng chạy dọc cáp, lối mở.

**Art Bible Anchors:** lore §1.1 Mạch Ngầm; §1 nguyên tắc 3 (máy móc người xưa gợi lối); §4 vật tương tác viền amber nhạt khi lại gần.

**Generation Prompt:** `old underground power cables broken by roots and water, oversized rusty lever, square metal junction box with lid, faint warm light running along a reconnected cable, hand-painted isometric puzzle props` — negative: `sci-fi neon, orange danger glow, text`

**Status:** Needed

## ASSET-029 — Lối tắt

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Shortcut_Ladder_01`, `SM_Shortcut_GateLatch_01`, `SM_Shortcut_RollerDoor_01`, prefab `P_Shortcut_[Name]` |
| Polycount | 300–1500 |

**Visual Description:** Thang sắt gấp thả xuống, cổng then cài chỉ mở từ một phía, cửa cuốn tôn của người xưa. Phía khóa nhìn rõ đây là lối (gợi bằng hình khối và ánh sáng), không biểu tượng.

**Art Bible Anchors:** brief (lối tắt mở từ phía bên kia); §1 nguyên tắc 3; §9 Tunic.

**Generation Prompt:** `folding iron ladder, gate with a latch on one side only, corrugated metal roller shutter door, rusted, hand-painted isometric props` — negative: `glowing outline, arrows, icons`

**Status:** Needed

## ASSET-030 — Đấu trường Spider King

| Field | Value |
|-------|-------|
| Category | 3D (bộ module + ánh sáng) |
| Naming | `SM_Arena_SpiderKing_Floor`, `SM_Arena_SpiderKing_Throne`, `SM_Arena_SpiderKing_SilkCurtain_01..03`, prefab `P_Arena_SpiderKing` |
| Polycount | ngai 2–5k; rèm tơ 300–1500 mỗi tấm |

**Visual Description:** Sân ga ngầm hoặc hầm tàu bỏ hoang, sàn gần như trống hoàn toàn. Viền đấu trường dựng đồ người xưa bị dùng sai mục đích: ghế chờ chồng thành ngai, biển báo tròn gãy làm "huy hiệu", tơ treo như rèm triều đình. Nền xanh đêm lạnh sâu; key ấm chỉ trên hiệp sĩ; nguồn "giả ấm" vàng-lục từ ngai. Ngưỡng của đấu trường bị che tới khi boss chết.

**Art Bible Anchors:** §2 đấu boss; §6 mật độ đấu trường; rest-point.md (Ngưỡng bị che).

**Generation Prompt:** `boss arena in an abandoned underground train platform, empty floor, throne made of stacked old waiting benches, broken round road sign as emblem, silk hanging like royal curtains along the edges, deep cold navy darkness, sickly yellow-green fake candle glow from the throne, hand-painted isometric` — negative: `gold, warm amber light, clutter on floor, readable text`

**Status:** Needed

## ASSET-031 — Decal dấu lối và dấu an toàn

| Field | Value |
|-------|-------|
| Category | Decal / 3D |
| Naming | `M_Decal_MossTrampled`, `SM_Prop_ClothMarker_Post_01` |

**Visual Description:** Vệt rêu bị dẫm (báo có lối); mảnh vải vá buộc trên cọc gỗ (cư dân từng qua, gần chỗ an toàn).

**Art Bible Anchors:** §6 từ vựng lặp lại.

**Generation Prompt:** `top-down decal of trampled moss path; wooden stake with a colorful patched cloth tied on it, hand-painted` — negative: `arrows, glowing`

**Status:** Needed

## ASSET-032 — VFX môi trường

| Field | Value |
|-------|-------|
| Category | VFX |
| Naming | `vfx_env_sunshaft`, `vfx_env_dust_pollen`, `vfx_env_falling_leaf` |
| Ràng buộc | ≤ 50 hạt mỗi hiệu ứng; overdraw ≤ ~2 lớp |

**Visual Description:** Tia nắng ấm hẹp xuyên tán (mesh card mềm, không bloom gắt) rơi lên lối ẩn; bụi phấn hoa trôi chậm trong vệt sáng; lá rơi thưa.

**Art Bible Anchors:** §2 khám phá; §1 nguyên tắc 3 (vệt sáng dẫn mắt).

**Generation Prompt:** `soft narrow warm sunlight shaft through forest canopy, floating pollen motes, sparse falling leaves, vfx sprites grayscale alpha` — negative: `god rays everywhere, lens flare`

**Status:** Needed

## ASSET-033 — VFX tàn ý

| Field | Value |
|-------|-------|
| Category | VFX |
| Naming | `vfx_remnant_drift` (từ quái chết bay vào người), `vfx_remnant_pile` (chỗ rơi khi chết), `vfx_remnant_pickup` |
| Ràng buộc | ≤ 50 hạt; bloom ~1.5 |

**Visual Description:** Đốm kem-vàng nhỏ có đuôi bay lên rồi hút về hiệp sĩ. Chỗ rơi khi chết: đốm tụ thành cột sáng mảnh kem-vàng thấy từ xa, kèm âm khi gần, không icon UI (đề xuất UX; chốt trong GDD Tàn ý rơi).

**Art Bible Anchors:** §4 tàn ý khác amber (lớn, đứng yên) và cam (sắc, nháy); §7 đánh dấu chỗ rơi.

**Generation Prompt:** `small cream-gold motes with short tails drifting upward, thin soft light pillar of motes, vfx sprites` — negative: `orange, coins, sparkle stars`

**Status:** Needed
