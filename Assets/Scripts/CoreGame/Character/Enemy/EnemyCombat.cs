using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyCombat : CharacterCombatBase
{
    private CharacterControllerBase character;
    private EnemyState enemyState;

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
    }

    private void HandleHpChanged(float currentHp, float maxHp)
    {
        if (currentHp < lastKnownHp)
            CancelTelegraph();
        lastKnownHp = currentHp;
    }
}
