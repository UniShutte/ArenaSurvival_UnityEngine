using ArenaSurvival.Gameplay.Beacon;
using ArenaSurvival.Gameplay.Core;
using ArenaSurvival.Gameplay.Door;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaSurvival.Gameplay.Debugging
{
    /// <summary>
    /// Учебный пульт: читает клавиши и передаёт команды двум объектам сцены.
    /// Не определяет допустимость переходов и не переключает Visuals.
    /// </summary>
    public sealed class StateDemoController : MonoBehaviour
    {
        [SerializeField] private StatefulDoor targetDoor;
        [SerializeField] private StatefulBeacon targetBeacon;

        private void Awake()
        {
            if (targetDoor == null || targetBeacon == null)
            {
                Debug.LogError("StateDemoController: назначьте Target Door и Target Beacon.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Используем цифровой ряд над буквами, не NumPad.
            if (keyboard.digit1Key.wasPressedThisFrame) targetDoor.Unlock();
            if (keyboard.digit2Key.wasPressedThisFrame) targetDoor.ToggleOpenClosed();
            if (keyboard.digit3Key.wasPressedThisFrame) targetDoor.Lock();
            if (keyboard.digit4Key.wasPressedThisFrame) targetBeacon.ToggleActive();
            if (keyboard.digit5Key.wasPressedThisFrame) targetBeacon.Break();
            if (keyboard.digit6Key.wasPressedThisFrame) targetBeacon.Repair();

            if (keyboard.digit7Key.wasPressedThisFrame)
            {
                PrintState("Door", targetDoor);
                PrintState("Beacon", targetBeacon);
            }
        }

        /// <summary>Читает общее свойство через интерфейс, независимо от типа объекта.</summary>
        private void PrintState(string label, IStateful target)
        {
            Debug.Log($"{label}: текущее состояние {target.CurrentStateName}.", this);
        }
    }
}
