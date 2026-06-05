using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UIGame
{
    [RequireComponent(typeof(View))]
    public abstract class BaseUIView : MonoBehaviour
    {
        [SerializeField] bool _openViewStopTime = false;
        [SerializeField] protected View _view;

        public View GetView => _view;
        protected virtual void OnValidate()
        {
            if (_view == null)
            {
                _view = GetComponent<View>();
            }
        }
        public virtual void OpenView()
        {
            if (_openViewStopTime)
            {
                Time.timeScale = 0f;
            }
            if (_view.IsViewStack)
            {
                ViewManager.Instance?.Open(this);
            }
            _view.Open();
            //AudioManager.Instance.PlaySfx(AudioSfxID.sfx_appear_popup, 1, -1, false);

        }
        public virtual void CloseView()
        {
            if (_openViewStopTime)
            {
                Time.timeScale = 1f;
            }
            if (_view.IsViewStack)
            {
                ViewManager.Instance?.Close(this);
            }
            _view.Close();
        }
        public void SetOrder(int order)
        {
            if (_view == null) return;
            _view.SetOrder(order);
        }
    }
}