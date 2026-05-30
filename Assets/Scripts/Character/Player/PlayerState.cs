using UnityEngine;

public class PlayerState : CharacterStateBase
{
    [Header("Ground Check")]
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected float groundCheckDistance = 5f;
    [SerializeField] protected float edgeCheckForwardDistance = 0.5f;
    [SerializeField] protected float maxStepDownHeight = 1f;

    [Header("Input Config")]
    [SerializeField] private float fullSpeedInputThreshold = 0.7f;
    [SerializeField] private Vector2 currentInput;
    [SerializeField] private Vector2 currentCameraInput;


    public float FullSpeedInputThreshold => fullSpeedInputThreshold;
    public Vector2 CurrentInput => currentInput;
    public Vector2 CurrentCameraInput => currentCameraInput;

    public LayerMask GroundLayer => groundLayer;
    public float GroundCheckDistance => groundCheckDistance;
    public float EdgeCheckForwardDistance => edgeCheckForwardDistance;
    public float MaxStepDownHeight => maxStepDownHeight;

    public float AnimationMoveAmount
    {
        get
        {
            float moveAmount = Mathf.Clamp01(CurrentInput.magnitude / FullSpeedInputThreshold);

            if (IsRunning)
            {
                return 2f;
            }

            return moveAmount;
        }
    }

    protected override void Die()
    {
        base.Die();

        Debug.Log("Player Dead");
    }
    public void SetCurrentInput(Vector2 value)
    {
        currentInput = value;
    }

    public void SetCurrentCameraInput(Vector2 value)
    {
        currentCameraInput = value;
    }
}