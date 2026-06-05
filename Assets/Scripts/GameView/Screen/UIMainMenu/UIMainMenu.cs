using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMainMenu : BaseView
{
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnSetting;
    [SerializeField] private TextMeshProUGUI txtLevel;
    UISetting UISetting => GameViewsManager.Instance.GetView<UISetting>();
    public override void Initialize(CancellationToken propagateCancellationToken)
    {
        base.Initialize(propagateCancellationToken);
        btnSetting.onClick.AddListener(OnClickSetting);
    }
    public override void Show()
    {
        base.Show();
        txtLevel.text = "Level " + PlayerprefSave.Level.ToString();
    }
    public override void Hide()
    {
        base.Hide();
    }
    public void OnClickPlayGame(UnityAction action)
    {
        btnPlay.onClick.RemoveAllListeners(); // Clear previous listeners to avoid duplicates
        btnPlay.onClick.AddListener(action);
    }
    public void OnClickSetting()
    {
        UISetting.Show();
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_positive);
    }
}
