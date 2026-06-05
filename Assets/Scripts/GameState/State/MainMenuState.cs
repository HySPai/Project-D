using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;

public class MainMenuState : BaseState
{
    UIMainMenu UIMainMenu => GameViewsManager.Instance.GetView<UIMainMenu>();
    public override void Initialize()
    {
        base.Initialize();

    }
    public override async UniTask Start()
    {
        await base.Start();
        UIMainMenu.Show();
        UIMainMenu.OnClickPlayGame(delegate
        {
            HapticController.Instance.PlayHaptic();
            this.RequestStateChange(1);
            AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_play_click);
        });
    }
    public override int Update()
    {
        return base.Update();
    }
    public override void End()
    {
        UIMainMenu.Hide();
        base.End();
    }
}
