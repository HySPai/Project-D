using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;

public class BaseState : IState
{
    static CancellationToken cancellationTokenInGame;

    protected int LoopCount;
    CancellationTokenSource cancellationTokenSourceInState;

    /// <summary>
    /// change true if state done
    ///skip if Update, End called
    /// </summary>
    protected bool AlreadyDone;

    /// <summary>
    /// notify while state complete
    /// </summary>
    protected Subject<int> CloseNotify;

    /// <summary>
    /// register receive notify while complete state
    /// </summary>
    /// <returns></returns>
    public IObservable<int> OnRequestStateChange()
    {
        return this.CloseNotify;
    }

    /// <summary>
    /// Init state
    /// </summary>
    public virtual void Initialize()
    {
        this.LoopCount = 0;
    }

    /// <summary>
    /// destroy state
    /// </summary>
    public virtual void Dispose()
    {
       
    }

    /// <summary>
    /// start state
    /// </summary>
    public virtual async UniTask Start()
    {
        this.AlreadyDone = false;

        this.CloseNotify = new Subject<int>();

        this.LoopCount++;

        // init cancellationtoken in state
        this.cancellationTokenSourceInState = new CancellationTokenSource();

        await UniTask.Yield();
    }

    /// <summary>
    /// update state
    /// </summary>
    public virtual int Update()
    {
        // skip if state complete
        if (this.AlreadyDone) return 0;

        var result = 0;
        if (Input.GetKeyDown(KeyCode.A))
        {
            result = 1;
        }
        else
        if (Input.GetKeyDown(KeyCode.B))
        {
            result = 2;
        }
        else
        if (Input.GetKeyDown(KeyCode.C))
        {
            result = 3;
        }

        return result;
    }

    /// <summary>
    /// end state
    /// </summary>
    public virtual void End()
    {
        // skip if state complete
        if (this.AlreadyDone) return;
        this.AlreadyDone = true;

        this.CloseNotify.Dispose();
        this.CloseNotify = null;


        this.cancellationTokenSourceInState.Cancel();
        this.cancellationTokenSourceInState.Dispose();
        this.cancellationTokenSourceInState = null;
    }

    /// <summary>
    ///get cancellationtoken in state
    /// </summary>
    /// <returns></returns>
    public CancellationToken GetCancellationTokenInState()
    {
        return this.cancellationTokenSourceInState.Token;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public CancellationToken GetCancellationTokenInGame()
    {
        return cancellationTokenInGame;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static void SetCancellationTokenInGame(CancellationToken cancellationToken)
    {
        cancellationTokenInGame = cancellationToken;
    }


    /// <summary>
    /// change state next in matrix state
    /// </summary>
    /// <param name="result"></param>
    public void RequestStateChange(int result)
    {
        if (this.AlreadyDone) return;

        if (result == 0) return;
        if (this.CloseNotify == null)
        {
           
        }
        this.CloseNotify.OnNext(result);
    }

}

