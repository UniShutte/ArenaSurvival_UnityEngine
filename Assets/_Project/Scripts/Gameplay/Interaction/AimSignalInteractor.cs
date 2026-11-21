using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaSurvival.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    public sealed class AimSignalInteractor : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField, Min(0.1f)] private float maxDistance = 8f;

        private int rayMask;

        private void Awake()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (viewCamera == null || playerLayer < 0 || gameObject.layer != playerLayer || maxDistance <= 0f)
            {
                Debug.LogError("AimSignalInteractor: assign View Camera, use the Player layer and set a positive range.", this);
                enabled = false;
                return;
            }

            // Keep solid scenery in the mask so a wall can block the panel.
            rayMask = Physics.DefaultRaycastLayers & ~(1 << playerLayer);
        }

        private void LateUpdate()
        {
            if (!Application.isFocused || Time.timeScale <= 0f || Cursor.lockState != CursorLockMode.Locked)
                return;

            // shortcut: keyboard-only lab; migrate F to an Input Action for rebinding or gamepads.
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.fKey.wasPressedThisFrame)
                TryActivate();
        }

        public bool TryActivate()
        {
            if (!isActiveAndEnabled || viewCamera == null || !viewCamera.isActiveAndEnabled) return false;

            Transform view = viewCamera.transform;
            Ray ray = new Ray(view.position, view.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, rayMask, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.yellow, 1f);
                Debug.Log("Raycast: no solid collider in range.", this);
                return false;
            }

            SignalPanel panel = hit.collider.GetComponentInParent<SignalPanel>();
            bool activated = panel != null && panel.TryToggle();
            Debug.DrawLine(ray.origin, hit.point, activated ? Color.green : Color.red, 1f);
            if (!activated)
                Debug.Log($"Raycast: first hit {hit.collider.name} at {hit.distance:F2} m; no active panel.", this);

            return activated;
        }
    }
}
