using UnityEngine;

public class PoiseMeter
{
    private readonly float max;
    private readonly float recoverDelay;
    private readonly float recoverRate;

    private float current;
    private float lastHitTime;
    private bool hitSinceReset;

    public PoiseMeter(float max, float recoverDelay, float recoverRate)
    {
        this.max = Mathf.Max(0f, max);
        this.recoverDelay = Mathf.Max(0f, recoverDelay);
        this.recoverRate = Mathf.Max(0f, recoverRate);
        Reset();
    }

    /// <summary>False when max is 0: the enemy never breaks and keeps the plain hit reaction.</summary>
    public bool Enabled => max > 0f;
    public float Max => max;

    /// <summary>Poise at time now, refilled at recoverRate once recoverDelay has passed since the last hit.</summary>
    public float GetCurrent(float now)
    {
        if (!Enabled) return 0f;
        if (!hitSinceReset || recoverRate <= 0f) return current;

        float idle = now - lastHitTime - recoverDelay;
        return idle > 0f ? Mathf.Min(max, current + idle * recoverRate) : current;
    }

    /// <summary>Removes amount of poise; returns true when it reaches 0, which breaks the enemy and refills the meter.</summary>
    public bool Hit(float amount, float now)
    {
        if (!Enabled || amount <= 0f) return false;

        float value = GetCurrent(now) - amount;
        lastHitTime = now;
        hitSinceReset = true;

        if (value <= 0f)
        {
            current = max;
            return true;
        }

        current = value;
        return false;
    }

    /// <summary>Fills the meter, as a world reset does.</summary>
    public void Reset()
    {
        current = max;
        lastHitTime = 0f;
        hitSinceReset = false;
    }
}
