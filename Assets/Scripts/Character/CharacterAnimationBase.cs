using UnityEngine;

public abstract class CharacterAnimationBase : MonoBehaviour
{
    [SerializeField] protected Animator animator;

    protected CharacterStateBase state;

    public virtual void Initialize(CharacterStateBase state)
    {
        this.state = state;
    }

    public abstract void UpdateAnimation(float moveAmount);

    public virtual void ApplyRootMotion(bool value)
    {
        animator.applyRootMotion = value;
    }

    public virtual void PlayTargetAnimation(
        string targetAnimation,
        bool isPerformingAction,
        bool applyRootMotion = true,
        bool canRotate = false,
        bool canMove = false)
    {
        if (state == null)
            return;

        animator.CrossFade(targetAnimation, 0.2f);

        state.SetAttacking(isPerformingAction);
        state.SetApplyRootMotion(applyRootMotion);
        state.SetCanRotate(canRotate);
        state.SetCanMove(canMove);
    }
}