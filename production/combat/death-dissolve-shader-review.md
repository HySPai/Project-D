# Death Dissolve: particle reduction and shader/script review

Scope: `S_EnemyDissolve.shader`, `EnemyDeathDissolve.cs`, `DeathDissolveProfile.cs`, `SO_DeathDissolve_Default.asset`.
Engine: Unity 6000.4.1f1, URP 17.4, Forward (all six pipeline renderers use `m_RenderingMode: 0`, no renderer features). Shuriken only.
Nothing under `Assets/` was edited. Nothing here was run or screenshotted; the look needs the retained screenshot in `production/qa/evidence/` before sign-off.

## 1. Particle reduction

### Tier A: SO values only, no code (recommended first step)

| Field | Now | New | Why |
|---|---|---|---|
| particleCount | 140 | **47** | one third |
| particleSizeRange | (0.03, 0.08) | **(0.045, 0.11)** | each spark stays readable. Total spark area is about 0.28 vs 0.42 before (about 2/3), so the burst loses count but not presence |
| particleLifetimeRange | (0.6, 1.3) | **(0.7, 1.4)** | longer overlap. Concurrent sparks about 35 vs about 95, but each lingers a little longer so the stream does not read as gaps |
| particleGlow | 3 | **3 (unchanged)** | larger quads bloom over more pixels at Bloom intensity 5. If it looks hot, try 2.6 |
| delayAfterDeath, dissolveDuration, dissolveEase, edgeWidth, edgeColor, noiseScale, particleRiseSpeedRange, particleTurbulence, particleColor | | **unchanged** | look already accepted |

`EnemyDeathDissolve` waits `ParticleLifetimeRange.y` before `Finish`, so the 1.3 to 1.4 change adds 0.1s to the total and needs no other edit.

### Tier B: emit where the surface is actually disappearing (optional, small code)

Finding: the shader dissolve value is `noise - amount*1.02`, where noise is a 0.65/0.35 blend of two value-noise octaves. That blend clusters around 0.5, so most surface disappears in a narrow `_DissolveAmount` band. With `Ease.InQuad` (`amount = t^2`), my estimate is that most of the visible vanishing happens in roughly t = 0.55 to 0.85 of the 1.4s. This is an estimate and needs checking in the Game view. The current flat `rateOverTime` spreads sparks evenly over 0 to 1.4s, so many spawn while the body is still intact. With only 47 sparks that waste is more noticeable, so concentrate them in the vanishing window.

Add one profile field and sample it in `StartParticles`. Total count stays exactly `particleCount` because the code normalises the curve area.

Default curve keys (normalised time, relative rate), to tune by eye:
`(0.00, 0.05) (0.45, 0.30) (0.60, 1.20) (0.72, 2.00) (0.85, 1.20) (1.00, 0.20)`

Limitation: Shuriken's SkinnedMeshRenderer shape picks random triangles of the whole mesh. It cannot know which triangles are already dissolved, so a spark can spawn over a gap. A true spatial match needs the shader noise on the CPU (or a custom particle shader), which is out of scope. At 47 sparks emitted mostly while most of the body is dissolving, the mismatch is hard to notice.

### Tier C: per-enemy scaling (optional, decide after measuring bounds)

Vertex count is not a good proxy (the Seed has 630, the King 2917, but the visible size differs). Use renderer bounds. A reference extent of 0 disables it, so it is safe to ship off. Suggested defaults once the Spider bounds are measured:
- particleCountReferenceExtent = Spider `bounds.size.magnitude` (measure via MCP, do not guess)
- particleCountScaleRange = (0.6, 1.8)

Expected result: Seed about 30 sparks, Spider 47, King about 80. Recommend holding Tier C until the user has seen Tier A, because the user asked for "one third" overall.

### Cap

Set `main.maxParticles` to `Mathf.CeilToInt(count * 1.25f)` instead of leaving the prefab default (1000). This is the "never unlimited" budget rule, and costs nothing.

## 2. Proposed code diff (Tier B + Tier C + cap + serialized renderer)

All of it is optional. Tier A needs none of it. No `//` comments, no tooltips, no coroutines.

`DeathDissolveProfile.cs`

```diff
     [Header("Particles")]
     [SerializeField, Range(0, 600)] private int particleCount = 140;
+    [SerializeField] private AnimationCurve particleEmissionCurve = new AnimationCurve(
+        new Keyframe(0f, 0.05f), new Keyframe(0.45f, 0.3f), new Keyframe(0.6f, 1.2f),
+        new Keyframe(0.72f, 2f), new Keyframe(0.85f, 1.2f), new Keyframe(1f, 0.2f));
+    [SerializeField, Min(0f)] private float particleCountReferenceExtent = 0f;
+    [SerializeField] private Vector2 particleCountScaleRange = new Vector2(0.6f, 1.8f);
     [SerializeField] private Vector2 particleLifetimeRange = new Vector2(0.6f, 1.3f);
@@
     public int ParticleCount => particleCount;
+    public AnimationCurve ParticleEmissionCurve => particleEmissionCurve;
+    public float ParticleCountReferenceExtent => particleCountReferenceExtent;
+    public Vector2 ParticleCountScaleRange => particleCountScaleRange;
```

A new field serialises to its C# default on existing assets. The default `particleCount = 140` in code is not the asset value, so the asset must be set to 47 explicitly (Tier A table). The new curve is picked up from the code default only if the asset has no entry, and Unity gives a missing AnimationCurve field an empty curve. The code below falls back to a flat rate when the curve is empty, so Tier B must be switched on by pasting the keys into the asset.

`EnemyDeathDissolve.cs`

```diff
+    private const int EmissionCurveSamples = 32;
+
     [SerializeField] private ParticleSystem dissolveParticles;
+    [SerializeField] private ParticleSystemRenderer dissolveParticlesRenderer;
```

```diff
     private void StartParticles(DeathDissolveProfile profile)
     {
-        if (dissolveParticles == null || profile.ParticleCount <= 0) return;
+        if (dissolveParticles == null) return;
+
+        int count = ResolveParticleCount(profile);
+        if (count <= 0) return;
@@
         ParticleSystem.MainModule main = dissolveParticles.main;
         main.duration = duration;
         main.loop = false;
+        main.maxParticles = Mathf.CeilToInt(count * 1.25f);
@@
         ParticleSystem.EmissionModule emission = dissolveParticles.emission;
-        emission.rateOverTime = profile.ParticleCount / duration;
+        emission.rateOverTime = BuildEmissionRate(profile.ParticleEmissionCurve, count, duration);
@@
-        ParticleSystemRenderer particleRenderer = dissolveParticles.GetComponent<ParticleSystemRenderer>();
-        particleRenderer.GetPropertyBlock(particleBlock);
+        dissolveParticlesRenderer.GetPropertyBlock(particleBlock);
         particleBlock.SetColor(BaseColorId, Color.white * profile.ParticleGlow);
-        particleRenderer.SetPropertyBlock(particleBlock);
+        dissolveParticlesRenderer.SetPropertyBlock(particleBlock);
```

```diff
+    private int ResolveParticleCount(DeathDissolveProfile profile)
+    {
+        float referenceExtent = profile.ParticleCountReferenceExtent;
+        if (referenceExtent <= 0f) return profile.ParticleCount;
+
+        bool hasBounds = false;
+        Bounds bounds = default;
+        for (int i = 0; i < renderers.Length; i++)
+        {
+            if (renderers[i] == null) continue;
+            if (!hasBounds) { bounds = renderers[i].bounds; hasBounds = true; }
+            else bounds.Encapsulate(renderers[i].bounds);
+        }
+        if (!hasBounds) return profile.ParticleCount;
+
+        Vector2 range = profile.ParticleCountScaleRange;
+        float scale = Mathf.Clamp(bounds.size.magnitude / referenceExtent, range.x, range.y);
+        return Mathf.RoundToInt(profile.ParticleCount * scale);
+    }
+
+    private static ParticleSystem.MinMaxCurve BuildEmissionRate(AnimationCurve curve, int count, float duration)
+    {
+        float flatRate = count / duration;
+        if (curve == null || curve.length == 0) return flatRate;
+
+        float area = 0f;
+        for (int i = 0; i < EmissionCurveSamples; i++)
+            area += Mathf.Max(0f, curve.Evaluate((i + 0.5f) / EmissionCurveSamples)) / EmissionCurveSamples;
+        if (area <= 0.0001f) return flatRate;
+
+        return new ParticleSystem.MinMaxCurve(flatRate / area, curve);
+    }
```

Wiring: `dissolveParticlesRenderer` must be assigned on the Spider, Seed and Spider King prefabs (same GameObject as `DeathParticles`'s ParticleSystem). Do it through MCP with `[SerializeField]`. Until it is wired, `StartParticles` would throw a NullReference on the renderer, so wire the three prefabs in the same step as the code change. The `Seed.prefab` match in the grep is the one confirmed prefab that contains a ParticleSystem; I did not open the other two.

Emission module curves are evaluated over normalised system time (time / `main.duration`) for a non-looping system. I have not confirmed that in the 6000.4 docs for this exact setup, so verify by looking at the spark stream timing in Play mode.

## 3. Correctness and perf findings

Ordered by severity.

### Shader `S_EnemyDissolve.shader`

1. **ForwardLit raw variant count is about 960, over the project's 500 per shader cap.** The count is 4 (main shadow) x 3 (additional lights) x 2 x 5 (soft shadows) x 2 (SSAO) x 4 (fog). URP's stripper reduces it per pipeline asset, but the cap is stated against what the shader declares. Proposed trims:
   - Drop `_MAIN_LIGHT_SHADOWS_SCREEN`. The shader sets `shadowCoord = TransformWorldToShadowCoord(...)` unconditionally, which is wrong for that variant. URP Lit uses a screen-space coordinate there. No renderer has a screen-space shadow feature, so it never runs today.
   - Drop `_SCREEN_SPACE_OCCLUSION`. No renderer feature uses SSAO today. If one is added later with a depth-normals source, enemies will not get the AO.
   - Result is 3 x 3 x 2 x 5 x 4 = 360. Replace the lines:
   ```diff
   -#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   +#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
   -#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
   ```
2. **`_ADDITIONAL_LIGHTS_VERTEX` variant lights nothing.** The pragma allows it, but `inputData.vertexLighting` is never filled (it stays zero from `(InputData)0`). If any quality tier's pipeline asset sets Additional Lights to Per Vertex, the dying enemy loses its additional lights. I did not check which of the six assets use it. Either remove `_ADDITIONAL_LIGHTS_VERTEX` from the pragma, or compute `VertexLighting(positionWS, normalWS)` in `Vert` under `#ifdef _ADDITIONAL_LIGHTS_VERTEX`, pass it in a new TEXCOORD, and assign `inputData.vertexLighting`. Removing it is the simpler and cheaper option for an effect that lasts about 1.4s.
3. **DepthNormals pass may write the wrong normal encoding.** `FragNormals` returns raw `half4(normalWS, 0)`. URP 17.4 `LitDepthNormalsPass.hlsl` has `#if defined(_GBUFFER_NORMALS_OCT)` packing, and a `_WRITE_RENDERING_LAYERS` second target. No renderer uses depth normals today (no features), so this is dormant. It will bite as soon as SSAO or another depth-normals consumer is added. Fix then by copying the `_GBUFFER_NORMALS_OCT` branch and its `multi_compile_fragment`, or by `UsePass`-ing / including `LitDepthNormalsPass.hlsl`. I read the package file but did not diff the full pass, so treat the exact 17.4 signature as unverified.
4. **No Forward+ keyword (`_CLUSTER_LIGHT_LOOP` in 17.4).** All current renderers are Forward, so it is dormant. If the project ever switches a tier to Forward+, additional lights will silently be missing on this shader. Note it in a comment block at the top of the shader (the shader-code rule asks for a header with purpose, target pipeline and variant list, which the file lacks).
5. **Look pop at swap time.** The dissolve material hard-codes `smoothness = 0`, `metallic = 0`, no normal map, and copies only base map/color and emission. Source URP/Lit materials (default smoothness 0.5) lose specular and normal detail at the moment `BeginDissolve` swaps materials. Low severity because the user accepted the look, and it happens on a dying body. Cheap fix: carry `_Smoothness` and `_Metallic` as two extra properties in `UnityPerMaterial`.
6. **Per-fragment noise cost.** `DissolveValue` is 2 value-noise calls, 16 hashes, evaluated in 4 passes (ForwardLit, ShadowCaster, DepthOnly, DepthNormals). That is acceptable for 1 to a few enemies on screen at ortho 4.5 for about 1.4s. It would only matter with mass simultaneous deaths. No change recommended.
7. **Noise is in object space of the skinned (post-skin) vertex position.** If the death animation is still moving during the dissolve, the pattern swims with the body. With `delayAfterDeath` 1.1s the pose is settled, so this is fine. Worth knowing if the delay is ever shortened.
8. Fine as is: SRP Batcher compatible (single `UnityPerMaterial` CBUFFER shared via `HLSLINCLUDE`), the clip is consistent across all four passes (no shadow or depth mismatch), the alpha-tested queue is right, `ColorMask R` DepthOnly matches URP 17, fog is applied, `SampleSH` is used for GI. No `multi_compile_instancing` is correct for skinned meshes. A hand-written HLSL shader is a departure from "Shader Graph first"; reasonable here because it needs all four passes and ForwardLit PBR, so I am not asking for a rewrite.

### `EnemyDeathDissolve.cs`

1. **Cannot be re-used after a death.** `BeginDissolve` assigns the dissolve materials to the renderers and `HideRenderers` disables them, and nothing restores the original `sharedMaterials`, `_DissolveAmount`, `renderer.enabled` or the particle state. Fine if enemies are destroyed or `hideOnComplete` is a one-shot. If enemies are ever pooled and respawned, they come back invisible. I did not check whether the project pools enemies. If it does, add a `ResetState()` that stores the original `sharedMaterials` array in `Prepare` and restores it, re-enables renderers and kills the sequence.
2. **`GetComponent<ParticleSystemRenderer>()` in `StartParticles`.** Violates the project rule; also a per-death lookup. Replaced by the serialized `dissolveParticlesRenderer` in the diff above (needs prefab wiring on the three enemy prefabs).
3. **Missing particle cap.** `main.maxParticles` is left at whatever the prefab has. Fixed in the diff (`ceil(count * 1.25)`).
4. **MaterialPropertyBlock is set on every Play.** Glow is constant per profile, so it could be set once in `Prepare`. The property block also opts that renderer out of SRP Batcher, which is irrelevant for 1 draw.
5. **Small allocations at death only (not per frame):** two lambdas in `Play` capturing `profile`, a method-group delegate for `SetDissolveAmount`, `new Material[0]` in `Prepare` (use `System.Array.Empty<Material>()`), and `sharedMaterials` getters returning new arrays. All are once per enemy and acceptable; `SetDissolveAmount` itself allocates nothing per frame.
6. **`prepared` is set before the shader null check can matter, but `Play` already guards `dissolveShader == null`,** so this is safe. `Prepare` ignores its `profile` argument beyond a null check; harmless.
7. **Duration maths is right.** The particle system `duration` equals `DissolveDuration`, and the final spark lives `ParticleLifetimeRange.y`, which is exactly what the trailing `AppendInterval` waits for. `SetLink(gameObject)` kills the sequence on destroy and `OnDestroy` frees the instantiated materials (no leak).
8. **Skinned-mesh shape cost.** Shuriken bakes the skinned mesh for emission while the system is emitting (about 2917 verts on the Spider King). It runs for only about 1.4s per death, which is acceptable; fewer sparks does not change it.

## 4. Verification to do before closing

- Apply Tier A, play a Spider, Seed and Spider King death, and keep one screenshot of each in `production/qa/evidence/` (the Visual/Feel evidence rule is blocking).
- Check the Game view at peak: is the spark stream continuous, or does it read as separate dots? If it reads sparse, raise size to (0.05, 0.12) before raising count.
- If Tier B is taken, confirm the emission-curve timing in Play mode (see the note under Tier B).
