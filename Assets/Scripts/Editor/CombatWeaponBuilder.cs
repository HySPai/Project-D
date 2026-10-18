using UnityEditor;
using UnityEngine;

public static class CombatWeaponBuilder
{
    private const string SwordPath = "Assets/SO/Weapon/SO_WeaponItem.asset";
    private const string GreatswordPath = "Assets/SO/Weapon/SO_WeaponItem_Greatsword.asset";
    private const string DualBladesPath = "Assets/SO/Weapon/SO_WeaponItem_DualBlades.asset";

    [MenuItem("Tools/Combat/Build Weapons")]
    public static void Build()
    {
        WeaponItem sword = AssetDatabase.LoadAssetAtPath<WeaponItem>(SwordPath);
        WeaponFeelProfile feel = sword.feelProfile;

        ConfigureSword(sword);
        EditorUtility.SetDirty(sword);

        WeaponItem greatsword = LoadOrCreate(GreatswordPath);
        ConfigureGreatsword(greatsword, feel);
        EditorUtility.SetDirty(greatsword);

        WeaponItem dual = LoadOrCreate(DualBladesPath);
        ConfigureDualBlades(dual, feel);
        EditorUtility.SetDirty(dual);

        AssetDatabase.SaveAssets();
        Debug.Log("CombatWeaponBuilder: done");
    }

    private static void ConfigureSword(WeaponItem weapon)
    {
        weapon.heavyHoldThreshold = 0.35f;
        weapon.heavyAttacks = new[]
        {
            Heavy(HeavyContext.Standing, "Main_Heavy1", 45f, 20f, 2f, 160f, 1.2f),
            Heavy(HeavyContext.AfterCombo, "Main_Heavy2", 45f, 20f, 2.1f, 170f, 1.2f),
            Heavy(HeavyContext.Running, "Main_Heavy3", 50f, 24f, 2.3f, 150f, 1.2f)
        };

        weapon.backstepAnimation = "Backstep_Main";
        weapon.backstepAttack = Attack("Main_Backstep_Attack_01", 30f, 15f, 2.4f, 120f, 1f, 1.3f, AttackWeight.Heavy);

        weapon.counterMultiplier = 3f;
        weapon.guard = new GuardData
        {
            animationSet = "Off",
            canGuard = true,
            canParry = true,
            blockedDamageRatio = 1f,
            staminaPerBlockedPoint = 0.8f
        };
    }

    private static void ConfigureGreatsword(WeaponItem weapon, WeaponFeelProfile feel)
    {
        weapon.weaponName = "Greatsword";
        weapon.weaponClass = WeaponClass.Greatsword;
        weapon.feelProfile = feel;

        weapon.lightAttacks = new[]
        {
            Attack("Th_Light_01", 50f, 24f, 2.2f, 200f, 1.15f, 1.3f, AttackWeight.Heavy),
            Attack("Th_Light_02", 50f, 24f, 2.2f, 200f, 1.15f, 1.3f, AttackWeight.Heavy),
            Attack("Th_Light_03", 55f, 26f, 2.4f, 200f, 1.15f, 1.5f, AttackWeight.Heavy)
        };
        weapon.runningAttack = Attack("Th_Run_Attack", 50f, 26f, 2.4f, 170f, 1.1f, 1.5f, AttackWeight.Heavy);
        weapon.rollingAttack = Attack("Th_Roll_Attack_01", 50f, 26f, 2.3f, 200f, 1.1f, 1.4f, AttackWeight.Heavy);

        weapon.heavyHoldThreshold = 0.35f;
        weapon.heavyAttacks = new[]
        {
            Heavy(HeavyContext.Standing, "Th_Heavy1", 90f, 40f, 2.5f, 220f, 1.4f),
            Heavy(HeavyContext.AfterCombo, "Th_Heavy2", 90f, 40f, 2.5f, 220f, 1.4f)
        };

        weapon.backstepAnimation = "Backstep_Th";
        weapon.backstepAttack = Attack("Th_Backstep_Attack_01", 55f, 24f, 2.5f, 120f, 1f, 1.4f, AttackWeight.Heavy);

        weapon.counterMultiplier = 1f;
        weapon.guard = new GuardData
        {
            animationSet = "Th",
            canGuard = true,
            canParry = false,
            blockedDamageRatio = 0.5f,
            staminaPerBlockedPoint = 1f
        };
    }

    private static void ConfigureDualBlades(WeaponItem weapon, WeaponFeelProfile feel)
    {
        weapon.weaponName = "Dual Blades";
        weapon.weaponClass = WeaponClass.DualBlades;
        weapon.feelProfile = feel;

        weapon.lightAttacks = new[]
        {
            Attack("Dw_Light_01", 14f, 6f, 1.7f, 180f, 1.25f, 0.8f, AttackWeight.Light),
            Attack("Dw_Light_02", 14f, 6f, 1.7f, 180f, 1.25f, 0.8f, AttackWeight.Light)
        };
        weapon.runningAttack = Attack("Dw_Run_Attack", 20f, 10f, 1.9f, 160f, 1.2f, 1f, AttackWeight.Light);
        weapon.rollingAttack = Attack("Dw_Roll_Attack_01", 20f, 10f, 1.8f, 200f, 1.2f, 1f, AttackWeight.Light);

        weapon.heavyHoldThreshold = 0.35f;
        weapon.heavyAttacks = new HeavyAttackData[0];

        weapon.backstepAnimation = "Backstep_Main";
        weapon.backstepAttack = Attack("Dw_Backstep_Attack_01", 24f, 10f, 1.9f, 140f, 1.1f, 1f, AttackWeight.Light);

        weapon.counterMultiplier = 1f;
        weapon.guard = new GuardData
        {
            animationSet = "Dw",
            canGuard = true,
            canParry = false,
            blockedDamageRatio = 0.5f,
            staminaPerBlockedPoint = 1f
        };
    }

    private static HeavyAttackData Heavy(HeavyContext context, string stateBase, float damage, float stamina, float range, float arc, float fullChargeTime)
    {
        return new HeavyAttackData
        {
            context = context,
            chargeAnimation = stateBase + "_Charge",
            holdAnimation = stateBase + "_Hold",
            fullChargeTime = fullChargeTime,
            release = Attack(stateBase + "_Release", damage, stamina, range, arc, 1f, 1.7f, AttackWeight.Heavy),
            releaseFull = Attack(stateBase + "_ReleaseFull", damage * 1.5f, stamina, range * 1.15f, arc + 20f, 1f, 2.2f, AttackWeight.Heavy)
        };
    }

    private static AttackData Attack(string animation, float damage, float stamina, float range, float arc, float speed, float impact, AttackWeight weight)
    {
        return new AttackData
        {
            animationName = animation,
            damage = damage,
            stamina = stamina,
            range = range,
            arc = arc,
            animationSpeed = speed,
            impact = impact,
            weight = weight
        };
    }

    private static WeaponItem LoadOrCreate(string path)
    {
        WeaponItem weapon = AssetDatabase.LoadAssetAtPath<WeaponItem>(path);
        if (weapon != null) return weapon;

        weapon = ScriptableObject.CreateInstance<WeaponItem>();
        AssetDatabase.CreateAsset(weapon, path);
        return weapon;
    }
}
