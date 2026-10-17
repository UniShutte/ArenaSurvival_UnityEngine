using ArenaSurvival.Gameplay.Beacon;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Views
{
    /// <summary>Подписывается на маяк и сообщает о переходах в Console.</summary>
    public sealed class BeaconStateConsoleView : MonoBehaviour
    {
        [SerializeField] private StatefulBeacon beacon;

        private void OnEnable()
        {
            if (beacon == null)
            {
                Debug.LogError("BeaconStateConsoleView: назначьте Beacon.", this);
                enabled = false;
                return;
            }

            beacon.StateChanged += HandleBeaconStateChanged;
        }

        private void Start()
        {
            Debug.Log($"Beacon: начальное состояние {beacon.CurrentStateName}.", this);
        }

        private void OnDisable()
        {
            if (beacon != null)
                beacon.StateChanged -= HandleBeaconStateChanged;
        }

        /// <summary>Получает BeaconState, но не переключает свет самостоятельно.</summary>
        private void HandleBeaconStateChanged(BeaconState state)
        {
            Debug.Log($"Beacon: переход в {state}.", this);
        }
    }
}
