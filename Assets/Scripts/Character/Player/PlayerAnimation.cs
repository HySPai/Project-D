using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    [SerializeField] private string moveParameter = "Move";

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

    private void OnAnimatorMove()
    {
        if (state == null)
            return;

        if (!state.ApplyRootMotion)
            return;

        Vector3 delta = animator.deltaPosition;

        if (state.IsRolling)
        {
            delta *= state.RollDistanceMultiplier;
        }

        Vector3 targetPosition =
            transform.position + delta;

        if (!CanMoveToPosition(targetPosition))
            return;

        transform.position = targetPosition;
        transform.rotation *= animator.deltaRotation;
    }
    private bool CanMoveToPosition(Vector3 position)
    {
        Vector3 center =
            position + Vector3.up * 0.2f;

        if (!Physics.Raycast(
                center,
                Vector3.down,
                out RaycastHit hit,
                ((PlayerState)state).GroundCheckDistance,
                ((PlayerState)state).GroundLayer))
        {
            return false;
        }

        float heightDiff =
            position.y - hit.point.y;

        return heightDiff <=
               ((PlayerState)state).MaxStepDownHeight;
    }
}