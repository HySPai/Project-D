using UnityEngine;

/// <summary>
/// Whole-heart health: damage points pile up in a remainder and every full pointsPerHeart removes one heart.
/// Pure C#, owned by PlayerState.
/// </summary>
public sealed class HeartPool
{
    private const float MinPointsPerHeart = 0.01f;

    private int current;
    private float damageRemainder;

    public int Current => current;

    /// <summary>Sets hearts to max and forgets any partial damage.</summary>
    public void Fill(int max)
    {
        current = max;
        damageRemainder = 0f;
    }

    /// <summary>Adds points of damage; returns true when at least one whole heart was taken.</summary>
    public bool ApplyDamage(float points, float pointsPerHeart)
    {
        float perHeart = Mathf.Max(MinPointsPerHeart, pointsPerHeart);
        damageRemainder += points;
        int hearts = Mathf.FloorToInt(damageRemainder / perHeart);
        damageRemainder -= hearts * perHeart;

        if (hearts <= 0) return false;

        current = Mathf.Max(0, current - hearts);
        return true;
    }

    /// <summary>Restores up to amount hearts without passing max, clears partial damage, and returns the hearts gained.</summary>
    public int Heal(int amount, int max)
    {
        damageRemainder = 0f;
        int before = current;
        current = Mathf.Min(max, current + amount);
        return current - before;
    }
}

/// <summary>
/// Stamina value, regeneration delay and the exhausted-run gate. Every mutator returns true only when the
/// value really changed, so listeners are not notified every frame. Pure C#, owned by PlayerState.
/// </summary>
public sealed class StaminaPool
{
    private float current;
    private float lastUseTime;
    private bool runExhausted;

    public float Current => current;

    /// <summary>Sets stamina to max and lifts the exhausted-run gate.</summary>
    public void Fill(float max)
    {
        current = max;
        runExhausted = false;
    }

    /// <summary>Forgets the last use, so regeneration is not delayed.</summary>
    public void ResetUseTime() => lastUseTime = 0f;

    /// <summary>Spends amount, restarting the regeneration delay at now; returns true when the value changed.</summary>
    public bool Drain(float amount, float now)
    {
        if (amount <= 0f) return false;

        float before = current;
        current = Mathf.Max(0f, current - amount);
        lastUseTime = now;
        return current != before;
    }

    /// <summary>Refills at rate per second once delay has passed since the last use; returns true when the value changed.</summary>
    public bool Regenerate(float max, float rate, float delay, float now, float deltaTime)
    {
        if (current >= max) return false;
        if (now - lastUseTime < delay) return false;

        float before = current;
        current = Mathf.Min(max, current + rate * deltaTime);
        return current != before;
    }

    /// <summary>Running locks out at zero stamina and unlocks again at resumeThreshold; returns whether running is allowed.</summary>
    public bool UpdateRunGate(float resumeThreshold)
    {
        if (current <= 0f) runExhausted = true;
        else if (current >= resumeThreshold) runExhausted = false;
        return !runExhausted;
    }
}
