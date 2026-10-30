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

    [Header("Status")]
    [SerializeField] protected BurnEffect burnEffect;
    [SerializeField] protected SlowEffect slowEffect;

    public CharacterController CharacterController => characterController;
    public MMF_Player DamageFeedback => damageFeedback;
    public MMF_Player DeathFeedback => deathFeedback;
    public BurnEffect GetBurn => burnEffect;
    public SlowEffect GetSlow => slowEffect;

    public abstract CharacterStateBase GetState { get; }
    public abstract CharacterMovementBase GetMovement { get; }
    public abstract CharacterCombatBase GetCombat { get; }
    public abstract CharacterAnimationBase GetAnimation { get; }

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    public Vector3 SpawnPosition => spawnPosition;
    public Quaternion SpawnRotation => spawnRotation;

    protected virtual void Awake()
    {
        SetSpawnPoint(transform.position, transform.rotation);
        if (burnEffect != null) burnEffect.Initialize(this);
        if (slowEffect != null) slowEffect.Initialize(this);
    }

    /// <summary>Changes where this character comes back after a world reset, e.g. a lit rest point.</summary>
    public void SetSpawnPoint(Vector3 position, Quaternion rotation)
    {
        spawnPosition = position;
        spawnRotation = rotation;
    }

    /// <summary>Brings the character back to its spawn point alive, with full vitals and no leftover actions.</summary>
    public void ResetToSpawn()
    {
        BeforeReset();
        if (burnEffect != null) burnEffect.Stop();
        if (slowEffect != null) slowEffect.Stop();
        TeleportTo(spawnPosition, spawnRotation);

        CharacterStateBase state = GetState;
        if (state != null) state.ResetState();

        CharacterCombatBase combat = GetCombat;
        if (combat != null) combat.ResetCombat();

        CharacterAnimationBase animation = GetAnimation;
        if (animation != null) animation.ResetAnimator();

        OnReset();
    }

    /// <summary>Moves the character to a new place without touching its vitals, e.g. when changing area.</summary>
    public void Teleport(Vector3 position, Quaternion rotation) => TeleportTo(position, rotation);

    protected virtual void BeforeReset() { }

    protected virtual void TeleportTo(Vector3 position, Quaternion rotation)
    {
        characterController.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        characterController.enabled = true;
    }

    protected virtual void OnReset() { }

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