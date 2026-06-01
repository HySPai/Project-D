using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    [SerializeField] private string moveParameter = "Move";
    [SerializeField] private string rollParameter = "Roll";

    private float currentMoveValue;
    public void Initialize(PlayerState state)
    {
        this.state = state;

        base.Initialize(state);
    }
    public override void UpdateAnimation(float moveAmount)
    {
        float targetMoveValue = 0f;

        animator.applyRootMotion = state.ApplyRootMotion;
        if (state != null)
        {
            if (state.IsAttacking)
            {
                targetMoveValue = 0f;
            }
            else
            {
                targetMoveValue = moveAmount;
            }
        }

        currentMoveValue = Mathf.MoveTowards(currentMoveValue, targetMoveValue, state.MoveSmoothSpeed * Time.deltaTime);

        animator.SetFloat(moveParameter, currentMoveValue);
    }
}