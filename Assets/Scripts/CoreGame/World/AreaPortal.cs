using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AreaPortal : MonoBehaviour
{
    [SerializeField] private AreaTransitionController controller;
    [SerializeField] private AreaRoom room;
    [SerializeField] private AreaPortal linkedPortal;
    [SerializeField] private Transform arrivalSpawn;

    public AreaRoom Room => room;
    public AreaPortal LinkedPortal => linkedPortal;
    public Transform ArrivalSpawn => arrivalSpawn;

    private void OnTriggerStay(Collider other)
    {
        if (other != controller.Player.CharacterController) return;
        if (!PortalMath.IsPastThreshold(transform.position, transform.rotation, other.transform.position)) return;

        controller.RequestTransition(this);
    }

    /// <summary>Where a pose on this side of the doorway ends up on the linked side.</summary>
    public void MapThrough(Vector3 position, Quaternion rotation, out Vector3 mappedPosition, out Quaternion mappedRotation)
    {
        Transform target = linkedPortal.transform;
        mappedPosition = PortalMath.MapPosition(transform.position, transform.rotation, target.position, target.rotation, position);
        mappedRotation = PortalMath.MapRotation(transform.rotation, target.rotation, rotation);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);

        if (linkedPortal == null) return;
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawLine(transform.position, linkedPortal.transform.position);
    }
}
