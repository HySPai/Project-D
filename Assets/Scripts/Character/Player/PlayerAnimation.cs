using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    [SerializeField] private Animator animator;
    [SerializeField] private string moveParameter = "Move";
    [SerializeField] private string attackParameter = "Attack";
    [SerializeField] private string rollParameter = "Roll";

    private PlayerState state;
    private float currentMoveValue;
    public void Initialize(PlayerState state)
    {
        this.state = state;
    }

    public override void UpdateAnimation(float moveAmount)
    {
        float targetMoveValue = 0f;

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

            animator.SetBool(
                attackParameter,
                state.IsAttacking);
        }

        currentMoveValue = Mathf.MoveTowards(
            currentMoveValue,
            targetMoveValue,
            state.MoveSmoothSpeed * Time.deltaTime);

        animator.SetFloat(
            moveParameter,
            currentMoveValue);
    }
}