using System;
using ArenaSurvival.Gameplay.Core;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Door
{
    /// <summary>Управляет замком, поворотом створки и остановкой перед игроком.</summary>
    [DisallowMultipleComponent]
    public sealed class StatefulDoor : MonoBehaviour, IStateful
    {
        [Header("Door")]
        [SerializeField] private Transform hinge;
        [SerializeField] private GameObject lockedIndicator;
        [SerializeField] private DoorState initialState = DoorState.Locked;
        [SerializeField, Range(-150f, 150f)] private float openAngle = 90f;
        [SerializeField, Min(1f)] private float rotationSpeed = 60f;

        [Header("Safety")]
        [SerializeField] private Transform safetyZone;

        private DoorState currentState;
        private Quaternion closedRotation;
        private Quaternion openRotation;
        private int playerMask;

        public DoorState CurrentState => currentState;
        public string CurrentStateName => currentState.ToString();
        public bool IsMoving => currentState == DoorState.Opening || currentState == DoorState.Closing;
        public event Action<DoorState> StateChanged;

        /// <summary>Проверяет объём хода створки только на обычные Collider слоя Player.</summary>
        public bool IsPassageOccupied
        {
            get
            {
                if (safetyZone == null || playerMask == 0) return true;
                // Player мог переместиться через Transform после последнего шага физики.
                Physics.SyncTransforms();
                return Physics.CheckBox(safetyZone.position, safetyZone.lossyScale * 0.5f,
                    safetyZone.rotation, playerMask, QueryTriggerInteraction.Ignore);
            }
        }

        private void Awake()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (hinge == null || hinge.parent != transform || safetyZone == null ||
                safetyZone.parent != transform || safetyZone == hinge ||
                lockedIndicator == null || lockedIndicator == gameObject ||
                !lockedIndicator.transform.IsChildOf(transform) || playerLayer < 0 ||
                rotationSpeed <= 0f || Mathf.Abs(openAngle) < 1f || Mathf.Abs(openAngle) > 150f ||
                (initialState != DoorState.Locked && initialState != DoorState.Closed &&
                 initialState != DoorState.Open))
            {
                Debug.LogError("StatefulDoor: проверьте Hinge, Safety Zone, индикатор, слой Player и начальное состояние.", this);
                enabled = false;
                return;
            }

            Vector3 size = safetyZone.lossyScale;
            if (size.x <= 0f || size.y <= 0f || size.z <= 0f)
            {
                Debug.LogError("StatefulDoor: размеры Safety Zone должны быть положительными.", this);
                enabled = false;
                return;
            }

            playerMask = 1 << playerLayer;
            closedRotation = hinge.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
            hinge.localRotation = initialState == DoorState.Open ? openRotation : closedRotation;
            ApplyState(initialState, false);
        }

        /// <summary>После движения Player проверяет безопасность и выполняет один шаг поворота.</summary>
        private void LateUpdate()
        {
            if (!IsMoving || Time.deltaTime <= 0f || IsPassageOccupied) return;

            bool opening = currentState == DoorState.Opening;
            Quaternion targetRotation = opening ? openRotation : closedRotation;
            hinge.localRotation = Quaternion.RotateTowards(
                hinge.localRotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Quaternion.Angle(hinge.localRotation, targetRotation) > 0.01f) return;
            hinge.localRotation = targetRotation;
            ApplyState(opening ? DoorState.Open : DoorState.Closed);
        }

        /// <summary>Запирает только полностью закрытую неподвижную дверь.</summary>
        public void Lock()
        {
            if (!isActiveAndEnabled || currentState != DoorState.Closed) return;
            ApplyState(DoorState.Locked);
        }

        /// <summary>Снимает блокировку, оставляя створку закрытой.</summary>
        public void Unlock()
        {
            if (!isActiveAndEnabled || currentState != DoorState.Locked) return;
            ApplyState(DoorState.Closed);
        }

        /// <summary>Запускает открывание, если замок снят и зона хода свободна.</summary>
        public void Open()
        {
            if (!isActiveAndEnabled || currentState != DoorState.Closed || IsPassageOccupied) return;
            ApplyState(DoorState.Opening);
        }

        /// <summary>Запускает закрывание из полностью открытого положения.</summary>
        public void Close()
        {
            if (!isActiveAndEnabled || currentState != DoorState.Open || IsPassageOccupied) return;
            ApplyState(DoorState.Closing);
        }

        public void ToggleOpenClosed()
        {
            if (currentState == DoorState.Open) Close();
            else if (currentState == DoorState.Closed) Open();
        }

        /// <summary>Сначала обновляет состояние и индикатор, затем сообщает о переходе.</summary>
        private void ApplyState(DoorState newState, bool notify = true)
        {
            bool changed = currentState != newState;
            currentState = newState;
            lockedIndicator.SetActive(currentState == DoorState.Locked);
            if (notify && changed) StateChanged?.Invoke(currentState);
        }

        private void OnDrawGizmosSelected()
        {
            if (safetyZone == null) return;
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;
            Gizmos.matrix = safetyZone.localToWorldMatrix;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }
    }
}
