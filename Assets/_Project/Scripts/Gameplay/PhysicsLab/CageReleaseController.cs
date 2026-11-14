using UnityEngine;

namespace ArenaSurvival.Gameplay.PhysicsLab
{
    /// <summary>Один раз плавно опускает кинематическую клеть по мировой оси Y.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CageReleaseController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float loweringDistance = 4f;
        [SerializeField, Min(0.1f)] private float loweringDuration = 3f;
        [SerializeField] private Rigidbody[] balls;

        private Rigidbody body;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float elapsed;

        public bool HasStarted { get; private set; }
        public bool IsLowering { get; private set; }
        public bool IsReleased => HasStarted && !IsLowering;
        public Vector3 TargetPosition => targetPosition;
        public float LoweringDuration => loweringDuration;

        public bool AreBallsSleeping
        {
            get
            {
                if (balls == null || balls.Length == 0) return false;
                foreach (var ball in balls)
                    if (ball == null || !ball.IsSleeping()) return false;
                return true;
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (!body.isKinematic || body.useGravity || loweringDistance <= 0f ||
                loweringDuration <= 0f || balls == null || balls.Length == 0)
            {
                FailSetup();
                return;
            }

            for (int i = 0; i < balls.Length; i++)
            {
                var ball = balls[i];
                if (ball == null || ball.isKinematic || !ball.useGravity ||
                    !ball.gameObject.activeInHierarchy || ball.transform.IsChildOf(transform))
                {
                    FailSetup();
                    return;
                }
                for (int j = 0; j < i; j++)
                    if (ball == balls[j])
                    {
                        FailSetup();
                        return;
                    }
            }

            startPosition = body.position;
            targetPosition = startPosition + Vector3.down * loweringDistance;
        }

        public bool TryRelease()
        {
            if (!isActiveAndEnabled || HasStarted) return false;
            HasStarted = true;
            IsLowering = true;
            elapsed = 0f;
            WakeBalls();
            return true;
        }

        private void FixedUpdate()
        {
            if (!IsLowering) return;
            if (elapsed >= loweringDuration)
            {
                // Последняя MovePosition уже обработана предыдущим шагом физики.
                IsLowering = false;
                WakeBalls();
                return;
            }

            elapsed = Mathf.Min(elapsed + Time.fixedDeltaTime, loweringDuration);
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / loweringDuration);
            body.MovePosition(Vector3.Lerp(startPosition, targetPosition, progress));
        }

        private void WakeBalls()
        {
            foreach (var ball in balls)
                if (ball != null && ball.gameObject.activeInHierarchy && !ball.isKinematic)
                    ball.WakeUp();
        }

        private void FailSetup()
        {
            Debug.LogError("CageReleaseController: проверьте кинематический Rigidbody без гравитации, расстояние, время и список отдельных динамических шаров без повторов.", this);
            enabled = false;
        }
    }
}
