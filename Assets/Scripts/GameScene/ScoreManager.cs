using System;
using UnityEngine;

namespace TabuOyunu.Gameplay
{
    public sealed class ScoreManager : MonoBehaviour
    {
        [SerializeField] private bool clampScoreAtZero = true;

        public int RedScore { get; private set; }
        public int BlueScore { get; private set; }

        public int TargetScore { get; private set; }
        public int TabuPenalty { get; private set; }

        public event Action<TeamId, int> OnScoreChanged;

        public void Initialize(int targetScoreValue, int tabuPenaltyValue)
        {
            RedScore = 0;
            BlueScore = 0;

            TargetScore = targetScoreValue;
            TabuPenalty = tabuPenaltyValue;

            OnScoreChanged?.Invoke(TeamId.Red, RedScore);
            OnScoreChanged?.Invoke(TeamId.Blue, BlueScore);
        }

        public int ApplyAction(TeamId teamId, WordAction action)
        {
            int delta = GetScoreDelta(action);
            ApplyDeltaToTeam(teamId, delta);

            return delta;
        }

        public int CalculateRoundTotal(int correctCount, int tabooCount)
        {
            return correctCount - (tabooCount * TabuPenalty);
        }

        public int GetScore(TeamId teamId)
        {
            return teamId == TeamId.Red ? RedScore : BlueScore;
        }

        public bool HasReachedTarget(TeamId teamId)
        {
            return GetScore(teamId) >= TargetScore;
        }

        public int GetScoreDelta(WordAction action)
        {
            switch (action)
            {
                case WordAction.Correct:
                    return 1;

                case WordAction.Tabu:
                    return -TabuPenalty;

                case WordAction.Pass:
                    return 0;

                default:
                    return 0;
            }
        }

        private void ApplyDeltaToTeam(TeamId teamId, int delta)
        {
            if (delta == 0)
            {
                OnScoreChanged?.Invoke(teamId, GetScore(teamId));
                return;
            }

            if (teamId == TeamId.Red)
            {
                RedScore = ApplyDelta(RedScore, delta);
                OnScoreChanged?.Invoke(teamId, RedScore);
            }
            else
            {
                BlueScore = ApplyDelta(BlueScore, delta);
                OnScoreChanged?.Invoke(teamId, BlueScore);
            }
        }

        private int ApplyDelta(int currentScore, int delta)
        {
            int result = currentScore + delta;
            return clampScoreAtZero ? Mathf.Max(0, result) : result;
        }
    }
}