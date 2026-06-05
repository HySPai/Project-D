using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UITutorial : BaseView
{
    [SerializeField] private Animator animatorHand;
    [SerializeField] private Canvas currentCanvas;
    private GameObject handTrail;
    public override void Initialize(CancellationToken propagateCancellationToken)
    {
        base.Initialize(propagateCancellationToken);
    }
    public override void Show()
    {
        base.Show();
    }
    public override void Hide()
    {
        base.Hide();
        if (handTrail != null)
        {
            handTrail.SetActive(false);
        }
    }
    public void ShowHandClick( Vector3 worldPos)
    {
        animatorHand.gameObject.SetActive(true);
        animatorHand.transform.position = Helper.CalculatePositionFromTransformToRectTransform(currentCanvas, worldPos, Camera.main);
        animatorHand.Play("Click");
        Show();
    }
    public void ShowHandTrail(Vector3 from, Vector3 to)
    {
        animatorHand.gameObject.SetActive(false);
        handTrail = EffectController.Instance.GetFx(VFXName.prefab_hand_trail, from);
        if(handTrail.TryGetComponent(out TrailMover trailMover))
        {
            trailMover.target = to;
            trailMover.StartAnim();
        }
        else
        {
            Debug.LogError("Trail Mover not found on hand trail effect");
        }
    }
}
//public enum TypeDirectionHand : byte
//{
//    Left,
//    Right,
//    Up,
//    Down,
//    Click
//}