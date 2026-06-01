using UnityEngine;

public class ResetActionFlag : StateMachineBehaviour
{
    private CharacterControllerBase character;

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        if (character == null)
        {
            character = animator.GetComponent<CharacterControllerBase>();
        }

        if (character == null)
            return;

        CharacterStateBase state = character.GetState;

        state.SetAttacking(false);

        state.SetApplyRootMotion(false);
        state.SetCanRotate(true);
        state.SetCanMove(true);
        state.SetRolling(false);

        character.GetCombat.DisableCanDoCombo();
        character.GetCombat.DisableCanDoRollingAttack();
    }
}