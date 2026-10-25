using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIGame
{
    public class DeathFadeView : MonoBehaviour
    {
        [SerializeField] private Volume deathVolume;
        [SerializeField] private float startDelay = 1.5f;
        [SerializeField] private float fadeDuration = 2f;
        [SerializeField] private float holdDuration = 0.4f;
        [SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] private float respawnFadeDuration = 1f;
        [SerializeField] private Ease respawnEase = Ease.OutSine;
        [SerializeField] private int respawnSkipFrames = 3;

        private Tween fadeTween;

        private void Awake() => deathVolume.weight = 0f;

        private void HandleWorldResetCompleted()
        {
            fadeTween?.Kill();
            PlayRespawnFadeAsync().Forget();
        }

        private async UniTaskVoid PlayRespawnFadeAsync()
        {
            bool canceled = await UniTask.DelayFrame(
                    respawnSkipFrames,
                    cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();
            if (canceled) return;

            fadeTween = DOTween.To(
                    () => deathVolume.weight,
                    value => deathVolume.weight = value,
                    0f,
                    respawnFadeDuration)
                .SetEase(respawnEase)
                .SetUpdate(true);
        }

        private void OnEnable()
        {
            GameEvent.OnPlayerDied += HandlePlayerDied;
            GameEvent.OnWorldResetCompleted += HandleWorldResetCompleted;
        }

        private void OnDisable()
        {
            GameEvent.OnPlayerDied -= HandlePlayerDied;
            GameEvent.OnWorldResetCompleted -= HandleWorldResetCompleted;
        }

        private void OnDestroy() => fadeTween?.Kill();

        private void HandlePlayerDied()
        {
            fadeTween?.Kill();
            PlayDeathFadeAsync().Forget();
        }

        private async UniTaskVoid PlayDeathFadeAsync()
        {
            var token = this.GetCancellationTokenOnDestroy();

            bool canceled = await UniTask.Delay(
                    TimeSpan.FromSeconds(startDelay),
                    DelayType.UnscaledDeltaTime,
                    cancellationToken: token)
                .SuppressCancellationThrow();
            if (canceled) return;

            fadeTween = DOTween.To(
                    () => deathVolume.weight,
                    value => deathVolume.weight = value,
                    1f,
                    fadeDuration)
                .SetEase(ease)
                .SetUpdate(true);

            await fadeTween.AsyncWaitForCompletion();
            if (token.IsCancellationRequested) return;

            canceled = await UniTask.Delay(
                    TimeSpan.FromSeconds(holdDuration),
                    DelayType.UnscaledDeltaTime,
                    cancellationToken: token)
                .SuppressCancellationThrow();
            if (canceled) return;

            GameEvent.OnRestartRequested?.Invoke();
        }
    }
}
