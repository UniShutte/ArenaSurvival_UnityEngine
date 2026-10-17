namespace ArenaSurvival.Gameplay.Core
{
    /// <summary>
    /// Объект умеет сообщить читаемое имя текущего состояния.
    /// Интерфейс не задаёт общий enum и не меняет состояние.
    /// </summary>
    public interface IStateful
    {
        string CurrentStateName { get; }
    }
}