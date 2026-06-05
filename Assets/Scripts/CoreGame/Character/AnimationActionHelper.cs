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

public static class PlayerAnimations
{
    // name,             isAction, rootMotion, canRotate, canMove
    public static readonly AnimationAction Death =
        new AnimationAction("Dead_01", false, false, false, false);

    public static readonly AnimationAction RollForward =
        new AnimationAction("Roll_Forward_01", true, true, false, false);

    // thêm đòn khác ở đây khi cần...
}