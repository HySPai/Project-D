using UnityEngine;

/// <summary>
/// Clears action flags when the animator returns to a free state. A StateMachineBehaviour cannot hold scene
/// references, so the owner is looked up once from the Animator's GameObject and cached.
/// </summary>
public class ResetActionFlag : StateMachineBehaviour
{
    private CharacterControllerBase character;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
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

        character.GetAnimation.SetPlaybackSpeed(1f);

        character.GetCombat.DisableCanDoCombo();
        character.GetCombat.DisableCanDoRollingAttack();
        character.GetCombat.DisableCanDoBackstepAttack();
    }
}