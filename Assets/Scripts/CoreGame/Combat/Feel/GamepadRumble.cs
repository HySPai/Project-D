using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct RumblePulse
{
    [Range(0f, 1f)] public float lowFrequency;
    [Range(0f, 1f)] public float highFrequency;
    [Range(0f, 1f)] public float duration;

    public RumblePulse(float lowFrequency, float highFrequency, float duration)
    {
        this.lowFrequency = lowFrequency;
        this.highFrequency = highFrequency;
        this.duration = duration;
    }
}

public class GamepadRumble : MonoBehaviour
{
    [SerializeField] private bool rumbleEnabled = true;

    [Header("Hit Dealt")]
    [SerializeField] private RumblePulse hitLight = new RumblePulse(0.15f, 0.35f, 0.08f);
    [SerializeField] private RumblePulse hitHeavy = new RumblePulse(0.3f, 0.5f, 0.12f);
    [SerializeField] private RumblePulse hitKill = new RumblePulse(0.45f, 0.6f, 0.18f);

    [Header("Damage Taken")]
    [SerializeField] private RumblePulse damageTaken = new RumblePulse(0.6f, 0.4f, 0.2f);
    [SerializeField] private RumblePulse death = new RumblePulse(0.9f, 0.6f, 0.45f);

    private CancellationTokenSource pulseCts;
    private Gamepad activeGamepad;

    /// <summary>Rumbles for a hit the player dealt. Example: PlayHit(HitFeelTier.Kill)</summary>
    public void PlayHit(HitFeelTier tier)
    {
        switch (tier)
        {
            case HitFeelTier.Heavy: Play(hitHeavy); break;
            case HitFeelTier.Kill: Play(hitKill); break;
            default: Play(hitLight); break;
        }
    }

    /// <summary>Rumbles for damage the player took. Example: PlayDamageTaken(died: false)</summary>
    public void PlayDamageTaken(bool died) => Play(died ? death : damageTaken);

    /// <summary>Runs one pulse on the last used gamepad, replacing any pulse in progress. Example: Play(new RumblePulse(0.5f, 0.5f, 0.1f))</summary>
    public void Play(RumblePulse pulse)
    {
        if (!rumbleEnabled || pulse.duration <= 0f) return;

        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;

        Stop();
        activeGamepad = gamepad;
        gamepad.SetMotorSpeeds(pulse.lowFrequency, pulse.highFrequency);

        pulseCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        StopAfterAsync(pulse.duration, pulseCts.Token).Forget();
    }

    /// <summary>Cancels the current pulse and silences the motors.</summary>
    public void Stop()
    {
        if (pulseCts != null)
        {
            pulseCts.Cancel();
            pulseCts.Dispose();
            pulseCts = null;
        }

        ResetMotors();
    }

    private async UniTaskVoid StopAfterAsync(float duration, CancellationToken token)
    {
        bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(duration), DelayType.Realtime, cancellationToken: token)
            .SuppressCancellationThrow();
        if (cancelled) return;

        ResetMotors();
    }

    private void ResetMotors()
    {
        if (activeGamepad == null) return;

        activeGamepad.ResetHaptics();
        activeGamepad = null;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) Stop();
    }

    private void OnDisable() => Stop();
}
