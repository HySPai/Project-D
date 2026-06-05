using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class TrailMover : MonoBehaviour
{
    public float moveDuration = 1f;
    public float delay = 1f;
    public Transform hand;
    public TrailRenderer trail;
    public ParticleSystem tuchEffect;
    public Vector3 target;
    private bool isDirty;
    public void StartAnim()
    {
        LoopMoveAsync(); // bắt đầu vòng lặp
    }
    private async UniTaskVoid LoopMoveAsync()
    {
        await UniTask.WaitForSeconds(delay);

        if (trail == null) return;
        while (trail)
        {
            hand.localPosition = Vector3.zero;

            // Bật trail
            trail.emitting = true;
            trail.ResetBounds();
            trail.Clear();
            if(!isDirty)
            {
                isDirty = true;
            }
            else
            {
                tuchEffect.Play();

            }
            // Di chuyển sang phải
            await hand.DOMove(target, moveDuration).SetEase(Ease.InOutSine).AsyncWaitForCompletion();

            // Tắt trail để không vẽ lại

            // Reset trail

            // Quay về vị trí cũ (không cần trail)

            // Chờ 1 giây
            await UniTask.Delay(1000);
            if (trail == null) return;
            trail.emitting = false;
            trail.Clear();

        }
    }
}
