using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AreaPortal : MonoBehaviour
{
    [SerializeField] private AreaTransitionController controller;
    [SerializeField] private Transform destination;
    [SerializeField] private bool setAsSpawn = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other != controller.Player.CharacterController) return;

        controller.RequestTransition(destination, setAsSpawn);
    }

    private void OnDrawGizmos()
    {
        if (destination == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, destination.position);
        Gizmos.DrawWireSphere(destination.position, 0.4f);
    }
}
