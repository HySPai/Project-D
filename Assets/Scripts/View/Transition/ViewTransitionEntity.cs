using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UIGame
{
    /// <summary>
    /// Holds the transitions of one UI element and adds their tweens to the owning <see cref="View"/> sequence.
    /// </summary>
    public class ViewTransitionEntity : MonoCached
    {
        [Title("Config")]
        [Range(0.1f, 1f)]
        [SerializeField] private float _duration = 1f;

        [CustomValueDrawer("GUIDrawInsertTime")]
        [SerializeField] private float _insertTime = 0f;

        [SerializeField] private CanvasGroup _canvasGroup;

        [ListDrawerSettings(AddCopiesLastElement = true, ListElementLabelName = "displayName")]
        [SerializeReference] private ViewTransition[] _transitions = new ViewTransition[0];

        private RectTransform _rectTransform;

        /// <summary>The RectTransform of this element, or null when it is not a UI element.</summary>
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = transformCached as RectTransform;

                return _rectTransform;
            }
        }

        /// <summary>The CanvasGroup faded by canvas group transitions; null when none is assigned.</summary>
        public CanvasGroup canvasGroup => _canvasGroup;

        /// <summary>Adds every transition tween of this element to the sequence of <paramref name="view"/>.</summary>
        public void Apply(View view)
        {
            for (int i = 0; i < _transitions.Length; i++)
            {
                Tween tween = _transitions[i].GetTween(this, _duration);
                if (tween == null)
                {
                    GameLog.Warn($"{name}: transition {_transitions[i].displayName} produced no tween; check its references.", this);
                    continue;
                }

                if (_insertTime > 0f)
                    view.sequence.Insert(_insertTime, tween);
                else
                    view.sequence.Join(tween);
            }
        }

#if UNITY_EDITOR

        private void Reset()
        {
            FillReferences();
        }

        private void OnValidate()
        {
            FillReferences();
        }

        private void FillReferences()
        {
            if (_canvasGroup == null) TryGetComponent(out _canvasGroup);
        }

        private float GUIDrawInsertTime(float insertTime, GUIContent label)
        {
            return UnityEditor.EditorGUILayout.Slider(label, insertTime, 0f, 1.0f - _duration);
        }

#endif
    }
}
