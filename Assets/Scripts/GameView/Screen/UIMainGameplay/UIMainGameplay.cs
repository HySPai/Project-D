using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIMainGameplay : BaseView
{
    [SerializeField] private Button btnSetting;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private TextMeshProUGUI txtCountMove;
    [SerializeField] private Image imgProgress;
    [SerializeField] private Image imgProgress_lose;
    [SerializeField] private Transform levelInfoTransfrom;
    [SerializeField] private Button btnShowWin;
    [SerializeField] private Button btnShowLose;

    UISetting UISetting =>GameViewsManager.Instance.GetView<UISetting>();
    UIWin UIWin => GameViewsManager.Instance.GetView<UIWin>();
    UILose UILose => GameViewsManager.Instance.GetView<UILose>();
    private bool hasFlashed = false; // trạng thái đã nháy chưa

    public override void Initialize(CancellationToken propagateCancellationToken)
    {
        base.Initialize(propagateCancellationToken);
        btnSetting.onClick.AddListener(OnClickSetting);
        btnShowWin.onClick.AddListener(OnShowWin);
        btnShowLose.onClick.AddListener(OnShowLose);
    }
    public override void Show()
    {
        base.Show();
        ChangeLevelText();
        ChangeMoveRemains(10, 10);
    }
   
    public override void Hide()
    {
        base.Hide();
    }
    private void OnClickSetting()
    {
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_positive);
        UISetting.Show();
    }
    public void ChangeMoveRemains(int moves, int maxMoves)
    {
        imgProgress
            .DOFillAmount((float)moves / maxMoves, 0.3f)
            .SetEase(Ease.OutQuad);

        imgProgress_lose
            .DOFillAmount((float)moves / maxMoves, 0.3f)
            .SetEase(Ease.OutQuad);
        imgProgress_lose.gameObject.SetActive(moves <= 5);
        txtCountMove.text = moves.ToString();
    }


    public void ChangeLevelText()
    {
        txtLevel.text ="Level "+ PlayerprefSave.Level;
    }
    private void OnShowWin()
    {
        UIWin.ShowByAlpha();
    }
    private void OnShowLose()
    {
        UILose.ShowByAlpha();
    }
}
