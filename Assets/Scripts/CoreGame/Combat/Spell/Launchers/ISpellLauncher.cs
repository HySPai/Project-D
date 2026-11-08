/// <summary>Casts the combat skill of one element; mana and cooldown stay with PlayerElementCaster.</summary>
public interface ISpellLauncher
{
    /// <summary>The element whose skill this launcher casts.</summary>
    ElementType Element { get; }

    /// <summary>True while the skill is still winding up, which blocks every other cast.</summary>
    bool IsBusy { get; }

    /// <summary>Starts the skill; false when it cannot be cast, so no mana or cooldown is spent.</summary>
    bool TryLaunch();
}
