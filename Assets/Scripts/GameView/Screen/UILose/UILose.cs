using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILose : BaseView
{
    public const string LOSE_TEXT = ", you can play again to complete the level.";
    [SerializeField] private Button btnTryAgain;
    [SerializeField] private TextMeshProUGUI loseTextMesh;
    public override void Initialize(CancellationToken propagateCancellationToken)
    {
        base.Initialize(propagateCancellationToken);
        btnTryAgain.onClick.AddListener(OnTryAgain);
    }
    protected override void OnDestroy()
    {
        btnTryAgain.onClick.RemoveAllListeners();
    }
    private async void OnTryAgain()
    {
        Debug.Log("try again");
        Hide();
        GameStateManager.Instance.GetCurrentStateClass<InGameState>().IsEndLevel = true;
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_positive);
    }

    public override void Show()
    {
        base.Show();
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_lose, 1, -1, false);
        TinySauceSDKController.Instance.OnLose();
    }
    public void SetLoseText(string text)
    {
        loseTextMesh.text = $"<color=\"red\">{text}</color>{LOSE_TEXT}";
    }
    public override void Hide()
    {
        base.Hide();
    }
}
