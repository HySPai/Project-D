using UnityEngine;
using UnityEngine.UI;

namespace UIGame
{
    public class GameOverView : BaseUIView
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private float openDelay = 2f;

        public float OpenDelay => openDelay;

        private void OnEnable()
        {
            restartButton.interactable = true;
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnDisable() => restartButton.onClick.RemoveListener(OnRestartClicked);

        private void OnRestartClicked()
        {
            restartButton.interactable = false;
            GameEvent.OnRestartRequested?.Invoke();
        }
    }
}
