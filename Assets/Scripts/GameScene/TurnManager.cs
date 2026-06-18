using UnityEngine;

namespace TabuOyunu.Gameplay
{
    public sealed class TurnManager : MonoBehaviour
    {
        public int CurrentRound { get; private set; }
        public TeamId CurrentTeam { get; private set; }

        public void Initialize()
        {
            CurrentRound = 1;
            CurrentTeam = TeamId.Red;
        }

        public void AdvanceTurn()
        {
            if (CurrentTeam == TeamId.Red)
            {
                CurrentTeam = TeamId.Blue;
            }
            else
            {
                CurrentTeam = TeamId.Red;
                CurrentRound++;
            }
        }

        public string GetCurrentTeamName(GameRuntimeSettings settings)
        {
            return GetTeamName(settings, CurrentTeam);
        }

        public string GetTeamName(GameRuntimeSettings settings, TeamId teamId)
        {
            if (settings == null)
                return teamId == TeamId.Red ? "Team Red" : "Team Blue";

            return teamId == TeamId.Red ? settings.redTeamName : settings.blueTeamName;
        }
    }
}