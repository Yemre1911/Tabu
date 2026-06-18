

using System.Collections;
using TabuOyunu.Data;
using TabuOyunu.Managers;
using TabuOyunu.UI.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TabuOyunu.UI.MainMenu
{
    public class MainMenuSessionBuilder : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private SettingsUIController settingsUI;
        [SerializeField] private LobbyUIController lobbyUI;
        [SerializeField] private CategoryUIController categoryUI;

        [Header("Loading")]
        [SerializeField] private LoadingFadeController loadingFade;

        [Header("Scene")]
        [SerializeField] private string gameSceneName = "GameScene";

        private bool isStartingGame;

        public void StartGame()
        {
            if (isStartingGame)
                return;

            if (!CanStartGame())
                return;

            StartCoroutine(StartGameRoutine());
        }

        private IEnumerator StartGameRoutine()
        {
            isStartingGame = true;

            GameSettings settings = BuildSettings();

            SessionManager.Instance.SetSettings(settings);

            if (loadingFade != null)
                yield return loadingFade.OpenRoutine();

            SceneManager.LoadScene(gameSceneName);
        }

        private bool CanStartGame()
        {
            if (settingsUI == null)
            {
                Debug.LogError($"{nameof(MainMenuSessionBuilder)}: SettingsUIController reference is missing.");
                return false;
            }

            if (lobbyUI == null)
            {
                Debug.LogError($"{nameof(MainMenuSessionBuilder)}: LobbyUIController reference is missing.");
                return false;
            }

            if (categoryUI == null)
            {
                Debug.LogError($"{nameof(MainMenuSessionBuilder)}: CategoryUIController reference is missing.");
                return false;
            }

            if (SessionManager.Instance == null)
            {
                Debug.LogError($"{nameof(MainMenuSessionBuilder)}: SessionManager is missing in the scene.");
                return false;
            }

            return true;
        }

        private GameSettings BuildSettings()
        {
            CategorySelection category = categoryUI.SelectedCategory;
            GameModeDefinition gameMode = lobbyUI.SelectedGameMode;

            return new GameSettings(
                categoryId: category.Id,
                categoryDisplayName: category.DisplayName,
                gameModeId: gameMode.Id,
                gameModeDisplayName: gameMode.DisplayName,
                roundTimeSeconds: settingsUI.RoundTimeSeconds,
                tabooPenalty: settingsUI.TabooPenalty,
                passCount: settingsUI.PassCount,
                targetScore: settingsUI.TargetScore,
                redTeamName: lobbyUI.RedTeamName,
                blueTeamName: lobbyUI.BlueTeamName
            );
        }
    }
}