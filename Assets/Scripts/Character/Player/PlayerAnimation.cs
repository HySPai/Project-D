using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    [SerializeField] private Animator animator;
    [SerializeField] private string moveParameter = "Move";
    [SerializeField] private string attackParameter = "Attack";
    [SerializeField] private string rollParameter = "Roll";

    private float currentMoveValue;
    private PlayerState state;

    public void Initialize(PlayerState state)
    {
        this.state = state;
    }

    public override void UpdateAnimation(float moveAmount)
    {
        float targetMoveValue = moveAmount;

        if (state != null)
        {
            if (state.IsAttacking)
            {
                targetMoveValue = 0f;
            }
            else if (state.IsRunning && moveAmount > 0f)
            {
                targetMoveValue = 1.5f;
            }
            else if (state.IsWalking && moveAmount > 0f)
            {
                targetMoveValue = 0.5f;
            }
            else if (moveAmount > 0f)
            {
                targetMoveValue = 1f;
            }
            else
            {
                targetMoveValue = 0f;
            }

            animator.SetBool(attackParameter, state.IsAttacking);
        }

        currentMoveValue = Mathf.MoveTowards(
            currentMoveValue,
            targetMoveValue,
            state.MoveSmoothSpeed * Time.deltaTime);

        animator.SetFloat(moveParameter, currentMoveValue);
    }
}