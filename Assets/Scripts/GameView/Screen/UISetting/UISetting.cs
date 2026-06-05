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

public class UISetting : BaseView
{
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnSound;
    [SerializeField] private Button btnMusicBg;
    [SerializeField] private Button btnHaptic;

    [SerializeField] private GameObject[] statusSound;
    [SerializeField] private GameObject[] statusMusic;
    [SerializeField] private GameObject[] statusHaptic;

    [SerializeField] TextMeshProUGUI txtVersion;
    [SerializeField] TMP_InputField inputField;

    public override void Initialize(CancellationToken propagateCancellationToken)
    {
        base.Initialize(propagateCancellationToken);
        btnClose.onClick.AddListener(OnClickClose);
        btnSound.onClick.AddListener(OnClickSound);
        btnMusicBg.onClick.AddListener(OnCLickMusicBg);
        btnHaptic.onClick.AddListener(OnClickHaptic);
        OnChangeVisualSound();
        OnChangeVisualMusic();
        OnChangeVisualHaptic();
        txtVersion.text = $"Version: {Application.version}";
    }
   
    public override void Show()
    {
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_appear_popup, 1, -1, false);
         
        base.Show();
    }
    public override void Hide()
    {
        base.Hide();
    }
    private void OnClickClose()
    {
        HideByScale();
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_positive);
    }
    private void OnClickSound()
    {
        AudioManager.Instance.SetSfxVolume(PlayerprefSave.Sound == 0 ? 1 : 0);
        OnChangeVisualSound();
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_positive);
    }
    private void OnCLickMusicBg()
    {
        AudioManager.Instance.SetBackgroundVolume(PlayerprefSave.Music == 0 ? 1 : 0);
        OnChangeVisualMusic();
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_positive);
    }
    private void OnClickHaptic()
    {
        PlayerprefSave.Haptic = PlayerprefSave.Haptic == 0 ? 1 : 0;
        HapticController.Instance.PlayHaptic();
        OnChangeVisualHaptic();
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_positive);
    }
    private void OnChangeVisualSound()
    {
        statusSound[0].SetActive(PlayerprefSave.Sound == 1);
        statusSound[1].SetActive(PlayerprefSave.Sound == 0);
    }
    private void OnChangeVisualMusic()
    {
        statusMusic[0].SetActive(PlayerprefSave.Music == 1);
        statusMusic[1].SetActive(PlayerprefSave.Music == 0);
    }
    private void OnChangeVisualHaptic()
    {
        statusHaptic[0].SetActive(PlayerprefSave.Haptic == 1);
        statusHaptic[1].SetActive(PlayerprefSave.Haptic == 0);
    }
}
