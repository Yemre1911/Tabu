using UnityEngine;

namespace TabuOyunu.Gameplay
{
    public sealed class ScoreManager : MonoBehaviour
    {
        [SerializeField] private bool clampScoreAtZero = true;

        public int RedScore { get; private set; }
        public int BlueScore { get; private set; }

        private int targetScore;
        private int tabuPenalty;

        public void Initialize(int targetScoreValue, int tabuPenaltyValue)
        {
            RedScore = 0;
            BlueScore = 0;

            targetScore = targetScoreValue;
            tabuPenalty = tabuPenaltyValue;
        }

        public int ApplyAction(TeamId teamId, WordAction action)
        {
            int delta = GetScoreDelta(action);

            if (teamId == TeamId.Red)
                RedScore = ApplyDelta(RedScore, delta);
            else
                BlueScore = ApplyDelta(BlueScore, delta);

            return delta;
        }

        public int GetScore(TeamId teamId)
        {
            return teamId == TeamId.Red ? RedScore : BlueScore;
        }

        public bool HasReachedTarget(TeamId teamId)
        {
            return GetScore(teamId) >= targetScore;
        }

        private int GetScoreDelta(WordAction action)
        {
            switch (action)
            {
                case WordAction.Correct:
                    return 1;

                case WordAction.Tabu:
                    return -tabuPenalty;

                case WordAction.Pass:
                    return 0;

                default:
                    return 0;
            }
        }

        private int ApplyDelta(int currentScore, int delta)
        {
            int result = currentScore + delta;
            return clampScoreAtZero ? Mathf.Max(0, result) : result;
        }
    }
}