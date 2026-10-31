using ArenaSurvival.Gameplay.Beacon;
using ArenaSurvival.Gameplay.Door;
using ArenaSurvival.Gameplay.Motion;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Interaction
{
    /// <summary>Передаёт одну команду выбранной цели и отображает её состояние рукояткой.</summary>
    [DisallowMultipleComponent]
    public sealed class SceneLever : MonoBehaviour
    {
        public enum LeverAction
        {
            DoorLock = 0,
            DoorOpen = 1,
            BeaconPower = 2,
            BeaconBreakRepair = 3,
            MotionPower = 4
        }

        [Header("Action")]
        [SerializeField] private LeverAction action;
        [SerializeField] private StatefulDoor door;
        [SerializeField] private StatefulBeacon beacon;
        [SerializeField] private PointToPointMover mover;

        [Header("Handle")]
        [SerializeField] private Transform handlePivot;
        [SerializeField] private Vector3 offAngles = new Vector3(0f, 0f, -35f);
        [SerializeField] private Vector3 onAngles = new Vector3(0f, 0f, 35f);
        [SerializeField, Min(1f)] private float rotationSpeed = 180f;

        private bool HasActiveTarget
        {
            get
            {
                switch (action)
                {
                    case LeverAction.DoorLock:
                    case LeverAction.DoorOpen:
                        return door != null && door.isActiveAndEnabled;
                    case LeverAction.BeaconPower:
                    case LeverAction.BeaconBreakRepair:
                        return beacon != null && beacon.isActiveAndEnabled;
                    case LeverAction.MotionPower:
                        return mover != null && mover.isActiveAndEnabled;
                    default:
                        return false;
                }
            }
        }

        public bool IsAvailable => isActiveAndEnabled && handlePivot != null && HasActiveTarget;

        private bool IsOn
        {
            get
            {
                switch (action)
                {
                    case LeverAction.DoorLock: return door.CurrentState == DoorState.Locked;
                    case LeverAction.DoorOpen:
                        return door.CurrentState == DoorState.Open || door.CurrentState == DoorState.Opening;
                    case LeverAction.BeaconPower: return beacon.CurrentState == BeaconState.Active;
                    case LeverAction.BeaconBreakRepair: return beacon.CurrentState == BeaconState.Broken;
                    case LeverAction.MotionPower: return mover.IsRunning;
                    default: return false;
                }
            }
        }

        public string Prompt
        {
            get
            {
                if (!IsAvailable) return "Рычаг недоступен: проверьте объект назначения.";
                if (action == LeverAction.DoorLock || action == LeverAction.DoorOpen)
                {
                    if (door.IsMoving)
                        return door.IsPassageOccupied ? "Освободите зону двери: движение приостановлено."
                            : "Дверь движется. Дождитесь остановки.";
                }

                switch (action)
                {
                    case LeverAction.DoorLock:
                        if (door.CurrentState == DoorState.Open) return "Сначала закройте дверь другим рычагом.";
                        return IsOn ? "[E] Разблокировать дверь" : "[E] Запереть дверь";
                    case LeverAction.DoorOpen:
                        if (door.CurrentState == DoorState.Locked) return "Дверь заперта: сначала снимите блокировку.";
                        if (door.IsPassageOccupied) return "Освободите зону движения двери.";
                        return IsOn ? "[E] Закрыть дверь" : "[E] Открыть дверь";
                    case LeverAction.BeaconPower:
                        if (beacon.CurrentState == BeaconState.Broken) return "Маяк сломан: сначала используйте рычаг ремонта.";
                        return IsOn ? "[E] Выключить маяк" : "[E] Включить маяк";
                    case LeverAction.BeaconBreakRepair:
                        return IsOn ? "[E] Починить маяк" : "[E] Сломать маяк";
                    case LeverAction.MotionPower:
                        return IsOn ? "[E] Остановить механизм" : "[E] Запустить механизм";
                    default:
                        return "Неизвестное действие рычага.";
                }
            }
        }

        private void Start()
        {
            if (!HasActiveTarget || handlePivot == null || handlePivot == transform ||
                !handlePivot.IsChildOf(transform) || rotationSpeed <= 0f)
            {
                Debug.LogError("SceneLever: назначьте активную цель, дочерний Handle Pivot и положительную скорость.", this);
                enabled = false;
                return;
            }

            handlePivot.localRotation = Quaternion.Euler(IsOn ? onAngles : offAngles);
        }

        private void Update()
        {
            if (!IsAvailable) return;
            Quaternion targetRotation = Quaternion.Euler(IsOn ? onAngles : offAngles);
            handlePivot.localRotation = Quaternion.RotateTowards(
                handlePivot.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        /// <summary>Не обходит проверки двери, маяка или механизма, а вызывает их публичную команду.</summary>
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
                case LeverAction.MotionPower:
                    mover.ToggleRunning();
                    break;
            }
        }
    }
}
