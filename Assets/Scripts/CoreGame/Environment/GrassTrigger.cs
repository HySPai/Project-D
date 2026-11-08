using UnityEngine;

public class GrassTrigger : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [SerializeField] private Grass grass;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(PlayerTag))
        {
            grass.Bend(other.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(PlayerTag))
        {
            grass.ResetGrass();
        }
    }
}