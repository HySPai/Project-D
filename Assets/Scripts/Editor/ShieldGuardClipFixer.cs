using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ShieldGuardClipFixer
{
    public const string OutputFolder = "Assets/Animations/Combat/off/ShieldFix";
    public const string ParryClipName = "straight_sword_off_guard_parry_bash";
    private const string SourceFolder = "Assets/Animations/Combat/off";
    private const string ParryBaseClip = "straight_sword_off_guard_idle_01";
    private const float SampleRate = 30f;
    private const float ParryLength = 0.5f;

    private static readonly Dictionary<string, float> ParryOffset = new Dictionary<string, float>
    {
        { "Left Shoulder Down-Up", -1.39f },
        { "Left Shoulder Front-Back", 0.04f },
        { "Left Arm Down-Up", 0.23f },
        { "Left Arm Front-Back", 0.06f },
        { "Left Arm Twist In-Out", 0.17f },
        { "Left Forearm Stretch", 0.62f },
        { "Left Forearm Twist In-Out", 0.08f },
        { "Left Hand Down-Up", -0.07f },
        { "Left Hand In-Out", -0.08f },
        { "Spine Twist Left-Right", 0.03f },
        { "Chest Twist Left-Right", -0.26f },
        { "UpperChest Twist Left-Right", -0.07f }
    };

    private static readonly Vector2[] ParryKeys =
    {
        new Vector2(0f, 0f),
        new Vector2(0.06f, -0.2f),
        new Vector2(0.15f, 1.05f),
        new Vector2(0.22f, 1f),
        new Vector2(0.3f, 0.85f),
        new Vector2(ParryLength, 0f)
    };

    private enum Blend { Full, In, Out, OutEarly }

    private static readonly Dictionary<string, float> MuscleOffset = new Dictionary<string, float>
    {
        { "Left Shoulder Down-Up", 0.51f },
        { "Left Shoulder Front-Back", 1.71f },
        { "Left Arm Down-Up", -0.21f },
        { "Left Arm Front-Back", -0.05f },
        { "Left Forearm Stretch", -0.76f },
        { "Left Forearm Twist In-Out", 0.86f },
        { "Left Hand Down-Up", -0.37f },
        { "Left Hand In-Out", 0.19f }
    };

    private static readonly Dictionary<string, Blend> Clips = new Dictionary<string, Blend>
    {
        { "straight_sword_off_guard_idle_01", Blend.Full },
        { "straight_sword_off_guard_up_01", Blend.In },
        { "straight_sword_off_guard_down_01", Blend.Out },
        { "straight_sword_off_guard_block_light_01", Blend.Full },
        { "straight_sword_off_guard_block_medium_01", Blend.Full },
        { "straight_sword_off_guard_block_heavy_01", Blend.Full },
        { "straight_sword_off_guard_block_ping_01", Blend.Full },
        { "straight_sword_off_guard_block_stun_01", Blend.OutEarly },
        { "straight_sword_off_guard_break_01", Blend.OutEarly },
        { "straight_sword_off_walk_guard_F_01", Blend.Full },
        { "straight_sword_off_walk_guard_B_01", Blend.Full },
        { "straight_sword_off_walk_guard_L_01", Blend.Full },
        { "straight_sword_off_walk_guard_R_01", Blend.Full },
        { "straight_sword_off_walk_guard_FL_01", Blend.Full },
        { "straight_sword_off_walk_guard_FR_01", Blend.Full },
        { "straight_sword_off_walk_guard_BL_01", Blend.Full },
        { "straight_sword_off_walk_guard_BR_01", Blend.Full }
    };

    [MenuItem("Tools/Combat/Fix Shield Guard Clips")]
    public static void Fix()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder(SourceFolder, "ShieldFix");

        int written = 0;
        foreach (KeyValuePair<string, Blend> entry in Clips)
        {
            AnimationClip source = LoadSource(entry.Key);
            if (source == null)
            {
                Debug.LogError("ShieldGuardClipFixer: clip not found " + entry.Key);
                continue;
            }

            WriteFixed(source, entry.Key, entry.Value);
            written++;
        }

        AnimationClip parryBase = LoadSource(ParryBaseClip);
        if (parryBase != null)
        {
            WriteParry(parryBase);
            written++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("ShieldGuardClipFixer: wrote " + written + " clips");
    }

    private static void WriteParry(AnimationClip source)
    {
        AnimationClip parry = new AnimationClip { name = ParryClipName, frameRate = source.frameRate };
        int frames = Mathf.CeilToInt(ParryLength * SampleRate);

        HashSet<string> present = new HashSet<string>();
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(source))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(source, binding);
            bool muscle = binding.type == typeof(Animator) && System.Array.IndexOf(HumanTrait.MuscleName, binding.propertyName) >= 0;
            float guard = muscle && MuscleOffset.TryGetValue(binding.propertyName, out float g) ? g : 0f;
            float bash = muscle && ParryOffset.TryGetValue(binding.propertyName, out float b) ? b : 0f;
            if (muscle) present.Add(binding.propertyName);

            AnimationUtility.SetEditorCurve(parry, binding, ParryCurve(curve, frames, guard, bash, muscle));
        }

        foreach (KeyValuePair<string, float> entry in ParryOffset)
        {
            if (present.Contains(entry.Key)) continue;
            MuscleOffset.TryGetValue(entry.Key, out float guard);
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), entry.Key);
            AnimationUtility.SetEditorCurve(parry, binding, ParryCurve(AnimationCurve.Constant(0f, ParryLength, 0f), frames, guard, entry.Value, true));
        }

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(source);
        settings.loopTime = false;
        settings.startTime = 0f;
        settings.stopTime = ParryLength;
        AnimationUtility.SetAnimationClipSettings(parry, settings);

        Save(parry, OutputFolder + "/" + ParryClipName + ".anim");
    }

    private static AnimationCurve ParryCurve(AnimationCurve curve, int frames, float guard, float bash, bool clamp)
    {
        Keyframe[] keys = new Keyframe[frames + 1];
        for (int i = 0; i <= frames; i++)
        {
            float time = ParryLength * i / frames;
            float value = curve.Evaluate(time) + guard + bash * ParryWeight(time);
            keys[i] = new Keyframe(time, clamp ? Mathf.Clamp(value, -1f, 1f) : value);
        }

        return Smooth(new AnimationCurve(keys));
    }

    private static float ParryWeight(float time)
    {
        for (int i = 1; i < ParryKeys.Length; i++)
        {
            if (time > ParryKeys[i].x) continue;
            Vector2 from = ParryKeys[i - 1];
            Vector2 to = ParryKeys[i];
            float t = Mathf.SmoothStep(0f, 1f, (time - from.x) / Mathf.Max(0.0001f, to.x - from.x));
            return Mathf.Lerp(from.y, to.y, t);
        }

        return ParryKeys[ParryKeys.Length - 1].y;
    }

    private static AnimationClip LoadSource(string clipName)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(SourceFolder + "/" + clipName + ".fbx"))
        {
            AnimationClip clip = asset as AnimationClip;
            if (clip != null && !clip.name.StartsWith("__preview")) return clip;
        }

        return null;
    }

    private static void WriteFixed(AnimationClip source, string clipName, Blend blend)
    {
        AnimationClip fixedClip = new AnimationClip { name = clipName, frameRate = source.frameRate };

        HashSet<string> present = new HashSet<string>();
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(source))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(source, binding);
            if (binding.type == typeof(Animator) && MuscleOffset.TryGetValue(binding.propertyName, out float offset))
            {
                curve = Offset(curve, source.length, offset, blend);
                present.Add(binding.propertyName);
            }

            AnimationUtility.SetEditorCurve(fixedClip, binding, curve);
        }

        foreach (KeyValuePair<string, float> muscle in MuscleOffset)
        {
            if (present.Contains(muscle.Key)) continue;
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), muscle.Key);
            AnimationUtility.SetEditorCurve(fixedClip, binding, Offset(AnimationCurve.Constant(0f, source.length, 0f), source.length, muscle.Value, blend));
        }

        AnimationUtility.SetAnimationClipSettings(fixedClip, AnimationUtility.GetAnimationClipSettings(source));
        AnimationUtility.SetAnimationEvents(fixedClip, AnimationUtility.GetAnimationEvents(source));

        Save(fixedClip, OutputFolder + "/" + clipName + ".anim");
    }

    private static void Save(AnimationClip clip, string path)
    {
        AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(clip, existing);
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(clip);
        }
        else
        {
            AssetDatabase.CreateAsset(clip, path);
        }
    }

    private static AnimationCurve Smooth(AnimationCurve curve)
    {
        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        return curve;
    }

    private static AnimationCurve Offset(AnimationCurve curve, float length, float offset, Blend blend)
    {
        int frames = Mathf.Max(1, Mathf.CeilToInt(length * SampleRate));
        Keyframe[] keys = new Keyframe[frames + 1];

        for (int i = 0; i <= frames; i++)
        {
            float time = length * i / frames;
            float value = curve.Evaluate(time) + offset * Weight(blend, length > 0f ? time / length : 1f);
            keys[i] = new Keyframe(time, Mathf.Clamp(value, -1f, 1f));
        }

        return Smooth(new AnimationCurve(keys));
    }

    private static float Weight(Blend blend, float t)
    {
        switch (blend)
        {
            case Blend.In: return Mathf.SmoothStep(0f, 1f, t / 0.6f);
            case Blend.Out: return 1f - Mathf.SmoothStep(0f, 1f, t / 0.6f);
            case Blend.OutEarly: return 1f - Mathf.SmoothStep(0f, 1f, t / 0.35f);
            default: return 1f;
        }
    }
}
