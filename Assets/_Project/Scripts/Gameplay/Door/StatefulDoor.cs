using System;
using ArenaSurvival.Gameplay.Core;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Door
{
    /// <summary>
    /// Проверяет команды двери, хранит состояние и обновляет её изображение.
    /// Не читает клавиатуру и не управляет Player.
    /// </summary>
    public sealed class StatefulDoor : MonoBehaviour, IStateful
    {
        [Header("Visual Objects")]
        [SerializeField] private GameObject closedVisual;
        [SerializeField] private GameObject openVisual;
        [SerializeField] private GameObject lockedIndicator;

        [Header("State")]
        [SerializeField] private DoorState initialState = DoorState.Locked;

        private DoorState currentState;

        public DoorState CurrentState => currentState;
        public string CurrentStateName => currentState.ToString();
        public event Action<DoorState> StateChanged;

        private void Awake()
        {
            if (closedVisual == null || openVisual == null || lockedIndicator == null)
            {
                Debug.LogError("StatefulDoor: назначьте все три Visual Objects.", this);
                enabled = false;
                return;
            }

            // Сначала устанавливаем начальное представление, без уведомления.
            ApplyState(initialState, notify: false);
        }

        /// <summary>Запирает только закрытую дверь.</summary>
        public void Lock()
        {
            if (!isActiveAndEnabled || currentState != DoorState.Closed) return;
            ApplyState(DoorState.Locked);
        }

        /// <summary>Снимает блокировку, но не открывает дверь.</summary>
        public void Unlock()
        {
            if (!isActiveAndEnabled || currentState != DoorState.Locked) return;
            ApplyState(DoorState.Closed);
        }

        /// <summary>Открывает закрытую незапертую дверь.</summary>
        public void Open()
        {
            if (!isActiveAndEnabled || currentState != DoorState.Closed) return;
            ApplyState(DoorState.Open);
        }

        /// <summary>Закрывает открытую дверь, не запирая её.</summary>
        public void Close()
        {
            if (!isActiveAndEnabled || currentState != DoorState.Open) return;
            ApplyState(DoorState.Closed);
        }

        /// <summary>Переключает Open/Closed; Locked остаётся Locked.</summary>
        public void ToggleOpenClosed()
        {
            if (currentState == DoorState.Open) Close();
            else if (currentState == DoorState.Closed) Open();
        }

        /// <summary>
        /// Записывает новое состояние, обновляет объекты и затем сообщает
        /// подписчикам о фактическом переходе.
        /// </summary>
        private void ApplyState(DoorState newState, bool notify = true)
        {
            bool stateChanged = currentState != newState;
            currentState = newState;
            RefreshVisuals();

            if (notify && stateChanged)
                StateChanged?.Invoke(currentState);
        }

        /// <summary>Приводит три дочерних объекта к текущему состоянию.</summary>
        private void RefreshVisuals()
        {
            closedVisual.SetActive(currentState != DoorState.Open);
            openVisual.SetActive(currentState == DoorState.Open);
            lockedIndicator.SetActive(currentState == DoorState.Locked);
        }
    }
}