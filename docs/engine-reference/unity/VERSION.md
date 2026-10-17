# Unity — Version Reference

Last verified: 2026-10-15

| Field | Value |
|-------|-------|
| **Engine Version** | 6000.4.1f1 (Unity 6.4) |
| **Project Pinned** | 2026-10-15 |
| **Last Docs Verified** | 2026-10-15 |
| **Installed at pin time** | 6000.4.1f1 — match (Hub editor present; `ProjectSettings/ProjectVersion.txt` = 6000.4.1f1 (336a400b9ea2)) |
| **LLM Knowledge Cutoff** | May 2025 (framework table; Unity coverage ~2022.3 LTS) |
| **Risk Level** | HIGH — version is beyond LLM training data |

## Post-cutoff timeline

| Version | Released | Source | Covered here |
|---------|----------|--------|--------------|
| 6000.0 – 6000.2 | NOT SOURCEABLE — not fetched this run | — | No |
| 6000.3 | NOT SOURCEABLE — release date not fetched | [Upgrade guide 6.3](https://docs.unity3d.com/6000.4/Documentation/Manual/UpgradeGuideUnity63.html) | Yes (breaking changes) |
| 6000.4.0f1 | 2026-03-18 | [Release notes](https://unity.com/releases/editor/whats-new/6000.4.0f1) | Yes |
| 6000.4.1f1 | NOT SOURCEABLE — release notes page not found | — | Patch; no notes fetched |

## Known gaps

- Breaking changes and deprecations for 2022.3 → 6000.2 are NOT SOURCEABLE in this
  set yet. Run `/setup-engine refresh` to add them.
- `current-best-practices.md` not created — no fetched source this run.

## Files

- `breaking-changes.md` — version-by-version breaking changes
- `deprecated-apis.md` — "Don't use X → Use Y" tables

## Agent rule

Before suggesting Unity API code, check the files above. For anything not
covered, verify with WebSearch against docs.unity3d.com for 6000.4 rather than
relying on training data.
