using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ArenaSurvival.Gameplay.Scenes
{
    [DisallowMultipleComponent]
    public sealed class SceneNavigation : MonoBehaviour
    {
        [SerializeField] private string menuScene = "MainMenu";
        [SerializeField] private string arenaScene = "Arena_02";
        [SerializeField] private bool listenForMenuKey;

        private bool isLoading;

        private void Start()
        {
            if (gameObject.scene.name != menuScene) return;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (!listenForMenuKey || gameObject.scene.name != arenaScene || !Application.isFocused) return;

            // shortcut: keyboard-only lab; use an Input Action when adding rebinding.
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.mKey.wasPressedThisFrame)
                ReturnToMenu();
        }

        public void StartGame() => LoadScene(arenaScene);

        public void ReturnToMenu() => LoadScene(menuScene);

        private void LoadScene(string sceneName)
        {
            if (isLoading) return;
            if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"SceneNavigation: cannot load '{sceneName}'. Check its name and the active Build Profile Scene List.", this);
                return;
            }

            isLoading = true;
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
    }
}
