# Unity — Deprecated / Removed APIs

Last verified: 2026-10-15

Entries before 6.3 are NOT SOURCEABLE in this file yet (not fetched).

## Removed or changed in 6.4

Source: https://docs.unity3d.com/6000.4/Documentation/Manual/UpgradeGuideUnity64.html

| Don't use | Status | Use instead |
|-----------|--------|-------------|
| URP Compatibility Mode / `URP_COMPATIBILITY_MODE` | Removed | Render graph |
| `IVolume` | Removed | `VolumeGizmoDrawer` |
| `LightShadowCasterMode.NonLightmappedOnly` | Renamed | `LightShadowCasterMode.ShadowMask` |
| `LightShadowCasterMode.Everything` | Renamed | `LightShadowCasterMode.DistanceShadowMask` |
| `CodeModuleInstructionAnalyzer`, `InstructionAnalysisContext` | Internal | None |
| iOS `Libraries/<Unity Runtime Lib>` path | Changed | `Frameworks/UnityRuntime.framework/UnityRuntime` |
| HoloLens 2 plug-in | Deprecated | OpenXR plug-in |

## Removed or changed in 6.3

Source: https://docs.unity3d.com/6000.4/Documentation/Manual/UpgradeGuideUnity63.html

| Don't use | Status | Use instead |
|-----------|--------|-------------|
| `[SerializeField]` on properties/methods/types | Compile error | Fields only; `[field: SerializeField]` for auto-properties |
| `PlayerSettings.Android.androidIsGame` | Obsolete | Player setting **App Category = Game** |
| Android round / legacy icons | Deprecated | Adaptive icons |
| `RenderGraphSettings.enableRenderCompatibilityMode` (as setter) | Read-only, `false` | Render graph |
| Legacy ETC compressor | Removed | Default ETC compressor (or Custom ETCPACK) |
| `AdditionalBakedProbes` | Removed | — |
| `CustomBake` | Obsolete | `LightTransport.IProbeIntegrator` |
| `AccessibilityNode.selected` | Deprecated | `AccessibilityNode.invoked` |
| Bitwise ops on `AccessibilityRole` | Not a flags enum | One role per node |
| Advertisement Legacy package | Deprecated | Ads Mediation (Unity LevelPlay) |
| `UPM_NPM_CACHE_PATH` | Unsupported | `UPM_CACHE_ROOT` |
| `NetworkTransform.Update` override (NGO) | Not overridable | `NetworkTransform.OnUpdate` |
| Facebook Instant Games platform | Deprecated | WebGL |
| Search Index Manager window | Removed | Preferences > Search > Indexing |
