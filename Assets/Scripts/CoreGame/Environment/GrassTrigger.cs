using UnityEngine;

public class GrassTrigger : MonoBehaviour
{
    private Grass grass;

    private void Awake()
    {
        grass = GetComponentInParent<Grass>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            grass.Bend(other.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            grass.ResetGrass();
        }
    }
}