using UnityEngine;

public class PlayerState : CharacterStateBase
{
    #region Ground Check

    [Header("Ground Check")]
    [SerializeField] protected float groundCheckDistance = 5f;
    [SerializeField] protected float edgeCheckForwardDistance = 0.5f;
    [SerializeField] protected float maxStepDownHeight = 1f;

    public float GroundCheckDistance => groundCheckDistance;
    public float EdgeCheckForwardDistance => edgeCheckForwardDistance;
    public float MaxStepDownHeight => maxStepDownHeight;

    #endregion

    #region Input

    [Header("Input")]
    [SerializeField] private float fullSpeedInputThreshold = 0.7f;

    [SerializeField] private Vector2 currentInput;
    [SerializeField] private Vector2 currentCameraInput;

    public float FullSpeedInputThreshold => fullSpeedInputThreshold;
    public Vector2 CurrentInput => currentInput;
    public Vector2 CurrentCameraInput => currentCameraInput;

    #endregion

    #region Animation

    public float AnimationMoveAmount
    {
        get
        {
            float moveAmount =
                Mathf.Clamp01(
                    CurrentInput.magnitude /
                    FullSpeedInputThreshold);

            if (moveAmount <= 0.01f)
            {
                return 0f;
            }

            if (IsRunning)
            {
                return 1.5f;
            }

            return moveAmount;
        }
    }

    #endregion

    #region Input Setters

    public void SetCurrentInput(Vector2 value)
    {
        currentInput = value;
    }

    public void SetCurrentCameraInput(Vector2 value)
    {
        currentCameraInput = value;
    }

    #endregion

    protected override void Die()
    {
        base.Die();

        Debug.Log("Player Dead");
    }
}