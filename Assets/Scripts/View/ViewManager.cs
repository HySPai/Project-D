using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace UIGame
{
    /// <summary>
    /// Tracks open stacked views and keeps the most recently opened one on top by sorting order.
    /// </summary>
    public class ViewManager : SingletonMonoBehaviour<ViewManager>
    {
        private int _order = 0;
        [ShowInInspector] private Stack<BaseUIView> _viewStack = new();

        [ShowInInspector] private Stack<BaseUIView> _tempStack = new();

        #region OPEN

        /// <summary>Pushes <paramref name="view"/> onto the stack, or brings it to the top if it is already active.</summary>
        public void Open(BaseUIView view)
        {
            if (view == null) return;

            if (view.gameObject.activeSelf)
            {
                BringToTop(view);
                return;
            }

            view.SetOrder(++_order);
            _viewStack.Push(view);
        }

        #endregion

        #region CLOSE

        /// <summary>Removes <paramref name="view"/> from the stack.</summary>
        public void Close(BaseUIView view)
        {
            if (view == null) return;

            RemoveFromStack(view);
        }

        /// <summary>Closes the top view of the stack.</summary>
        public void CloseTop()
        {
            if (_viewStack.Count == 0) return;

            var top = _viewStack.Pop();
            top.CloseView();
        }

        /// <summary>Closes every stacked view.</summary>
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
