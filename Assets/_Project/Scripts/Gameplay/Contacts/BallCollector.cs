using System.Collections.Generic;
using ArenaSurvival.Gameplay.PhysicsLab;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Contacts
{
    /// <summary>Принимает заданное число разных шаров и показывает выполнение цели.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class BallCollector : MonoBehaviour
    {
        [SerializeField] private Renderer indicator;
        [SerializeField] private Material idleMaterial;
        [SerializeField] private Material completeMaterial;
        [SerializeField, Min(1)] private int requiredBalls = 3;

        private readonly HashSet<PhysicsBall> collected = new HashSet<PhysicsBall>();

        public int CollectedCount => collected.Count;
        public int RequiredBalls => requiredBalls;
        public bool IsComplete => CollectedCount >= requiredBalls;

        private void Awake()
        {
            var box = GetComponent<BoxCollider>();
            var body = GetComponent<Rigidbody>();
            if (!box.enabled || !box.isTrigger || !body.isKinematic || body.useGravity || !body.detectCollisions ||
                indicator == null || !indicator.transform.IsChildOf(transform) ||
                idleMaterial == null || completeMaterial == null || requiredBalls < 1)
            {
                Debug.LogError("BallCollector: нужны BoxCollider с Is Trigger, кинематический Rigidbody без гравитации, свой Renderer и материалы.", this);
                enabled = false;
                return;
            }
            indicator.sharedMaterial = idleMaterial;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActiveAndEnabled || IsComplete || other.isTrigger || other.attachedRigidbody == null) return;
            if (!other.attachedRigidbody.TryGetComponent<PhysicsBall>(out var ball) ||
                !ball.isActiveAndEnabled || !collected.Add(ball)) return;

            // Считаем тело один раз, даже если у него несколько коллайдеров.
            ball.gameObject.SetActive(false);
            Debug.Log($"BallCollector: {CollectedCount}/{RequiredBalls}, collected={ball.name}.", this);
            if (!IsComplete) return;
            indicator.sharedMaterial = completeMaterial;
            Debug.Log("BallCollector: COMPLETE. Приёмник заполнен.", this);
        }
    }
}
