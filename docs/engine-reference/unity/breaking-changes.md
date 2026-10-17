# Unity — Breaking Changes

Last verified: 2026-10-15

Spans before 6.2 → 6.3 are NOT SOURCEABLE in this file yet (not fetched).

## 6.3 → 6.4

Source: https://docs.unity3d.com/6000.4/Documentation/Manual/UpgradeGuideUnity64.html

### Assets and media
- **Artifact dependency narrowing**: dependents reimport only when a dependency's
  import result changes, not its source. Some `ScriptedImporter` /
  `AssetPostprocessor` classes may stop running.
  - Importer reading another asset's source and result: register both
    `AssetImportContext.DependsOnArtifact` and `AssetImportContext.DependsOnSourceAsset`.
  - Indirect chains (e.g. mesh from `Model.fbx` used by `Nest.prefab`): register a
    direct `DependsOnArtifact` on the source asset.
- **`Object.Destroy` now calls `OnDisable` on all descendant GameObjects**, matching
  `DestroyImmediate`. Remove manual workarounds that propagate `OnDisable` — they now
  cause duplicate calls.

### Graphics
- URP Compatibility Mode removed; `URP_COMPATIBILITY_MODE` define removed. Use render graph.
- `com.unity.modules.physics` is a soft dependency of SRP Core. Disabling it breaks
  local volumes; packages using Physics on SRP Core must declare it explicitly.
- `IVolume` removed → `VolumeGizmoDrawer`.
- `LightShadowCasterMode.NonLightmappedOnly` → `ShadowMask`;
  `Everything` → `DistanceShadowMask` (auto-updated).

### Optimization
- Project Auditor ships with the Editor. `CodeModuleInstructionAnalyzer`,
  `InstructionAnalysisContext` and related types are now internal.

### Platforms — iOS / iPadOS / tvOS / visionOS
- Device runtime libs wrapped in a static framework: path changes from
  `Libraries/<Unity Runtime Lib>` to `Frameworks/UnityRuntime.framework/UnityRuntime`.
  Post-processing scripts referencing the old path may need updating.
- Simulator `UnityRuntime.framework` is now static.

### XR
- HoloLens 2 support deprecated → OpenXR plug-in.

## 6.2 → 6.3

Source: https://docs.unity3d.com/6000.4/Documentation/Manual/UpgradeGuideUnity63.html

### Programming
- **`[SerializeField]` only on fields.** On properties, methods or types it is a compile
  error. For auto-properties use `[field: SerializeField]`.

### Graphics
- URP Compatibility Mode removed; `RenderGraphSettings.enableRenderCompatibilityMode`
  is read-only `false`.
- Legacy ETC compression removed; projects move to default ETC compressor (output may
  differ, size unchanged). Approximate legacy via Project Settings > Editor > Texture
  Compressors > ETC Compressor = Custom (ETCPACK Fast/Normal/Best).
- Experimental lightmapping: `AdditionalBakedProbes` removed; `CustomBake` obsolete →
  `LightTransport.IProbeIntegrator`.

### Platforms — Android
- Round and legacy icons deprecated → adaptive icons.
- Android 16 stops enforcing fixed orientation/resizability/aspect on large screens.
  Set Player setting **App Category = Game** to keep old behaviour (replaces obsolete
  `PlayerSettings.Android.androidIsGame`).

### Platforms — Web
- Facebook Instant Games deprecated → WebGL.

### Entities / scene
- GlobalIllumination light structs: `int` fields → `EntityId` (renamed `entityID`;
  `instanceID` forwards). `Scene.handle`: `int` → `SceneHandle`. Implicit conversions keep
  C# compiling; precompiled DLLs may throw `MissingFieldException` — recompile.

### Accessibility
- `AccessibilityRole` is a plain enum (not flags); `AccessibilityRole`/`AccessibilityState`
  are `byte`. `AccessibilityNode.selected` → `invoked`.

### Ads
- Advertisement Legacy → Ads Mediation (Unity LevelPlay); Unity Ads SDK 4.16.4.

### Editor / Package Manager
- Search Index Manager removed → Preferences > Search > Indexing.
- `UPM_NPM_CACHE_PATH` unsupported → `UPM_CACHE_ROOT`.

### Optimization
- Adaptive Performance is a core module, not a package.

### UI Toolkit
- Stricter USS parser: invalid selectors, syntax errors and unsupported CSS now reported;
  syntax errors block import.

### Multiplayer
- Netcode for GameObjects 1.x deprecated → 2.x (`NetworkTransform.Update` not
  overridable → `OnUpdate`). Multiplay Hosting removed.
