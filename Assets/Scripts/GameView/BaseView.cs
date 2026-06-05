using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BaseView : MonoBehaviour, IView
{
    protected CancellationTokenSource ViewCancellationTokenSource;
    protected CancellationToken ViewCancellationToken;
    protected CancellationToken ViewManagerCancellationToken;
    [SerializeField] private bool _isWaitShow = false;
    /// <summary>
    /// init view
    /// </summary>
    /// <param name="propagateCancellationToken"></param>
    public virtual void Initialize(CancellationToken propagateCancellationToken)
    {
        PropagateCancellationToken(propagateCancellationToken);
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// show UI
    /// </summary>
    public virtual void Show()
    {
        this.gameObject.SetActive(true);
        if (_isWaitShow)
        {
            GameViewsManager.Instance.isShowingPopup = true;
        }
    }
    public virtual void ShowQueue()
    {
        GameViewsManager.Instance.AddViewToQueue(this);
        GameViewsManager.Instance.ShowQueueView();
    }
    public virtual void ShowByAlpha(float timeShow = 0.5f)
    {
        if (this.gameObject.TryGetComponent(out CanvasGroup canvasGroup))
        {
            canvasGroup.alpha = 0;
            Show();
            canvasGroup.DOFade(1, 0.5f).SetEase(Ease.Linear);
        }
        else
        {
            Debug.LogError("Not exist canvas group");
            Hide();
        }
    }
    /// <summary>
    /// hide ui
    /// </summary>
    public virtual void Hide()
    {
        this.gameObject.SetActive(false);
        if (_isWaitShow)
        {
            GameViewsManager.Instance.isShowingPopup = false;
        }
        GameViewsManager.Instance.ShowQueueView();
    }
    /// <summary>
    /// hide ui
    /// </summary>
    public virtual void HideByAlpha(float timeHide = 0.5f)
    {
        if (this.gameObject.TryGetComponent(out CanvasGroup canvasGroup))
        {
            canvasGroup.DOFade(0, timeHide).SetEase(Ease.Linear).OnComplete(() =>
            {
                Hide();
                ChangeAlpha(1);
            });
        }
        else
        {
            Debug.LogError("Not exist canvas group");
            Hide();
        }

    }
    /// <summary>
    /// hide ui
    /// </summary>
    public virtual void HideByScale(float timeHide = 0.1f)
    {
        this.transform.GetChild(0).DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), timeHide).SetEase(Ease.Linear).OnComplete(() =>
             {
                 Hide();
                 this.transform.GetChild(0).localScale = Vector3.one;
             });
    }
    public virtual void ChangeAlpha(float valueAlpha)
    {
        if (this.gameObject.TryGetComponent(out CanvasGroup canvasGroup))
        {
            canvasGroup.alpha = valueAlpha;
        }
        else
        {
            Debug.LogError("Not exist canvas group");
        }
    }

    /// <summary>
    /// while viewmanager is destroy, create cancellationtokensource destroy
    /// </summary>
    /// <returns></returns>
    public CancellationTokenSource CreateLinkedTokenSource()
    {
        return CancellationTokenSource.CreateLinkedTokenSource(this.ViewCancellationToken, this.ViewManagerCancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void PropagateCancellationToken(CancellationToken cancellationToken)
    {
        this.ViewManagerCancellationToken = cancellationToken;
    }

    protected virtual void OnEnable()
    {
        this.ViewCancellationTokenSource = new CancellationTokenSource();
        this.ViewCancellationToken = this.ViewCancellationTokenSource.Token;
    }

    protected virtual void OnDisable()
    {
        if (!this.ViewCancellationTokenSource.IsCancellationRequested)
        {
            this.ViewCancellationTokenSource.Cancel();
        }
        this.ViewCancellationTokenSource.Dispose();
    }
    protected virtual void OnDestroy()
    {

    }
}
