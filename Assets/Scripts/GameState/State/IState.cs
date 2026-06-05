using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public interface IState
{
    /// <summary>
    /// request change state
    /// </summary>
    /// <returns></returns>
    public IObservable<int> OnRequestStateChange();

    /// <summary>
    /// init state
    /// </summary>
    public void Initialize();

    /// <summary>
    /// destroy state
    /// </summary>
    public void Dispose();

    /// <summary>
    /// start state
    /// </summary>
    public UniTask Start();

    /// <summary>
    /// update in state
    /// </summary>
    public int Update();

    /// <summary>
    /// end state
    /// </summary>
    public void End();


    /// <summary>
    ///create cancellationtoken in state
    /// </summary>
    /// <returns></returns>
    public CancellationToken GetCancellationTokenInState();

    /// <summary>
    /// create cancellationtoken in game
    /// </summary>
    /// <returns></returns>
    public CancellationToken GetCancellationTokenInGame();
}

