using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace UIGame
{
    public class GUIViewManager : SingletonMonoBehaviour<GUIViewManager>
    {
        [SerializeField] GamePlayView _gamePlayView;
        [SerializeField] GameOverView _gameOverView;

        public GamePlayView GetGamePlayView => _gamePlayView;

        private void Start()
        {
            SetupEvent();
        }

        protected override void OnDestroy()
        {
            GameEvent.OnPlay -= ShowGamePlayUI;
            GameEvent.OnPlayerDied -= HandlePlayerDied;
            base.OnDestroy();
        }

        private void SetupEvent()
        {
            GameEvent.OnPlay += ShowGamePlayUI;
            GameEvent.OnPlayerDied += HandlePlayerDied;
        }

        private void ShowGamePlayUI()
        {
            ViewManager.Instance.CloseAll();
            _gamePlayView.OpenView();
        }

        private void HandlePlayerDied() => ShowGameOverAsync().Forget();

        private async UniTaskVoid ShowGameOverAsync()
        {
            if (_gameOverView == null) return;

            bool canceled = await UniTask.Delay(
                    TimeSpan.FromSeconds(_gameOverView.OpenDelay),
                    DelayType.UnscaledDeltaTime,
                    cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();
            if (canceled) return;

            _gameOverView.OpenView();
        }
    }
}
