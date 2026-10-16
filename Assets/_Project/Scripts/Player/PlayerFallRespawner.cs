using UnityEngine;

namespace ArenaSurvival.Player
{
    /// <summary>
    /// Возвращает Player в начальную позицию,
    /// если персонаж упал ниже разрешённой высоты.
    ///
    /// Компонент запоминает Transform Player в момент Start.
    /// Для более сложной игры эту позицию можно заменить
    /// системой контрольных точек.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerFallRespawner : MonoBehaviour
    {
        [Header("Respawn Settings")]
        [SerializeField]
        private float fallThresholdY = -10f;

        [SerializeField, Min(0f)]
        private float safeHeightOffset = 0.2f;

        private CharacterController characterController;

        private Vector3 respawnPosition;
        private Quaternion respawnRotation;

        private void Awake()
        {
            characterController =
                GetComponent<CharacterController>();
        }

        private void Start()
        {
            // PlayerSpawner создаёт Player в точке появления,
            // поэтому начальный Transform можно сохранить
            // как позицию возврата.
            SaveCurrentTransformAsRespawnPoint();
        }

        private void Update()
        {
            if (transform.position.y < fallThresholdY)
            {
                Respawn();
            }
        }

        /// <summary>
        /// Сохраняет текущие позицию и вращение
        /// как новую точку возврата.
        ///
        /// Метод пригодится в будущем для checkpoints.
        /// </summary>
        [ContextMenu("Save Current Transform As Respawn Point")]
        public void SaveCurrentTransformAsRespawnPoint()
        {
            respawnPosition = transform.position;
            respawnRotation = transform.rotation;
        }

        /// <summary>
        /// Возвращает Player в сохранённую позицию.
        /// </summary>
        [ContextMenu("Respawn")]
        public void Respawn()
        {
            // Перед прямым изменением Transform временно
            // отключаем CharacterController. Иначе он может
            // попытаться разрешить пересечение в старой позиции.
            characterController.enabled = false;

            transform.SetPositionAndRotation(
                respawnPosition +
                Vector3.up * safeHeightOffset,
                respawnRotation);

            characterController.enabled = true;
        }
    }
}