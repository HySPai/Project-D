using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    private const string GuardLayerName = "Guard";

    private int guardLayerIndex = -1;

    public void Initialize(PlayerState state)
    {
        base.Initialize(state);

        if (animator != null)
            guardLayerIndex = animator.GetLayerIndex(GuardLayerName);
    }

    public void CrossFadeGuard(string stateName, float fadeDuration)
    {
        if (animator == null || guardLayerIndex < 0 || string.IsNullOrEmpty(stateName)) return;

        animator.CrossFadeInFixedTime(stateName, fadeDuration, guardLayerIndex, 0f);
    }
}
