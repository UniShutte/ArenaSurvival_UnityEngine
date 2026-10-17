using System;
using ArenaSurvival.Gameplay.Core;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Beacon
{
    /// <summary>
    /// Хранит состояние маяка, переключает свет и изображения.
    /// Сломанный маяк сначала требует ремонта, затем отдельного включения.
    /// </summary>
    public sealed class StatefulBeacon : MonoBehaviour, IStateful
    {
        [Header("Visual Objects")]
        [SerializeField] private Light beaconLight;
        [SerializeField] private GameObject activeVisual;
        [SerializeField] private GameObject brokenVisual;

        [Header("State")]
        [SerializeField] private BeaconState initialState = BeaconState.Inactive;

        private BeaconState currentState;

        public BeaconState CurrentState => currentState;
        public string CurrentStateName => currentState.ToString();
        public event Action<BeaconState> StateChanged;

        private void Awake()
        {
            if (beaconLight == null || activeVisual == null || brokenVisual == null)
            {
                Debug.LogError("StatefulBeacon: назначьте Light и оба Visual Objects.", this);
                enabled = false;
                return;
            }

            ApplyState(initialState, notify: false);
        }

        /// <summary>Включает исправный неактивный маяк.</summary>
        public void Activate()
        {
            if (!isActiveAndEnabled || currentState != BeaconState.Inactive) return;
            ApplyState(BeaconState.Active);
        }

        /// <summary>Выключает активный маяк, не выполняя ремонт.</summary>
        public void Deactivate()
        {
            if (!isActiveAndEnabled || currentState != BeaconState.Active) return;
            ApplyState(BeaconState.Inactive);
        }

        /// <summary>Ломает исправный маяк; повторная поломка ничего не меняет.</summary>
        public void Break()
        {
            if (!isActiveAndEnabled || currentState == BeaconState.Broken) return;
            ApplyState(BeaconState.Broken);
        }

        /// <summary>Восстанавливает сломанный маяк в выключенное состояние.</summary>
        public void Repair()
        {
            if (!isActiveAndEnabled || currentState != BeaconState.Broken) return;
            ApplyState(BeaconState.Inactive);
        }

        /// <summary>Переключает только Inactive/Active.</summary>
        public void ToggleActive()
        {
            if (currentState == BeaconState.Active) Deactivate();
            else if (currentState == BeaconState.Inactive) Activate();
        }

        /// <summary>Применяет состояние и уведомляет только о переходе.</summary>
        private void ApplyState(BeaconState newState, bool notify = true)
        {
            bool stateChanged = currentState != newState;
            currentState = newState;
            RefreshVisuals();

            if (notify && stateChanged)
                StateChanged?.Invoke(currentState);
        }

        /// <summary>Свет нужен только активному маяку, повреждение только сломанному.</summary>
        private void RefreshVisuals()
        {
            beaconLight.enabled = currentState == BeaconState.Active;
            activeVisual.SetActive(currentState == BeaconState.Active);
            brokenVisual.SetActive(currentState == BeaconState.Broken);
        }
    }
}
