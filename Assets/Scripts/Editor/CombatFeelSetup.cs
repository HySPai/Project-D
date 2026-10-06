using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class CombatFeelSetup
{
    private const string MenuPath = "Tools/Project D/Setup Combat Feel";
    private const string SessionKey = "CombatFeelSetup.Look2AutoRan";
    private const string ReportPath = "Logs/CombatFeelSetup.txt";

    private const string PlayerPrefabPath = "Assets/Prefabs/CoreGame/Player.prefab";
    private const string CamerasPrefabPath = "Assets/Prefabs/CoreGame/Cameras.prefab";
    private const string FeedbackPrefabPath = "Assets/Prefabs/GameManager/prefab_FeedbackController.prefab";
    private const string TrailMaterialPath = "Assets/Prefabs/Vfx/M_SwordTrail.mat";
    private const string TrailShaderName = "Universal Render Pipeline/Particles/Unlit";
    private const string TrailObjectName = "SwingTrail";

    private const string HitSparkPrefabPath = "Assets/Prefabs/Vfx/vfx_HitSpark.prefab";
    private const string HitSparkMaterialPath = "Assets/Prefabs/Vfx/M_HitSpark.mat";
    private const string ParticleTexturePath = "Assets/Prefabs/Vfx/T_SoftGlow.png";

    private const float OldTrailWidthThreshold = 0.5f;
    private const float DamageParticlesScale = 1f;
    private const float DeathParticlesScale = 1.7f;

    private const string FeedbacksRootName = "Feedbacks";
    private const string AttackHitFeedbackName = "AttackHitFeedback";
    private const string AttackKillFeedbackName = "AttackKillFeedback";
    private const string DamageFeedbackName = "DamageFeedback";
    private const string DeathFeedbackName = "DeathFeedback";
    private const string FlashPropertyName = "_BaseColor";

    private static readonly string[] EnemyPrefabPaths =
    {
        "Assets/Prefabs/CoreGame/Seed.prefab",
        "Assets/Prefabs/CoreGame/Spider.prefab",
        "Assets/Prefabs/CoreGame/Spider King.prefab"
    };

    private static readonly string[] ObsoleteComponentNames =
    {
        "CharacterHitFeedback",
        "CameraShakeExtension",
        "CombatFeedbackController"
    };

    private static readonly string[] ObsoleteScriptPaths =
    {
        "Assets/Scripts/CoreGame/Character/CharacterHitFeedback.cs",
        "Assets/Scripts/FeedbackController/CameraShakeExtension.cs",
        "Assets/Scripts/FeedbackController/CombatFeedbackController.cs"
    };

    private static readonly string[] ObsoleteParticlePaths =
    {
        "Assets/Storepackages/Epic Toon FX/Prefabs/Combat/Brawling/Soft/vfx_SoftBodySlam.prefab",
        "Assets/Storepackages/Epic Toon FX/Prefabs/Combat/Brawling/Toon/vfx_ToonBodySlam.prefab"
    };

    private static readonly Color EnemyFlashColor = new Color(3f, 3f, 3f, 1f);
    private static readonly Color PlayerFlashColor = new Color(3f, 0.6f, 0.5f, 1f);
    private static readonly Color TrailColor = new Color(1f, 0.97f, 0.88f, 1f);
    private static readonly Color SparkColorA = new Color(1f, 1f, 1f, 1f);
    private static readonly Color SparkColorB = new Color(1f, 0.82f, 0.35f, 1f);
    private static readonly Color FlashColor = new Color(1f, 0.97f, 0.85f, 0.7f);

    [InitializeOnLoadMethod]
    private static void ScheduleAutoRun()
    {
        if (SessionState.GetBool(SessionKey, false)) return;

        EditorApplication.delayCall += AutoRun;
    }

    private static void AutoRun()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.playModeStateChanged -= RunAfterPlayMode;
            EditorApplication.playModeStateChanged += RunAfterPlayMode;
            return;
        }

        SessionState.SetBool(SessionKey, true);

        if (IsSetupComplete()) return;

        Run();
    }

    private static void RunAfterPlayMode(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredEditMode) return;

        EditorApplication.playModeStateChanged -= RunAfterPlayMode;
        EditorApplication.delayCall += AutoRun;
    }

    [MenuItem(MenuPath)]
    public static void Run()
    {
        var report = new StringBuilder();
        report.AppendLine("Combat feel setup (Feel) - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        try
        {
            EnsureHitSparkPrefab(report);

            EditPrefab(PlayerPrefabPath, report, root => SetupPlayer(root, report));

            for (int i = 0; i < EnemyPrefabPaths.Length; i++)
                EditPrefab(EnemyPrefabPaths[i], report, root => SetupEnemy(root, report));

            EditPrefab(CamerasPrefabPath, report, root => SetupCamera(root, report));
            EditPrefab(FeedbackPrefabPath, report, root => SetupTimeManager(root, report));

            AssetDatabase.SaveAssets();
            DeleteObsoleteScripts(report);

            report.AppendLine("Result: OK");
        }
        catch (Exception exception)
        {
            report.AppendLine("Result: FAILED - " + exception);
            Debug.LogException(exception);
        }

        WriteReport(report.ToString());
        Debug.Log(report.ToString());
    }

    private static bool IsSetupComplete()
    {
        for (int i = 0; i < ObsoleteScriptPaths.Length; i++)
        {
            if (AssetDatabase.LoadAssetAtPath<MonoScript>(ObsoleteScriptPaths[i]) != null) return false;
        }

        if (NeedsLookUpdate()) return false;

        GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (player != null)
        {
            if (!HasReference(player.GetComponent<PlayerCombat>(), "hitFeedback")) return false;
            if (!HasReference(player.GetComponent<PlayerCombat>(), "swingTrail")) return false;
            if (!HasReference(player.GetComponent<CharacterControllerBase>(), "damageFeedback")) return false;
        }

        for (int i = 0; i < EnemyPrefabPaths.Length; i++)
        {
            GameObject enemy = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPaths[i]);
            if (enemy == null) continue;
            if (!HasReference(enemy.GetComponent<CharacterControllerBase>(), "damageFeedback")) return false;
        }

        GameObject cameras = AssetDatabase.LoadAssetAtPath<GameObject>(CamerasPrefabPath);
        if (cameras != null && cameras.GetComponentInChildren<CinemachineImpulseListener>(true) == null) return false;

        GameObject feedback = AssetDatabase.LoadAssetAtPath<GameObject>(FeedbackPrefabPath);
        if (feedback != null && feedback.GetComponent<MMTimeManager>() == null) return false;

        return true;
    }

    private static bool NeedsLookUpdate()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(HitSparkPrefabPath) == null) return true;

        GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (player == null) return false;

        TrailRenderer trail = FindSwingTrail(player);
        if (trail != null && trail.widthMultiplier > OldTrailWidthThreshold) return true;

        MMF_Flicker flicker = FindFeedback<MMF_Flicker>(player, DamageFeedbackName);
        return flicker != null && flicker.UseMaterialPropertyBlocks;
    }

    private static TrailRenderer FindSwingTrail(GameObject player)
    {
        PlayerCombat combat = player.GetComponent<PlayerCombat>();
        if (combat == null) return null;

        SerializedProperty property = new SerializedObject(combat).FindProperty("swingTrail");
        return property != null ? property.objectReferenceValue as TrailRenderer : null;
    }

    private static MMF_Player FindFeedbackPlayer(GameObject root, string playerName)
    {
        Transform holder = root.transform.Find(FeedbacksRootName);
        Transform playerTransform = holder != null ? holder.Find(playerName) : null;
        return playerTransform != null ? playerTransform.GetComponent<MMF_Player>() : null;
    }

    private static T FindFeedback<T>(GameObject root, string playerName) where T : MMF_Feedback
    {
        MMF_Player player = FindFeedbackPlayer(root, playerName);
        if (player == null || player.FeedbacksList == null) return null;

        for (int i = 0; i < player.FeedbacksList.Count; i++)
        {
            if (player.FeedbacksList[i] is T feedback) return feedback;
        }

        return null;
    }

    private static bool HasReference(Component component, string propertyName)
    {
        if (component == null) return true;

        SerializedProperty property = new SerializedObject(component).FindProperty(propertyName);
        return property == null || property.objectReferenceValue != null;
    }

    private static void EditPrefab(string path, StringBuilder report, Func<GameObject, bool> edit)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            report.AppendLine("MISSING   " + path);
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            if (edit(root))
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                report.AppendLine("SAVED     " + path);
            }
            else
            {
                report.AppendLine("UNCHANGED " + path);
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    #region Player
    private static bool SetupPlayer(GameObject root, StringBuilder report)
    {
        bool changed = RemoveObsoleteComponents(root, report);
        changed |= SetupSwingTrail(root, report);

        CharacterControllerBase controller = root.GetComponent<CharacterControllerBase>();
        PlayerCombat combat = root.GetComponent<PlayerCombat>();
        if (controller == null || combat == null)
        {
            report.AppendLine("  " + root.name + ": missing PlayerCombat or controller");
            return changed;
        }

        List<Renderer> renderers = CollectFlashRenderers(root);

        var combatObject = new SerializedObject(combat);
        changed |= AssignFeedbackPlayer(combatObject, "hitFeedback", root, AttackHitFeedbackName, report,
            player => BuildImpactFeedback(player, 0.05f, new Vector3(0.08f, -0.1f, 0f), 0.18f));
        changed |= AssignFeedbackPlayer(combatObject, "killFeedback", root, AttackKillFeedbackName, report,
            player => BuildImpactFeedback(player, 0.1f, new Vector3(0.16f, -0.2f, 0f), 0.3f));
        combatObject.ApplyModifiedPropertiesWithoutUndo();

        var controllerObject = new SerializedObject(controller);
        changed |= AssignFeedbackPlayer(controllerObject, "damageFeedback", root, DamageFeedbackName, report,
            player =>
            {
                BuildImpactFeedback(player, 0.08f, new Vector3(-0.2f, 0.25f, 0f), 0.3f);
                AddFlicker(player, renderers, PlayerFlashColor, false);
            });
        controllerObject.ApplyModifiedPropertiesWithoutUndo();

        changed |= UpdatePlayerLook(root, report);

        return changed;
    }

    private static bool UpdatePlayerLook(GameObject root, StringBuilder report)
    {
        bool changed = false;

        MMF_Flicker flicker = FindFeedback<MMF_Flicker>(root, DamageFeedbackName);
        if (flicker != null && flicker.UseMaterialPropertyBlocks)
        {
            flicker.UseMaterialPropertyBlocks = false;
            EditorUtility.SetDirty(FindFeedbackPlayer(root, DamageFeedbackName));
            report.AppendLine("  " + root.name + ": flash no longer uses MaterialPropertyBlock");
            changed = true;
        }

        TrailRenderer trail = FindSwingTrail(root);
        if (trail != null && trail.widthMultiplier > OldTrailWidthThreshold)
        {
            PlayerCombat combat = root.GetComponent<PlayerCombat>();
            ApplyTrailLook(trail, FindWeapon(new SerializedObject(combat)));
            EditorUtility.SetDirty(trail);
            report.AppendLine("  " + root.name + ": swing trail restyled");
            changed = true;
        }

        return changed;
    }

    private static void BuildImpactFeedback(MMF_Player player, float freezeDuration, Vector3 shakeVelocity, float shakeDuration)
    {
        player.ForceTimescaleMode = true;
        player.ForcedTimescaleMode = TimescaleModes.Unscaled;

        AddFreezeFrame(player, freezeDuration);
        AddCameraShake(player, shakeVelocity, shakeDuration);
    }
    #endregion

    #region Enemy
    private static bool SetupEnemy(GameObject root, StringBuilder report)
    {
        bool changed = RemoveObsoleteComponents(root, report);

        CharacterControllerBase controller = root.GetComponent<CharacterControllerBase>();
        if (controller == null)
        {
            report.AppendLine("  " + root.name + ": no CharacterControllerBase");
            return changed;
        }

        List<Renderer> renderers = CollectFlashRenderers(root);

        var controllerObject = new SerializedObject(controller);
        changed |= AssignFeedbackPlayer(controllerObject, "damageFeedback", root, DamageFeedbackName, report,
            player =>
            {
                player.transform.localScale = Vector3.one * DamageParticlesScale;
                AddFlicker(player, renderers, EnemyFlashColor, true);
                AddParticles(player, 3, report);
            });
        changed |= AssignFeedbackPlayer(controllerObject, "deathFeedback", root, DeathFeedbackName, report,
            player =>
            {
                player.transform.localScale = Vector3.one * DeathParticlesScale;
                AddFlicker(player, renderers, EnemyFlashColor, true);
                AddParticles(player, 2, report);
            });
        controllerObject.ApplyModifiedPropertiesWithoutUndo();

        changed |= UpdateEnemyParticles(root, DamageFeedbackName, DamageParticlesScale, report);
        changed |= UpdateEnemyParticles(root, DeathFeedbackName, DeathParticlesScale, report);

        return changed;
    }

    private static bool UpdateEnemyParticles(GameObject root, string playerName, float scale, StringBuilder report)
    {
        MMF_ParticlesInstantiation particles = FindFeedback<MMF_ParticlesInstantiation>(root, playerName);
        if (particles == null) return false;

        string currentPath = particles.ParticlesPrefab != null
            ? AssetDatabase.GetAssetPath(particles.ParticlesPrefab)
            : string.Empty;

        if (particles.ParticlesPrefab != null && Array.IndexOf(ObsoleteParticlePaths, currentPath) < 0) return false;

        ParticleSystem hitSpark = AssetDatabase.LoadAssetAtPath<ParticleSystem>(HitSparkPrefabPath);
        if (hitSpark == null) return false;

        MMF_Player player = FindFeedbackPlayer(root, playerName);
        particles.ParticlesPrefab = hitSpark;
        player.transform.localScale = Vector3.one * scale;
        EditorUtility.SetDirty(player);

        report.AppendLine("  " + root.name + ": " + playerName + " particles -> vfx_HitSpark");
        return true;
    }
    #endregion

    #region Feel Feedbacks
    private static bool AssignFeedbackPlayer(SerializedObject target, string propertyName, GameObject root,
        string playerName, StringBuilder report, Action<MMF_Player> build)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        if (property == null)
        {
            report.AppendLine("  " + root.name + ": field '" + propertyName + "' not found");
            return false;
        }

        if (property.objectReferenceValue != null) return false;

        Transform holder = root.transform.Find(FeedbacksRootName);
        if (holder == null)
        {
            var holderObject = new GameObject(FeedbacksRootName);
            holderObject.transform.SetParent(root.transform, false);
            holder = holderObject.transform;
        }

        Transform existing = holder.Find(playerName);
        MMF_Player player = existing != null ? existing.GetComponent<MMF_Player>() : null;

        if (player == null)
        {
            var playerObject = new GameObject(playerName);
            playerObject.transform.SetParent(holder, false);
            player = playerObject.AddComponent<MMF_Player>();
            build(player);
            EditorUtility.SetDirty(player);
        }

        int feedbackCount = player.FeedbacksList != null ? player.FeedbacksList.Count : 0;

        property.objectReferenceValue = player;
        report.AppendLine("  " + root.name + ": " + playerName + " (" + feedbackCount + " feedbacks)");
        return true;
    }

    private static void AddFreezeFrame(MMF_Player player, float duration)
    {
        var freeze = (MMF_FreezeFrame)player.AddFeedback(typeof(MMF_FreezeFrame));
        freeze.Label = "Hit Stop";
        freeze.FreezeFrameDuration = duration;
    }

    private static void AddCameraShake(MMF_Player player, Vector3 velocity, float duration)
    {
        var impulse = (MMF_CinemachineImpulse)player.AddFeedback(typeof(MMF_CinemachineImpulse));
        impulse.Label = "Camera Shake";

        if (impulse.m_ImpulseDefinition == null)
            impulse.m_ImpulseDefinition = new CinemachineImpulseDefinition();

        impulse.m_ImpulseDefinition.ImpulseChannel = 1;
        impulse.m_ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        impulse.m_ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        impulse.m_ImpulseDefinition.ImpulseDuration = duration;
        impulse.Velocity = velocity;
    }

    private static void AddFlicker(MMF_Player player, List<Renderer> renderers, Color color, bool usePropertyBlocks)
    {
        if (renderers.Count == 0) return;

        var flicker = (MMF_Flicker)player.AddFeedback(typeof(MMF_Flicker));
        flicker.Label = "Flash";
        flicker.BoundRenderer = renderers[0];
        flicker.ExtraBoundRenderers = renderers.GetRange(1, renderers.Count - 1);
        flicker.Mode = MMF_Flicker.Modes.PropertyName;
        flicker.PropertyName = FlashPropertyName;
        flicker.UseMaterialPropertyBlocks = usePropertyBlocks;
        flicker.FlickerColor = color;
        flicker.FlickerDuration = 0.12f;
        flicker.FlickerPeriod = 0.06f;
    }

    private static void AddParticles(MMF_Player player, int poolSize, StringBuilder report)
    {
        ParticleSystem prefab = AssetDatabase.LoadAssetAtPath<ParticleSystem>(HitSparkPrefabPath);
        if (prefab == null)
        {
            report.AppendLine("  particles prefab not found: " + HitSparkPrefabPath);
            return;
        }

        var particles = (MMF_ParticlesInstantiation)player.AddFeedback(typeof(MMF_ParticlesInstantiation));
        particles.Label = "Hit Particles";
        particles.ParticlesPrefab = prefab;
        particles.Mode = MMF_ParticlesInstantiation.Modes.Pool;
        particles.ObjectPoolSize = poolSize;
        particles.PositionMode = MMF_ParticlesInstantiation.PositionModes.FeedbackPosition;
        particles.Offset = new Vector3(0f, 0.5f, 0f);
        particles.NestParticles = true;
        particles.ForceStopAction = true;
        particles.StopAction = ParticleSystemStopAction.Disable;
    }
    #endregion

    #region Scene Helpers
    private static bool SetupCamera(GameObject root, StringBuilder report)
    {
        bool changed = RemoveObsoleteComponents(root, report);

        CinemachineCamera virtualCamera = root.GetComponentInChildren<CinemachineCamera>(true);
        if (virtualCamera == null)
        {
            report.AppendLine("  no CinemachineCamera in " + root.name);
            return changed;
        }

        if (virtualCamera.GetComponent<CinemachineImpulseListener>() != null) return changed;

        CinemachineImpulseListener listener = virtualCamera.gameObject.AddComponent<CinemachineImpulseListener>();
        listener.ApplyAfter = CinemachineCore.Stage.Noise;
        listener.ChannelMask = 1;
        listener.Gain = 1f;
        listener.Use2DDistance = false;
        listener.UseCameraSpace = true;
        listener.SignalCombinationMode = CinemachineImpulseListener.SignalCombinationModes.Additive;
        listener.ReactionSettings = new CinemachineImpulseListener.ImpulseReaction
        {
            AmplitudeGain = 1f,
            FrequencyGain = 1f,
            Duration = 1f
        };

        report.AppendLine("  CinemachineImpulseListener added to " + virtualCamera.name);
        return true;
    }

    private static bool SetupTimeManager(GameObject root, StringBuilder report)
    {
        bool changed = RemoveObsoleteComponents(root, report);

        if (root.GetComponent<MMTimeManager>() != null) return changed;

        root.AddComponent<MMTimeManager>();
        report.AppendLine("  MMTimeManager added to " + root.name);
        return true;
    }
    #endregion

    #region Cleanup
    private static bool RemoveObsoleteComponents(GameObject root, StringBuilder report)
    {
        bool removed = false;

        for (int i = 0; i < ObsoleteComponentNames.Length; i++)
        {
            Type type = FindComponentType(ObsoleteComponentNames[i]);
            if (type == null) continue;

            Component[] components = root.GetComponentsInChildren(type, true);
            for (int c = 0; c < components.Length; c++)
            {
                UnityEngine.Object.DestroyImmediate(components[c], true);
                removed = true;
            }

            if (components.Length > 0)
                report.AppendLine("  " + root.name + ": removed " + ObsoleteComponentNames[i]);
        }

        return removed;
    }

    private static Type FindComponentType(string typeName)
    {
        System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Type type = assemblies[i].GetType(typeName);
            if (type != null && typeof(Component).IsAssignableFrom(type)) return type;
        }

        return null;
    }

    private static void DeleteObsoleteScripts(StringBuilder report)
    {
        for (int i = 0; i < ObsoleteScriptPaths.Length; i++)
        {
            string path = ObsoleteScriptPaths[i];
            if (AssetDatabase.LoadAssetAtPath<MonoScript>(path) == null) continue;

            bool deleted = AssetDatabase.DeleteAsset(path);
            report.AppendLine((deleted ? "DELETED   " : "KEPT      ") + path);
        }
    }
    #endregion

    #region Renderers
    private static List<Renderer> CollectFlashRenderers(GameObject root)
    {
        var result = new List<Renderer>();
        Renderer[] all = root.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < all.Length; i++)
        {
            Renderer candidate = all[i];
            if (!(candidate is SkinnedMeshRenderer) && !(candidate is MeshRenderer)) continue;
            if (!IsActiveUnder(candidate.transform, root.transform)) continue;
            if (!HasFlashProperty(candidate)) continue;

            result.Add(candidate);
        }

        return result;
    }

    private static bool HasFlashProperty(Renderer candidate)
    {
        Material[] materials = candidate.sharedMaterials;
        return materials.Length > 0 && materials[0] != null && materials[0].HasProperty(FlashPropertyName);
    }

    private static bool IsActiveUnder(Transform target, Transform root)
    {
        Transform current = target;
        while (current != null && current != root)
        {
            if (!current.gameObject.activeSelf) return false;
            current = current.parent;
        }

        return true;
    }
    #endregion

    #region Swing Trail
    private static bool SetupSwingTrail(GameObject root, StringBuilder report)
    {
        PlayerCombat combat = root.GetComponent<PlayerCombat>();
        if (combat == null) return false;

        var combatObject = new SerializedObject(combat);
        SerializedProperty trailProperty = combatObject.FindProperty("swingTrail");
        if (trailProperty == null || trailProperty.objectReferenceValue != null) return false;

        DamageCollider weapon = FindWeapon(combatObject);
        if (weapon == null)
        {
            report.AppendLine("  " + root.name + ": no weapon DamageCollider, swing trail skipped");
            return false;
        }

        Transform weaponTransform = weapon.transform;
        Transform existing = weaponTransform.Find(TrailObjectName);
        TrailRenderer trail = existing != null ? existing.GetComponent<TrailRenderer>() : null;
        if (trail == null)
        {
            var trailObject = new GameObject(TrailObjectName);
            trailObject.transform.SetParent(weaponTransform, false);
            trail = trailObject.AddComponent<TrailRenderer>();
            ApplyTrailLook(trail, weapon);
        }

        trailProperty.objectReferenceValue = trail;
        combatObject.ApplyModifiedPropertiesWithoutUndo();

        report.AppendLine("  " + root.name + ": swing trail on " + weaponTransform.name);
        return true;
    }

    private static DamageCollider FindWeapon(SerializedObject combatObject)
    {
        SerializedProperty colliders = combatObject.FindProperty("damageColliders");
        if (colliders == null || colliders.arraySize == 0) return null;

        return colliders.GetArrayElementAtIndex(0).objectReferenceValue as DamageCollider;
    }

    private static void ApplyTrailLook(TrailRenderer trail, DamageCollider weapon)
    {
        trail.transform.localPosition = GetBladeOuterPoint(weapon);

        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(TrailColor, 1f) },
            new[] { new GradientAlphaKey(0.55f, 0f), new GradientAlphaKey(0.3f, 0.5f), new GradientAlphaKey(0f, 1f) });

        trail.time = 0.12f;
        trail.minVertexDistance = 0.03f;
        trail.widthMultiplier = 0.3f;
        trail.widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        trail.colorGradient = gradient;
        trail.alignment = LineAlignment.View;
        trail.textureMode = LineTextureMode.Stretch;
        trail.numCornerVertices = 2;
        trail.numCapVertices = 0;
        trail.shadowCastingMode = ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.autodestruct = false;
        trail.emitting = false;
        trail.sharedMaterial = GetParticleMaterial(TrailMaterialPath, false);
    }

    private static Vector3 GetBladeOuterPoint(DamageCollider weapon)
    {
        BoxCollider box = weapon != null ? weapon.GetCollider as BoxCollider : null;
        if (box == null) return new Vector3(0f, 0f, 0.7f);

        Vector3 size = box.size;
        Vector3 axis = Vector3.forward;
        float length = size.z;

        if (size.x >= size.y && size.x >= size.z)
        {
            axis = Vector3.right;
            length = size.x;
        }
        else if (size.y >= size.x && size.y >= size.z)
        {
            axis = Vector3.up;
            length = size.y;
        }

        float direction = Vector3.Dot(box.center, axis) < 0f ? -1f : 1f;
        return box.center + axis * (direction * length * 0.3f);
    }
    #endregion

    #region Particle Assets
    private static Material GetParticleMaterial(string path, bool additive)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        bool isNew = material == null;

        if (isNew)
        {
            Shader shader = Shader.Find(TrailShaderName);
            if (shader == null) return null;

            material = new Material(shader);
        }

        material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(ParticleTexturePath));
        material.SetColor("_BaseColor", Color.white);
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", additive ? 2f : 0f);
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", additive
            ? (float)UnityEngine.Rendering.BlendMode.One
            : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
        material.SetFloat("_DstBlendAlpha", additive
            ? (float)UnityEngine.Rendering.BlendMode.One
            : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_Cull", (float)CullMode.Off);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)RenderQueue.Transparent;

        if (isNew)
            AssetDatabase.CreateAsset(material, path);
        else
            EditorUtility.SetDirty(material);

        return material;
    }

    private static void EnsureHitSparkPrefab(StringBuilder report)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(HitSparkPrefabPath) != null) return;

        Material material = GetParticleMaterial(HitSparkMaterialPath, false);
        UnityEngine.SceneManagement.Scene previewScene = EditorSceneManager.NewPreviewScene();

        try
        {
            var root = new GameObject("vfx_HitSpark");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, previewScene);
            BuildSparks(root, material);

            var flashObject = new GameObject("Flash");
            flashObject.transform.SetParent(root.transform, false);
            BuildFlash(flashObject, material);

            PrefabUtility.SaveAsPrefabAsset(root, HitSparkPrefabPath);
            report.AppendLine("CREATED   " + HitSparkPrefabPath);
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(previewScene);
        }
    }

    private static void BuildSparks(GameObject target, Material material)
    {
        ParticleSystem sparks = target.AddComponent<ParticleSystem>();
        sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = sparks.main;
        main.duration = 0.4f;
        main.loop = false;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.16f, 0.3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
        main.startColor = new ParticleSystem.MinMaxGradient(SparkColorA, SparkColorB);
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.maxParticles = 32;

        ParticleSystem.EmissionModule emission = sparks.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)9, (short)13) });

        ParticleSystem.ShapeModule shape = sparks.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        ParticleSystem.LimitVelocityOverLifetimeModule limit = sparks.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.limit = 1.5f;
        limit.dampen = 0.18f;

        ApplyFadeOut(sparks, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

        ParticleSystemRenderer sparksRenderer = target.GetComponent<ParticleSystemRenderer>();
        sparksRenderer.renderMode = ParticleSystemRenderMode.Stretch;
        sparksRenderer.velocityScale = 0.05f;
        sparksRenderer.lengthScale = 1.5f;
        ApplyParticleRenderer(sparksRenderer, material);
    }

    private static void BuildFlash(GameObject target, Material material)
    {
        ParticleSystem flash = target.AddComponent<ParticleSystem>();
        flash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = flash.main;
        main.duration = 0.2f;
        main.loop = false;
        main.playOnAwake = true;
        main.startLifetime = 0.1f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.1f);
        main.startColor = FlashColor;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.maxParticles = 2;

        ParticleSystem.EmissionModule emission = flash.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)1) });

        ParticleSystem.ShapeModule shape = flash.shape;
        shape.enabled = false;

        ApplyFadeOut(flash, new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(0.4f, 1f), new Keyframe(1f, 1f)));

        ParticleSystemRenderer flashRenderer = target.GetComponent<ParticleSystemRenderer>();
        flashRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        ApplyParticleRenderer(flashRenderer, material);
    }

    private static void ApplyFadeOut(ParticleSystem system, AnimationCurve sizeCurve)
    {
        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });

        ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
        color.enabled = true;
        color.color = new ParticleSystem.MinMaxGradient(fade);

        ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
    }

    private static void ApplyParticleRenderer(ParticleSystemRenderer renderer, Material material)
    {
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }
    #endregion

    private static void WriteReport(string content)
    {
        try
        {
            string directory = Path.GetDirectoryName(ReportPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(ReportPath, content);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("CombatFeelSetup: cannot write report - " + exception.Message);
        }
    }
}
