public struct DamageResult
{
    public float appliedPoints;
    public bool blocked;
    public bool parried;
    public bool targetDied;

    public DamageResult(float appliedPoints, bool blocked, bool parried, bool targetDied)
    {
        this.appliedPoints = appliedPoints;
        this.blocked = blocked;
        this.parried = parried;
        this.targetDied = targetDied;
    }
}
