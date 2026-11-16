using System.Collections.Generic;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Contacts
{
    /// <summary>Показывает присутствие CharacterController с тегом Player.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class PlayerPresenceZone : MonoBehaviour
    {
        [SerializeField] private Renderer indicator;
        [SerializeField] private Material emptyMaterial;
        [SerializeField] private Material occupiedMaterial;

        private readonly HashSet<CharacterController> occupants = new HashSet<CharacterController>();
        private BoxCollider zone;

        public bool IsOccupied { get; private set; }

        private void Awake()
        {
            zone = GetComponent<BoxCollider>();
            var body = GetComponent<Rigidbody>();
            if (!zone.enabled || !zone.isTrigger || !body.isKinematic || body.useGravity || !body.detectCollisions ||
                indicator == null || !indicator.transform.IsChildOf(transform) ||
                emptyMaterial == null || occupiedMaterial == null)
            {
                Debug.LogError("PlayerPresenceZone: проверьте Trigger, кинематический Rigidbody, свой Renderer и материалы.", this);
                enabled = false;
                return;
            }
            indicator.sharedMaterial = emptyMaterial;
        }

        private void OnTriggerEnter(Collider other) => Register(other);

        // Восстанавливает состояние после включения компонента внутри уже занятой зоны.
        private void OnTriggerStay(Collider other) => Register(other);

        private void Register(Collider other)
        {
            if (!isActiveAndEnabled || !other.CompareTag("Player") ||
                !other.TryGetComponent<CharacterController>(out var player) || !player.enabled) return;
            occupants.Add(player);
            RefreshIndicator();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isActiveAndEnabled || !other.TryGetComponent<CharacterController>(out var player)) return;
            occupants.Remove(player);
            RefreshIndicator();
        }

        private void Update()
        {
            // Отключение или уничтожение Collider не обязано присылать OnTriggerExit.
            // shortcut: bounds даёт грубый отсев телепортаций; для точного контроля углов повёрнутой зоны нужна проверка формы.
            occupants.RemoveWhere(player => player == null || !player.enabled ||
                !player.gameObject.activeInHierarchy || !player.CompareTag("Player") ||
                !zone.bounds.Intersects(player.bounds));
            RefreshIndicator();
        }

        private void RefreshIndicator()
        {
            bool occupied = occupants.Count > 0;
            if (IsOccupied == occupied) return;
            IsOccupied = occupied;
            indicator.sharedMaterial = occupied ? occupiedMaterial : emptyMaterial;
            Debug.Log(occupied ? "PlayerPresenceZone: ENTER." : "PlayerPresenceZone: EXIT.", this);
        }

        private void OnDisable()
        {
            occupants.Clear();
            IsOccupied = false;
            if (indicator != null && emptyMaterial != null) indicator.sharedMaterial = emptyMaterial;
        }
    }
}
