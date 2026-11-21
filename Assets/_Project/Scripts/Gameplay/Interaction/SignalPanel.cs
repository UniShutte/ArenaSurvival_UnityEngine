using UnityEngine;

namespace ArenaSurvival.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider), typeof(MeshRenderer))]
    public sealed class SignalPanel : MonoBehaviour
    {
        [SerializeField] private Light signalLight;
        [SerializeField] private Material idleMaterial;
        [SerializeField] private Material activeMaterial;

        private MeshRenderer indicator;

        public bool IsOn { get; private set; }

        private void Awake()
        {
            indicator = GetComponent<MeshRenderer>();
            BoxCollider box = GetComponent<BoxCollider>();
            if (!box.enabled || box.isTrigger || signalLight == null || !signalLight.transform.IsChildOf(transform) ||
                idleMaterial == null || activeMaterial == null)
            {
                Debug.LogError("SignalPanel: use a solid BoxCollider, a child Light and two materials.", this);
                enabled = false;
                return;
            }

            ApplyState();
        }

        public bool TryToggle()
        {
            if (!isActiveAndEnabled || signalLight == null || indicator == null) return false;

            IsOn = !IsOn;
            ApplyState();
            Debug.Log(IsOn ? "SignalPanel: ON" : "SignalPanel: OFF", this);
            return true;
        }

        private void ApplyState()
        {
            signalLight.enabled = IsOn;
            indicator.sharedMaterial = IsOn ? activeMaterial : idleMaterial;
        }
    }
}
