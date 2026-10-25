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
            base.OnDestroy();
        }

        private void SetupEvent()
        {
            GameEvent.OnPlay += ShowGamePlayUI;
        }

        private void ShowGamePlayUI()
        {
            ViewManager.Instance.CloseAll();
            _gamePlayView.OpenView();
        }
    }
}
