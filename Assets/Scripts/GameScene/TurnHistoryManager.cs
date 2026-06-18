using System.Collections.Generic;
using UnityEngine;

namespace TabuOyunu.Gameplay
{
    public sealed class TurnHistoryManager : MonoBehaviour
    {
        private readonly List<TurnRecord> records = new List<TurnRecord>();

        private TurnRecord activeTurn;

        public int RecordCount => records.Count;

        public int MaxRound
        {
            get
            {
                int max = 0;

                for (int i = 0; i < records.Count; i++)
                {
                    if (records[i].roundNumber > max)
                        max = records[i].roundNumber;
                }

                return max;
            }
        }

        public void Clear()
        {
            records.Clear();
            activeTurn = null;
        }

        public void BeginTurn(int roundNumber, TeamId teamId, string teamName)
        {
            activeTurn = new TurnRecord
            {
                roundNumber = roundNumber,
                teamId = teamId,
                teamName = teamName
            };

            records.Add(activeTurn);
        }

        public void EndCurrentTurn()
        {
            activeTurn = null;
        }

        public void AddWordAction(string word, IEnumerable<string> forbiddenWords, WordAction action, int scoreDelta)
        {
            if (activeTurn == null)
                return;

            WordActionRecord record = new WordActionRecord
            {
                word = word,
                action = action,
                scoreDelta = scoreDelta
            };

            if (forbiddenWords != null)
                record.forbiddenWords.AddRange(forbiddenWords);

            activeTurn.words.Add(record);
        }

        public List<TurnRecord> GetRecordsForRound(int roundNumber)
        {
            List<TurnRecord> result = new List<TurnRecord>();

            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].roundNumber == roundNumber)
                    result.Add(records[i]);
            }

            return result;
        }
    }
}