namespace ArenaSurvival.Gameplay.Door
{
    /// <summary>Устойчивые состояния и два незавершённых движения двери.</summary>
    public enum DoorState
    {
        // Сохраняем числовые значения, уже записанные в prefab ЛР3.
        Locked = 0,
        Closed = 1,
        Open = 2,
        Opening = 3,
        Closing = 4
    }
}
