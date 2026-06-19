using System.Collections;
using UnityEngine;

public class CurtainRoundStatsTester : MonoBehaviour
{
    [SerializeField] private CurtainRoundStatsAnimator statsAnimator;

    [Header("Test Values")]
    [SerializeField] private TeamSide playingTeam = TeamSide.Red;
    [SerializeField] private int correctCount = 11;
    [SerializeField] private int tabooCount = 3;
    [SerializeField] private int tabooPenalty = 2;

    [SerializeField] private int redScoreBefore = 0;
    [SerializeField] private int blueScoreBefore = 0;

    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float delay = 1.5f;

    private IEnumerator Start()
    {
        if (statsAnimator == null)
        {
            Debug.LogError("CurtainRoundStatsTester: StatsAnimator referansı verilmemiş.");
            yield break;
        }

        statsAnimator.PrepareForFirstRound(redScoreBefore, blueScoreBefore);

        if (!playOnStart)
            yield break;

        yield return new WaitForSeconds(delay);

        RoundResultData result = new RoundResultData
        {
            playingTeam = playingTeam,
            correctCount = correctCount,
            tabooCount = tabooCount,
            tabooPenalty = tabooPenalty
        };

        int redScoreAfter = redScoreBefore;
        int blueScoreAfter = blueScoreBefore;

        if (playingTeam == TeamSide.Red)
            redScoreAfter += result.TotalScore;
        else
            blueScoreAfter += result.TotalScore;

        redScoreAfter = Mathf.Max(0, redScoreAfter);
        blueScoreAfter = Mathf.Max(0, blueScoreAfter);

        yield return statsAnimator.PlayRoundResultRoutine(
            result,
            redScoreBefore,
            blueScoreBefore,
            redScoreAfter,
            blueScoreAfter
        );
    }
}