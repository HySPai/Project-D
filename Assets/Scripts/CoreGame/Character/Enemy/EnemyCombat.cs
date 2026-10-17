using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoreMountains.Feedbacks;
using UnityEngine;

public class EnemyCombat : CharacterCombatBase
{
    [Header("Telegraph")]
    [SerializeField] private MMF_Player telegraphFeedback;
    [SerializeField] private AudioSource telegraphAudioSource;

    private CharacterControllerBase character;
    private EnemyState enemyState;

    private MMF_Flicker telegraphFlicker;
    private bool telegraphFlickerResolved;
    private bool telegraphActive;
    private int attackSerial;
    private float lastKnownHp;
    private CancellationToken destroyToken;

    public void Initialize(CharacterStateBase state, CharacterControllerBase character)
    {
        base.Initialize(state);
        this.character = character;
        this.enemyState = state as EnemyState;

        // Cho phép đánh ngay đòn đầu (không phải chờ hết 1 cooldown khi vừa spawn).
        lastAttackTime = -enemyState.AttackCooldown;

        destroyToken = this.GetCancellationTokenOnDestroy();
        lastKnownHp = state.CurrentHp;
        state.OnHpChanged -= HandleHpChanged;
        state.OnHpChanged += HandleHpChanged;
    }

    private void OnDestroy()
    {
        if (state != null)
            state.OnHpChanged -= HandleHpChanged;
    }

    public bool CanAttack => Time.time - lastAttackTime >= enemyState.AttackCooldown;

    public override void Attack()
    {
        if (state == null || state.IsDead) return;
        if (state.IsAttacking) return;
        if (!CanAttack) return;

        lastAttackTime = Time.time;

        SetDamage(0, enemyState.AttackDamage, DamageSource.Weapon, ElementType.None, enemyState.AttackUnblockable);
        PlayAttackAnimation(enemyState.AttackAnimationName, character);

        attackSerial++;

        EnemyCombatFeelProfile feel = enemyState.CombatFeel;
        if (feel != null && feel.HasTelegraph)
            BeginTelegraph(feel, attackSerial);
    }

    public override void DisableAllDamageColliders()
    {
        CancelTelegraph();
        base.DisableAllDamageColliders();
    }

    private void BeginTelegraph(EnemyCombatFeelProfile feel, int serial)
    {
        telegraphActive = true;

        character.GetAnimation.SetPlaybackSpeed(feel.WindupAnimationSpeed);

        if (telegraphFeedback != null)
        {
            MMF_Flicker flicker = ResolveTelegraphFlicker();
            if (flicker != null)
            {
                flicker.FlickerColor = feel.TelegraphColor;
                flicker.FlickerDuration = feel.TelegraphDuration;
                flicker.FlickerPeriod = feel.TelegraphFlickerPeriod;
            }
            telegraphFeedback.PlayFeedbacks(transform.position);
        }

        if (feel.TelegraphSound != null && telegraphAudioSource != null)
            telegraphAudioSource.PlayOneShot(feel.TelegraphSound);

        EndTelegraphAfterDelay(serial, feel.TelegraphDuration).Forget();
    }

    private async UniTaskVoid EndTelegraphAfterDelay(int serial, float duration)
    {
        bool cancelled = await UniTask
            .Delay(TimeSpan.FromSeconds(duration), false, PlayerLoopTiming.Update, destroyToken)
            .SuppressCancellationThrow();

        if (cancelled) return;
        if (serial != attackSerial || !telegraphActive) return;

        telegraphActive = false;

        if (state == null || state.IsDead || !state.IsAttacking) return;

        character.GetAnimation.SetPlaybackSpeed(1f);
    }

    private void CancelTelegraph()
    {
        if (!telegraphActive) return;

        telegraphActive = false;
        attackSerial++;

        if (telegraphFeedback != null)
        {
            telegraphFeedback.StopFeedbacks();
            telegraphFeedback.ResetFeedbacks();
        }
    }

    private void HandleHpChanged(float currentHp, float maxHp)
    {
        if (currentHp < lastKnownHp)
            CancelTelegraph();
        lastKnownHp = currentHp;
    }

    private MMF_Flicker ResolveTelegraphFlicker()
    {
        if (!telegraphFlickerResolved)
        {
            telegraphFlicker = telegraphFeedback.GetFeedbackOfType<MMF_Flicker>();
            telegraphFlickerResolved = true;
        }
        return telegraphFlicker;
    }
}
