using System;
using System.Collections.Generic;
using UnityEngine;

namespace TabuOyunu.Gameplay
{
    public enum TeamId
    {
        Red,
        Blue
    }

    public enum WordAction
    {
        Correct,
        Tabu,
        Pass
    }

    public enum GameFlowState
    {
        Setup,
        CurtainIdle,
        CurtainOpening,
        Playing,
        CurtainClosing,
        GameOver,
        ReturningToMenu
    }

    [Serializable]
    public sealed class GameRuntimeSettings
    {
        [Header("Category")]
        public string categoryId = "classic";
        public string categoryDisplayName = "Klasik";

        [Header("Game Mode")]
        public string gameModeId = "classic";
        public string gameModeDisplayName = "Klasik Mod";

        [Header("Teams")]
        public string redTeamName = "Team Red";
        public string blueTeamName = "Team Blue";

        [Header("Rules")]
        public int roundSeconds = 90;
        public int tabuPenalty = 2;
        public int passCount = 3;
        public int targetScore = 50;
    }
}