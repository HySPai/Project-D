using UnityEngine;

/// <summary>Tuning values for CharacterGroundProbe, read from the owning CharacterStateBase every frame.</summary>
public readonly struct GroundProbeSettings
{
    public readonly LayerMask groundLayer;
    public readonly float checkSphereRadius;
    public readonly float gravityForce;
    public readonly float groundedYVelocity;
    public readonly float fallStartYVelocity;
    public readonly float maxStepDownHeight;
    public readonly float edgeCheckDistance;

    public GroundProbeSettings(LayerMask groundLayer, float checkSphereRadius, float gravityForce,
        float groundedYVelocity, float fallStartYVelocity, float maxStepDownHeight, float edgeCheckDistance)
    {
        this.groundLayer = groundLayer;
        this.checkSphereRadius = checkSphereRadius;
        this.gravityForce = gravityForce;
        this.groundedYVelocity = groundedYVelocity;
        this.fallStartYVelocity = fallStartYVelocity;
        this.maxStepDownHeight = maxStepDownHeight;
        this.edgeCheckDistance = edgeCheckDistance;
    }
}

/// <summary>
/// Grounded check, vertical velocity and the ledge guard that stops a character from walking off an edge.
/// Pure C#: the owning state calls Tick once per frame, then movement and root motion ask for deltas.
/// </summary>
public sealed class CharacterGroundProbe
{
    private const float AxisEpsilon = 0.0001f;
    private const float EdgeRayLift = 0.2f;

    private Vector3 yVelocity;
    private bool fallingVelocityHasBeenSet;
    private float inAirTimer;
    private bool isGrounded = true;

    public bool IsGrounded => isGrounded;
    public Vector3 YVelocity => yVelocity;
    public float InAirTimer => inAirTimer;

    /// <summary>Probes the ground at position and integrates gravity over deltaTime.</summary>
    public void Tick(Vector3 position, float deltaTime, in GroundProbeSettings settings)
    {
        bool grounded = Physics.CheckSphere(
            position, settings.checkSphereRadius, settings.groundLayer, QueryTriggerInteraction.Ignore);
        Integrate(grounded, deltaTime, in settings);
    }

    /// <summary>Applies one frame of the grounded or falling velocity rules; split from Tick so it can be unit tested.</summary>
    public void Integrate(bool grounded, float deltaTime, in GroundProbeSettings settings)
    {
        isGrounded = grounded;

        if (isGrounded)
        {
            if (yVelocity.y < 0f)
            {
                inAirTimer = 0f;
                fallingVelocityHasBeenSet = false;
                yVelocity.y = settings.groundedYVelocity;
            }
            return;
        }

        if (!fallingVelocityHasBeenSet)
        {
            fallingVelocityHasBeenSet = true;
            yVelocity.y = settings.fallStartYVelocity;
        }
        inAirTimer += deltaTime;
        yVelocity.y += settings.gravityForce * deltaTime;
    }

    /// <summary>Vertical displacement for this frame, to add to CharacterController.Move.</summary>
    public Vector3 GetGravityDelta(float deltaTime) => yVelocity * deltaTime;

    /// <summary>Zeroes each horizontal axis of delta that would leave valid ground; does nothing while airborne.</summary>
    public Vector3 ResolveEdgeGuard(Vector3 position, Vector3 horizontalDelta, in GroundProbeSettings settings)
    {
        if (!isGrounded) return horizontalDelta;

        if (Mathf.Abs(horizontalDelta.x) > AxisEpsilon
            && !HasGroundBelow(position, position + new Vector3(horizontalDelta.x, 0f, 0f), in settings))
            horizontalDelta.x = 0f;

        if (Mathf.Abs(horizontalDelta.z) > AxisEpsilon
            && !HasGroundBelow(position, position + new Vector3(0f, 0f, horizontalDelta.z), in settings))
            horizontalDelta.z = 0f;

        return horizontalDelta;
    }

    /// <summary>Clears velocity and air time and marks the character grounded.</summary>
    public void Reset()
    {
        yVelocity = Vector3.zero;
        fallingVelocityHasBeenSet = false;
        inAirTimer = 0f;
        isGrounded = true;
    }

    private static bool HasGroundBelow(Vector3 position, Vector3 target, in GroundProbeSettings settings)
    {
        Vector3 rayOrigin = target + Vector3.up * EdgeRayLift;

        if (!Physics.Raycast(
                rayOrigin, Vector3.down, out RaycastHit hit,
                settings.edgeCheckDistance, settings.groundLayer, QueryTriggerInteraction.Ignore))
            return false;

        return position.y - hit.point.y <= settings.maxStepDownHeight;
    }
}
