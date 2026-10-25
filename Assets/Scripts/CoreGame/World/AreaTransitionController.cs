using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class AreaTransitionController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Volume fadeVolume;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float holdDuration = 0.15f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private Ease fadeOutEase = Ease.InSine;
    [SerializeField] private Ease fadeInEase = Ease.OutSine;

    private Tween fadeTween;

    public bool IsTransitioning { get; private set; }
    public PlayerController Player => player;

    private void Awake() => fadeVolume.weight = 0f;

    private void OnDestroy() => fadeTween?.Kill();

    /// <summary>Fades out, moves the player to the destination, snaps the camera and fades back in.</summary>
    public void RequestTransition(Transform destination, bool setAsSpawn)
    {
        if (IsTransitioning) return;
        if (player.GetState.IsDead) return;

        TransitionAsync(destination, setAsSpawn).Forget();
    }

    private async UniTaskVoid TransitionAsync(Transform destination, bool setAsSpawn)
    {
        var token = this.GetCancellationTokenOnDestroy();
        IsTransitioning = true;

        player.GetMovement.SetInput(Vector2.zero);
        player.GetAnimation.UpdateAnimation(0f);
        player.enabled = false;

        await FadeTo(1f, fadeOutDuration, fadeOutEase, token);
        if (token.IsCancellationRequested) return;

        Vector3 delta = destination.position - player.transform.position;
        player.Teleport(destination.position, destination.rotation);
        if (virtualCamera != null) virtualCamera.OnTargetObjectWarped(player.transform, delta);
        if (setAsSpawn) player.SetSpawnPoint(destination.position, destination.rotation);

        bool canceled = await UniTask.Delay(
                System.TimeSpan.FromSeconds(holdDuration),
                DelayType.UnscaledDeltaTime,
                cancellationToken: token)
            .SuppressCancellationThrow();
        if (canceled) return;

        player.enabled = true;
        await FadeTo(0f, fadeInDuration, fadeInEase, token);

        IsTransitioning = false;
    }

    private async UniTask FadeTo(float weight, float duration, Ease ease, System.Threading.CancellationToken token)
    {
        fadeTween?.Kill();
        fadeTween = DOTween.To(() => fadeVolume.weight, v => fadeVolume.weight = v, weight, duration)
            .SetEase(ease)
            .SetUpdate(true);

        await fadeTween.AsyncWaitForCompletion();
    }
}
