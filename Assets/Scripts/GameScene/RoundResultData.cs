using System;

[Serializable]
public enum TeamSide
{
    Red,
    Blue
}

[Serializable]
public struct RoundResultData
{
    public TeamSide playingTeam;

    public int correctCount;
    public int tabooCount;
    public int tabooPenalty;

    public int TotalScore
    {
        get
        {
            return correctCount - (tabooCount * tabooPenalty);
        }
    }
}