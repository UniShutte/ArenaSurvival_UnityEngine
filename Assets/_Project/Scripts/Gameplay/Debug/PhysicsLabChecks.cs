using System.Collections;
using ArenaSurvival.Gameplay.Interaction;
using ArenaSurvival.Gameplay.PhysicsLab;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Debugging
{
    /// <summary>Проверяет покой перед выпуском и однократный полный ход клети.</summary>
    [DisallowMultipleComponent]
    public sealed class PhysicsLabChecks : MonoBehaviour
    {
        [SerializeField] private CageReleaseController cage;
        [SerializeField] private SceneLever releaseLever;
        [SerializeField, Min(1f)] private float settlingTimeout = 30f;

        private bool running;

        [ContextMenu("Run Physics Lab Checks (Play)")]
        public void RunChecks()
        {
            if (!Application.isPlaying || !isActiveAndEnabled || running) return;
            if (cage == null || !cage.isActiveAndEnabled || releaseLever == null ||
                !releaseLever.IsAvailable || cage.HasStarted || !Mathf.Approximately(Time.timeScale, 1f))
            {
                Debug.LogError("PhysicsLabChecks: начните новый Play с Time Scale = 1, назначьте клеть и её доступный рычаг; не запускайте выпуск заранее.", this);
                return;
            }
            StartCoroutine(CheckSequence());
        }

        private IEnumerator CheckSequence()
        {
            running = true;
            float deadline = Time.realtimeSinceStartup + settlingTimeout;
            float quietTime = 0f;
            while (quietTime < 1f && Time.realtimeSinceStartup < deadline)
            {
                yield return new WaitForFixedUpdate();
                quietTime = cage.AreBallsSleeping ? quietTime + Time.fixedDeltaTime : 0f;
            }
            if (quietTime < 1f)
            {
                Fail("Шары не пробыли во сне одну секунду. Проверьте пересечения, опору, материалы и затухание.");
                yield break;
            }

            releaseLever.Interact();
            if (!cage.HasStarted || !cage.IsLowering)
            {
                Fail("Рычаг не запустил назначенную клеть.");
                yield break;
            }
            releaseLever.Interact();
            deadline = Time.realtimeSinceStartup + cage.LoweringDuration + 5f;
            while (cage.IsLowering && Time.realtimeSinceStartup < deadline)
                yield return new WaitForFixedUpdate();

            var body = cage.GetComponent<Rigidbody>();
            if (!cage.IsReleased || Vector3.Distance(body.position, cage.TargetPosition) > 0.01f || cage.TryRelease())
            {
                Fail("Не достигнута нижняя точка или повторная команда принята.");
                yield break;
            }

            Vector3 stoppedPosition = body.position;
            releaseLever.Interact();
            yield return new WaitForSeconds(0.25f);
            if (Vector3.Distance(body.position, stoppedPosition) > 0.01f)
            {
                Fail("Повторное нажатие сдвинуло опущенную клеть.");
                yield break;
            }

            running = false;
            Debug.Log("PhysicsLabChecks: PASS — покой, запуск рычагом, нижняя точка и защита от повторной команды. Выход шаров и работу E проверьте вручную.", this);
        }

        private void Fail(string message)
        {
            running = false;
            Debug.LogError("PhysicsLabChecks: FAIL — " + message, this);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            running = false;
        }
    }
}
