using UnityEngine;

public abstract class CharacterMovementBase : MonoBehaviour
{
    public abstract void SetInput(Vector2 input);

    public abstract void Move();
}