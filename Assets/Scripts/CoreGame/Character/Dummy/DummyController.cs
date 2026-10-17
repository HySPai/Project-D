using UnityEngine;

/// <summary>
/// Static, unkillable hit target for testing combat feel. Registers with CombatTargetRegistry
/// through CharacterControllerBase, so PlayerCombat hits it like any enemy.
/// </summary>
public class DummyController : CharacterControllerBase
{
    [SerializeField] private DummyState state;

    public override CharacterStateBase GetState => state;
    public override CharacterMovementBase GetMovement => null;
    public override CharacterCombatBase GetCombat => null;
    public override CharacterAnimationBase GetAnimation => null;

    private void Awake()
    {
        state.Initialize(this);
    }
}
