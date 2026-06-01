using UnityEngine;

public abstract class CharacterCombatBase : MonoBehaviour
{
    protected CharacterStateBase state;
    [SerializeField] protected Collider colAttack;

    [Header("Attack Type")]
    public AttackType currentAttackType;

    [Header("Last Attack Animation Performed")]
    public string lastAttackAnimationPerformed;

    [Header("Lock On Transform")]
    public Transform lockOnTransform;

    [Header("Attack Flags")]
    public bool canPerformRollingAttack = false;
    public bool canPerformBackstepAttack = false;
    public bool canBlock = true;

    [Header("Attack Animations")]
    [SerializeField] protected string light_Attack_01 = "Main_Light_Attack_01";
    [SerializeField] protected string light_Attack_02 = "Main_Light_Attack_02";
    [SerializeField] protected string roll_Attack_01 = "Main_Roll_Attack_01";
    [SerializeField] protected string run_Attack_01 = "Main_Run_Attack_01";

    public virtual void Initialize(CharacterStateBase state)
    {
        this.state = state;
    }

    public abstract void Attack();

    public void EnableCanDoRollingAttack()
    {
        canPerformRollingAttack = true;
    }

    public void DisableCanDoRollingAttack()
    {
        canPerformRollingAttack = false;
    }

    public virtual void EnableCanDoCombo()
    {

    }

    public virtual void DisableCanDoCombo()
    {

    }

    protected void PlayAttackAnimation(
    string animationName,
    CharacterControllerBase character)
    {
        state.SetAttacking(true);

        state.SetCanMove(false);
        state.SetCanRotate(false);
        state.SetApplyRootMotion(true);

        lastAttackAnimationPerformed = animationName;

        character.GetAnimation.PlayTargetAnimation(
            animationName,
            true);
    }

    protected string GetLightAttackAnimation()
    {
        if (lastAttackAnimationPerformed == light_Attack_01)
        {
            return light_Attack_02;
        }

        return light_Attack_01;
    }
}