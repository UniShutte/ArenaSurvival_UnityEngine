using System;
using ArenaSurvival.Gameplay.Beacon;
using ArenaSurvival.Gameplay.Door;
using ArenaSurvival.Gameplay.Interaction;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Debugging
{
    /// <summary>Проверяет команды четырёх рычагов; дистанцию и препятствия проверяют в игре.</summary>
    public sealed class LeverChecks : MonoBehaviour
    {
        [SerializeField] private StatefulDoor door;
        [SerializeField] private StatefulBeacon beacon;
        [SerializeField] private SceneLever doorLockLever;
        [SerializeField] private SceneLever doorOpenLever;
        [SerializeField] private SceneLever beaconPowerLever;
        [SerializeField] private SceneLever beaconRepairLever;

        [ContextMenu("Run Lever Checks")]
        private void RunChecks()
        {
            if (!Application.isPlaying || door == null || beacon == null ||
                doorLockLever == null || doorOpenLever == null ||
                beaconPowerLever == null || beaconRepairLever == null ||
                !door.isActiveAndEnabled || !beacon.isActiveAndEnabled ||
                !doorLockLever.IsAvailable || !doorOpenLever.IsAvailable ||
                !beaconPowerLever.IsAvailable || !beaconRepairLever.IsAvailable)
            {
                Debug.LogError("LeverChecks: запустите Play и назначьте два объекта и четыре работающих рычага.", this);
                return;
            }

            door.Close();
            door.Lock();
            beacon.Repair();
            beacon.Deactivate();

            doorOpenLever.Interact();
            Check(door.CurrentState == DoorState.Locked, "Запертую дверь нельзя открыть.");
            doorLockLever.Interact();
            Check(door.CurrentState == DoorState.Closed, "Снятие блокировки должно дать Closed.");
            doorOpenLever.Interact();
            Check(door.CurrentState == DoorState.Open, "Рычаг открытия должен открыть дверь.");
            doorLockLever.Interact();
            Check(door.CurrentState == DoorState.Open, "Открытую дверь нельзя запереть.");
            doorOpenLever.Interact();
            Check(door.CurrentState == DoorState.Closed, "Повторное действие должно закрыть дверь.");
            doorLockLever.Interact();
            Check(door.CurrentState == DoorState.Locked, "Закрытую дверь нужно запереть.");

            beaconPowerLever.Interact();
            Check(beacon.CurrentState == BeaconState.Active, "Рычаг питания должен включить маяк.");
            beaconRepairLever.Interact();
            Check(beacon.CurrentState == BeaconState.Broken, "Второй рычаг должен сломать маяк.");
            beaconPowerLever.Interact();
            Check(beacon.CurrentState == BeaconState.Broken, "Сломанный маяк нельзя включить.");
            beaconRepairLever.Interact();
            Check(beacon.CurrentState == BeaconState.Inactive, "Ремонт не должен включать маяк.");
            beaconPowerLever.Interact();
            Check(beacon.CurrentState == BeaconState.Active, "После ремонта маяк должен включиться.");
            beaconPowerLever.Interact();
            Check(beacon.CurrentState == BeaconState.Inactive, "Повторное действие должно выключить маяк.");

            Debug.Log("LeverChecks: PASS. Door = Locked, Beacon = Inactive.", this);
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("LeverChecks: " + message);
        }
    }
}
