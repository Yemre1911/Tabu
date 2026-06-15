using System;

namespace TabuOyunu.Data
{
    [Serializable]
    public class CategorySelection
    {
        public string Id;
        public string DisplayName;

        public CategorySelection(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }
    }
}