using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    [SerializeField] private Animator animator;

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

            animator.SetBool("Attack", state.IsAttacking);
        }

        animator.SetFloat("Move", moveValue);
    }
}