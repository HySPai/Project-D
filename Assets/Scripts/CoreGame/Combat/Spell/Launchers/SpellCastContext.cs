using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>What every spell launcher needs from the caster: who casts, where it faces, what it may hit, plus shared query buffers.</summary>
public sealed class SpellCastContext
{
    private const int HitBufferSize = 16;
    private const float MinSqrDirection = 0.0001f;

    private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
    private readonly List<CharacterControllerBase> targets = new List<CharacterControllerBase>();
    private readonly PlayerCombat combat;
    private readonly LayerMask hitLayer;

    public SpellCastContext(
        Transform casterTransform,
        CharacterControllerBase character,
        CharacterStateBase state,
        PlayerCombat combat,
        LayerMask hitLayer,
        CancellationToken lifetimeToken)
    {
        CasterTransform = casterTransform;
        Character = character;
        State = state;
        this.combat = combat;
        this.hitLayer = hitLayer;
        LifetimeToken = lifetimeToken;
    }

    public Transform CasterTransform { get; }
    public CharacterControllerBase Character { get; }
    public CharacterStateBase State { get; }

    /// <summary>Cancelled when the caster is destroyed.</summary>
    public CancellationToken LifetimeToken { get; }

    /// <summary>Targets found by the last CollectTargets call.</summary>
    public IReadOnlyList<CharacterControllerBase> Targets => targets;

    /// <summary>True while the caster can still act, i.e. it is neither dead nor rolling.</summary>
    public bool CasterCanAct => State != null && !State.IsDead && !State.IsRolling;

    /// <summary>Flat direction toward the lock-on target, or the caster's forward when nothing is locked.</summary>
    public Vector3 GetCastDirection()
    {
        Transform target = combat != null ? combat.LockOnTransform : null;
        Vector3 direction = target != null ? target.position - CasterTransform.position : CasterTransform.forward;
        direction.y = 0f;

        return direction.sqrMagnitude > MinSqrDirection ? direction.normalized : Vector3.forward;
    }

    /// <summary>Turns the caster toward the cast direction and returns that direction.</summary>
    public Vector3 FaceCastDirection()
    {
        Vector3 direction = GetCastDirection();
        CasterTransform.rotation = Quaternion.LookRotation(direction);
        return direction;
    }

    /// <summary>Gathers living targets other than the caster inside the sphere into Targets.</summary>
    public int CollectTargets(Vector3 center, float radius) =>
        SpellTargeting.CollectTargets(center, radius, hitLayer, Character, hitBuffer, targets);
}
