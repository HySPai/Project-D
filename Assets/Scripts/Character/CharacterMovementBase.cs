using UnityEngine;

public abstract class CharacterMovementBase : MonoBehaviour, IMovable
{
    public abstract void SetInput(Vector2 input);

    public abstract void Move();
    public abstract void Roll();
}