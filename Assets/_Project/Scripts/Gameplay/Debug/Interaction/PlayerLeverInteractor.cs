using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaSurvival.Gameplay.Interaction
{
    /// <summary>Выбирает видимый близкий рычаг по направлению камеры и обрабатывает E.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerLeverInteractor : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField, Min(0.1f)] private float interactionDistance = 2.5f;

        private int interactionMask;
        private SceneLever focusedLever;
        private GUIStyle promptStyle;

        private bool CanReadInput => viewCamera != null && viewCamera.isActiveAndEnabled &&
            Application.isFocused && Time.timeScale > 0f && Cursor.lockState == CursorLockMode.Locked;

        private void Awake()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (viewCamera == null || playerLayer < 0 || gameObject.layer != playerLayer)
            {
                Debug.LogError("PlayerLeverInteractor: назначьте View Camera и слой Player на корне персонажа.", this);
                enabled = false;
                return;
            }

            // Окружение остаётся в маске: первый Collider может оказаться стеной.
            interactionMask = Physics.DefaultRaycastLayers & ~(1 << playerLayer);
        }

        private void LateUpdate()
        {
            focusedLever = null;
            if (!CanReadInput) return;

            Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance,
                    interactionMask, QueryTriggerInteraction.Ignore)) return;

            focusedLever = hit.collider.GetComponentInParent<SceneLever>();
            if (focusedLever == null || !focusedLever.IsAvailable) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
                focusedLever.Interact();
        }

        private void OnDisable()
        {
            focusedLever = null;
        }

        private void OnGUI()
        {
            if (!CanReadInput) return;

            Color previousColor = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(Screen.width * 0.5f - 2f,
                Screen.height * 0.5f - 2f, 4f, 4f), Texture2D.whiteTexture);
            GUI.color = previousColor;

            if (focusedLever == null) return;
            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 20,
                    wordWrap = true
                };
            }

            float width = Mathf.Min(640f, Screen.width - 24f);
            GUI.Box(new Rect((Screen.width - width) * 0.5f, Screen.height - 112f,
                width, 88f), focusedLever.Prompt, promptStyle);
        }
    }
}
