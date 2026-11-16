using ArenaSurvival.Gameplay.Contacts;
using ArenaSurvival.Gameplay.PhysicsLab;
using UnityEngine;

namespace ArenaSurvival.Gameplay.Debugging
{
    /// <summary>Проверяет настройку ЛР6, но не подменяет реальные столкновения.</summary>
    [DisallowMultipleComponent]
    public sealed class CollisionLabChecks : MonoBehaviour
    {
        [SerializeField] private BallImpactTarget impactTarget;
        [SerializeField] private BallCollector collector;
        [SerializeField] private PlayerPresenceZone presenceZone;

        [ContextMenu("Check Lab 6 Setup (Play)")]
        public void RunChecks()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            bool valid = Check(impactTarget != null && impactTarget.isActiveAndEnabled, "Назначьте исправную плиту.");
            valid &= Check(collector != null && collector.isActiveAndEnabled, "Назначьте исправный приёмник.");
            valid &= Check(presenceZone != null && presenceZone.isActiveAndEnabled, "Назначьте исправную зону игрока.");
            if (!valid) return;

            string[] names = { "Default", "Player", "PhysicsBall", "BallZone", "PlayerZone" };
            var layers = new int[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                layers[i] = LayerMask.NameToLayer(names[i]);
                valid &= Check(layers[i] >= 0, "Отсутствует слой " + names[i]);
            }
            if (!valid) return;

            valid &= Check(impactTarget.gameObject.layer == layers[0], "Плита должна быть на Default.");
            valid &= Check(collector.gameObject.layer == layers[3], "Приёмник должен быть на BallZone.");
            valid &= Check(presenceZone.gameObject.layer == layers[4], "Зона игрока должна быть на PlayerZone.");
            valid &= Check(collector.RequiredBalls == 3 && collector.CollectedCount == 0, "Нужен новый Play: цель 3, собрано 0.");

            var balls = FindObjectsByType<PhysicsBall>(FindObjectsSortMode.None);
            valid &= Check(balls.Length == 9, "Для проверки оставьте девять основных активных шаров, без тестовых копий.");
            foreach (var ball in balls)
            {
                var body = ball.GetComponent<Rigidbody>();
                var sphere = ball.GetComponent<SphereCollider>();
                valid &= Check(ball.isActiveAndEnabled && ball.gameObject.layer == layers[2] &&
                    body != null && !body.isKinematic && body.useGravity && body.detectCollisions &&
                    sphere != null && sphere.enabled && !sphere.isTrigger, "Проверьте шар " + ball.name);
            }

            var players = FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
            valid &= Check(players.Length == 1, "Нужен один активный CharacterController игрока.");
            foreach (var player in players)
                valid &= Check(player.enabled && player.CompareTag("Player") && player.gameObject.layer == layers[1] &&
                    player.GetComponent<Rigidbody>() == null, "На корне Player нужны Tag/Layer Player, без Rigidbody.");

            bool[,] expected = {
                { true, true, true, false, false },
                { true, true, true, false, true },
                { true, true, true, true, false },
                { false, false, true, false, false },
                { false, true, false, false, false }
            };
            // Проверяем только пары с новыми слоями, старые правила не переписываем.
            for (int i = 2; i < names.Length; i++)
            for (int j = 0; j <= i; j++)
                valid &= Check(!Physics.GetIgnoreLayerCollision(layers[i], layers[j]) == expected[i, j],
                    "Неверная пара слоёв: " + names[i] + " / " + names[j]);

            if (valid) Debug.Log("CollisionLabChecks: PASS — ссылки, шары, Player и матрица настроены. Удары, подбор и вход/выход проверьте вручную.", this);
        }

        private bool Check(bool condition, string message)
        {
            if (!condition) Debug.LogError("CollisionLabChecks: FAIL — " + message, this);
            return condition;
        }
    }
}
