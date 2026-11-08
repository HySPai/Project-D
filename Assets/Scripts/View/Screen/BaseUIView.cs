using UnityEngine;

namespace UIGame
{
    /// <summary>
    /// Base for screens: opens and closes its <see cref="View"/>, registers with the <see cref="ViewManager"/> stack
    /// and optionally pauses game time while open.
    /// </summary>
    [RequireComponent(typeof(View))]
    public abstract class BaseUIView : MonoBehaviour
    {
        [SerializeField] bool _openViewStopTime = false;
        [SerializeField] protected View _view;

        /// <summary>The view this screen drives.</summary>
        public View GetView => _view;

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            FillReferences();
        }

        protected virtual void OnValidate()
        {
            FillReferences();
        }

        private void FillReferences()
        {
            if (_view == null) TryGetComponent(out _view);
        }
#endif

        /// <summary>Opens the screen.</summary>
        public virtual void OpenView()
        {
            if (_openViewStopTime)
            {
                GameTime.SetPaused(true);
            }
            if (_view.IsViewStack)
            {
                ViewManager.Instance?.Open(this);
            }
            _view.Open();
        }

        /// <summary>Closes the screen.</summary>
        public virtual void CloseView()
        {
            if (_openViewStopTime)
            {
                GameTime.SetPaused(false);
            }
            if (_view.IsViewStack)
            {
                ViewManager.Instance?.Close(this);
            }
            _view.Close();
        }

        /// <summary>Sets the sorting order of the screen canvas.</summary>
        public void SetOrder(int order)
        {
            if (_view == null) return;
            _view.SetOrder(order);
        }
    }
}
