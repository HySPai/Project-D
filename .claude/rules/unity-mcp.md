# Unity MCP Rules

- The UnityMCP server (tools `mcp__UnityMCP__*`, loaded through ToolSearch) gives full
  control of the Unity Editor for this project. That covers scenes, GameObjects,
  components, prefabs, UI, materials, scripts, compiling and the console, play mode,
  tests, build settings and screenshots.
- Make Unity-side changes yourself through MCP. Never hand them to the user as manual
  steps.
- Load the `unity-mcp-skill` skill before the first UnityMCP call of a session.
- After every code change, use MCP to: refresh and compile, check the console for
  errors, set up the scene and wire references, run play mode, and capture an evidence
  screenshot into `production/qa/evidence/`.
- CLAUDE.md conventions still apply to work done through MCP: wire references through
  `[SerializeField]` in the Inspector, with no GetComponent or Find*.
- Changes to the scene or project are still file writes: show the plan and get approval
  before making them, per the Collaboration Protocol.
