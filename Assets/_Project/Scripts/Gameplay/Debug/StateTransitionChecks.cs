using System;
using ArenaSurvival.Gameplay.Beacon;
using ArenaSurvival.Gameplay.Door;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Debugging
{
    /// <summary>Запускаемая вручную проверка переходов и числа событий в Play Mode.</summary>
    public sealed class StateTransitionChecks : MonoBehaviour
    {
        [SerializeField] private StatefulDoor door;
        [SerializeField] private StatefulBeacon beacon;

        [ContextMenu("Run State Checks")]
        private void RunChecks()
        {
            if (!Application.isPlaying || door == null || beacon == null ||
                !door.isActiveAndEnabled || !beacon.isActiveAndEnabled)
            {
                Debug.LogError("StateChecks: нужен Play Mode и два настроенных активных объекта.", this);
                return;
            }

            // Подготовка не входит в измеряемую последовательность событий.
            door.Close();
            door.Lock();
            beacon.Repair();
            beacon.Deactivate();

            int doorEvents = 0;
            int beaconEvents = 0;
            Action<DoorState> countDoor = state => doorEvents++;
            Action<BeaconState> countBeacon = state => beaconEvents++;
            door.StateChanged += countDoor;
            beacon.StateChanged += countBeacon;

            try
            {
                door.Open();
                Check(door.CurrentState == DoorState.Locked && doorEvents == 0,
                    "Locked не должен открываться или публиковать переход.");
                door.Unlock();
                Check(door.CurrentState == DoorState.Closed && doorEvents == 1,
                    "Unlock должен дать Closed и одно событие.");
                door.Unlock();
                Check(doorEvents == 1, "Повторный Unlock не должен публиковать событие.");
                door.Open();
                Check(door.CurrentState == DoorState.Open && doorEvents == 2,
                    "Open должен дать Open и второе событие.");
                door.Lock();
                Check(door.CurrentState == DoorState.Open && doorEvents == 2,
                    "Lock не должен запирать открытую дверь.");
                door.Close();
                door.Lock();
                Check(door.CurrentState == DoorState.Locked && doorEvents == 4,
                    "Close и Lock должны вернуть Locked, всего четыре события.");

                beacon.Activate();
                Check(beacon.CurrentState == BeaconState.Active && beaconEvents == 1,
                    "Activate должен дать Active и одно событие.");
                beacon.Break();
                Check(beacon.CurrentState == BeaconState.Broken && beaconEvents == 2,
                    "Break должен дать Broken и второе событие.");
                beacon.Activate();
                beacon.Deactivate();
                beacon.Break();
                Check(beacon.CurrentState == BeaconState.Broken && beaconEvents == 2,
                    "Broken должен отклонять включение, выключение и повторную поломку.");
                beacon.Repair();
                Check(beacon.CurrentState == BeaconState.Inactive && beaconEvents == 3,
                    "Repair должен вернуть Inactive, всего три события.");
            }
            finally
            {
                // Отписываемся и при успехе, и при исключении из Check.
                door.StateChanged -= countDoor;
                beacon.StateChanged -= countBeacon;
            }

            Debug.Log("StateChecks: PASS. Door events = 4, Beacon events = 3.", this);
        }

        /// <summary>Останавливает проверку, если ожидаемое условие не выполнено.</summary>
        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("StateChecks: " + message);
        }
    }
}