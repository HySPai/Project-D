using UnityEngine;

public static class PortalMath
{
    private static readonly Quaternion HalfTurn = Quaternion.Euler(0f, 180f, 0f);

    /// <summary>True once a point has crossed the portal's midline going outward along its forward axis.</summary>
    public static bool IsPastThreshold(Vector3 portalPosition, Quaternion portalRotation, Vector3 point) =>
        (Quaternion.Inverse(portalRotation) * (point - portalPosition)).z > 0f;

    /// <summary>
    /// Carries a point through a pair of facing doorways as if they were one opening:
    /// a step past the source midline lands the same step short of the target midline.
    /// </summary>
    public static Vector3 MapPosition(
        Vector3 sourcePosition, Quaternion sourceRotation, Vector3 targetPosition, Quaternion targetRotation, Vector3 point)
    {
        Vector3 local = Quaternion.Inverse(sourceRotation) * (point - sourcePosition);
        return targetPosition + targetRotation * (HalfTurn * local);
    }

    /// <summary>Carries a facing through the doorway pair so walking out of one keeps walking into the other.</summary>
    public static Quaternion MapRotation(Quaternion sourceRotation, Quaternion targetRotation, Quaternion rotation) =>
        targetRotation * HalfTurn * Quaternion.Inverse(sourceRotation) * rotation;
}
