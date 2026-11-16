using ArenaSurvival.Gameplay.PhysicsLab;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Contacts
{
    /// <summary>Кратко меняет цвет плиты при достаточно быстром ударе шара.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class BallImpactTarget : MonoBehaviour
    {
        [SerializeField] private Renderer indicator;
        [SerializeField] private Material idleMaterial;
        [SerializeField] private Material hitMaterial;
        [SerializeField, Min(0.1f)] private float minimumSpeed = 2f;
        [SerializeField, Min(0.1f)] private float flashDuration = 0.5f;

        private float flashUntil;
        private bool flashing;

        public int HitCount { get; private set; }

        private void Awake()
        {
            var box = GetComponent<BoxCollider>();
            if (!box.enabled || box.isTrigger || box.attachedRigidbody != null ||
                indicator == null || !indicator.transform.IsChildOf(transform) ||
                idleMaterial == null || hitMaterial == null || minimumSpeed <= 0f || flashDuration <= 0f)
            {
                Debug.LogError("BallImpactTarget: нужны обычный статический BoxCollider, свой Renderer, два материала и положительные параметры.", this);
                enabled = false;
                return;
            }
            indicator.sharedMaterial = idleMaterial;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isActiveAndEnabled || collision.rigidbody == null ||
                !collision.rigidbody.TryGetComponent<PhysicsBall>(out var ball) || !ball.isActiveAndEnabled)
                return;

            float speed = collision.relativeVelocity.magnitude;
            if (speed < minimumSpeed) return;
            HitCount++;
            flashUntil = Time.time + flashDuration;
            flashing = true;
            indicator.sharedMaterial = hitMaterial;
            Debug.Log($"BallImpactTarget: hit={HitCount}, ball={ball.name}, relativeSpeed={speed:F2} m/s.", this);
        }

        private void Update()
        {
            if (!flashing || Time.time < flashUntil) return;
            flashing = false;
            indicator.sharedMaterial = idleMaterial;
        }

        private void OnDisable()
        {
            flashing = false;
            if (indicator != null && idleMaterial != null) indicator.sharedMaterial = idleMaterial;
        }
    }
}
