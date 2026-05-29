using UnityEngine;

public class PlayerState : CharacterStateBase
{
    protected override void Die()
    {
        base.Die();

        Debug.Log("Player Dead");
    }
}