using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CombatClipEventBuilder
{
    private const string CombatRoot = "Assets/Animations/Combat";
    private const string PlayerPrefabPath = "Assets/Prefabs/CoreGame/Player.prefab";
    private const float SampleRate = 60f;
    private const float WindowThreshold = 0.35f;
    private const float MergeGap = 0.12f;
    private const float MinWindow = 0.05f;
    private const int MaxWindows = 4;
    private const float ComboDelay = 0.04f;

    private enum ClipKind
    {
        Attack,
        LightAttack,
        Hold,
        Backstep
    }

    private readonly struct ClipEntry
    {
        public readonly string path;
        public readonly ClipKind kind;
        public readonly bool bothHands;

        public ClipEntry(string path, ClipKind kind, bool bothHands = false)
        {
            this.path = path;
            this.kind = kind;
            this.bothHands = bothHands;
        }
    }

    [MenuItem("Tools/Combat/Build Clip Events")]
    public static void Build()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        GameObject instance = Object.Instantiate(prefab);
        instance.hideFlags = HideFlags.HideAndDontSave;

        Animator animator = instance.GetComponentInChildren<Animator>();
        StringBuilder report = new StringBuilder();

        foreach (ClipEntry entry in CollectEntries())
            ProcessClip(entry, animator, report);

        Object.DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
        Debug.Log(report.ToString());
    }

    private static List<ClipEntry> CollectEntries()
    {
        List<ClipEntry> list = new List<ClipEntry>();
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { CombatRoot });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string file = System.IO.Path.GetFileNameWithoutExtension(path);

            if (file.EndsWith("_hold"))
            {
                list.Add(new ClipEntry(path, ClipKind.Hold));
                continue;
            }

            if (file.StartsWith("core_") && file.Contains("back_step"))
            {
                list.Add(new ClipEntry(path, ClipKind.Backstep));
                continue;
            }

            bool dual = file.StartsWith("straight_sword_dw_");
            bool isLight = file.Contains("_light_attack_") && !file.Contains("jump");
            bool isRelease = file.Contains("_release") && (file.Contains("charged_attack") || file.Contains("back_step_attack") || file.Contains("roll_attack"));
            bool isRun = file.Contains("_run_attack_");

            if (file.StartsWith("straight_sword_off_")) continue;
            if (file.Contains("jump")) continue;

            if (isLight) list.Add(new ClipEntry(path, ClipKind.LightAttack, dual));
            else if (isRelease || isRun) list.Add(new ClipEntry(path, ClipKind.Attack, dual));
        }

        return list;
    }

    private static void ProcessClip(ClipEntry entry, Animator animator, StringBuilder report)
    {
        ModelImporter importer = AssetImporter.GetAtPath(entry.path) as ModelImporter;
        if (importer == null) return;

        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
            clips = importer.defaultClipAnimations;
        if (clips.Length == 0) return;

        AnimationClip clip = LoadClip(entry.path);
        if (clip == null) return;

        ModelImporterClipAnimation target = clips[0];
        List<AnimationEvent> events = new List<AnimationEvent>();

        switch (entry.kind)
        {
            case ClipKind.Hold:
                target.loopTime = true;
                break;

            case ClipKind.Backstep:
                events.Add(MakeEvent(0.04f, "EnableIsInvulnerable"));
                events.Add(MakeEvent(0.38f, "DisableIsInvulnerable"));
                events.Add(MakeEvent(0.3f, "EnableCanDoBackstepAttack"));
                break;

            default:
                BuildAttackEvents(entry, clip, animator, events, report);
                break;
        }

        target.events = events.ToArray();
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    private static void BuildAttackEvents(ClipEntry entry, AnimationClip clip, Animator animator, List<AnimationEvent> events, StringBuilder report)
    {
        List<Vector2> windows = FindHitWindows(clip, animator, entry.bothHands);
        report.AppendLine(clip.name + " len=" + clip.length.ToString("F2") + " windows=" + windows.Count);

        if (windows.Count == 0)
        {
            windows.Add(new Vector2(clip.length * 0.3f, clip.length * 0.4f));
            report.AppendLine("  fallback window used");
        }

        events.Add(MakeEvent(Mathf.Max(0f, windows[0].x - 0.02f), "DrainStaminaBasedOnAttack"));

        for (int i = 0; i < windows.Count; i++)
        {
            events.Add(MakeEvent(windows[i].x, "OpenDamageCollider"));
            events.Add(MakeEvent(windows[i].y, "CloseDamageCollider"));
            report.AppendLine("  window " + windows[i].x.ToString("F2") + " - " + windows[i].y.ToString("F2"));
        }

        if (entry.kind == ClipKind.LightAttack)
            events.Add(MakeEvent(Mathf.Min(clip.length * 0.85f, windows[windows.Count - 1].y + ComboDelay), "EnableCanDoCombo"));
    }

    private static List<Vector2> FindHitWindows(AnimationClip clip, Animator animator, bool bothHands)
    {
        Transform right = animator.GetBoneTransform(HumanBodyBones.RightHand);
        Transform left = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        Transform root = animator.transform;

        int count = Mathf.CeilToInt(clip.length * SampleRate) + 1;
        float dt = 1f / SampleRate;
        float[] speed = new float[count];
        Vector3 prevRight = Vector3.zero;
        Vector3 prevLeft = Vector3.zero;
        float max = 0f;

        for (int i = 0; i < count; i++)
        {
            clip.SampleAnimation(animator.gameObject, Mathf.Min(i * dt, clip.length));

            Vector3 r = root.InverseTransformPoint(right.position);
            Vector3 l = root.InverseTransformPoint(left.position);

            if (i > 0)
            {
                float sr = (r - prevRight).magnitude / dt;
                float sl = bothHands ? (l - prevLeft).magnitude / dt : 0f;
                speed[i] = Mathf.Max(sr, sl);
                max = Mathf.Max(max, speed[i]);
            }

            prevRight = r;
            prevLeft = l;
        }

        List<Vector2> windows = new List<Vector2>();
        if (max <= 0.01f) return windows;

        float threshold = max * WindowThreshold;
        bool inside = false;
        float start = 0f;

        for (int i = 1; i < count; i++)
        {
            bool above = speed[i] >= threshold;
            float time = i * dt;

            if (above && !inside)
            {
                inside = true;
                start = time;
            }
            else if (!above && inside)
            {
                inside = false;
                AddWindow(windows, start, time);
            }
        }

        if (inside)
            AddWindow(windows, start, clip.length);

        while (windows.Count > MaxWindows)
            windows.RemoveAt(windows.Count - 1);

        return windows;
    }

    private static void AddWindow(List<Vector2> windows, float start, float end)
    {
        if (windows.Count > 0 && start - windows[windows.Count - 1].y < MergeGap)
        {
            Vector2 last = windows[windows.Count - 1];
            windows[windows.Count - 1] = new Vector2(last.x, end);
            return;
        }

        if (end - start < MinWindow) end = start + MinWindow;
        windows.Add(new Vector2(start, end));
    }

    private static AnimationClip LoadClip(string path)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            AnimationClip clip = asset as AnimationClip;
            if (clip != null && !clip.name.StartsWith("__preview")) return clip;
        }

        return null;
    }

    private static AnimationEvent MakeEvent(float time, string function)
    {
        return new AnimationEvent { time = time, functionName = function };
    }
}
