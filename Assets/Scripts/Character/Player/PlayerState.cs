using UnityEngine;

public class PlayerState : CharacterStateBase
{

    [Header("Ground Check")]
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected float groundCheckDistance = 5f;
    [SerializeField] protected float edgeCheckForwardDistance = 0.5f;
    [SerializeField] protected float maxStepDownHeight = 1f;

    public LayerMask GroundLayer => groundLayer;
    public float GroundCheckDistance => groundCheckDistance;
    public float EdgeCheckForwardDistance => edgeCheckForwardDistance;
    public float MaxStepDownHeight => maxStepDownHeight;

    protected override void Die()
    {
        base.Die();

        Debug.Log("Player Dead");
    }
}