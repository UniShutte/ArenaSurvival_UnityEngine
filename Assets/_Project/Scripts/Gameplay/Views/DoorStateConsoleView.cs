using ArenaSurvival.Gameplay.Door;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Views
{
    /// <summary>Показывает состояние двери в Console, не управляя ею.</summary>
    public sealed class DoorStateConsoleView : MonoBehaviour
    {
        [SerializeField] private StatefulDoor door;

        private void OnEnable()
        {
            if (door == null)
            {
                Debug.LogError("DoorStateConsoleView: назначьте Door.", this);
                enabled = false;
                return;
            }

            door.StateChanged += HandleDoorStateChanged;
        }

        private void Start()
        {
            // Читаем начальное состояние после Awake объектов сцены.
            Debug.Log($"Door: начальное состояние {door.CurrentStateName}.", this);
        }

        private void OnDisable()
        {
            if (door != null)
                door.StateChanged -= HandleDoorStateChanged;
        }

        /// <summary>Получает новое состояние от события и только выводит его.</summary>
        private void HandleDoorStateChanged(DoorState state)
        {
            Debug.Log($"Door: переход в {state}.", this);
        }
    }
}
