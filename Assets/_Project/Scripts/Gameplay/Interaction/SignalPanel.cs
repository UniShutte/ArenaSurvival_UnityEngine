using UnityEngine;

namespace ArenaSurvival.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider), typeof(MeshRenderer))]
    public sealed class SignalPanel : MonoBehaviour
    {
        private static readonly int IsOnParameter = Animator.StringToHash("IsOn");

        [SerializeField] private Light signalLight;
        [SerializeField] private Material idleMaterial;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Animator signalAnimator;

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

            if (signalAnimator != null &&
                (signalAnimator.transform == transform || !signalAnimator.transform.IsChildOf(transform) ||
                 signalAnimator.runtimeAnimatorController == null ||
                 !System.Array.Exists(signalAnimator.parameters, parameter =>
                     parameter.nameHash == IsOnParameter && parameter.type == AnimatorControllerParameterType.Bool)))
            {
                Debug.LogWarning("SignalPanel: animation needs a child Animator with a controller and a Bool named IsOn.", this);
                signalAnimator = null;
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
            if (signalAnimator != null)
                signalAnimator.SetBool(IsOnParameter, IsOn);
        }
    }
}