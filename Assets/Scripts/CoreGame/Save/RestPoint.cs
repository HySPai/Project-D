using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RestPoint : MonoBehaviour
{
    [SerializeField] private SaveController controller;
    [SerializeField] private string id;
    [SerializeField] private AreaRoom room;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CampfireView fire;
    [SerializeField] private Transform pulseTarget;
    [SerializeField] private float pulseStrength = 0.12f;
    [SerializeField] private float pulseDuration = 0.35f;

    private Tween pulseTween;

    public string Id => id;
    public AreaRoom Room => room;
    public Transform SpawnPoint => spawnPoint;

    private void OnDestroy() => pulseTween?.Kill();

    private void OnTriggerEnter(Collider other)
    {
        if (other != controller.Player.CharacterController) return;

        controller.SetFocus(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != controller.Player.CharacterController) return;

        controller.ClearFocus(this);
    }

    /// <summary>Makes the fire blaze up or settle back to embers; a fresh lighting also pulses the stone ring.</summary>
    public void SetLit(bool lit, bool animate)
    {
        fire.SetBurning(lit, animate);
        if (lit && animate) Pulse();
    }

    /// <summary>Feedback for a rest the fire turned down because enemies are hunting the player nearby.</summary>
    public void PlayRefused() => Pulse();

    private void Pulse()
    {
        pulseTween?.Complete();
        pulseTween = pulseTarget.DOPunchScale(Vector3.one * pulseStrength, pulseDuration, 6, 0.5f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, 0.4f);

        if (spawnPoint == null) return;
        Gizmos.DrawLine(transform.position, spawnPoint.position);
        Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward);
    }
}
