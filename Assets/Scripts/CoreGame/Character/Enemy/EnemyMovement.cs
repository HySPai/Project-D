using UnityEngine;

public class EnemyMovement : CharacterMovementBase
{
    private EnemyState state;

    public void Initialize(EnemyState state)
    {
        this.state = state;
    }

    public override void SetInput(Vector2 input)
    {

    }

    public override void Move()
    {

    }

    public override void Roll()
    {

    }

    public override Vector3 GetMoveDirection()
    {
        return Vector3.zero;
    }
}