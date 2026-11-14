using UnityEngine;

namespace ArenaSurvival.Gameplay.PhysicsLab
{
    /// <summary>Настраивает устойчивость шара и предоставляет два учебных действия.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class PhysicsBall : MonoBehaviour
    {
        [SerializeField, Min(0.001f)] private float sleepThreshold = 0.01f;
        [SerializeField, Min(0.01f)] private float testImpulse = 3f;

        private Rigidbody body;

        public bool IsSleeping => body != null && body.IsSleeping();

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            var sphere = GetComponent<SphereCollider>();
            if (body.isKinematic || !body.useGravity || !body.detectCollisions ||
                sphere.isTrigger || sphere.sharedMaterial == null ||
                sleepThreshold <= 0f || testImpulse <= 0f)
            {
                Debug.LogError("PhysicsBall: нужны динамический Rigidbody с гравитацией, обычный SphereCollider и Physics Material.", this);
                enabled = false;
                return;
            }

            body.sleepThreshold = sleepThreshold;
            body.solverIterations = 10;
            body.solverVelocityIterations = 4;
        }

        [ContextMenu("Apply Test Impulse (Play)")]
        public void ApplyTestImpulse()
        {
            if (!Application.isPlaying || !isActiveAndEnabled || body == null || body.isKinematic) return;
            body.WakeUp();
            // Один вызов задаёт импульс, а не силу за каждый кадр.
            body.AddForce(Vector3.right * testImpulse, ForceMode.Impulse);
        }

        [ContextMenu("Report Physics State (Play)")]
        public void ReportPhysicsState()
        {
            if (!Application.isPlaying || body == null) return;
            Debug.Log($"{name}: mass={body.mass:F2} kg, speed={body.linearVelocity.magnitude:F3} m/s, " +
                $"angularSpeed={body.angularVelocity.magnitude:F3} rad/s, sleeping={body.IsSleeping()}.", this);
        }
    }
}
