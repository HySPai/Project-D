using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using System.Threading;
using Sirenix.Utilities;

namespace UIGame
{
    /// <summary>
    /// A UI screen root that plays its transition sequence forward on open and backward on close.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class View : MonoCached
    {
        [SerializeField] bool _viewStack = true;

        [ShowIf(nameof(_viewStack))]
        [SerializeField] Canvas _canvas;

        [SerializeField] private CanvasGroup _canvasGroup;

        [Title("Config")]
        [MinValue(0f)]
        [SerializeField] private float _openDuration = 0.3f;

        [MinValue(0f)]
        [SerializeField] private float _closeDuration = 0.3f;

        [FoldoutGroup("Transition", Expanded = false)]
        [SerializeField] private ViewTransitionEntity[] _transitionEntities;

        [FoldoutGroup("Event")]
        [SerializeField] private UnityEvent _onOpenStart;
        [FoldoutGroup("Event")]
        [SerializeField] private UnityEvent _onOpenEnd;
        [FoldoutGroup("Event")]
        [SerializeField] private UnityEvent _onCloseStart;
        [FoldoutGroup("Event")]
        [SerializeField] private UnityEvent _onCloseEnd;
        [FoldoutGroup("Event")]
        [SerializeField] private UnityEvent _onShowStart;
        [FoldoutGroup("Event")]
        [SerializeField] private UnityEvent _onShowEnd;
        [FoldoutGroup("Event")]
        [SerializeField] private UnityEvent _onHideStart;
        [FoldoutGroup("Event")]
        [SerializeField] private UnityEvent _onHideEnd;

        private CancellationTokenSource _cancelToken;

        private Sequence _sequence;

        /// <summary>The transition sequence that <see cref="ViewTransitionEntity"/> instances append their tweens to.</summary>
        public Sequence sequence { get { return _sequence; } }

        /// <summary>Raised when an open transition starts.</summary>
        public UnityEvent onOpenStart { get { return _onOpenStart; } }
        /// <summary>Raised when an open transition ends.</summary>
        public UnityEvent onOpenEnd { get { return _onOpenEnd; } }
        /// <summary>Raised when a close transition starts.</summary>
        public UnityEvent onCloseStart { get { return _onCloseStart; } }
        /// <summary>Raised when a close transition ends.</summary>
        public UnityEvent onCloseEnd { get { return _onCloseEnd; } }
        /// <summary>Raised when a show transition starts.</summary>
        public UnityEvent onShowStart { get { return _onShowStart; } }
        /// <summary>Raised when a show transition ends.</summary>
        public UnityEvent onShowEnd { get { return _onShowEnd; } }
        /// <summary>Raised when a hide transition starts.</summary>
        public UnityEvent onHideStart { get { return _onHideStart; } }
        /// <summary>Raised when a hide transition ends.</summary>
        public UnityEvent onHideEnd { get { return _onHideEnd; } }

        /// <summary>Whether the view accepts input.</summary>
        public bool interactable { get { return _canvasGroup.interactable; } set { _canvasGroup.interactable = value; } }

        /// <summary>Whether the view is tracked by the <see cref="ViewManager"/> stack.</summary>
        public bool IsViewStack => _viewStack;

        #region MonoBehaviour

        private void Awake()
        {
            if (_canvas)
            {
                _canvas.overrideSorting = true;
            }
        }

        private void OnDestroy()
        {
            CancelPending();
            _sequence?.Kill();
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

        [FoldoutGroup("Transition")]
        [Button]
        private void GetTransitionEntities()
        {
            _transitionEntities = GetComponentsInChildren<ViewTransitionEntity>(true);
        }
#endif

        #endregion

        #region Function -> Private

        private void CancelPending()
        {
            if (_cancelToken == null) return;

            _cancelToken.Cancel();
            _cancelToken.Dispose();
            _cancelToken = null;
        }

        private CancellationToken BeginTransition()
        {
            CancelPending();
            _cancelToken = new CancellationTokenSource();
            return _cancelToken.Token;
        }

        private void ConstructSequence()
        {
            if (_sequence != null)
                return;

            _sequence = DOTween.Sequence();

            if (_transitionEntities.IsNullOrEmpty())
            {
                _sequence.AppendInterval(1.0f);
            }
            else
            {
                for (int i = 0; i < _transitionEntities.Length; i++)
                    _transitionEntities[i].Apply(this);
            }

            _sequence.SetUpdate(true);
            _sequence.SetAutoKill(false);
        }

        private async UniTaskVoid ProcessOpen(bool isShow)
        {
            CancellationToken token = BeginTransition();

            ConstructSequence();

            if (isShow)
                _onShowStart?.Invoke();
            else
                _onOpenStart?.Invoke();

            gameObject.SetActive(true);

            _canvasGroup.interactable = false;

            if (_openDuration > 0.0f)
            {
                _sequence.timeScale = 1.0f / _openDuration;

                _sequence.Complete();
                _sequence.Restart();
                _sequence.Play();

                bool canceled = await UniTask.WaitForSeconds(_openDuration, true, cancellationToken: token)
                    .SuppressCancellationThrow();
                if (canceled) return;
            }
            else
            {
                _sequence.Complete();
            }

            if (isShow)
                _onShowEnd?.Invoke();
            else
                _onOpenEnd?.Invoke();

            _canvasGroup.interactable = true;
        }

        private async UniTaskVoid ProcessClose(bool isHiding)
        {
            CancellationToken token = BeginTransition();

            ConstructSequence();

            if (isHiding)
                _onHideStart?.Invoke();
            else
                _onCloseStart?.Invoke();

            _canvasGroup.interactable = false;

            if (_closeDuration > 0.0f)
            {
                _sequence.timeScale = 1.0f / _closeDuration;

                _sequence.Complete();
                _sequence.PlayBackwards();

                bool canceled = await UniTask.WaitForSeconds(_closeDuration, true, cancellationToken: token)
                    .SuppressCancellationThrow();
                if (canceled) return;
            }
            else
            {
                _sequence.Rewind();
            }

            if (isHiding)
            {
                _onHideEnd?.Invoke();

                gameObject.SetActive(false);
            }
            else
            {
                _onCloseEnd?.Invoke();
            }
        }

        #endregion

        #region Function -> Public

        /// <summary>Returns whether the view accepts input; true when no canvas group is assigned.</summary>
        public bool Interactable()
        {
            if (_canvasGroup == null)
            {
                return true;
            }
            return _canvasGroup.interactable;
        }

        /// <summary>Plays the open transition.</summary>
        public void Open()
        {
            ProcessOpen(false).Forget();
        }

        /// <summary>Plays the close transition.</summary>
        public void Close()
        {
            ProcessClose(false).Forget();
        }

        /// <summary>Makes the view accept input again.</summary>
        public void Reveal()
        {
            if (_canvasGroup)
            {
                _canvasGroup.interactable = true;
            }
        }

        /// <summary>Stops the view from accepting input.</summary>
        public void Block()
        {
            if (_canvasGroup)
            {
                _canvasGroup.interactable = false;
            }
        }

        /// <summary>Sets the sorting order of the view canvas, if it has one.</summary>
        public void SetOrder(int order)
        {
            if (_canvas == null) return;
            _canvas.sortingOrder = order;
        }

        #endregion
    }
}
