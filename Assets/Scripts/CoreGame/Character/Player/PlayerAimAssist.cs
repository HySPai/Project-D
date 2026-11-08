using UnityEngine;

/// <summary>Tuning values for PlayerAimAssist, taken from PlayerCombat's serialized fields.</summary>
public readonly struct AimAssistSettings
{
    public readonly LayerMask hitLayer;
    public readonly LayerMask lockOnLayer;
    public readonly float range;
    public readonly float angleWithInput;
    public readonly float angleWithoutInput;
    public readonly float heightTolerance;
    public readonly float lockOnBroadphase;

    public AimAssistSettings(LayerMask hitLayer, LayerMask lockOnLayer, float range, float angleWithInput,
        float angleWithoutInput, float heightTolerance, float lockOnBroadphase)
    {
        this.hitLayer = hitLayer;
        this.lockOnLayer = lockOnLayer;
        this.range = range;
        this.angleWithInput = angleWithInput;
        this.angleWithoutInput = angleWithoutInput;
        this.heightTolerance = heightTolerance;
        this.lockOnBroadphase = lockOnBroadphase;
    }
}

/// <summary>
/// Picks what the player's attacks and lock-on aim at: the aim-assist target for a swing, the nearest on-screen
/// character for a fresh lock, and the best candidate in a stick direction when switching locks. Pure C#.
/// </summary>
public sealed class PlayerAimAssist
{
    private const float BroadphasePadding = 1.5f;
    private const float AngleScorePerDegree = 0.022f;
    private const float InputEpsilon = 0.01f;
    private const float MinFacingDistance = 0.05f;
    private const float ScreenDeltaEpsilon = 0.0001f;
    private const int AimBufferSize = 24;
    private const int LockOnBufferSize = 64;

    private readonly Collider[] aimBuffer = new Collider[AimBufferSize];
    private readonly Collider[] lockOnBuffer = new Collider[LockOnBufferSize];
    private readonly Transform owner;
    private readonly CharacterControllerBase self;
    private readonly Camera viewCamera;

    private AimAssistSettings settings;

    public PlayerAimAssist(Transform owner, CharacterControllerBase self, Camera viewCamera, in AimAssistSettings settings)
    {
        this.owner = owner;
        this.self = self;
        this.viewCamera = viewCamera;
        this.settings = settings;
    }

    /// <summary>Replaces the tuning values, e.g. after an Inspector edit in play mode.</summary>
    public void Configure(in AimAssistSettings value) => settings = value;

    #region Aim Assist
    /// <summary>
    /// Direction to swing in: toward lockedTarget when given, otherwise toward the best aim-assist candidate,
    /// otherwise along the input direction or the current facing.
    /// </summary>
    public Vector3 ResolveAttackDirection(Vector3 inputDirection, CharacterControllerBase lockedTarget)
    {
        bool hasInput = inputDirection.sqrMagnitude > InputEpsilon;
        Vector3 desiredDirection = hasInput ? inputDirection : CombatTargeting.GetFlatForward(owner);

        CharacterControllerBase target = lockedTarget;
        if (target == null)
        {
            float halfAngle = hasInput ? settings.angleWithInput : settings.angleWithoutInput;
            target = FindAimAssistTarget(desiredDirection, halfAngle);
        }

        if (target == null) return desiredDirection;

        Vector3 toTarget = target.transform.position - owner.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        return distance > MinFacingDistance ? toTarget / distance : desiredDirection;
    }

    private CharacterControllerBase FindAimAssistTarget(Vector3 desiredDirection, float halfAngle)
    {
        Vector3 origin = owner.position;
        int count = Physics.OverlapSphereNonAlloc(
            origin, settings.range + BroadphasePadding, aimBuffer, settings.hitLayer, QueryTriggerInteraction.Ignore);

        CharacterControllerBase best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (!CombatTargeting.TryResolveTarget(aimBuffer[i], self, out CharacterControllerBase candidate)) continue;

            Vector3 toCandidate = candidate.transform.position - origin;
            if (Mathf.Abs(toCandidate.y) > settings.heightTolerance) continue;
            toCandidate.y = 0f;

            float edgeDistance = toCandidate.magnitude - CombatTargeting.GetTargetRadius(candidate);
            if (!TryScoreAimCandidate(edgeDistance, Vector3.Angle(desiredDirection, toCandidate),
                    settings.range, halfAngle, out float score)) continue;

            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    /// <summary>Aim-assist score, lower is better: edge distance plus a cost per degree off the desired direction.</summary>
    public static bool TryScoreAimCandidate(float edgeDistance, float angle, float range, float halfAngle, out float score)
    {
        score = float.MaxValue;
        if (edgeDistance > range || angle > halfAngle) return false;

        score = Mathf.Max(0f, edgeDistance) + angle * AngleScorePerDegree;
        return true;
    }
    #endregion

    #region Lock On
    /// <summary>True when worldPosition is in front of the view camera and inside its viewport.</summary>
    public bool IsInView(Vector3 worldPosition)
    {
        if (viewCamera == null) return false;
        return IsInsideViewport(viewCamera.WorldToViewportPoint(worldPosition));
    }

    public static bool IsInsideViewport(Vector3 viewport)
    {
        return viewport.z > 0f
            && viewport.x >= 0f && viewport.x <= 1f
            && viewport.y >= 0f && viewport.y <= 1f;
    }

    /// <summary>Nearest on-screen lock-on candidate; false when nothing is in view.</summary>
    public bool TryFindNearestLockTarget(out Transform target, out CharacterStateBase targetState)
    {
        target = null;
        targetState = null;

        Vector3 origin = owner.position;
        int count = GatherLockOnCandidates(origin);
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider hit = lockOnBuffer[i];
            if (!TryResolveLockState(hit, out CharacterStateBase candidateState)) continue;

            Transform candidate = hit.transform;
            if (!IsInView(candidate.position)) continue;

            float sqrDistance = (candidate.position - origin).sqrMagnitude;
            if (sqrDistance < nearestDistance)
            {
                nearestDistance = sqrDistance;
                target = candidate;
                targetState = candidateState;
            }
        }

        return target != null;
    }

    /// <summary>Best on-screen candidate other than current in the stick direction, projected on the camera plane.</summary>
    public bool TryFindSwitchTarget(Transform current, Vector2 inputDirection, Vector3 cameraRight, Vector3 cameraForward,
        out Transform target, out CharacterStateBase targetState)
    {
        target = null;
        targetState = null;

        Vector3 origin = owner.position;
        Vector2 currentScreen = ToScreenPlane(current.position - origin, cameraRight, cameraForward);
        Vector2 desired = inputDirection.normalized;

        int count = GatherLockOnCandidates(origin);
        float bestScore = float.MinValue;

        for (int i = 0; i < count; i++)
        {
            Collider hit = lockOnBuffer[i];
            Transform candidate = hit.transform;
            if (candidate == current) continue;
            if (!TryResolveLockState(hit, out CharacterStateBase candidateState)) continue;
            if (!IsInView(candidate.position)) continue;

            Vector2 candidateScreen = ToScreenPlane(candidate.position - origin, cameraRight, cameraForward);
            if (!TryScoreSwitchCandidate(currentScreen, candidateScreen, desired, out float score)) continue;

            if (score > bestScore)
            {
                bestScore = score;
                target = candidate;
                targetState = candidateState;
            }
        }

        return target != null;
    }

    /// <summary>Switch score, higher is better: alignment with the stick divided by on-screen distance.</summary>
    public static bool TryScoreSwitchCandidate(Vector2 currentScreen, Vector2 candidateScreen, Vector2 desiredDirection, out float score)
    {
        score = float.MinValue;

        Vector2 delta = candidateScreen - currentScreen;
        if (delta.sqrMagnitude < ScreenDeltaEpsilon) return false;

        float alignment = Vector2.Dot(delta.normalized, desiredDirection);
        if (alignment <= 0f) return false;

        score = alignment / delta.magnitude;
        return true;
    }

    public static Vector2 ToScreenPlane(Vector3 worldDirection, Vector3 cameraRight, Vector3 cameraForward)
    {
        return new Vector2(Vector3.Dot(worldDirection, cameraRight), Vector3.Dot(worldDirection, cameraForward));
    }

    private int GatherLockOnCandidates(Vector3 origin)
    {
        return Physics.OverlapSphereNonAlloc(origin, settings.lockOnBroadphase, lockOnBuffer, settings.lockOnLayer);
    }

    private static bool TryResolveLockState(Collider hit, out CharacterStateBase targetState)
    {
        targetState = null;
        if (!CombatTargetRegistry.TryGet(hit, out CharacterControllerBase character) || character == null) return false;

        targetState = character.GetState;
        return targetState != null;
    }
    #endregion
}
