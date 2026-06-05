using UnityEngine;

namespace UIGame
{
    public class GUIViewManager : SingletonMonoBehaviour<GUIViewManager>
    {
        [SerializeField] GamePlayView _gamePlayView;

        public GamePlayView GetGamePlayView => _gamePlayView;

        private void Start()
        {
            SetupEvent();
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
