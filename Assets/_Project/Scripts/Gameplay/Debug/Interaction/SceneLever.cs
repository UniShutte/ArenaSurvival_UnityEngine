using ArenaSurvival.Gameplay.Beacon;
using ArenaSurvival.Gameplay.Door;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Interaction
{
    /// <summary>Передаёт команды существующим объектам и показывает их реальное состояние.</summary>
    [DisallowMultipleComponent]
    public sealed class SceneLever : MonoBehaviour
    {
        public enum LeverAction
        {
            DoorLock,
            DoorOpen,
            BeaconPower,
            BeaconBreakRepair
        }

        [Header("Action")]
        [SerializeField] private LeverAction action;
        [SerializeField] private StatefulDoor door;
        [SerializeField] private StatefulBeacon beacon;

        [Header("Handle")]
        [SerializeField] private Transform handlePivot;
        [SerializeField] private Vector3 offAngles = new Vector3(0f, 0f, -35f);
        [SerializeField] private Vector3 onAngles = new Vector3(0f, 0f, 35f);
        [SerializeField, Min(1f)] private float rotationSpeed = 180f;

        private bool HasTarget
        {
            get
            {
                switch (action)
                {
                    case LeverAction.DoorLock:
                    case LeverAction.DoorOpen:
                        return door != null;
                    case LeverAction.BeaconPower:
                    case LeverAction.BeaconBreakRepair:
                        return beacon != null;
                    default:
                        return false;
                }
            }
        }

        public bool IsAvailable => isActiveAndEnabled && handlePivot != null && HasTarget &&
            (action == LeverAction.DoorLock || action == LeverAction.DoorOpen
                ? door.isActiveAndEnabled
                : beacon.isActiveAndEnabled);

        private bool IsOn
        {
            get
            {
                switch (action)
                {
                    case LeverAction.DoorLock: return door.CurrentState == DoorState.Locked;
                    case LeverAction.DoorOpen: return door.CurrentState == DoorState.Open;
                    case LeverAction.BeaconPower: return beacon.CurrentState == BeaconState.Active;
                    case LeverAction.BeaconBreakRepair: return beacon.CurrentState == BeaconState.Broken;
                    default: return false;
                }
            }
        }

        public string Prompt
        {
            get
            {
                if (!IsAvailable) return "Рычаг недоступен: проверьте объект назначения.";
                switch (action)
                {
                    case LeverAction.DoorLock:
                        if (door.CurrentState == DoorState.Open) return "Сначала закройте дверь другим рычагом.";
                        return IsOn ? "[E] Разблокировать дверь" : "[E] Запереть дверь";
                    case LeverAction.DoorOpen:
                        if (door.CurrentState == DoorState.Locked) return "Дверь заперта: сначала снимите блокировку.";
                        return IsOn ? "[E] Закрыть дверь" : "[E] Открыть дверь";
                    case LeverAction.BeaconPower:
                        if (beacon.CurrentState == BeaconState.Broken) return "Маяк сломан: сначала используйте рычаг ремонта.";
                        return IsOn ? "[E] Выключить маяк" : "[E] Включить маяк";
                    case LeverAction.BeaconBreakRepair:
                        return IsOn ? "[E] Починить маяк" : "[E] Сломать маяк";
                    default:
                        return "Неизвестное действие рычага.";
                }
            }
        }

        private void Start()
        {
            if (!HasTarget || handlePivot == null || handlePivot == transform ||
                !handlePivot.IsChildOf(transform))
            {
                Debug.LogError("SceneLever: назначьте цель и дочерний Handle Pivot.", this);
                enabled = false;
                return;
            }

            // Start читает состояние после начальных Awake двери и маяка.
            handlePivot.localRotation = Quaternion.Euler(IsOn ? onAngles : offAngles);
        }

        private void Update()
        {
            if (!IsAvailable) return;
            Quaternion targetRotation = Quaternion.Euler(IsOn ? onAngles : offAngles);
            handlePivot.localRotation = Quaternion.RotateTowards(
                handlePivot.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        /// <summary>Правила переходов остаются в StatefulDoor и StatefulBeacon.</summary>
        public void Interact()
        {
            if (!IsAvailable) return;
            switch (action)
            {
                case LeverAction.DoorLock:
                    if (door.CurrentState == DoorState.Locked) door.Unlock();
                    else door.Lock();
                    break;
                case LeverAction.DoorOpen:
                    door.ToggleOpenClosed();
                    break;
                case LeverAction.BeaconPower:
                    beacon.ToggleActive();
                    break;
                case LeverAction.BeaconBreakRepair:
                    if (beacon.CurrentState == BeaconState.Broken) beacon.Repair();
                    else beacon.Break();
                    break;
            }
        }
    }
}
