using System;
using System.Collections;
using ArenaSurvival.Gameplay.Door;
using ArenaSurvival.Gameplay.Interaction;
using ArenaSurvival.Gameplay.Motion;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Debugging
{
    /// <summary>Проверяет движение во времени и команды рычагов; запускается вручную в Play.</summary>
    public sealed class MotionChecks : MonoBehaviour
    {
        [SerializeField] private StatefulDoor door;
        [SerializeField] private SceneLever doorLockLever;
        [SerializeField] private SceneLever doorOpenLever;
        [SerializeField] private PointToPointMover mover;
        [SerializeField] private SceneLever motionLever;

        private bool running;

        [ContextMenu("Run Motion Checks")]
        private void RunChecks()
        {
            if (!Application.isPlaying || !isActiveAndEnabled || running ||
                Mathf.Abs(Time.timeScale - 1f) > 0.001f || door == null || mover == null ||
                doorLockLever == null || doorOpenLever == null || motionLever == null ||
                !door.isActiveAndEnabled || !mover.isActiveAndEnabled ||
                !doorLockLever.IsAvailable || !doorOpenLever.IsAvailable || !motionLever.IsAvailable ||
                door.CurrentState != DoorState.Locked || door.IsPassageOccupied || mover.IsRunning)
            {
                Debug.LogError("MotionChecks: нужен Play, Time Scale = 1, Locked, свободная зона, остановленный механизм и пять ссылок.", this);
                return;
            }

            running = true;
            StartCoroutine(CheckSequence());
        }

        private IEnumerator CheckSequence()
        {
            int doorEvents = 0;
            Action<DoorState> countEvent = state => doorEvents++;
            door.StateChanged += countEvent;
            try
            {
                CheckFrameSteps(30);
                CheckFrameSteps(144);

                doorOpenLever.Interact();
                Check(door.CurrentState == DoorState.Locked && doorEvents == 0, "Locked не открывается.");
                doorLockLever.Interact();
                Check(door.CurrentState == DoorState.Closed && doorEvents == 1, "Unlock даёт Closed.");
                door.Unlock();
                Check(doorEvents == 1, "Повторный Unlock не публикует событие.");

                doorOpenLever.Interact();
                Check(door.CurrentState == DoorState.Opening && doorEvents == 2, "Открывание не завершается мгновенно.");
                doorOpenLever.Interact();
                doorLockLever.Interact();
                Check(door.CurrentState == DoorState.Opening && doorEvents == 2, "Повторные команды во время движения отклоняются.");

                float deadline = Time.realtimeSinceStartup + 10f;
                while (door.IsMoving && Time.realtimeSinceStartup < deadline) yield return null;
                Check(door.CurrentState == DoorState.Open && doorEvents == 3, "Створка должна достичь Open за 10 секунд.");
                doorLockLever.Interact();
                Check(door.CurrentState == DoorState.Open && doorEvents == 3, "Открытая дверь не запирается.");

                doorOpenLever.Interact();
                Check(door.CurrentState == DoorState.Closing && doorEvents == 4, "Закрывание начинается с Closing.");
                doorLockLever.Interact();
                Check(door.CurrentState == DoorState.Closing && doorEvents == 4, "Closing ещё не разрешает Lock.");
                deadline = Time.realtimeSinceStartup + 10f;
                while (door.IsMoving && Time.realtimeSinceStartup < deadline) yield return null;
                Check(door.CurrentState == DoorState.Closed && doorEvents == 5, "Створка должна достичь Closed за 10 секунд.");
                doorLockLever.Interact();
                Check(door.CurrentState == DoorState.Locked && doorEvents == 6, "Законченная последовательность даёт шесть событий.");

                Vector3 stoppedPosition = mover.CurrentPosition;
                yield return new WaitForSeconds(0.25f);
                Check(Vector3.Distance(stoppedPosition, mover.CurrentPosition) < 0.001f, "Выключенный механизм не движется.");
                motionLever.Interact();
                Check(mover.IsRunning, "Рычаг запускает механизм.");
                // При штатной паузе 1 с этих двух секунд достаточно для видимого перемещения.
                yield return new WaitForSeconds(2f);
                Check(Vector3.Distance(stoppedPosition, mover.CurrentPosition) > 0.05f, "Механизм должен покинуть исходную позицию.");
                motionLever.Interact();
                Check(!mover.IsRunning, "Повторное действие останавливает механизм.");
                stoppedPosition = mover.CurrentPosition;
                yield return new WaitForSeconds(0.25f);
                Check(Vector3.Distance(stoppedPosition, mover.CurrentPosition) < 0.001f, "Остановка сохраняет позицию.");

                Debug.Log("MotionChecks: PASS. Door events = 6; lever commands and 30/144-step calculations passed.", this);
            }
            finally
            {
                if (door != null) door.StateChanged -= countEvent;
                running = false;
            }
        }

        /// <summary>За одну расчётную секунду получаются 2 м и 60 градусов при разном числе шагов.</summary>
        private static void CheckFrameSteps(int frames)
        {
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            float stepTime = 1f / frames;
            for (int i = 0; i < frames; i++)
            {
                position = Vector3.MoveTowards(position, Vector3.right * 6f, 2f * stepTime);
                rotation = Quaternion.RotateTowards(rotation, Quaternion.Euler(0f, 90f, 0f), 60f * stepTime);
            }

            Check(Mathf.Abs(position.x - 2f) < 0.002f, "Расчёт пути зависит от числа шагов.");
            Check(Quaternion.Angle(rotation, Quaternion.Euler(0f, 60f, 0f)) < 0.1f, "Расчёт поворота зависит от числа шагов.");
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            running = false;
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("MotionChecks: " + message);
        }
    }
}
