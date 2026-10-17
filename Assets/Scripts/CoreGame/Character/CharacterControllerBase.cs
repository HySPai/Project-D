using UnityEngine;
using MMF_Player = MoreMountains.Feedbacks.MMF_Player;

[RequireComponent(typeof(CharacterController))]
public abstract class CharacterControllerBase : MonoBehaviour, ICharacter
{
    [SerializeField] protected CharacterController characterController;
    [SerializeField] protected Collider[] hurtboxes;

    [Header("Feedbacks (Feel)")]
    [SerializeField] protected MMF_Player damageFeedback;
    [SerializeField] protected MMF_Player deathFeedback;

    public CharacterController CharacterController => characterController;
    public MMF_Player DamageFeedback => damageFeedback;
    public MMF_Player DeathFeedback => deathFeedback;

    public abstract CharacterStateBase GetState { get; }
    public abstract CharacterMovementBase GetMovement { get; }
    public abstract CharacterCombatBase GetCombat { get; }
    public abstract CharacterAnimationBase GetAnimation { get; }

    protected virtual void OnEnable()
    {
        CombatTargetRegistry.Register(characterController, this);

        if (hurtboxes == null) return;
        for (int i = 0; i < hurtboxes.Length; i++)
        {
            if (hurtboxes[i] != null)
                CombatTargetRegistry.Register(hurtboxes[i], this);
        }
    }

    protected virtual void OnDisable()
    {
        CombatTargetRegistry.Unregister(characterController);

        if (hurtboxes == null) return;
        for (int i = 0; i < hurtboxes.Length; i++)
            CombatTargetRegistry.Unregister(hurtboxes[i]);
    }

    protected virtual void Reset()
    {
        characterController = GetComponent<CharacterController>();
    }
}