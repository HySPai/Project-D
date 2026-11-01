import base64, json, sys, time, urllib.request

KEY, OUT = sys.argv[1], sys.argv[2]
REFS = [r"D:\Git\Project-D\design\art\style\style-ruins-fox-knight.webp",
        r"D:\Git\Project-D\design\art\style\character-sheet-fox-knight.webp"]
API = "https://api.meshy.ai/openapi/v1/image-to-image"

def data_uri(p):
    return "data:image/webp;base64," + base64.b64encode(open(p, "rb").read()).decode()

STYLE = ("In-game screenshot of a real-time 3D isometric action-adventure game (soulslike + metroidvania), 16:9. "
         "Level design follows the layout principles of Tunic and Death's Door: a fixed high isometric camera at about 45 degrees with near-parallel lines and no horizon; "
         "each screen is a compact handcrafted diorama 'room' that can be read at a glance; the playable area is a cut-away slab of terrain with broken edges "
         "floating in soft dark void, so walls never block the view; clear walkable floor, readable elevation tiers connected by stairs, ladders and ramps; "
         "secrets and alternate routes are hinted only by shape and light, never by markers. "
         "Use the FIRST reference image as the art style target: soft stylized 3D with painterly matte textures, large soft colour patches, rounded mossy stone, "
         "warm sunlight through haze, warm greens. Use the SECOND reference image for the hero: small jackal-like dog knight, huge upright pointed ears, beige-brown fur, "
         "cream face with a LONG pointed muzzle, maroon scarf and cape, light-blue tunic, brown belt, sword and round wooden shield. "
         "The hero is tiny, about 5% of image height. "
         "World: long-extinct humans left oversized square concrete and rusty steel ruins, railways and machines powered by broken underground cables; "
         "trees, roots and moss have swallowed them; mindless insect monsters roam. "
         "Minimal 2D HUD: top-left four small dark-red fabric hearts and a thin segmented stamina bar; bottom-right a small cream-gold mote counter. "
         "No text labels, no arrows, no quest markers, no minimap, no humans, no watermark. ")

SCENES = {
    "01-zone-hub-diorama": STYLE + (
        "Scene: wide view of one interconnected forest-ruin zone as a single large diorama with three height tiers. "
        "Bottom tier: the rest point (an old bus shelter with a glowing amber lantern) beside a mossy plaza where the hero stands. "
        "Middle tier, reached by a long stone staircase: a ruined railway platform with a tilted overgrown train car and a locked rusty iron gate that clearly leads back down "
        "to the rest point as a future shortcut. Top tier: a broken concrete bridge with a gap too wide to jump, a ledge visible but unreachable for now. "
        "A narrow path hides behind a thick tree canopy at one edge, revealed only by a thin warm shaft of light on trampled moss. "
        "A few small insect monsters patrol the middle tier. Calm daylight, strong readable silhouettes of paths."),
    "02-combat-arena-room": STYLE + (
        "Scene: a compact combat arena room in the style of Death's Door room design: a square sunken concrete courtyard of an old human building, "
        "cut-away walls on the near side, floating in dark navy void, one entrance at the lower left where the hero enters and one exit door at the upper right "
        "sealed by a corrugated metal roller shutter that opens only after the fight. Four broken square pillars give cover, roots and moss on the edges, "
        "the center floor is open and empty for dodging. Insect monsters crawl out of cracks in the walls with small glowing orange eye points; "
        "cooler, higher-contrast lighting with a warm key light on the hero."),
    "03-shortcut-reveal": STYLE + (
        "Scene: the satisfying shortcut moment. The hero stands on a high rusted steel walkway and has just kicked down an old human maintenance ladder; "
        "it drops to the lower level and connects back to the familiar rest point bus shelter with its amber lantern, visible below and close by. "
        "The path the hero took to get up here winds far around the diorama, making the loop obvious. Dust falls from the ladder, warm evening light, "
        "the lower area slightly darker. No enemies."),
    "04-machine-puzzle-room": STYLE + (
        "Scene: an environmental puzzle room inside an underground human maintenance tunnel, shown as a cut-away diorama in dark void. "
        "Thick old power cables run along the floor; one is torn apart by tree roots. A heavy square concrete block sits on rails and can be pushed onto a large pressure plate; "
        "an oversized rusty lever on a junction box stands nearby, and a dormant human turret machine sleeps in an alcove watching the walkway. "
        "A faint warm amber pulse runs along the reconnected half of the cable toward a closed roller shutter. Cold dim slate and navy light, the amber pulse "
        "is the only warm accent. The hero is pushing the block with both paws."),
}

def call(url, body=None):
    req = urllib.request.Request(url, data=json.dumps(body).encode() if body else None,
                                 headers={"Authorization": f"Bearer {KEY}", "Content-Type": "application/json"})
    return json.load(urllib.request.urlopen(req))

refs = [data_uri(p) for p in REFS]
tasks = {}
for name, prompt in SCENES.items():
    r = call(API, {"ai_model": "nano-banana-2", "prompt": prompt, "reference_image_urls": refs, "aspect_ratio": "16:9"})
    tasks[name] = r["result"]
    print("submitted", name, r["result"], flush=True)

pending = dict(tasks)
while pending:
    time.sleep(8)
    for name, tid in list(pending.items()):
        j = call(f"{API}/{tid}")
        if j["status"] in ("SUCCEEDED", "FAILED", "CANCELED"):
            print(name, j["status"], "credits", j.get("consumed_credits"), j.get("task_error"), flush=True)
            for i, u in enumerate(j.get("image_urls") or []):
                path = f"{OUT}/{name}{'' if i == 0 else '-' + str(i)}.png"
                urllib.request.urlretrieve(u, path)
                print("  saved", path, flush=True)
            del pending[name]
