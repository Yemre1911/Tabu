using System.Collections;
using TMPro;
using UnityEngine;

public class CurtainRoundStatsAnimator : MonoBehaviour
{
    [System.Serializable]
    public class StatBox
    {
        public GameObject root;
        public RectTransform rectTransform;
        public TMP_Text numberText;

        [HideInInspector] public Vector3 defaultScale = Vector3.one;

        public void Cache()
        {
            if (rectTransform == null && root != null)
                rectTransform = root.transform as RectTransform;

            if (rectTransform != null)
                defaultScale = rectTransform.localScale;
        }

        public void Hide()
        {
            if (numberText != null)
                numberText.text = "0";

            if (rectTransform != null)
                rectTransform.localScale = defaultScale;

            if (root != null)
                root.SetActive(false);
        }

        public void ShowWithStartScale(float startScaleMultiplier)
        {
            if (root != null)
                root.SetActive(true);

            if (numberText != null)
                numberText.text = "0";

            if (rectTransform != null)
                rectTransform.localScale = defaultScale * startScaleMultiplier;
        }
    }

    [Header("Main Root")]
    [SerializeField] private GameObject lastRoundStatsRoot;

    [Header("Stat Boxes")]
    [SerializeField] private StatBox correctBox;
    [SerializeField] private StatBox tabooBox;
    [SerializeField] private StatBox totalBox;

    [Header("Score Texts")]
    [SerializeField] private TMP_Text redScoreText;
    [SerializeField] private TMP_Text blueScoreText;

    [Header("Animation")]
    [SerializeField] private float startScaleMultiplier = 1.55f;
    [SerializeField] private float boxScaleDuration = 0.18f;
    [SerializeField] private float numberCountDuration = 0.35f;
    [SerializeField] private float delayBetweenBoxes = 0.08f;
    [SerializeField] private float delayBeforeScoreCount = 0.15f;
    [SerializeField] private float scoreCountDuration = 0.45f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        CacheAll();
    }

    private void CacheAll()
    {
        correctBox.Cache();
        tabooBox.Cache();
        totalBox.Cache();
    }

    public void PrepareForFirstRound(int redScore, int blueScore)
    {
        StopCurrentRoutine();

        CacheAll();

        SetScoreTexts(redScore, blueScore);

        if (lastRoundStatsRoot != null)
            lastRoundStatsRoot.SetActive(false);

        HideAllBoxes();
    }

    public void SetScoresImmediate(int redScore, int blueScore)
    {
        SetScoreTexts(redScore, blueScore);
    }

    public void PlayRoundResult(
        RoundResultData result,
        int redScoreBefore,
        int blueScoreBefore,
        int redScoreAfter,
        int blueScoreAfter)
    {
        StopCurrentRoutine();

        currentRoutine = StartCoroutine(PlayRoundResultRoutine(
            result,
            redScoreBefore,
            blueScoreBefore,
            redScoreAfter,
            blueScoreAfter
        ));
    }

    public IEnumerator PlayRoundResultRoutine(
        RoundResultData result,
        int redScoreBefore,
        int blueScoreBefore,
        int redScoreAfter,
        int blueScoreAfter)
    {
        CacheAll();

        SetScoreTexts(redScoreBefore, blueScoreBefore);

        if (lastRoundStatsRoot != null)
            lastRoundStatsRoot.SetActive(true);

        HideAllBoxes();

        yield return PlayOneBox(correctBox, result.correctCount);

        yield return new WaitForSeconds(delayBetweenBoxes);

        yield return PlayOneBox(tabooBox, result.tabooCount);

        yield return new WaitForSeconds(delayBetweenBoxes);

        yield return PlayOneBox(totalBox, result.TotalScore);

        yield return new WaitForSeconds(delayBeforeScoreCount);

        if (result.playingTeam == TeamSide.Red)
        {
            yield return CountText(redScoreText, redScoreBefore, redScoreAfter, scoreCountDuration);
            SetScoreTexts(redScoreAfter, blueScoreAfter);
        }
        else
        {
            yield return CountText(blueScoreText, blueScoreBefore, blueScoreAfter, scoreCountDuration);
            SetScoreTexts(redScoreAfter, blueScoreAfter);
        }

        currentRoutine = null;
    }

    private IEnumerator PlayOneBox(StatBox box, int targetValue)
    {
        box.ShowWithStartScale(startScaleMultiplier);

        float elapsed = 0f;

        Vector3 startScale = box.defaultScale * startScaleMultiplier;
        Vector3 endScale = box.defaultScale;

        while (elapsed < boxScaleDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / boxScaleDuration);
            float eased = EaseOutCubic(t);

            if (box.rectTransform != null)
                box.rectTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);

            yield return null;
        }

        if (box.rectTransform != null)
            box.rectTransform.localScale = endScale;

        yield return CountText(box.numberText, 0, targetValue, numberCountDuration);
    }

    private IEnumerator CountText(TMP_Text text, int from, int to, float duration)
    {
        if (text == null)
            yield break;

        if (duration <= 0f || from == to)
        {
            text.text = to.ToString();
            yield break;
        }

        float elapsed = 0f;
        int lastValue = int.MinValue;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutCubic(t);

            int currentValue = Mathf.RoundToInt(Mathf.Lerp(from, to, eased));

            if (currentValue != lastValue)
            {
                text.text = currentValue.ToString();
                lastValue = currentValue;
            }

            yield return null;
        }

        text.text = to.ToString();
    }

    private void SetScoreTexts(int redScore, int blueScore)
    {
        if (redScoreText != null)
            redScoreText.text = redScore.ToString();

        if (blueScoreText != null)
            blueScoreText.text = blueScore.ToString();
    }

    private void HideAllBoxes()
    {
        correctBox.Hide();
        tabooBox.Hide();
        totalBox.Hide();
    }

    private void StopCurrentRoutine()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}