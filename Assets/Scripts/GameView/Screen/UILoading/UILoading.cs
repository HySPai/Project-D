using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class UILoading : BaseView
{
    [SerializeField] private TextMeshProUGUI progressText;
    public override void Initialize(CancellationToken propagateCancellationToken)
    {
        base.Initialize(propagateCancellationToken);
    }
    public override void Show()
    {
        base.Show();
        StartProgress(3).Forget();

    }
    public override void Hide()
    {
        base.Hide();
    }
    public void RunNumber()
    {
    }
  
    public async UniTaskVoid StartProgress(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int percent = Mathf.RoundToInt(t * 100f);

            if (progressText != null)
                progressText.text = $"{percent}%";

            await UniTask.Yield(); // Đợi frame tiếp theo
        }

        // Đảm bảo kết thúc ở 100%
        if (progressText != null)
            progressText.text = "100%";
    
    }

}
