public readonly struct AnimationAction
{
    public readonly string name;
    public readonly bool isAction;
    public readonly bool rootMotion;
    public readonly bool canRotate;
    public readonly bool canMove;

    public AnimationAction(string name, bool isAction,
        bool rootMotion = true, bool canRotate = false, bool canMove = false)
    {
        this.name = name;
        this.isAction = isAction;
        this.rootMotion = rootMotion;
        this.canRotate = canRotate;
        this.canMove = canMove;
    }
}

// Bộ animation dùng chung cho mọi character (Player & Enemy).
// Nếu một loại character cần clip khác, override DeathAnimation/GetHitAnimation trong state của nó.
public static class CharacterAnimations
{
    // name,             isAction, rootMotion, canRotate, canMove
    public static readonly AnimationAction Death =
        new AnimationAction("Dead_01", false, false, false, false);

    public static readonly AnimationAction RollForward =
        new AnimationAction("Roll_Forward_01", true, true, false, false);

    // Hit reactions — isAction = true để khoá action, rootMotion = true.
    public static readonly AnimationAction HitForward =
        new AnimationAction("Hit_Forward", true, true, false, false);
    public static readonly AnimationAction HitBackward =
        new AnimationAction("Hit_Backward", true, true, false, false);
    public static readonly AnimationAction HitLeft =
        new AnimationAction("Hit_Left", true, true, false, false);
    public static readonly AnimationAction HitRight =
        new AnimationAction("Hit_Right", true, true, false, false);
}