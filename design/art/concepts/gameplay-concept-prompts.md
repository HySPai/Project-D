# Prompt concept ảnh gameplay (vòng 3)

> **Owner**: art-director · **Ngày**: 2026-10-28 · **Trạng thái**: Draft, chờ người dùng chạy thử
> **Nguồn**: `design/art/art-bible.md` (§2, §4, §5, §6, §7, §9), `design/game-brief.md`, `design/narrative/characters/hiep-si-cho.md`, `design/narrative/lore/world-foundation.md` (§1, §4.0), `design/assets/specs/vung-1-assets.md`, `design/assets/specs/lang-mai-va-assets.md`, và 12 ảnh tham chiếu người dùng gửi ngày 2026-10-28.
> **Dùng cho**: Gemini / GPT image (chỉ prompt chữ, không đưa ảnh vào). Ghép **Style block** (mục 2) + **một prompt cảnh** (mục 3) thành một prompt duy nhất.
> **Quyết định người dùng**: quái hiện có (Spider, Spider King, Seed, Turret) là demo. Prompt chỉ dùng "insect monster" chung theo luật art bible. Không nhện, không tên quái.

---

## 0. Vì sao hai vòng trước hỏng (chẩn đoán)

- **Vòng 1 ra tranh minh họa**: prompt có chữ "hand-painted", "concept", "isometric art". Model ảnh hiểu "hand-painted" là *tranh vẽ*, không hiểu là *texture vẽ tay trên model 3D*. Từ vòng này, prompt **không dùng** "hand-painted", "painting", "art"; thay bằng "matte stylized textures made of large soft colour patches".
- **Vòng 2 có góc iso nhưng chưa giống đang chơi**: ảnh thiếu những dấu hiệu chỉ có ở game đang chạy:
  - nhân vật quá to và bị đặt giữa khung như poster;
  - không có HUD;
  - sàn không trống, chỗ nào cũng có chi tiết;
  - không có bóng realtime đổ đúng một hướng.
  Ảnh tham chiếu thì ngược lại: hero ~4–6% chiều cao ảnh, sàn trống là "sân chơi", HUD phẳng đè lên, prop dồn ra mép.

## 1. Quy tắc render rút từ ảnh tham chiếu

Ghi chú tham chiếu: T = Tunic (ảnh 3, 4, 6, 7, 8, 11, 12) · DD = Death's Door (5, 9, 10) · R = game rừng ảnh 1–2.

| # | Quy tắc thấy được | Thấy ở | Khớp art bible | Ghi chú / xung đột |
|---|---|---|---|---|
| 1 | **Camera**: chiếu trực giao (orthographic) hoặc gần trực giao, nhìn xuống ~45°, xoay 45°; cạnh song song, không có đường chân trời, không thấy trời. | T, DD | §2 key khớp iso 45°, §5 ortho 4.5 | Khớp. R dùng phối cảnh thấp hơn, không lấy. |
| 2 | **Cỡ hero**: hero chiếm ~5–7% chiều cao ảnh và nằm trong vùng giữa 50%, lệch tâm nhẹ, không đứng chính giữa kiểu poster. | T (~5%), DD (~4%) | §5: hiệp sĩ ≈ 65–70 px / 1080 ≈ 6% | Khớp. Đây là dấu hiệu "đang chơi" quan trọng nhất. |
| 3 | **Khung hình là sân chơi**: 60–80% khung là sàn đi được, phần lớn trống; prop tụm ở mép và góc. | T 3/11, DD 5/9 | §6 mật độ prop (~70% sàn giao tranh trống) | Khớp. |
| 4 | **Một nguồn sáng chính + gobo bóng lá**: một mặt trời realtime đổ bóng một hướng; bóng lá lốm đốm trên sàn; phần sáng ấm, bóng nghiêng lạnh. | T 7/8, DD 9, R 1–2 (tia sáng) | §2 key trên-trái, nắng xiên, tia sáng xuyên tán | Khớp. Tia sáng (god ray) chỉ dùng **một vệt** dẫn tới lối ẩn, không rải khắp khung. ASSET-032 đã cấm "god rays everywhere". |
| 5 | **Phòng cắt (cut-away) trong void tối**: hầm, nội thất, điểm nghỉ là một "khối diorama" mép vỡ, lơ lửng giữa nền gần đen. | T 6, DD 10 | §9: mỗi khung là một "phòng" đọc trọn | **Xung đột nhẹ** với §5–§6 (khăn hiệp sĩ là điểm tối nhất, môi trường không dùng cực tối). **Đề xuất**: void dùng xanh đêm rất tối #0E151C, không đen tuyệt đối; mép khối được chiếu sáng để hero không bao giờ đứng sát nền void. Chỉ dùng cho hầm, Ngưỡng, đấu trường; rừng và làng thì đầy khung. |
| 6 | **Hình khối**: model low-to-mid poly, khối to đơn giản, cạnh vát rõ; bóng đổ mềm có chuyển sắc; bề mặt là mảng màu lớn (vết loang trên đá kiểu T 3/11, rêu loang kiểu DD 9). Không nhiễu nhỏ. | T, DD | §3 khối lớn đơn giản; §6 mảng màu ≥ ~4 px | **Xung đột**: art bible §6 chốt texture vẽ tay toàn bộ, §9 tránh "low-poly màu phẳng"; còn brief ghi "low-poly stylized". **Đề xuất pha trộn**: silhouette low-to-mid poly (đọc rõ ở 65 px, rẻ cho dev một người), albedo vẽ tay nét cọ lớn ≥ 4 px (giữ chất ấm, kể chuyện của art bible). Death's Door chứng minh hướng này: hình khối đơn giản, texture loang mềm. Trong prompt ảnh, gọi là "matte stylized textures made of large soft colour patches" để model không vẽ thành tranh (xem mục 0). |
| 7 | **Độ sâu trường ảnh (DOF)**: mép trên và mép dưới hơi mờ; vật che tiền cảnh mờ. | T (mạnh), DD (nhẹ) | §9 Tunic: "không tilt-shift mạnh" | **Xung đột**: dải mờ mạnh của Tunic làm cảnh trông như mô hình đồ chơi và giảm độ đọc ở mép. **Đề xuất**: mờ nhẹ, chỉ ~8% trên và dưới. Như vậy vẫn đủ báo "đây là camera game" mà không thành tilt-shift. |
| 8 | **Hậu kỳ**: bloom chỉ trên nguồn phát sáng (đèn, điểm cam, đốm tàn ý); grain rất nhẹ; vignette nhẹ khi đánh; grading theo mood. | T, DD | §4 cam HDR > bloom 1.1, tàn ý bloom ~1.5; §2 vignette rất nhẹ khi đấu; §4 UI không bloom | Khớp. Không lens flare, không chromatic aberration. |
| 9 | **Bảng màu theo mood**: mỗi trạng thái đọc khác nhau ngay khi liếc (bảng dưới). | T 4 vs 6 vs 7 khác hẳn nhau | §2, §4 | T dùng tím/hồng cho bóng; ta **không** dùng tím, bóng nghiêng xanh lạnh (§4: xanh chỉ là bối cảnh). |
| 10 | **HUD**: 2D phẳng đè lên ảnh, bám góc, chừa trống giữa khung. | T: thanh lục giác dưới-trái + 3 ô tròn trên-phải; DD: ô thoi + chấm trắng trên-trái | **§7 thắng tham chiếu**: trên-trái tim/stamina/nguyên tố; dưới-phải tàn ý, mũi tên, Mảnh Sắt Cũ; đáy giữa thanh máu boss | **Khác tham chiếu**: bố cục của ta gần DD (cụm sống trên-trái) hơn T. Giữ §7. Model ảnh viết chữ hay sai, nên HUD chỉ có icon + số ngắn, không chữ. |
| 11 | **Đọc giao tranh**: máu là chấm và vệt sẫm đục trên sàn; vệt chém sáng mảnh; sàn giao tranh tương phản thấp. | DD 5/9 | §4 đỏ máu sẫm #8C1C22 không phát sáng; §5 đúng một điểm cam; §9 DD | Máu của DD đỏ tươi hơn, ta giữ #8C1C22 tối và đục. |
| 12 | **Khung tiền cảnh**: tán lá hoặc khối tối mờ chỉ chặn ở **một góc** khung, không bao giờ đè lên hero. | R 1–2, T 7 | §6 tán lá cao là khung, không che lối; §1 tường đặc | R che nhiều nửa dưới ảnh, ta không làm thế: chỉ một góc. |

**Bảng màu theo mood (dùng trong prompt cảnh)**

| Mood | Nhiệt | Màu chủ | Điểm nhấn |
|---|---|---|---|
| Khám phá rừng | ~4500K | rêu #5F8F4E, đất ấm, nâu mật #A56F34 | một vệt nắng amber #E9A23B |
| Giao tranh / tàn tích | ~6000K | bê tông #66747D, xanh đêm #22313F | điểm cam #FF5A1F, máu #8C1C22 |
| Ngưỡng | đèn amber trên nền tối | xanh đêm #22313F, void #0E151C | amber #E9A23B, lõi kem #F2E4C4 |
| Làng Mái Vá | 2700–3200K | amber, nâu mật, vải vá ấm | nhiều nguồn sáng nhỏ |
| Hầm Mạch Ngầm | ~6000K, tối | #66747D, #22313F, void | dòng amber nhạt dọc cáp |
| Đấu boss | lạnh sâu | xanh đêm #22313F | nguồn "giả ấm" vàng-lục của boss, telegraph cam #FF5A1F |

> Màu "giả ấm" vàng-lục (#B5B04A) lấy từ nháp ASSET-030. Art bible để màu boss mở; đây chỉ là giá trị tạm cho ảnh concept.

## 2. Style block (dán trước mọi prompt cảnh)

```text
In-game screenshot from a real-time 3D isometric action-adventure game, rendered in Unity URP, captured mid-play at 1920x1080, 16:9, rendering quality similar to Tunic and Death's Door. High fixed orthographic camera looking down at 45 degrees and rotated 45 degrees; parallel lines, no perspective, no horizon. Chunky low-to-mid-poly models with bevelled edges and matte stylized textures made of large soft colour patches. Real-time lighting with soft-edged shadows, gentle bloom on light sources, slight depth-of-field blur only along the top and bottom edges, faint film grain. The player character is tiny. A flat 2D HUD overlays the render. Not an illustration, painting, concept art, poster or key art; no brush strokes, no title, no watermark, no frame border, no fox, no crow.
```

**Đánh đổi khi nêu tên Tunic / Death's Door**: tên game kéo model về đúng chất render rất mạnh, nhưng có thể kéo theo con cáo hoặc con quạ. Vì vậy style block có "no fox, no crow". Nếu ảnh ra vẫn giống Tunic quá, bỏ cụm "rendering quality similar to Tunic and Death's Door".

## 3. Sáu prompt cảnh

Mỗi prompt đã có sẵn câu mô tả hero. Dán **Style block + một prompt** thành một khối.

### a) Vùng 1 — khám phá

```text
Scene: exploring a forest that has swallowed an old railway; calm, curious mood; about 4500K daylight. The hero is a tiny jackal-like dog knight, about 6% of image height: huge upright pointed ears, golden-brown fur #A56F34, cream muzzle, long very dark navy scarf, short muted blue tunic, sword strapped diagonally across his back. He walks slightly left of center along an oversized rusty railway track running diagonally from lower left to upper right; its rotten wooden sleepers reach his knees and thick tree roots bend the rails upward. Upper center: an abandoned square steel passenger railcar lifted and tilted about 20 degrees by the buttress roots of a giant tree, empty broken windows, vines down one side, moss on the roof. Warm sun from the upper left throws dappled leaf-shadow patterns across the ground; shadows are cool, pale blue-green. One narrow warm shaft of light #E9A23B falls between the railcar and a solid concrete wall onto a strip of trampled moss, the only hint of a hidden path. Ground: warm reddish soil, low warm-green moss #5F8F4E, a few small wildflowers, a human-sized wooden chair sinking into the grass with a flower growing from a mug on its seat. Pollen drifts in the light shaft. A dark out-of-focus canopy frames the top-left corner only. HUD faded to 40% opacity, top-left corner only: a row of four small dark-red #8C1C22 patched-fabric hearts with cream outlines, a thin segmented stamina bar beneath, a short element bar with a round icon beneath that. Avoid: enemies, text, signs, glowing outlines, arrows, quest markers, purple shadows.
```

### b) Giao tranh thường

```text
Scene: a normal fight on the cracked concrete plaza of a ruin at the forest edge; cooler daylight around 6000K; background darkened and desaturated toward slate grey #66747D and navy #22313F, with a very light vignette. The hero is a tiny jackal-like dog knight, about 6% of image height: huge upright pointed ears, golden-brown fur #A56F34, cream muzzle, long very dark navy scarf, short muted blue tunic, leather chest strap. He stands just left of center mid-swing, sword in hand, scarf flying; a thin pale cream slash arc trails from the blade, and at the impact point there is a small white hit flash with a few sharp orange #FF5A1F sparks. A warm key light stays on him. Three insect monsters, each about his size or slightly bigger: six-legged, angular, segmented cold blue-grey chitin, spiky legs radiating outward, heavy upper body, no face, no clothing, each with exactly one small glowing orange #FF5A1F eye point. One is being hit and recoils. One at upper right crouches low with a forelimb raised high, its orange point enlarged and blazing as an attack wind-up. One creeps in from the lower right. Dark matte blood #8C1C22 sprays as small droplets and lies in a few non-glowing splatters on the concrete. Small dust puff at the knight's feet. The floor is mostly empty: moss in the cracks, a toppled oversized concrete bench at the edge, pale insect web in one dark corner. HUD fully opaque: top-left four patched-fabric hearts with cream outlines (the last one an empty outline), a segmented stamina bar with two segments spent, an element bar; bottom-right a cream-gold mote icon with the number 248, above it an arrow icon with 12 and a worn bolt icon with 3. Avoid: spiders, eight legs, bright red blood, gore, text.
```

### c) Ngưỡng — điểm nghỉ

```text
Scene: resting at a save point called a Threshold; relief, quiet, very low energy. The location is a cut-away diorama: a small square slab of forest ground with irregular broken edges, floating in near-black navy void #0E151C, with nothing visible beyond it. Two tree trunks rise at the back edge and are cut off by the frame. On the slab stands a small abandoned bus shelter of the old humans: a square concrete-and-steel frame whose roof sags in the middle and is patched with stitched cloth pieces. Hanging at its center is an old lantern glowing a steady amber #E9A23B with a cream #F2E4C4 core. It casts a round, soft pool of warm light on the floor that falls off into cold blue-black #22313F before the slab edges, so the circle reads like a room. Inside the circle: an oversized concrete bench, a folded patched cloth and a small cup left by earlier travellers. The hero, a tiny jackal-like dog knight about 6% of image height (huge upright pointed ears, golden-brown fur #A56F34, cream muzzle, very dark navy scarf, muted blue tunic, sword on his back), sits on the bench edge beside the lantern, slightly right of center, ears relaxed. Tiny cream-gold motes drift upward from the lantern like glowing ash. Soft contrast; the lantern is the only light source. HUD: everything hidden except the bottom-right corner, which shows a cream-gold mote icon with the number 1024 in rounded cream digits. Avoid: bonfire, sword stuck in the ground, flickering flame, runes, magic circles, sky, text, enemies.
```

### d) Làng Mái Vá

```text
Scene: a patchwork refugee village built inside an old human train station, at evening; warm 2700–3200K light from many small sources, soft shadows, low-to-medium contrast, cozy and busy. A long concrete platform runs diagonally through the frame, with old railway carriages parked along both sides as homes. They have round wooden doors cut into their steel sides, windows patched with planks, thin chimney pipes with wisps of smoke, an oil lamp at each carriage end and firelight spilling out of the open doors. The broken station roof along the edge is patched with stretched fabric in many warm muted colours, with big visible stitches. Clotheslines of patched laundry hang between the carriages. Center: a communal shrine built from an old ticket counter, covered in candles of many sizes and small offerings, with a steady amber lantern #E9A23B in the middle as the brightest warm spot. The hero, a tiny jackal-like dog knight about 6% of image height (huge upright pointed ears, golden-brown fur #A56F34, cream muzzle, very dark navy scarf, muted blue tunic, sword on his back), stands just below the shrine looking up. Small, round, chubby anthropomorphic villagers wearing patched cloth, all about the knight's size: a goat elder with a staff sitting on a chest, a squirrel hugging a seed bag, a hedgehog smith at an oil-drum forge, a rabbit herbalist, a mouse with a huge backpack and coiled cable, a duck holding a map scroll, a bear with a wooden hind leg by the gate. Clear empty walkway down the middle. Far end: the rails fade into cold dark blue. HUD: top-left hearts, stamina and element bars at 40% opacity; bottom-right a cream-gold mote icon with 1024, above it an arrow icon with 12 and a bolt icon with 3. Avoid: humans, readable signs, neon, enemies, weapons on villagers.
```

### e) Giải đố Mạch Ngầm

```text
Scene: an underground maintenance tunnel of the old humans; cold 6000K, dim and quiet. The room is shown as a cut-away diorama floating in near-black navy void #0E151C, with broken concrete edges and no ceiling. Square concrete walls carry rust streaks and a lighter top edge; roots push through the cracks, and a shallow groundwater puddle lies on the floor. Thick old power cables run along the floor and walls. One was torn apart by a root, and its frayed ends have just been clamped back together. Right side: an oversized rusty lever, twice the knight's height, on a square metal junction box with an open lid. The hero, a tiny jackal-like dog knight about 6% of image height (huge upright pointed ears, golden-brown fur #A56F34, cream muzzle, very dark navy scarf, muted blue tunic, sword on his back), hangs from the lever handle with his whole weight, feet off the floor, scarf swinging. The lever has just been thrown. A faint warm amber glow #E9A23B travels as a soft pulse along the reconnected cable, from the junction box across the floor to a corrugated metal roller shutter in the upper-left wall. The shutter has lifted a few centimetres, warm light leaks out beneath it, and dust falls from it. Everything else stays cold slate #66747D and navy #22313F; the amber pulse and a pale amber rim on the lever are the only warm points. HUD faded to 40% opacity, top-left only: four patched dark-red hearts with cream outlines, a segmented stamina bar, an element bar. Avoid: orange danger glow, neon, blue sci-fi electricity, sparks, enemies, text, icons, arrows.
```

### f) Boss Vùng 1

```text
Scene: boss fight in an abandoned underground train platform; menace, peak tension, maximum contrast. The arena is a wide cut-away slab floating in near-black navy void #0E151C. The floor is almost completely empty, deep cold navy #22313F. Around the edges, objects of the old humans are misused as a royal court: waiting-room benches stacked into a crooked throne at the back, a broken round road sign mounted above it like an emblem, tattered umbrellas propped up like banners, pale insect membranes draped like curtains. A sickly yellow-green "fake warm" glow #B5B04A comes from the throne. Upper center: a huge insect boss about five times the knight's height, standing upright like a mocking monarch, wearing an old human object as a crown. Its body is angular, segmented, cold blue-grey chitin, with many spiky limbs, and its exact shape is half lost in shadow. Several dim orange points mark its body, but only one, its chest core, blazes bright orange #FF5A1F. It rears back with both blade-like forelimbs raised as an attack wind-up, casting an orange wedge of light across the floor toward the knight. The hero, a tiny jackal-like dog knight about 6% of image height (huge upright pointed ears, golden-brown fur #A56F34, cream muzzle, very dark navy scarf, muted blue tunic), stands lower left with sword drawn, lit by a warm rim light. A few dark matte blood drops #8C1C22 lie on the floor. HUD fully opaque: top-left three patched hearts with cream outlines, segmented stamina bar, element bar; bottom-right mote icon with 248; bottom center a long thin boss health bar with dark-red fill and cream outline, no name text. Avoid: spiders, gold, warm amber light on the boss, clutter on the floor, readable text.
```

## 4. Cách chạy và kiểm

- Đặt tỉ lệ ảnh **16:9** trong tool, nếu tool có tùy chọn này.
- Ảnh ra cần qua 5 phép thử:
  1. Hero cao chừng 1/15–1/20 ảnh.
  2. Có HUD đúng góc.
  3. Không có đường chân trời.
  4. Bóng đổ một hướng.
  5. Thu ảnh xuống 25% vẫn thấy được hero và điểm cam.
- Nếu ảnh vẫn ra kiểu tranh: bỏ cụm "Tunic and Death's Door", thêm "PC game capture, gameplay footage frame" ở đầu.
- Nếu hero quá to: thêm "the hero occupies less than 7% of the image height; most of the frame is empty walkable floor".
- Gemini hay bỏ qua phủ định. Nếu vẫn ra nhện, xóa chữ "spiders" khỏi dòng Avoid (vì chính chữ đó có thể gợi ra nhện) và giữ "six-legged".
- Ảnh concept chỉ để chốt cảm giác render. Màu, tỉ lệ và luật cuối cùng vẫn theo art bible.
