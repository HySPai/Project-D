import base64, json, sys, time, urllib.request

KEY, OUT = sys.argv[1], sys.argv[2]
REFS = [r"D:\Git\Project-D\design\art\style\style-ruins-fox-knight.webp",
        r"D:\Git\Project-D\design\art\style\character-sheet-fox-knight.webp"]
API = "https://api.meshy.ai/openapi/v1/image-to-image"

def data_uri(p):
    return "data:image/webp;base64," + base64.b64encode(open(p, "rb").read()).decode()

STYLE = ("In-game screenshot from a real-time 3D isometric action-adventure game (soulslike, metroidvania), 16:9. "
         "Use the FIRST reference image as the art style target: soft stylized 3D with painterly matte textures, large soft colour patches, "
         "rounded mossy stone ruins, warm sunlight slanting through haze, warm greens, gentle atmospheric depth. "
         "Use the SECOND reference image for the hero's exact design: small anthropomorphic jackal-like dog knight with huge upright pointed ears, "
         "beige-brown fur, cream face and LONG pointed muzzle with black nose, big white oval eyes with vertical black pupils, "
         "dark maroon hooded scarf and tattered maroon cape, short light-blue tunic, brown leather belt with square buckle and diagonal chest strap. "
         "High fixed isometric camera looking down at about 45 degrees, no horizon, the hero is small (about 8% of image height) inside a large readable play space. "
         "World: humans are long extinct; their oversized square concrete and rusty metal ruins are being swallowed by trees, roots, moss and vines; "
         "evolved cute animals now live here, and mindless insect monsters roam. "
         "Minimal flat 2D HUD overlay: top-left a row of four small dark-red fabric hearts with cream outlines, a thin segmented stamina bar beneath; "
         "bottom-right a small cream-gold mote icon with a number. No title, no watermark, no readable signs, no humans. ")

SCENES = {
    "01-explore-ruins": STYLE + (
        "Scene: calm exploration. The hero walks along a mossy stone path between overgrown human ruins: a toppled rusty railway car tilted by giant tree roots, "
        "a cracked concrete underpass arch half buried in moss, an oversized old bench sinking into grass. A narrow warm shaft of sunlight falls on a hidden side path "
        "behind a pillar, hinting at a secret. Sword sheathed diagonally on his back, round wooden shield on his left arm. Pollen drifts in the light. No enemies."),
    "02-combat-insects": STYLE + (
        "Scene: tense souls-like fight on a cracked concrete plaza of a ruin at the forest edge, background slightly darker and cooler. "
        "The hero is mid-swing with a chunky short steel sword with square pommel and flat crossguard in his right hand, a round wooden shield with metal rim on his left arm, "
        "cape and scarf flying, a pale cream slash arc trailing the blade, small white hit flash with orange sparks. "
        "Three insect monsters about his size: six-legged, angular segmented cold blue-grey chitin, spiky legs, no face, each with exactly one small glowing orange eye point; "
        "one recoils from the hit, one crouches with a forelimb raised as a telegraphed attack wind-up with its orange point blazing, one creeps in from the edge. "
        "Small dark matte splatters, dust puff at the hero's feet. HUD fully opaque, one heart empty, two stamina segments spent."),
    "03-threshold-rest": STYLE + (
        "Scene: quiet rest point called a Threshold, evening. A small abandoned human bus shelter of concrete and steel, roof sagging and patched with stitched cloth, "
        "overgrown with moss and flowers, sits in a forest clearing among ruins. An old lantern hangs at its center glowing steady warm amber, casting a soft round pool of light "
        "that fades into cool blue dusk. The hero sits on an oversized concrete bench beside the lantern, ears relaxed, sword and round shield resting against the bench. "
        "Tiny cream-gold motes drift upward from the lantern. HUD hidden except the bottom-right mote counter. No bonfire, no enemies."),
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
