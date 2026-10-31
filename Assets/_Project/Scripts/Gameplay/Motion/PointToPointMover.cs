using UnityEngine;

namespace ArenaSurvival.Gameplay.Motion
{
    /// <summary>Перемещает отдельную деталь между неподвижными точками A и B.</summary>
    [DisallowMultipleComponent]
    public sealed class PointToPointMover : MonoBehaviour
    {
        [SerializeField] private Transform movingPart;
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField, Min(0.1f)] private float speed = 2f;
        [SerializeField, Min(0f)] private float pauseAtPoint = 1f;
        [SerializeField] private bool startRunning;

        private bool goingToB = true;
        private float pauseRemaining;

        public bool IsRunning { get; private set; }
        public Vector3 CurrentPosition => movingPart.position;

        private void Awake()
        {
            if (movingPart == null || pointA == null || pointB == null ||
                movingPart == transform || !movingPart.IsChildOf(transform) ||
                pointA == pointB || pointA.IsChildOf(movingPart) || pointB.IsChildOf(movingPart) ||
                Vector3.Distance(pointA.position, pointB.position) < 0.1f ||
                speed <= 0f || pauseAtPoint < 0f)
            {
                Debug.LogError("PointToPointMover: назначьте отдельную деталь и две неподвижные разные точки.", this);
                enabled = false;
                return;
            }

            movingPart.position = pointA.position;
            IsRunning = startRunning;
        }

        /// <summary>Отсчитывает паузу, приближает деталь к цели и меняет направление в конце.</summary>
        private void Update()
        {
            if (!IsRunning || Time.deltaTime <= 0f) return;
            float stepTime = Time.deltaTime;
            if (pauseRemaining > 0f)
            {
                float usedTime = Mathf.Min(pauseRemaining, stepTime);
                pauseRemaining -= usedTime;
                stepTime -= usedTime;
                if (stepTime <= 0f) return;
            }

            Vector3 targetPosition = goingToB ? pointB.position : pointA.position;
            movingPart.position = Vector3.MoveTowards(movingPart.position, targetPosition, speed * stepTime);
            if (Vector3.Distance(movingPart.position, targetPosition) > 0.0001f) return;

            movingPart.position = targetPosition;
            goingToB = !goingToB;
            // shortcut: остаток кадра в конце пути отбрасывается; точный таймлайн нужен только для синхронизации механизмов.
            pauseRemaining = pauseAtPoint;
        }

        /// <summary>Останавливает или продолжает путь, не сбрасывая позицию, направление и паузу.</summary>
        public void ToggleRunning()
        {
            if (!isActiveAndEnabled) return;
            IsRunning = !IsRunning;
        }

        private void OnDrawGizmosSelected()
        {
            if (pointA == null || pointB == null) return;
            Color previousColor = Gizmos.color;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.15f);
            Gizmos.DrawWireSphere(pointB.position, 0.15f);
            Gizmos.color = previousColor;
        }
    }
}
