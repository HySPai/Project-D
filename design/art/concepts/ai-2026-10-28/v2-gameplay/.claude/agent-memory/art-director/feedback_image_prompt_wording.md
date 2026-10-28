---
name: image-prompt-wording
description: Lessons for Gemini/GPT concept-image prompts in Project-D — wording that made outputs look like illustrations or posters instead of gameplay screenshots
metadata:
  type: feedback
---

For text-only image prompts meant to look like real gameplay, never use "hand-painted", "painting", "concept art" or "isometric art". Describe textures as "matte stylized textures made of large soft colour patches", and require a tiny hero (~6% of image height), a flat HUD in the art-bible §7 corners, an orthographic 45° camera, and mostly empty floor.

**Why:** Round 1 (2026-10) came out as illustrations. Round 2 had the iso camera but "did not look like a real game being played" (big centered hero, no HUD). The user's references are Tunic and Death's Door gameplay frames.

**How to apply:** Any concept-image prompt work. The current prompt set is in design/art/concepts/gameplay-concept-prompts.md. Monsters stay generic "insect monsters", never spiders or named demo monsters (user decision 2026-10-28).
