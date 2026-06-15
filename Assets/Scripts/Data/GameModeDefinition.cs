using System;

namespace TabuOyunu.Data
{
    [Serializable]
    public class GameModeDefinition
    {
        public string Id;
        public string DisplayName;

        public GameModeDefinition(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }
    }
}