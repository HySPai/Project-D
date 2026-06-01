public class PlayerCombat : CharacterCombatBase
{
    private CharacterControllerBase character;

    public void Initialize(
        CharacterStateBase state,
        CharacterControllerBase character)
    {
        base.Initialize(state);
        this.character = character;
    }

    public override void Attack()
    {
        if (state == null) return;
        if (state.IsDead) return;
        if (state.IsAttacking) return;
        if (state.IsRolling) return;

        string attackAnimation = light_Attack_01;

        character.GetAnimation.PlayTargetAnimation(
            attackAnimation,
            true,
            true,
            false,
            false);

        lastAttackAnimationPerformed = attackAnimation;
    }
}