using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    [SerializeField] private Animator animator;
    [SerializeField] private string moveParameter = "Move";
    [SerializeField] private string attackParameter = "Attack";
    [SerializeField] private string dashParameter = "Dash";
    [SerializeField] private string rollParameter = "Roll";

    private PlayerState state;

    public void Initialize(PlayerState state)
    {
        this.state = state;
    }

    public override void UpdateAnimation(Vector2 moveInput)
    {
        float moveValue = moveInput.magnitude;

        if (state != null)
        {
            if (state.IsAttacking)
            {
                moveValue = 0;
            }

            animator.SetBool(attackParameter, state.IsAttacking);
        }

        animator.SetFloat(moveParameter, moveValue);
    }
}