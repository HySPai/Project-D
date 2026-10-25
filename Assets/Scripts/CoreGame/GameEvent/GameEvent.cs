using System;

public static class GameEvent
{
    public static Action OnPlay { get; set; }
    public static Action OnPlayerDied { get; set; }
    public static Action OnRestartRequested { get; set; }
    public static Action OnWorldResetCompleted { get; set; }
}
