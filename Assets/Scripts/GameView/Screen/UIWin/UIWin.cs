using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UIWin : BaseView
{
    [SerializeField] private GameObject popupWin;
    [SerializeField] private Sprite[] spNumbers;
    [SerializeField] private Image[] imgNumbers;
    [SerializeField] private Button btnNextLevel;
    private bool btnClicked;
    public override void Initialize(CancellationToken propagateCancellationToken)
    {
        base.Initialize(propagateCancellationToken);
        btnNextLevel.onClick.AddListener(OnClickNextLevel);
        popupWin.SetActive(false);
    }

    private void OnClickNextLevel()
    {
        if (btnClicked) return;
        btnClicked = true;
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_positive);
        GameViewsManager.Instance.GetView<UIMainGameplay>().ChangeLevelText();
        Hide();
        GameStateManager.Instance.GetCurrentStateClass<InGameState>().IsEndLevel = true;
    }

    public override void Show()
    {
        base.Show();
        int level = PlayerprefSave.Level;
        int number = level / 10;
        int number2 = level % 10;
        imgNumbers[0].sprite = spNumbers[number];
        imgNumbers[1].sprite = spNumbers[number2];
        
        imgNumbers[0].SetNativeSize();
        imgNumbers[1].SetNativeSize();

        imgNumbers[0].gameObject.SetActive(number > 0);

        DOVirtual.DelayedCall(1.5f, () =>
        {
            popupWin.SetActive(true);
            AudioManager.Instance.PlaySfx(AudioSfxID.sfx_level_up, 1, -1, false);

        });
        TinySauceSDKController.Instance.OnWin();
    }
    public override void Hide()
    {
        popupWin.SetActive(false);
        base.Hide();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        btnClicked = false;
    }
}
