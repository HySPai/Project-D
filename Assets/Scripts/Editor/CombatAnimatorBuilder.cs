using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CombatAnimatorBuilder
{
    private const string ControllerPath = "Assets/Animations/Humanoid.controller";
    private const string ActionLayerName = "ActionOverride";
    private const string GuardLayerName = "Guard";
    private const string EmptyStateName = "Empty";
    private const string GuardEmptyStateName = "Guard_Empty";
    private const string SearchRoot = "Assets";

    private static readonly string[] ManagedActionPrefixes =
    {
        "Main_Heavy", "Main_Backstep", "Backstep_", "Th_", "Dw_", "Guard_"
    };

    private static readonly string[] GuardSets = { "Off", "Th", "Dw" };

    private static readonly Dictionary<string, string> GuardSetPrefix = new Dictionary<string, string>
    {
        { "Off", "straight_sword_off_" },
        { "Th", "straight_sword_th_" },
        { "Dw", "straight_sword_dw_" }
    };

    private static readonly Dictionary<string, string> StunClip = new Dictionary<string, string>
    {
        { "Off", "straight_sword_off_guard_block_stun_01" },
        { "Th", "straight_sword_th_guard_block_stun_01" },
        { "Dw", "straight_sword_dw_guard_stun_01" }
    };

    private static readonly Dictionary<string, AnimationClip> clipCache = new Dictionary<string, AnimationClip>();

    [MenuItem("Tools/Combat/Build Animator States")]
    public static void Build()
    {
        clipCache.Clear();

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError("Humanoid.controller not found");
            return;
        }

        BuildActionLayer(controller);
        BuildGuardLayer(controller);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("CombatAnimatorBuilder: done");
    }

    private static void BuildActionLayer(AnimatorController controller)
    {
        int index = FindLayer(controller, ActionLayerName);
        AnimatorStateMachine machine = controller.layers[index].stateMachine;

        RemoveManagedStates(machine);

        AnimatorState empty = FindState(machine, EmptyStateName);

        AddAction(machine, empty, "Backstep_Main", "core_main_back_step_medium_01", 0.9f, 0.15f);
        AddAction(machine, empty, "Backstep_Th", "core_th_back_step_medium_01", 0.9f, 0.15f);

        for (int i = 1; i <= 3; i++)
            AddHeavy(machine, empty, "Main_Heavy" + i, "straight_sword_main_charged_attack_0" + i, 0.85f);

        AddAction(machine, empty, "Main_Backstep_Attack_01", "straight_sword_main_back_step_attack_01_release", 0.85f, 0.2f);
        AddAction(machine, empty, "Main_Backstep_Attack_02", "straight_sword_main_back_step_attack_02_release", 0.85f, 0.2f);

        for (int i = 1; i <= 3; i++)
            AddAction(machine, empty, "Th_Light_0" + i, "straight_sword_th_light_attack_0" + i, 0.85f, 0.15f);
        AddAction(machine, empty, "Th_Run_Attack", "straight_sword_th_run_attack_01", 0.85f, 0.2f);
        AddAction(machine, empty, "Th_Roll_Attack_01", "straight_sword_th_roll_attack_01_release", 0.85f, 0.2f);
        AddAction(machine, empty, "Th_Roll_Attack_02", "straight_sword_th_roll_attack_02_release", 0.85f, 0.2f);
        AddHeavy(machine, empty, "Th_Heavy1", "straight_sword_th_charged_attack_01", 0.85f);
        AddHeavy(machine, empty, "Th_Heavy2", "straight_sword_th_charged_attack_02", 0.85f);
        AddAction(machine, empty, "Th_Backstep_Attack_01", "straight_sword_th_back_step_attack_01_release", 0.85f, 0.2f);
        AddAction(machine, empty, "Th_Backstep_Attack_02", "straight_sword_th_back_step_attack_02_release", 0.85f, 0.2f);

        for (int i = 1; i <= 2; i++)
            AddAction(machine, empty, "Dw_Light_0" + i, "straight_sword_dw_light_attack_0" + i, 0.85f, 0.15f);
        AddAction(machine, empty, "Dw_Run_Attack", "straight_sword_dw_run_attack_01", 0.85f, 0.2f);
        AddAction(machine, empty, "Dw_Roll_Attack_01", "straight_sword_dw_roll_attack_01_release", 0.85f, 0.2f);
        AddAction(machine, empty, "Dw_Roll_Attack_02", "straight_sword_dw_roll_attack_02_release", 0.85f, 0.2f);
        for (int i = 1; i <= 4; i++)
            AddAction(machine, empty, "Dw_Backstep_Attack_0" + i, "straight_sword_dw_back_step_attack_0" + i + "_release", 0.85f, 0.2f);

        foreach (string set in GuardSets)
        {
            string prefix = GuardSetPrefix[set];
            string breakClip = prefix + (set == "Off" ? "guard_break_01" : "guard_break_01");
            AddAction(machine, empty, "Guard_" + set + "_Break", breakClip, 0.55f, 0.1f);
            AddAction(machine, empty, "Guard_" + set + "_Stun", StunClip[set], 0.33f, 0.1f);
        }
    }

    private static void BuildGuardLayer(AnimatorController controller)
    {
        int existing = FindLayer(controller, GuardLayerName, false);
        if (existing >= 0)
            controller.RemoveLayer(existing);

        controller.AddLayer(GuardLayerName);
        AnimatorControllerLayer[] layers = controller.layers;
        int last = layers.Length - 1;
        int actionIndex = FindLayer(controller, ActionLayerName);

        AnimatorControllerLayer guardLayer = layers[last];
        guardLayer.defaultWeight = 1f;
        guardLayer.blendingMode = AnimatorLayerBlendingMode.Override;

        for (int i = last; i > actionIndex; i--)
            layers[i] = layers[i - 1];
        layers[actionIndex] = guardLayer;
        controller.layers = layers;

        int guardIndex = FindLayer(controller, GuardLayerName);
        AnimatorStateMachine machine = controller.layers[guardIndex].stateMachine;

        AnimatorState empty = machine.AddState(GuardEmptyStateName);
        machine.defaultState = empty;

        foreach (string set in GuardSets)
            BuildGuardSet(controller, guardIndex, machine, empty, set);
    }

    private static void BuildGuardSet(AnimatorController controller, int layerIndex, AnimatorStateMachine machine, AnimatorState empty, string set)
    {
        string prefix = GuardSetPrefix[set];
        string tag = prefix + "guard_";
        bool dual = set == "Dw";

        BlendTree tree;
        AnimatorState loco = controller.CreateBlendTreeInController("Guard_" + set + "_Loco", out tree, layerIndex);
        tree.blendType = BlendTreeType.FreeformDirectional2D;
        tree.blendParameter = "Horizontal";
        tree.blendParameterY = "Vertical";
        tree.useAutomaticThresholds = false;

        const float d = 0.7071f;
        tree.AddChild(Clip(tag + "idle_01"), new Vector2(0f, 0f));
        tree.AddChild(Clip(prefix + "walk_guard_F_01"), new Vector2(0f, 1f));
        tree.AddChild(Clip(prefix + "walk_guard_B_01"), new Vector2(0f, -1f));
        tree.AddChild(Clip(prefix + "walk_guard_L_01"), new Vector2(-1f, 0f));
        tree.AddChild(Clip(prefix + "walk_guard_R_01"), new Vector2(1f, 0f));
        tree.AddChild(Clip(prefix + "walk_guard_FL_01"), new Vector2(-d, d));
        tree.AddChild(Clip(prefix + "walk_guard_FR_01"), new Vector2(d, d));
        tree.AddChild(Clip(prefix + "walk_guard_BL_01"), new Vector2(-d, -d));
        tree.AddChild(Clip(prefix + "walk_guard_BR_01"), new Vector2(d, -d));

        AnimatorState up = AddGuardState(machine, "Guard_" + set + "_Up", Clip(tag + "up_01"));
        Link(up, loco, 0.95f, 0.05f);

        AnimatorState down = AddGuardState(machine, "Guard_" + set + "_Down", Clip(tag + "down_01"));
        Link(down, empty, 0.9f, 0.1f);

        string pingClip = dual ? tag + "block_ping_01" : tag + "block_ping_01";
        AddReaction(machine, loco, "Guard_" + set + "_Parry", pingClip);
        AddReaction(machine, loco, "Guard_" + set + "_BlockLight", tag + "block_light_01");
        AddReaction(machine, loco, "Guard_" + set + "_BlockMedium", tag + "block_medium_01");
        AddReaction(machine, loco, "Guard_" + set + "_BlockHeavy", tag + "block_heavy_01");
    }

    private static void AddReaction(AnimatorStateMachine machine, AnimatorState back, string stateName, string clipName)
    {
        AnimatorState state = AddGuardState(machine, stateName, Clip(clipName));
        Link(state, back, 0.9f, 0.1f);
    }

    private static AnimatorState AddGuardState(AnimatorStateMachine machine, string name, AnimationClip clip)
    {
        AnimatorState state = machine.AddState(name);
        state.motion = clip;
        state.writeDefaultValues = true;
        return state;
    }

    private static void AddAction(AnimatorStateMachine machine, AnimatorState empty, string name, string clipName, float exitTime, float duration)
    {
        AnimatorState state = machine.AddState(name);
        state.motion = Clip(clipName);
        state.writeDefaultValues = true;
        Link(state, empty, exitTime, duration);
    }

    private static void AddHeavy(AnimatorStateMachine machine, AnimatorState empty, string baseName, string clipBase, float releaseExit)
    {
        AnimatorState charge = machine.AddState(baseName + "_Charge");
        charge.motion = Clip(clipBase + "_charge");
        charge.writeDefaultValues = true;

        AnimatorState hold = machine.AddState(baseName + "_Hold");
        hold.motion = Clip(clipBase + "_hold");
        hold.writeDefaultValues = true;

        Link(charge, hold, 0.98f, 0.05f);

        AddAction(machine, empty, baseName + "_Release", clipBase + "_release", releaseExit, 0.2f);
        AddAction(machine, empty, baseName + "_ReleaseFull", clipBase + "_release_full", releaseExit, 0.2f);
    }

    private static void Link(AnimatorState from, AnimatorState to, float exitTime, float duration)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = exitTime;
        transition.hasFixedDuration = true;
        transition.duration = duration;
        transition.canTransitionToSelf = false;
    }

    private static void RemoveManagedStates(AnimatorStateMachine machine)
    {
        List<AnimatorState> toRemove = new List<AnimatorState>();

        foreach (ChildAnimatorState child in machine.states)
        {
            foreach (string prefix in ManagedActionPrefixes)
            {
                if (child.state.name.StartsWith(prefix))
                {
                    toRemove.Add(child.state);
                    break;
                }
            }
        }

        foreach (AnimatorState state in toRemove)
            machine.RemoveState(state);
    }

    private static int FindLayer(AnimatorController controller, string name, bool required = true)
    {
        AnimatorControllerLayer[] layers = controller.layers;
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].name == name) return i;
        }

        if (required)
            Debug.LogError("Layer not found: " + name);
        return -1;
    }

    private static AnimatorState FindState(AnimatorStateMachine machine, string name)
    {
        foreach (ChildAnimatorState child in machine.states)
        {
            if (child.state.name == name) return child.state;
        }

        return null;
    }

    private static AnimationClip Clip(string clipName)
    {
        if (clipCache.TryGetValue(clipName, out AnimationClip cached)) return cached;

        string[] guids = AssetDatabase.FindAssets(clipName + " t:Model", new[] { SearchRoot });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != clipName) continue;

            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                AnimationClip clip = asset as AnimationClip;
                if (clip != null && !clip.name.StartsWith("__preview"))
                {
                    clipCache[clipName] = clip;
                    return clip;
                }
            }
        }

        Debug.LogError("Clip not found: " + clipName);
        return null;
    }
}
