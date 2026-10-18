# Asset Specs — Character: Hiệp sĩ chó

> **Source**: design/narrative/characters/hiep-si-cho.md, design/gdd/weapons.md
> **Art Bible**: design/art/art-bible.md
> **Generated**: 2026-10-18
> **Status**: 6 assets specced / 0 approved / 0 in production / 0 done
> **Review**: art-director và technical-artist không được gọi (Solo mode). Spec soạn từ luật art bible; ràng buộc kỹ thuật chưa được technical-artist kiểm.

## ASSET-001 — Hiệp sĩ: mesh và rig

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SK_Char_Knight` (FBX), prefab `P_Char_Knight` |
| Polycount | 6–10k tam giác (§8, một asset chung gameplay và menu) |
| Material | 1 slot `M_Char_Knight` (vũ khí tách, ASSET-003) |
| Rig | ≤ 75 bone deform, ≤ 4 weight/vertex; chuỗi bone khăn quàng 4–6 bone; bone tai trái/phải riêng (xoay dựng/cụp/nghiêng); bone đuôi 2–3 |
| Socket | `Socket_Back_Sword`, `Socket_Back_Bow`, `Socket_Hip_Shield`, `Socket_Hand_R`, `Socket_Hand_L`, `Socket_Head_Fx` (dấu "!") — gán qua `[SerializeField]` |
| Pivot / scale | giữa hai chân, 1 unit = 1 m, +Z mặt trước |

**Visual Description:** Chó nhỏ dáng chó rừng, đầu tròn to chiếm ~40% chiều cao, hai tai nhọn rất to dựng thẳng là khối nhô nổi nhất của silhouette. Thân bầu ngắn, chân ngắn, đuôi rậm vừa. Khăn quàng sẫm dài quấn cổ, đuôi khăn thả ra sau; áo chẽn xanh ngắn tới hông, dây da chéo ngực từ vai phải xuống hông trái giữ kiếm chéo lưng.

**Art Bible Anchors:**
- §3: đầu tròn + tai là "tam giác cảnh giác" duy nhất của phe sống.
- §5: thứ tự nhận diện tai → khăn → áo chẽn → kiếm; tai đổi hình đủ lớn để đọc ở 30 px; tối đa ~4 mảng màu chính.
- §8: chung asset menu; chỉ làm biến thể `_Menu` nếu test cận thấy thiếu.
- Chưa kiểm: chiều cao thật trong scene (ước tính ~0,8 m). Đo trước khi khóa tỉ lệ.

**Generation Prompt (concept sheet):**
`character turnaround sheet, small jackal-like dog knight, chibi proportions, oversized round head, very large upright pointed ears, golden-brown fur, cream muzzle and chest, big round black eyes, long dark scarf trailing behind, short blue tunic, leather strap across chest, simple cloth-wrapped sword on back, hand-painted texture style, large brush strokes, warm key light from top-left, front side back views, neutral grey background, readable silhouette` — negative: `fox, green tunic, shield on arm, anime cel shading, comic ink outline, realistic fur, photoreal, PBR, armor plates, text`

**Status:** Needed

## ASSET-002 — Hiệp sĩ: texture albedo

| Field | Value |
|-------|-------|
| Category | Texture |
| Naming | `T_Char_Knight_A` |
| Texture Res | 1024² (§8, ~800 px/m) |
| Format | nguồn `.kra/.psd` ở `SourceArt/Characters/Knight/`, export `.png` 8-bit, BC7, sRGB, mip Kaiser |

**Visual Description:** Vẽ tay nét cọ lớn. Lông nâu mật #A56F34 sáng dần lên nâu vàng ở đỉnh đầu và mép trên tai; mõm, má, ngực kem #F2E4C4 (điểm sáng nhất). Khăn quàng là điểm tối nhất, gần xanh đêm #22313F pha nâu; áo chẽn là xanh đêm sáng hơn. AO và bóng vẽ nhẹ dưới cằm, dưới khăn; không vẽ hướng sáng mạnh.

**Art Bible Anchors:**
- §4: nâu mật = màu "nhà"; kem mõm; xanh áo chẽn tách khỏi bóng.
- §5: mõm kem sáng nhất, khăn tối nhất; mắt đen tròn không tròng, không ánh phản chiếu ở gameplay.
- §6/§9: albedo vẽ tay kiểu Blizzard, không lông chi tiết dưới ~4 px màn hình.

**Generation Prompt:** `hand-painted texture atlas for a stylized dog character, golden-brown fur painted in broad strokes, cream muzzle and chest, dark navy scarf, muted blue tunic, brown leather strap, soft painted ambient occlusion, no harsh directional light, flat UV layout` — negative: `photo texture, normal map, noisy fur strands, high contrast lighting, text`

**Status:** Needed

## ASSET-003 — Vũ khí: Kiếm, Cung, Khiên

| Field | Value |
|-------|-------|
| Category | 3D |
| Naming | `SM_Wpn_Sword_01`, `SM_Wpn_Bow_01`, `SM_Wpn_Shield_01`; texture chung `T_Wpn_Knight_A` |
| Polycount | kiếm ≤ 400, cung ≤ 600, khiên ≤ 600 |
| Texture Res | 512² dùng chung cho cả ba |
| Material | 1 slot `M_Wpn_Knight` |
| Pivot | ở chuôi/tay cầm, khớp `Socket_Hand_R/L` |

**Visual Description:** Đồ nhặt và vá của cư dân, không phải vũ khí hiệp sĩ hoàng gia. Kiếm thẳng ngắn, lưỡi bê tông lạnh mòn, chuôi quấn vải nâu; cung gỗ cong một miếng, dây buộc vải; khiên tròn làm từ nắp kim loại cũ của người xưa, viền buộc dây, một mảng vá vải.

**Art Bible Anchors:**
- §3: khiên tròn (phe sống), lưỡi kiếm là vạch chéo cố định trên lưng.
- §6: vật người xưa được cư dân dùng lại (nắp kim loại, vải buộc).
- §5: không hoa văn kiếm (không sống sót ở khoảng cách camera).

**Generation Prompt:** `prop sheet, three small hand-made weapons for a tiny dog knight: short straight sword with cloth-wrapped grip, simple curved wooden bow tied with cloth, round shield made from an old metal lid with rope rim and a fabric patch, hand-painted texture, muted steel grey, warm brown wood, side and front views` — negative: `ornate engraving, gold, runes, glowing blade, fantasy spikes`

**Status:** Needed

## ASSET-004 — Hiệp sĩ: bộ animation

| Field | Value |
|-------|-------|
| Category | Animation |
| Naming | `AN_Knight_[Action]` (vd `AN_Knight_Idle`, `AN_Knight_Roll`); controller `AC_Knight` |
| Format | FBX chỉ animation, 30 fps nguồn, root motion tắt trừ khi gameplay yêu cầu |

**Danh sách clip:**
- Di chuyển: `Idle`, `Idle_Sniff` (nghiêng đầu đánh hơi), `Run`, `Run_Tired` (khăn rủ, stamina cạn), `Roll` (co tròn người), `Hit`, `Death`, `Respawn` (đứng dậy ở Ngưỡng), `Rest_Sit`.
- Chém: `Sword_Light_01..03`, `Sword_Heavy`; anticipation rõ, follow-through của khăn và tai.
- Cung, khiên: `Bow_Aim`, `Bow_Release`, `Shield_Block`, `Shield_Parry`.
- Tai (layer additive riêng): `Ears_Up`, `Ears_Down`, `Ears_Tilt_L`, `Ears_Tilt_R`.
- Biểu cảm: `Pickup_Bone_Happy` (vẫy đuôi), `Awe` (trầm trồ), `Hesitate_Dark` (chần chừ trước chỗ tối).

**Art Bible Anchors:**
- §5: biểu cảm cao; né là co tròn, không dáng ngầu; tai là kênh cảm xúc chính.
- §7: tai cụp + thở dốc khi còn 1 tim chỉ là tín hiệu phụ.

**Generation Prompt:** không áp dụng (animation). Tham chiếu dáng: pose sheet vẽ tay theo ASSET-001.

**Status:** Needed

## ASSET-005 — Bong bóng "!" đánh hơi

| Field | Value |
|-------|-------|
| Category | UI (world-space) |
| Naming | `UI_Bubble_Sniff`, prefab `P_Ui_SniffBubble` |
| Dimensions | ~28 px trên màn hình 1080p; vẽ 2× (56 px), Sprite Atlas UI |
| Format | `.png`, BC7, không mip |

**Visual Description:** Bong bóng tròn nền kem, viền nâu mật đều 3 px, dấu "!" nâu mật đậm ở giữa; không đuôi chỉ hướng. Hiện trên `Socket_Head_Fx`, nảy OutBack 0,2 giây, kèm âm.

**Art Bible Anchors:**
- §7: thông tin, không phải cảnh báo → hình tròn, kem, không cam; hiện cả khi HUD mờ.
- §1 nguyên tắc 3: chỉ báo "có gì đó", không chỉ đường.

**Generation Prompt:** `flat UI icon, round cream speech bubble with a bold brown exclamation mark, even outline, no tail, no gradient, no shadow` — negative: `orange, red, triangle, arrow, glow`

**Status:** Needed

## ASSET-006 — Material viền xuyên tường của hiệp sĩ

| Field | Value |
|-------|-------|
| Category | Material / Shader |
| Naming | `M_Fx_Occlusion_Knight` (dùng với pass RenderObjects `PlayerHidden` hiện có) |
| Ràng buộc | SRP Batcher compatible; 1 draw thêm khi bị che |

**Visual Description:** Silhouette đặc màu kem #F2E4C4 alpha ~60%, viền sáng hơn một bậc, không HDR, không nhấp nháy.

**Art Bible Anchors:**
- §5: hiệp sĩ kem, quái cam đục; NPC không xuyên.
- §1 nguyên tắc 1: tường luôn đặc, chỉ nhân vật được "xuyên".

**Generation Prompt:** không áp dụng (shader).

**Status:** Needed
