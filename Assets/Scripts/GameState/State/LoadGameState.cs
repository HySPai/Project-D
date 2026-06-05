using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LoadGameState : BaseState
{
    UILoading UILoading => GameViewsManager.Instance.GetView<UILoading>();
    public override void Initialize()
    {
        base.Initialize();
    }
    public override async UniTask Start()
    {
        await base.Start();
        UILoading.Show();
        await UniTask.Delay(3000).AttachExternalCancellation(GetCancellationTokenInState());
        await UniTask.WaitUntil(() => TinySauceSDKController.Instance.isInitialized).AttachExternalCancellation(GetCancellationTokenInState());
        this.RequestStateChange(1);
    }
    public override int Update()
    {
        return base.Update();
    }
    public override void End()
    {
        UILoading.Hide();
        base.End();
    }

}
