using System;

namespace TabuOyunu.Data
{
    [Serializable]
    public class GameSettings
    {
        public string CategoryId;
        public string CategoryDisplayName;

        public string GameModeId;
        public string GameModeDisplayName;

        public int RoundTimeSeconds;
        public int TabooPenalty;
        public int PassCount;
        public int TargetScore;

        public string RedTeamName;
        public string BlueTeamName;

        public GameSettings(
            string categoryId,
            string categoryDisplayName,
            string gameModeId,
            string gameModeDisplayName,
            int roundTimeSeconds,
            int tabooPenalty,
            int passCount,
            int targetScore,
            string redTeamName,
            string blueTeamName)
        {
            CategoryId = categoryId;
            CategoryDisplayName = categoryDisplayName;

            GameModeId = gameModeId;
            GameModeDisplayName = gameModeDisplayName;

            RoundTimeSeconds = roundTimeSeconds;
            TabooPenalty = tabooPenalty;
            PassCount = passCount;
            TargetScore = targetScore;

            RedTeamName = redTeamName;
            BlueTeamName = blueTeamName;
        }
    }
}