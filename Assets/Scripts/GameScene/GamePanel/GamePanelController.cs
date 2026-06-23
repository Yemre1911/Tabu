using System;
using System.Collections;
using System.Collections.Generic;
using TabuOyunu.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabuOyunu.Gameplay
{
    public sealed class GamePanelController : MonoBehaviour
    {

        private enum ComboState
        {
            Inactive,
            WaitingForSecondCorrect,
            ActiveWindow
        }

        [Serializable]
        public sealed class WordGroup
        {
            public string mainWord;
            public string[] forbiddenWords = new string[5];
        }

        [Serializable]
        private sealed class JokerWordJson
        {
            public List<string> words = new List<string>();
        }

        [Header("Top UI")]
        [SerializeField] private TMP_Text activeTeamNameText;
        [SerializeField] private TMP_Text liveScoreText;
        [SerializeField] private TMP_Text remainingPassText;
        [SerializeField] private GameObject trophyIcon;

        [Header("Word UI")]
        [SerializeField] private TMP_Text mainWordText;
        [SerializeField] private TMP_Text[] forbiddenWordTexts;

        [Header("Joker Mode UI")]
        [SerializeField] private GameObject jokerRoot;
        [SerializeField] private TMP_Text jokerWordText;

        [Header("Combo Mode UI")]
        [SerializeField] private Animator comboAnimator;
        [SerializeField] private string comboOnTriggerName = "on";
        [SerializeField] private string comboOffTriggerName = "off";
        [SerializeField] private Image comboFillImage;
        [SerializeField] private float comboWindowDuration = 5f;
        [SerializeField] private float comboBonusSeconds = 10f;

        [Header("Action Buttons")]
        [SerializeField] private Button correctButton;
        [SerializeField] private Button tabooButton;
        [SerializeField] private Button passButton;
        [SerializeField] private Button pauseButton;

        [Header("Break Panel")]
        [SerializeField] private GameObject breakPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;

        [Header("Action Flash")]
        [SerializeField] private Image actionFlashImage;
        [SerializeField] private float flashDuration = 0.3f;
        [SerializeField] private Color correctFlashColor = new Color(0.62f, 1f, 0.4f);
        [SerializeField] private Color tabooFlashColor = new Color(1f, 0.42f, 0.42f);
        [SerializeField] private Color passFlashColor = new Color(0.57f, 0.75f, 1f);

        [Header("Word Provider")]
        [SerializeField] private WordProvider wordProvider;

        [Header("Temporary Word Source")]
        [SerializeField] private List<WordGroup> defaultWordGroups = new List<WordGroup>();

        [Header("Joker Word Source")]
        [SerializeField] private string jokerWordsResourcesPath = "Data/joker_words";
        [SerializeField]
        private List<string> fallbackJokerWords = new List<string>
        {
            "Kalem",
            "Telefon",
            "Kahve",
            "Anahtar",
            "Yıldız"
        };

        public TeamId PlayingTeam { get; private set; }

        public int CorrectCountThisRound { get; private set; }
        public int TabooCountThisRound { get; private set; }
        public int PassUsedThisRound { get; private set; }

        public int RedScoreBeforeRound { get; private set; }
        public int BlueScoreBeforeRound { get; private set; }

        private GameFlowController flowController;
        private GameRuntimeSettings settings;
        private TurnManager turnManager;
        private ScoreManager scoreManager;
        private TimerController timerController;

        private readonly List<string> jokerWords = new List<string>();

        private int remainingPass;
        private int currentWordIndex = -1;
        private int currentJokerWordIndex = -1;

        private bool isTurnActive;
        private Color defaultFlashColor;
        private Coroutine flashRoutine;

        private ComboState comboState = ComboState.Inactive;
        private float comboWindowRemaining;
        private Coroutine comboRoutine;

        private bool IsClassicMode => settings == null || settings.gameModeId == GameModeIds.Classic;
        private bool IsComboMode => settings != null && settings.gameModeId == GameModeIds.Combo;
        private bool IsJokerMode => settings != null && settings.gameModeId == GameModeIds.Joker;

        private void Awake()
        {
            if (actionFlashImage != null)
            {
                defaultFlashColor = actionFlashImage.color;
                actionFlashImage.raycastTarget = false;
            }

            if (comboFillImage != null)
                comboFillImage.fillAmount = 0f;

            if (breakPanel != null)
                breakPanel.SetActive(false);

            if (trophyIcon != null)
                trophyIcon.SetActive(false);

            if (jokerRoot != null)
                jokerRoot.SetActive(false);

            if (wordProvider == null)
                wordProvider = GetComponent<WordProvider>();
        }

        public void Bind(
            GameFlowController flow,
            GameRuntimeSettings runtimeSettings,
            TurnManager turn,
            ScoreManager score,
            TimerController timer)
        {
            flowController = flow;
            settings = runtimeSettings;
            turnManager = turn;
            scoreManager = score;
            timerController = timer;

            if (wordProvider != null)
                wordProvider.Initialize(settings.categoryId);

            LoadJokerWords();

            if (correctButton != null)
            {
                correctButton.onClick.RemoveAllListeners();
                correctButton.onClick.AddListener(OnCorrectPressed);
            }

            if (tabooButton != null)
            {
                tabooButton.onClick.RemoveAllListeners();
                tabooButton.onClick.AddListener(OnTabooPressed);
            }

            if (passButton != null)
            {
                passButton.onClick.RemoveAllListeners();
                passButton.onClick.AddListener(OnPassPressed);
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveAllListeners();
                pauseButton.onClick.AddListener(OnPausePressed);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(OnResumePressed);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(OnQuitPressed);
            }
        }

        public void BeginTurn()
        {
            if (settings == null || turnManager == null || scoreManager == null)
                return;

            isTurnActive = true;

            PlayingTeam = turnManager.CurrentTeam;

            CorrectCountThisRound = 0;
            TabooCountThisRound = 0;
            PassUsedThisRound = 0;

            RedScoreBeforeRound = scoreManager.RedScore;
            BlueScoreBeforeRound = scoreManager.BlueScore;

            remainingPass = Mathf.Max(0, settings.passCount);

            if (breakPanel != null)
                breakPanel.SetActive(false);

            if (jokerRoot != null)
                jokerRoot.SetActive(IsJokerMode);

            StopComboWindow(false);

            RefreshTeamName();
            RefreshLiveScore();
            RefreshPassText();
            RefreshTrophy();

            SetGameplayButtonsInteractable(true);

            ShowNextWord();
        }

        public void EndTurn()
        {
            isTurnActive = false;

            StopComboWindow(false);
            SetGameplayButtonsInteractable(false);

            if (breakPanel != null)
                breakPanel.SetActive(false);
        }

        private void OnCorrectPressed()
        {
            if (!CanUseGameplayButtons())
                return;

            CorrectCountThisRound++;

            scoreManager.ApplyAction(PlayingTeam, WordAction.Correct);

            RefreshLiveScore();
            RefreshTrophy();

            HandleComboCorrect();

            PlayFlash(correctFlashColor);
            ShowNextWord();
        }

        private void OnTabooPressed()
        {
            if (!CanUseGameplayButtons())
                return;

            TabooCountThisRound++;

            scoreManager.ApplyAction(PlayingTeam, WordAction.Tabu);

            RefreshLiveScore();
            RefreshTrophy();

            BreakComboByWrongAction();

            PlayFlash(tabooFlashColor);
            ShowNextWord();
        }

        private void OnPassPressed()
        {
            if (!CanUseGameplayButtons())
                return;

            if (remainingPass <= 0)
                return;

            remainingPass--;
            PassUsedThisRound++;

            RefreshPassText();

            BreakComboByWrongAction();

            PlayFlash(passFlashColor);
            ShowNextWord();
        }

        private void OnPausePressed()
        {
            if (!isTurnActive)
                return;

            if (timerController != null)
                timerController.PauseTimer();

            SetGameplayButtonsInteractable(false);

            if (breakPanel != null)
                breakPanel.SetActive(true);
        }

        private void OnResumePressed()
        {
            if (!isTurnActive)
                return;

            if (breakPanel != null)
                breakPanel.SetActive(false);

            if (timerController != null)
                timerController.ResumeTimer();

            SetGameplayButtonsInteractable(true);
        }

        private void OnQuitPressed()
        {
            StopComboWindow(false);

            if (flowController != null)
                flowController.OnFinishPressed();
        }

        private bool CanUseGameplayButtons()
        {
            if (!isTurnActive)
                return false;

            if (timerController != null && timerController.IsPaused)
                return false;

            return true;
        }

        private void SetGameplayButtonsInteractable(bool value)
        {
            if (correctButton != null)
                correctButton.interactable = value;

            if (tabooButton != null)
                tabooButton.interactable = value;

            if (passButton != null)
                passButton.interactable = value && remainingPass > 0;

            if (pauseButton != null)
                pauseButton.interactable = value;
        }

        private void RefreshTeamName()
        {
            if (activeTeamNameText == null || turnManager == null || settings == null)
                return;

            activeTeamNameText.text = turnManager.GetTeamName(settings, PlayingTeam);
        }

        private void RefreshLiveScore()
        {
            if (liveScoreText == null || scoreManager == null)
                return;

            liveScoreText.text = scoreManager.GetScore(PlayingTeam).ToString();
        }

        private void RefreshPassText()
        {
            if (remainingPassText != null)
                remainingPassText.text = remainingPass.ToString();

            if (passButton != null)
                passButton.interactable = isTurnActive && remainingPass > 0;
        }

        private void RefreshTrophy()
        {
            if (trophyIcon == null || scoreManager == null)
                return;

            trophyIcon.SetActive(scoreManager.HasReachedTarget(PlayingTeam));
        }

        private void ShowNextWord()
        {
            WordGroup wordGroup = GetNextWordGroup();

            if (mainWordText != null)
                mainWordText.text = wordGroup.mainWord;

            if (forbiddenWordTexts != null)
            {
                for (int i = 0; i < forbiddenWordTexts.Length; i++)
                {
                    if (forbiddenWordTexts[i] == null)
                        continue;

                    if (wordGroup.forbiddenWords != null && i < wordGroup.forbiddenWords.Length)
                        forbiddenWordTexts[i].text = wordGroup.forbiddenWords[i];
                    else
                        forbiddenWordTexts[i].text = "-";
                }
            }

            if (IsJokerMode)
                ShowNextJokerWord();
        }

        private WordGroup GetNextWordGroup()
        {
            if (wordProvider != null && wordProvider.HasWords)
            {
                return wordProvider.GetNextWord();
            }

            if (defaultWordGroups == null || defaultWordGroups.Count == 0)
            {
                return new WordGroup
                {
                    mainWord = "KELİME",
                    forbiddenWords = new[]
                    {
                "Yasak 1",
                "Yasak 2",
                "Yasak 3",
                "Yasak 4",
                "Yasak 5"
            }
                };
            }

            if (defaultWordGroups.Count == 1)
            {
                currentWordIndex = 0;
                return defaultWordGroups[0];
            }

            int nextIndex = currentWordIndex;

            while (nextIndex == currentWordIndex)
            {
                nextIndex = UnityEngine.Random.Range(0, defaultWordGroups.Count);
            }

            currentWordIndex = nextIndex;
            return defaultWordGroups[currentWordIndex];
        }

        private void LoadJokerWords()
        {
            jokerWords.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(jokerWordsResourcesPath);

            if (jsonAsset != null)
            {
                JokerWordJson json = JsonUtility.FromJson<JokerWordJson>(jsonAsset.text);

                if (json != null && json.words != null)
                    jokerWords.AddRange(json.words);
            }

            if (jokerWords.Count == 0 && fallbackJokerWords != null)
                jokerWords.AddRange(fallbackJokerWords);
        }

        private void ShowNextJokerWord()
        {
            if (jokerRoot != null)
                jokerRoot.SetActive(true);

            if (jokerWordText == null)
                return;

            if (jokerWords.Count == 0)
            {
                jokerWordText.text = "JOKER";
                return;
            }

            if (jokerWords.Count == 1)
            {
                currentJokerWordIndex = 0;
                jokerWordText.text = jokerWords[0];
                return;
            }

            int nextIndex = currentJokerWordIndex;

            while (nextIndex == currentJokerWordIndex)
            {
                nextIndex = UnityEngine.Random.Range(0, jokerWords.Count);
            }

            currentJokerWordIndex = nextIndex;
            jokerWordText.text = jokerWords[currentJokerWordIndex];
        }

        private void HandleComboCorrect()
        {
            if (!IsComboMode)
                return;

            switch (comboState)
            {
                case ComboState.Inactive:
                    // 1. doğru: UI açılmaz, sadece gizli 5 saniye başlar.
                    StartHiddenComboWindow();
                    break;

                case ComboState.WaitingForSecondCorrect:
                    // 2. doğru: Kombo UI artık açılır.
                    // Burada +10 saniye YOK.
                    StartVisibleComboWindow();
                    break;

                case ComboState.ActiveWindow:
                    // 3. ve sonrası: Artık kombo bonusu verilir.
                    if (timerController != null)
                        timerController.AddTime(comboBonusSeconds);

                    StartVisibleComboWindow();
                    break;
            }
        }

        private void BreakComboByWrongAction()
        {
            if (!IsComboMode)
                return;

            StopComboWindow(true);
        }
        private void StartHiddenComboWindow()
        {
            comboState = ComboState.WaitingForSecondCorrect;
            comboWindowRemaining = comboWindowDuration;

            if (comboFillImage != null)
                comboFillImage.fillAmount = 0f;

            RestartComboRoutine();
        }
        private void StartVisibleComboWindow()
        {
            bool wasAlreadyVisible = comboState == ComboState.ActiveWindow;

            comboState = ComboState.ActiveWindow;
            comboWindowRemaining = comboWindowDuration;

            if (!wasAlreadyVisible)
                PlayComboOn();

            RefreshComboFill();
            RestartComboRoutine();
        }
        private void RestartComboRoutine()
        {
            if (comboRoutine != null)
            {
                StopCoroutine(comboRoutine);
                comboRoutine = null;
            }

            comboRoutine = StartCoroutine(ComboWindowRoutine());
        }



        private IEnumerator ComboWindowRoutine()
        {
            while (comboState != ComboState.Inactive && comboWindowRemaining > 0f)
            {
                if (timerController == null || !timerController.IsPaused)
                {
                    comboWindowRemaining -= Time.deltaTime;

                    if (comboWindowRemaining < 0f)
                        comboWindowRemaining = 0f;

                    if (comboState == ComboState.ActiveWindow)
                        RefreshComboFill();
                }

                yield return null;
            }

            comboRoutine = null;

            if (comboState == ComboState.WaitingForSecondCorrect)
            {
                // İlk doğru geldi ama ikinci doğru 5 saniye içinde gelmedi.
                // UI zaten açılmamıştı, sessizce sıfırla.
                StopComboWindow(false);
            }
            else if (comboState == ComboState.ActiveWindow)
            {
                // Kombo aktifken 5 saniye doldu, seri bozuldu.
                StopComboWindow(true);
            }
        }

        private void StopComboWindow(bool playOffAnimation)
        {
            bool shouldPlayOff = playOffAnimation && comboState == ComboState.ActiveWindow;

            comboState = ComboState.Inactive;
            comboWindowRemaining = 0f;

            if (comboRoutine != null)
            {
                StopCoroutine(comboRoutine);
                comboRoutine = null;
            }

            if (comboFillImage != null)
                comboFillImage.fillAmount = 0f;

            if (shouldPlayOff)
                PlayComboOff();
        }

        private void RefreshComboFill()
        {
            if (comboFillImage == null)
                return;

            if (comboWindowDuration <= 0f)
            {
                comboFillImage.fillAmount = 0f;
                return;
            }

            comboFillImage.fillAmount = Mathf.Clamp01(comboWindowRemaining / comboWindowDuration);
        }

        private void PlayComboOn()
        {
            if (comboAnimator == null)
                return;

            comboAnimator.ResetTrigger(comboOffTriggerName);
            comboAnimator.SetTrigger(comboOnTriggerName);
        }

        private void PlayComboOff()
        {
            if (comboAnimator == null)
                return;

            comboAnimator.ResetTrigger(comboOnTriggerName);
            comboAnimator.SetTrigger(comboOffTriggerName);
        }

        private void PlayFlash(Color flashColor)
        {
            if (actionFlashImage == null)
                return;

            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(FlashRoutine(flashColor));
        }

        private IEnumerator FlashRoutine(Color flashColor)
        {
            actionFlashImage.color = flashColor;

            yield return new WaitForSeconds(flashDuration);

            actionFlashImage.color = defaultFlashColor;

            flashRoutine = null;
        }
    }
}