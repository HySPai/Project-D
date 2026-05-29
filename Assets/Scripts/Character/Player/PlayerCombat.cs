using UnityEngine;

public class PlayerCombat : CharacterCombatBase
{
    public override void Attack()
    {
        if (state == null) return;

        if (state.IsDead) return;
        if (state.IsAttacking) return;

        state.SetAttacking(true);
    }
}