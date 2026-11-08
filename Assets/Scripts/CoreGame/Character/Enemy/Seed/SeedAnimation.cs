using UnityEngine;

public class SeedAnimation : EnemyAnimation
{
    private float horizontalTarget;

    public void SetHorizontalTarget(float value) => horizontalTarget = value;

    protected override void UpdateLocomotionParameters(float moveAmount)
    {
        animator.SetFloat(horizontalHash, horizontalTarget, movementDampTime, Time.deltaTime);
    }
}
