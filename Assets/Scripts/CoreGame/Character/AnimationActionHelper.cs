using UnityEngine;

/// <summary>An animator state to cross-fade into plus the action flags it sets. The state hash is computed once.</summary>
public readonly struct AnimationAction
{
    public readonly string name;
    public readonly int hash;
    public readonly bool isAction;
    public readonly bool rootMotion;
    public readonly bool canRotate;
    public readonly bool canMove;

    public AnimationAction(string name, bool isAction,
        bool rootMotion = true, bool canRotate = false, bool canMove = false)
    {
        this.name = name;
        hash = string.IsNullOrEmpty(name) ? 0 : Animator.StringToHash(name);
        this.isAction = isAction;
        this.rootMotion = rootMotion;
        this.canRotate = canRotate;
        this.canMove = canMove;
    }
}

/// <summary>
/// Animations shared by every character. A character that needs other clips overrides DeathAnimation or
/// GetHitAnimation in its state. Arguments: name, isAction, rootMotion, canRotate, canMove.
/// </summary>
public static class CharacterAnimations
{
    public static readonly AnimationAction Death =
        new AnimationAction("Dead_01", false, false, false, false);

    public static readonly AnimationAction RollForward =
        new AnimationAction("Roll_Forward_01", true, true, false, false);

    public static readonly AnimationAction HitForward =
        new AnimationAction("Hit_Forward", true, true, false, false);
    public static readonly AnimationAction HitBackward =
        new AnimationAction("Hit_Backward", true, true, false, false);
    public static readonly AnimationAction HitLeft =
        new AnimationAction("Hit_Left", true, true, false, false);
    public static readonly AnimationAction HitRight =
        new AnimationAction("Hit_Right", true, true, false, false);
    public static readonly AnimationAction TakeDamage =
        new AnimationAction("TakeDamage", true, true, false, false);
}
