using UnityEngine;

/// <summary>
/// Training dummy state: takes hits, reports the damage, never loses health and never dies.
/// Example: put it on a box with a DummyController to test hit blood and feel without killing anything.
/// </summary>
public class DummyState : CharacterStateBase
{
    [Header("Dummy")]
    [SerializeField] private HitBloodProfile hitBlood;

    public override HitBloodProfile HitBlood => hitBlood;

    protected override DamageResult ApplyDamage(in DamageInfo info)
    {
        PlayDamageFeedback();
        RaiseHpChanged();
        return new DamageResult(info.points, false, false, false);
    }
}
