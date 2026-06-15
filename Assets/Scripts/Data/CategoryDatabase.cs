using System;
using System.Collections.Generic;

namespace TabuOyunu.Data
{
    [Serializable]
    public class CategoryDatabase
    {
        public List<CategoryData> categories = new();
    }
}