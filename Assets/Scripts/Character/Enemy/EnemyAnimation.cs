using UnityEngine;

public class EnemyAnimation : CharacterAnimationBase
{
    public void Initialize(EnemyState state)
    {
        base.Initialize(state);
    }

    public override void UpdateAnimation(float moveAmount)
    {
        if (state == null)
            return;

        animator.applyRootMotion = state.ApplyRootMotion;
    }
}