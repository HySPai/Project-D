using UnityEngine;

public class SlowEffect : MonoBehaviour
{
    [SerializeField] private SO_SlowData data;

    private CharacterControllerBase owner;
    private float remaining;

    public bool IsSlowed => remaining > 0f;

    /// <summary>Factor for move and animation speed: the slow multiplier while slowed, otherwise 1.</summary>
    public float SpeedMultiplier => IsSlowed && data != null ? data.speedMultiplier : 1f;

    public void Initialize(CharacterControllerBase owner) => this.owner = owner;

    /// <summary>Slows the target, or refreshes the timer if it is already slowed; slow never stacks.</summary>
    public void Apply()
    {
        if (data == null || owner == null) return;

        CharacterStateBase state = owner.GetState;
        if (state == null || state.IsDead) return;

        remaining = data.duration;
    }

    /// <summary>Ends the slow at once.</summary>
    public void Stop() => remaining = 0f;

    private void Update()
    {
        if (!IsSlowed) return;

        remaining -= Time.deltaTime;
    }
}
