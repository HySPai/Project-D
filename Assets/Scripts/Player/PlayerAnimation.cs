using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator animator;
    public float animSmooth = 8f;

    private float animMoveValue;

    private PlayerCombat combat;

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
    }

    public void UpdateAnimation(Vector2 input)
    {
        HandleMovementAnimation(input);
        HandleAttackAnimation();
    }

    private void HandleMovementAnimation(Vector2 input)
    {
        float inputMagnitude = Mathf.Clamp01(input.magnitude);

        float deadZone = 0.05f;
        float midThreshold = 0.2f;

        float target;

        if (inputMagnitude <= deadZone)
        {
            target = 0f;
        }
        else if (inputMagnitude < midThreshold)
        {
            target = Mathf.Lerp(0f, 0.5f, (inputMagnitude - deadZone) / (midThreshold - deadZone));
        }
        else
        {
            target = Mathf.Lerp(0.5f, 1f, (inputMagnitude - midThreshold) / (1f - midThreshold));
        }

        // ❌ nếu đang attack thì đứng im animation
        if (combat != null && combat.IsAttacking())
        {
            target = 0f;
        }

        animMoveValue = Mathf.Lerp(animMoveValue, target, animSmooth * Time.deltaTime);
        animator.SetFloat("Move", animMoveValue);
    }

    private void HandleAttackAnimation()
    {
        if (combat == null) return;

        animator.SetBool("Attack", combat.IsAttacking());
    }
}