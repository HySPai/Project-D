using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class InGameState : BaseState
{
    UIMainGameplay UIMainGameplay => GameViewsManager.Instance.GetView<UIMainGameplay>();
    public bool IsEndLevel;
    public override void Initialize()
    {
        base.Initialize();

    }
    public override async UniTask Start()
    {
        IsEndLevel = false;
        await base.Start();
        UIMainGameplay.ShowByAlpha();
        await UniTask.WaitUntil(() => IsEndLevel == true).AttachExternalCancellation(GetCancellationTokenInState());
        RequestStateChange(1);
    }
   
    public override int Update()
    {
        return base.Update();
    }
    public override void End()
    {
        UIMainGameplay.Hide();
        base.End();
    }
}
