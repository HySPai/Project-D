using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


public class GameViewsManager : SingletonMonoBehaviour<GameViewsManager>
{
    [SerializeField] private List<BaseView> views = new List<BaseView>();
    public bool isShowingPopup = false;
    CancellationToken cancellationToken;
    Queue<BaseView> queueViews = new Queue<BaseView>();
    //public static GameViewsManager Instance;
    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject);
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}
    public async UniTask Initialize(CancellationToken cancellationToken)
    {
        Debug.Log("Initialized UI Start");
        this.cancellationToken = cancellationToken;
        foreach (var view in this.views)
        {
            view.Initialize(this.cancellationToken);
        }
        await UniTask.Yield(PlayerLoopTiming.Update);
        Debug.Log("Initialized UI Done");
    }

    public T GetView<T>() where T : BaseView
    {
        foreach (var view in this.views)
        {
            if (view is T)
            {
                return view as T;
            }
        }

        return default(T);
    }
    public void AddViewToQueue(BaseView view)
    {
        queueViews.Enqueue(view);
    }
    public void ShowQueueView()
    {
        if (isShowingPopup) return;
        if (queueViews.Count > 0)
        {
            queueViews.Dequeue().Show();
        }
    }
}
