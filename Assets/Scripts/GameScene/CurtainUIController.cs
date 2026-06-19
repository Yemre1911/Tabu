/*using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabuOyunu.Gameplay
{
    public sealed class CurtainUIController : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator animator;
        [SerializeField] private string onTriggerName = "on";
        [SerializeField] private string offTriggerName = "off";
        [SerializeField] private float animationWait = 0.35f;

        [Header("Top Info")]
        [SerializeField] private TMP_Text roundValueText;
        [SerializeField] private TMP_Text targetScoreValueText;

        [Header("Teams")]
        [SerializeField] private TMP_Text redTeamNameText;
        [SerializeField] private TMP_Text blueTeamNameText;
        [SerializeField] private TMP_Text redScoreText;
        [SerializeField] private TMP_Text blueScoreText;

        [Header("Next Team")]
        [SerializeField] private TMP_Text nextTeamNameText;
        [SerializeField] private Image nextTeamBackground;
        [SerializeField] private Color redTeamColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private Color blueTeamColor = new Color(0.35f, 0.5f, 1f);

        [Header("Info Panel")]
        [SerializeField] private TMP_Text infoRoundText;
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private Button previousInfoButton;
        [SerializeField] private Button nextInfoButton;

        [Header("Buttons")]
        [SerializeField] private Button goButton;
        [SerializeField] private Button finishButton;

        private GameFlowController flowController;
        private GameRuntimeSettings settings;
        private TurnManager turnManager;
        private ScoreManager scoreManager;
        private TurnHistoryManager historyManager;

        private int selectedInfoRound = 1;

        public void Bind(
            GameFlowController flow,
            GameRuntimeSettings runtimeSettings,
            TurnManager turn,
            ScoreManager score,
            TurnHistoryManager history)
        {
            flowController = flow;
            settings = runtimeSettings;
            turnManager = turn;
            scoreManager = score;
            historyManager = history;

            if (goButton != null)
            {
                goButton.onClick.RemoveAllListeners();
                goButton.onClick.AddListener(flowController.OnGoPressed);
            }

            if (finishButton != null)
            {
                finishButton.onClick.RemoveAllListeners();
                finishButton.onClick.AddListener(flowController.OnFinishPressed);
            }

            if (previousInfoButton != null)
            {
                previousInfoButton.onClick.RemoveAllListeners();
                previousInfoButton.onClick.AddListener(() => ChangeInfoRound(-1));
            }

            if (nextInfoButton != null)
            {
                nextInfoButton.onClick.RemoveAllListeners();
                nextInfoButton.onClick.AddListener(() => ChangeInfoRound(1));
            }
        }

        public void RefreshAll(bool isGameOver, bool jumpToLatestRound)
        {
            if (settings == null || turnManager == null || scoreManager == null)
                return;

            if (roundValueText != null)
                roundValueText.text = turnManager.CurrentRound.ToString();

            if (targetScoreValueText != null)
                targetScoreValueText.text = settings.targetScore.ToString();

            if (redTeamNameText != null)
                redTeamNameText.text = settings.redTeamName;

            if (blueTeamNameText != null)
                blueTeamNameText.text = settings.blueTeamName;

            if (redScoreText != null)
                redScoreText.text = scoreManager.RedScore.ToString();

            if (blueScoreText != null)
                blueScoreText.text = scoreManager.BlueScore.ToString();

            RefreshNextTeam(isGameOver);

            if (goButton != null)
                goButton.gameObject.SetActive(!isGameOver);

            if (jumpToLatestRound)
                selectedInfoRound = Mathf.Max(1, historyManager != null ? historyManager.MaxRound : 1);

            ClampSelectedInfoRound();
            RefreshInfoPanel();
        }

        public IEnumerator PlayOnRoutine()
        {
            if (animator != null)
            {
                animator.ResetTrigger(offTriggerName);
                animator.SetTrigger(onTriggerName);
            }

            yield return new WaitForSeconds(animationWait);
        }

        public IEnumerator PlayOffRoutine()
        {
            if (animator != null)
            {
                animator.ResetTrigger(onTriggerName);
                animator.SetTrigger(offTriggerName);
            }

            yield return new WaitForSeconds(animationWait);
        }

        private void RefreshNextTeam(bool isGameOver)
        {
            TeamId team = turnManager.CurrentTeam;

            if (nextTeamNameText != null)
            {
                nextTeamNameText.text = isGameOver
                    ? "Oyun Bitti"
                    : turnManager.GetCurrentTeamName(settings);
            }

            if (nextTeamBackground != null)
                nextTeamBackground.color = team == TeamId.Red ? redTeamColor : blueTeamColor;
        }

        private void ChangeInfoRound(int direction)
        {
            selectedInfoRound += direction;
            ClampSelectedInfoRound();
            RefreshInfoPanel();
        }

        private void ClampSelectedInfoRound()
        {
            int maxRound = historyManager != null ? Mathf.Max(1, historyManager.MaxRound) : 1;
            selectedInfoRound = Mathf.Clamp(selectedInfoRound, 1, maxRound);
        }

        private void RefreshInfoPanel()
        {
            if (infoRoundText != null)
                infoRoundText.text = selectedInfoRound.ToString();

            if (infoText == null)
                return;

            if (historyManager == null || historyManager.RecordCount == 0)
            {
                infoText.text = "Henüz oynanan kelime yok.";
                return;
            }

            List<TurnRecord> records = historyManager.GetRecordsForRound(selectedInfoRound);

            if (records.Count == 0)
            {
                infoText.text = "Bu turda kayıt yok.";
                return;
            }

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < records.Count; i++)
            {
                TurnRecord turn = records[i];

                builder.AppendLine(turn.teamName);
                builder.AppendLine("----------------");

                if (turn.words.Count == 0)
                {
                    builder.AppendLine("Kelime oynanmadı.");
                }
                else
                {
                    for (int j = 0; j < turn.words.Count; j++)
                    {
                        WordActionRecord word = turn.words[j];

                        builder.Append("- ");
                        builder.Append(word.word);
                        builder.Append(" : ");
                        builder.Append(GetActionText(word.action));

                        if (word.scoreDelta != 0)
                        {
                            builder.Append(" (");
                            builder.Append(word.scoreDelta > 0 ? "+" : "");
                            builder.Append(word.scoreDelta);
                            builder.Append(")");
                        }

                        builder.AppendLine();
                    }
                }

                if (i < records.Count - 1)
                    builder.AppendLine();
            }

            infoText.text = builder.ToString();
        }

        private string GetActionText(WordAction action)
        {
            switch (action)
            {
                case WordAction.Correct:
                    return "Doğru";

                case WordAction.Tabu:
                    return "Tabu";

                case WordAction.Pass:
                    return "Pas";

                default:
                    return "-";
            }
        }
    }
}*/

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabuOyunu.Gameplay
{
    public sealed class CurtainUIController : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator animator;
        [SerializeField] private string onTriggerName = "on";
        [SerializeField] private string offTriggerName = "off";
        [SerializeField] private float animationWait = 0.35f;

        [Header("Top Info")]
        [SerializeField] private TMP_Text roundValueText;
        [SerializeField] private TMP_Text targetScoreValueText;

        [Header("Teams")]
        [SerializeField] private TMP_Text redTeamNameText;
        [SerializeField] private TMP_Text blueTeamNameText;
        [SerializeField] private TMP_Text redScoreText;
        [SerializeField] private TMP_Text blueScoreText;

        [Header("Next Team")]
        [SerializeField] private TMP_Text nextTeamNameText;
        [SerializeField] private Image nextTeamBackground;
        [SerializeField] private Color redTeamColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private Color blueTeamColor = new Color(0.35f, 0.5f, 1f);

        [Header("Buttons")]
        [SerializeField] private Button goButton;
        [SerializeField] private Button finishButton;

        private GameFlowController flowController;
        private GameRuntimeSettings settings;
        private TurnManager turnManager;
        private ScoreManager scoreManager;

        public void Bind(
            GameFlowController flow,
            GameRuntimeSettings runtimeSettings,
            TurnManager turn,
            ScoreManager score)
        {
            flowController = flow;
            settings = runtimeSettings;
            turnManager = turn;
            scoreManager = score;

            if (goButton != null)
            {
                goButton.onClick.RemoveAllListeners();
                goButton.onClick.AddListener(flowController.OnGoPressed);
            }

            if (finishButton != null)
            {
                finishButton.onClick.RemoveAllListeners();
                finishButton.onClick.AddListener(flowController.OnFinishPressed);
            }
        }

        public void RefreshAll(bool isGameOver)
        {
            if (settings == null || turnManager == null || scoreManager == null)
                return;

            if (roundValueText != null)
                roundValueText.text = turnManager.CurrentRound.ToString();

            if (targetScoreValueText != null)
                targetScoreValueText.text = settings.targetScore.ToString();

            if (redTeamNameText != null)
                redTeamNameText.text = settings.redTeamName;

            if (blueTeamNameText != null)
                blueTeamNameText.text = settings.blueTeamName;

            SetScoreTexts(scoreManager.RedScore, scoreManager.BlueScore);

            RefreshNextTeam(isGameOver);

            if (goButton != null)
                goButton.gameObject.SetActive(!isGameOver);
        }

        public void SetScoreTexts(int redScore, int blueScore)
        {
            if (redScoreText != null)
                redScoreText.text = redScore.ToString();

            if (blueScoreText != null)
                blueScoreText.text = blueScore.ToString();
        }

        public IEnumerator PlayOnRoutine()
        {
            if (animator != null)
            {
                animator.ResetTrigger(offTriggerName);
                animator.SetTrigger(onTriggerName);
            }

            yield return new WaitForSeconds(animationWait);
        }

        public IEnumerator PlayOffRoutine()
        {
            if (animator != null)
            {
                animator.ResetTrigger(onTriggerName);
                animator.SetTrigger(offTriggerName);
            }

            yield return new WaitForSeconds(animationWait);
        }

        private void RefreshNextTeam(bool isGameOver)
        {
            TeamId team = turnManager.CurrentTeam;

            if (nextTeamNameText != null)
            {
                nextTeamNameText.text = isGameOver
                    ? "Oyun Bitti"
                    : turnManager.GetCurrentTeamName(settings);
            }

            if (nextTeamBackground != null)
                nextTeamBackground.color = team == TeamId.Red ? redTeamColor : blueTeamColor;
        }
    }
}