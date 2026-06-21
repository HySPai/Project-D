using UnityEngine;

public class EnemyController : CharacterControllerBase
{
    [SerializeField] private EnemyState state;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyCombat combat;
    [SerializeField] private EnemyAnimation anim;

    public override CharacterStateBase GetState => state;
    public override CharacterMovementBase GetMovement => movement;
    public override CharacterCombatBase GetCombat => combat;
    public override CharacterAnimationBase GetAnimation => anim;

    private void Awake()
    {
        state.Initialize(this);
        movement.Initialize(state);
        combat.Initialize(state);
        anim.Initialize(state);
    }

    private void Update()
    {
        // combat.Update() (đã override) tự quét lockOnLayer -> gán lockOnTransform
        Transform target = combat.LockOnTransform;

        movement.Tick(target);
        anim.UpdateAnimation(movement.CurrentNormalizedSpeed); // đưa tốc độ thật vào blend tree
    }
}