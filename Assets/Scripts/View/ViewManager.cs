using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace UIGame
{
    public class ViewManager : SingletonMonoBehaviour<ViewManager>
    {
        private int _order = 0;
        [ShowInInspector] private Stack<BaseUIView> _viewStack = new();

        [ShowInInspector] private Stack<BaseUIView> _tempStack = new();

        #region OPEN

        public void Open(BaseUIView view)
        {
            if (view == null) return;

            if (view.gameObject.activeSelf)
            {
                BringToTop(view);
                return;
            }

            // Set order
            view.SetOrder(++_order);

            // Block UI 
            if (_viewStack.Count > 0)
                _viewStack.Peek();

            _viewStack.Push(view);
        }

        #endregion

        #region CLOSE

        public void Close(BaseUIView view)
        {
            if (view == null) return;

            RemoveFromStack(view);

           /* // Reveal top
            if (_viewStack.Count > 0)
                _viewStack.Peek().Reveal();*/
        }

        public void CloseTop()
        {
            if (_viewStack.Count == 0) return;

            var top = _viewStack.Pop();
            top.CloseView();

            /*if (_viewStack.Count > 0)
                _viewStack.Peek().Reveal();*/
        }

        public void CloseAll()
        {
            while (_viewStack.Count > 0)
            {
                var v = _viewStack.Pop();
                v.CloseView();
            }
        }

        #endregion

        #region INTERNAL

        private void RemoveFromStack(BaseUIView view)
        {
            if (_viewStack.Count == 0) return;

            _tempStack.Clear();

            while (_viewStack.Count > 0)
            {
                var top = _viewStack.Pop();

                if (top != view)
                    _tempStack.Push(top);
            }

            while (_tempStack.Count > 0)
            {
                _viewStack.Push(_tempStack.Pop());
            }
        }

        private void BringToTop(BaseUIView view)
        {
            view.SetOrder(++_order);
        }

        #endregion
    }
}
