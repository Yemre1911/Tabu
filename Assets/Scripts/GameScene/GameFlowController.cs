/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TabuOyunu.Data;
using TabuOyunu.Managers;

namespace TabuOyunu.Gameplay
{
    public sealed class GameFlowController : MonoBehaviour
    {
        [Header("Runtime Settings")]
        [SerializeField] private GameRuntimeSettings settings = new GameRuntimeSettings();

        [Header("Managers")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private TimerController timerController;
        [SerializeField] private TurnHistoryManager historyManager;

        [Header("UI Controllers")]
        [SerializeField] private CurtainController curtainController;
        [SerializeField] private CurtainUIController curtainUIController;

        [Header("Scene Objects")]
        [SerializeField] private GameObject gamePanelRoot;

        [Header("Return To Menu")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";
        [SerializeField] private float returnToMenuDelay = 0.4f;
        [SerializeField] private UnityEvent onBeforeReturnToMainMenu;

        public GameFlowState State { get; private set; }

        private Coroutine flowRoutine;


        private void LoadSettingsFromSession()
        {
            if (SessionManager.Instance == null)
            {
                Debug.LogWarning($"{nameof(GameFlowController)}: SessionManager bulunamadı. Inspector değerleri kullanılacak.");
                return;
            }

            if (!SessionManager.Instance.HasSettings())
            {
                Debug.LogWarning($"{nameof(GameFlowController)}: Session ayarları bulunamadı. Inspector değerleri kullanılacak.");
                return;
            }

            GameSettings sessionSettings = SessionManager.Instance.CurrentSettings;

            settings.categoryId = sessionSettings.CategoryId;
            settings.categoryDisplayName = sessionSettings.CategoryDisplayName;

            settings.gameModeId = sessionSettings.GameModeId;
            settings.gameModeDisplayName = sessionSettings.GameModeDisplayName;

            settings.redTeamName = sessionSettings.RedTeamName;
            settings.blueTeamName = sessionSettings.BlueTeamName;

            settings.roundSeconds = sessionSettings.RoundTimeSeconds;
            settings.tabuPenalty = sessionSettings.TabooPenalty;
            settings.passCount = sessionSettings.PassCount;
            settings.targetScore = sessionSettings.TargetScore;

            Debug.Log(
                $"GameScene settings loaded: " +
                $"{settings.redTeamName} vs {settings.blueTeamName}, " +
                $"Time: {settings.roundSeconds}, " +
                $"Tabu: {settings.tabuPenalty}, " +
                $"Pass: {settings.passCount}, " +
                $"Target: {settings.targetScore}, " +
                $"Category: {settings.categoryDisplayName}, " +
                $"Mode: {settings.gameModeDisplayName}"
            );
        }

        private void OnEnable()
        {
            if (timerController != null)
                timerController.OnTimeExpired += HandleTimeExpired;
        }

        private void OnDisable()
        {
            if (timerController != null)
                timerController.OnTimeExpired -= HandleTimeExpired;
        }

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            State = GameFlowState.Setup;

            LoadSettingsFromSession();

            turnManager.Initialize();
            scoreManager.Initialize(settings.targetScore, settings.tabuPenalty);
            historyManager.Clear();

            if (timerController != null)
                timerController.StopTimer();

            if (gamePanelRoot != null)
                gamePanelRoot.SetActive(false);

            if (curtainController != null)
                curtainController.SetClosedInstant();

            if (curtainUIController != null)
            {
                curtainUIController.Bind(this, settings, turnManager, scoreManager, historyManager);
                curtainUIController.RefreshAll(false, true);
            }

            State = GameFlowState.CurtainIdle;
        }

        public void OnGoPressed()
        {
            if (State != GameFlowState.CurtainIdle)
                return;

            StartFlowRoutine(StartTurnRoutine());
        }

        public void OnFinishPressed()
        {
            if (State == GameFlowState.ReturningToMenu)
                return;

            StartFlowRoutine(ReturnToMainMenuRoutine());
        }

        public void RegisterWordAction(string word, IEnumerable<string> forbiddenWords, WordAction action)
        {
            if (State != GameFlowState.Playing)
                return;

            int scoreDelta = scoreManager.ApplyAction(turnManager.CurrentTeam, action);

            historyManager.AddWordAction(
                word,
                forbiddenWords,
                action,
                scoreDelta
            );
        }

        private IEnumerator StartTurnRoutine()
        {
            State = GameFlowState.CurtainOpening;

            if (curtainUIController != null)
                yield return curtainUIController.PlayOffRoutine();

            if (gamePanelRoot != null)
                gamePanelRoot.SetActive(true);

            if (curtainController != null)
                yield return curtainController.OpenRoutine();

            historyManager.BeginTurn(
                turnManager.CurrentRound,
                turnManager.CurrentTeam,
                turnManager.GetCurrentTeamName(settings)
            );

            State = GameFlowState.Playing;

            timerController.StartTimer(settings.roundSeconds);
        }

        private void HandleTimeExpired()
        {
            if (State != GameFlowState.Playing)
                return;

            StartFlowRoutine(EndTurnRoutine());
        }

        private IEnumerator EndTurnRoutine()
        {
            State = GameFlowState.CurtainClosing;

            if (curtainController != null)
                yield return curtainController.CloseRoutine();

            if (gamePanelRoot != null)
                gamePanelRoot.SetActive(false);

            historyManager.EndCurrentTurn();

            bool isGameOver = scoreManager.HasReachedTarget(turnManager.CurrentTeam);

            if (!isGameOver)
                turnManager.AdvanceTurn();

            if (curtainUIController != null)
            {
                curtainUIController.RefreshAll(isGameOver, true);
                yield return curtainUIController.PlayOnRoutine();
            }

            State = isGameOver ? GameFlowState.GameOver : GameFlowState.CurtainIdle;
        }

        private IEnumerator ReturnToMainMenuRoutine()
        {
            State = GameFlowState.ReturningToMenu;

            if (timerController != null)
                timerController.StopTimer();

            onBeforeReturnToMainMenu?.Invoke();

            yield return new WaitForSeconds(returnToMenuDelay);

            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void StartFlowRoutine(IEnumerator routine)
        {
            if (flowRoutine != null)
                StopCoroutine(flowRoutine);

            flowRoutine = StartCoroutine(routine);
        }
    }
}*/

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TabuOyunu.Data;
using TabuOyunu.Managers;

namespace TabuOyunu.Gameplay
{
    public sealed class GameFlowController : MonoBehaviour
    {
        [Header("Runtime Settings")]
        [SerializeField] private GameRuntimeSettings settings = new GameRuntimeSettings();

        [Header("Managers")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private TimerController timerController;

        [Header("UI Controllers")]
        [SerializeField] private CurtainController curtainController;
        [SerializeField] private CurtainUIController curtainUIController;
        [SerializeField] private CurtainRoundStatsAnimator curtainRoundStatsAnimator;
        [SerializeField] private GamePanelController gamePanelController;

        [Header("Scene Objects")]
        [SerializeField] private GameObject gamePanelRoot;

        [Header("Return To Menu")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";
        [SerializeField] private float returnToMenuDelay = 0.4f;
        [SerializeField] private UnityEvent onBeforeReturnToMainMenu;

        public GameFlowState State { get; private set; }

        private Coroutine flowRoutine;

        private void OnEnable()
        {
            if (timerController != null)
                timerController.OnTimeExpired += HandleTimeExpired;
        }

        private void OnDisable()
        {
            if (timerController != null)
                timerController.OnTimeExpired -= HandleTimeExpired;
        }

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            State = GameFlowState.Setup;

            LoadSettingsFromSession();

            turnManager.Initialize();
            scoreManager.Initialize(settings.targetScore, settings.tabuPenalty);

            if (timerController != null)
                timerController.StopTimer();

            if (gamePanelRoot != null)
                gamePanelRoot.SetActive(false);

            if (curtainController != null)
                curtainController.SetClosedInstant();

            if (curtainUIController != null)
            {
                curtainUIController.Bind(this, settings, turnManager, scoreManager);
                curtainUIController.RefreshAll(false);
            }
            if (gamePanelController != null)
            {
                gamePanelController.Bind(
                    this,
                    settings,
                    turnManager,
                    scoreManager,
                    timerController
                );
            }

            if (curtainRoundStatsAnimator != null)
            {
                curtainRoundStatsAnimator.PrepareForFirstRound(
                    scoreManager.RedScore,
                    scoreManager.BlueScore
                );
            }

            State = GameFlowState.CurtainIdle;
        }

        public void OnGoPressed()
        {
            if (State != GameFlowState.CurtainIdle)
                return;

            StartFlowRoutine(StartTurnRoutine());
        }

        public void OnFinishPressed()
        {
            if (State == GameFlowState.ReturningToMenu)
                return;

            StartFlowRoutine(ReturnToMainMenuRoutine());
        }

        private IEnumerator StartTurnRoutine()
        {
            State = GameFlowState.CurtainOpening;

            if (curtainUIController != null)
                yield return curtainUIController.PlayOffRoutine();

            if (gamePanelRoot != null)
                gamePanelRoot.SetActive(true);

            if (gamePanelController != null)
                gamePanelController.BeginTurn();

            if (curtainController != null)
                yield return curtainController.OpenRoutine();

            State = GameFlowState.Playing;

            if (timerController != null)
                timerController.StartTimer(settings.roundSeconds);
        }

        private void HandleTimeExpired()
        {
            if (State != GameFlowState.Playing)
                return;

            StartFlowRoutine(EndTurnRoutine());
        }

        /*private IEnumerator EndTurnRoutine()
        {
            State = GameFlowState.CurtainClosing;

            TeamId playedTeam = turnManager.CurrentTeam;

            int correctCount = gamePanelController != null ? gamePanelController.CorrectCountThisRound : 0;
            int tabooCount = gamePanelController != null ? gamePanelController.TabooCountThisRound : 0;

            int redScoreBefore = gamePanelController != null ? gamePanelController.RedScoreBeforeRound : scoreManager.RedScore;
            int blueScoreBefore = gamePanelController != null ? gamePanelController.BlueScoreBeforeRound : scoreManager.BlueScore;

            if (gamePanelController != null)
                gamePanelController.EndTurn();

            if (curtainController != null)
                yield return curtainController.CloseRoutine();

            if (gamePanelRoot != null)
                gamePanelRoot.SetActive(false);

            bool isGameOver = scoreManager.HasReachedTarget(playedTeam);

            if (!isGameOver)
                turnManager.AdvanceTurn();

            if (curtainUIController != null)
            {
                curtainUIController.RefreshAll(isGameOver);
                yield return curtainUIController.PlayOnRoutine();
            }

            if (curtainRoundStatsAnimator != null)
            {
                RoundResultData result = new RoundResultData
                {
                    playingTeam = playedTeam == TeamId.Red ? TeamSide.Red : TeamSide.Blue,
                    correctCount = correctCount,
                    tabooCount = tabooCount,
                    tabooPenalty = settings.tabuPenalty
                };

                yield return curtainRoundStatsAnimator.PlayRoundResultRoutine(
                    result,
                    redScoreBefore,
                    blueScoreBefore,
                    scoreManager.RedScore,
                    scoreManager.BlueScore
                );
            }

            State = isGameOver ? GameFlowState.GameOver : GameFlowState.CurtainIdle;
        }*/

        private IEnumerator EndTurnRoutine()
        {
            State = GameFlowState.CurtainClosing;

            TeamId playedTeam = turnManager.CurrentTeam;

            int correctCount = gamePanelController != null
                ? gamePanelController.CorrectCountThisRound
                : 0;

            int tabooCount = gamePanelController != null
                ? gamePanelController.TabooCountThisRound
                : 0;

            int redScoreBefore = gamePanelController != null
                ? gamePanelController.RedScoreBeforeRound
                : scoreManager.RedScore;

            int blueScoreBefore = gamePanelController != null
                ? gamePanelController.BlueScoreBeforeRound
                : scoreManager.BlueScore;

            int redScoreAfter = scoreManager.RedScore;
            int blueScoreAfter = scoreManager.BlueScore;

            if (gamePanelController != null)
                gamePanelController.EndTurn();

            if (curtainController != null)
                yield return curtainController.CloseRoutine();

            if (gamePanelRoot != null)
                gamePanelRoot.SetActive(false);

            bool isGameOver = scoreManager.HasReachedTarget(playedTeam);

            if (!isGameOver)
                turnManager.AdvanceTurn();

            if (curtainUIController != null)
            {
                curtainUIController.RefreshAll(isGameOver);
                yield return curtainUIController.PlayOnRoutine();
            }

            if (curtainRoundStatsAnimator != null)
            {
                RoundResultData result = new RoundResultData
                {
                    playingTeam = playedTeam == TeamId.Red ? TeamSide.Red : TeamSide.Blue,
                    correctCount = correctCount,
                    tabooCount = tabooCount,
                    tabooPenalty = settings.tabuPenalty
                };

                yield return curtainRoundStatsAnimator.PlayRoundResultRoutine(
                    result,
                    redScoreBefore,
                    blueScoreBefore,
                    redScoreAfter,
                    blueScoreAfter
                );
            }
            else
            {
                Debug.LogError("GameFlowController: CurtainRoundStatsAnimator referansı yok.");
            }

            State = isGameOver ? GameFlowState.GameOver : GameFlowState.CurtainIdle;
        }

        private IEnumerator ReturnToMainMenuRoutine()
        {
            State = GameFlowState.ReturningToMenu;

            if (timerController != null)
                timerController.StopTimer();

            onBeforeReturnToMainMenu?.Invoke();

            yield return new WaitForSeconds(returnToMenuDelay);

            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void StartFlowRoutine(IEnumerator routine)
        {
            if (flowRoutine != null)
                StopCoroutine(flowRoutine);

            flowRoutine = StartCoroutine(routine);
        }

        private void LoadSettingsFromSession()
        {
            if (SessionManager.Instance == null)
            {
                Debug.LogWarning($"{nameof(GameFlowController)}: SessionManager bulunamadı. Inspector değerleri kullanılacak.");
                return;
            }

            if (!SessionManager.Instance.HasSettings())
            {
                Debug.LogWarning($"{nameof(GameFlowController)}: Session ayarları bulunamadı. Inspector değerleri kullanılacak.");
                return;
            }

            GameSettings sessionSettings = SessionManager.Instance.CurrentSettings;

            settings.categoryId = sessionSettings.CategoryId;
            settings.categoryDisplayName = sessionSettings.CategoryDisplayName;

            settings.gameModeId = sessionSettings.GameModeId;
            settings.gameModeDisplayName = sessionSettings.GameModeDisplayName;

            settings.redTeamName = sessionSettings.RedTeamName;
            settings.blueTeamName = sessionSettings.BlueTeamName;

            settings.roundSeconds = sessionSettings.RoundTimeSeconds;
            settings.tabuPenalty = sessionSettings.TabooPenalty;
            settings.passCount = sessionSettings.PassCount;
            settings.targetScore = sessionSettings.TargetScore;

            Debug.Log(
                $"GameScene settings loaded: " +
                $"{settings.redTeamName} vs {settings.blueTeamName}, " +
                $"Time: {settings.roundSeconds}, " +
                $"Tabu: {settings.tabuPenalty}, " +
                $"Pass: {settings.passCount}, " +
                $"Target: {settings.targetScore}, " +
                $"Category: {settings.categoryDisplayName}, " +
                $"Mode: {settings.gameModeDisplayName}"
            );
        }
    }
}