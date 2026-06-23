using System;
using System.Collections.Generic;
using UnityEngine;

namespace TabuOyunu.Gameplay
{
    public sealed class WordProvider : MonoBehaviour
    {
        [Serializable]
        private sealed class WordDatabaseJson
        {
            public List<WordJson> words = new List<WordJson>();
        }

        [Serializable]
        private sealed class WordJson
        {
            public string word;
            public string mainWord;

            public string[] forbidden;
            public string[] forbiddenWords;
        }

        [Header("Resources Path")]
        [SerializeField] private string wordsFolderResourcesPath = "Data/Words";

        [Header("Debug")]
        [SerializeField] private bool logLoadedWords = true;

        private readonly List<GamePanelController.WordGroup> loadedWords = new List<GamePanelController.WordGroup>();
        private readonly List<int> drawBag = new List<int>();

        private string currentCategoryId;
        private int lastWordIndex = -1;

        public bool HasWords => loadedWords.Count > 0;
        public int LoadedWordCount => loadedWords.Count;
        public string CurrentCategoryId => currentCategoryId;

        public void Initialize(string categoryId)
        {
            currentCategoryId = string.IsNullOrWhiteSpace(categoryId)
                ? "classic"
                : categoryId;

            LoadWordsForCategory(currentCategoryId);
            RefillAndShuffleBag();
        }

        public GamePanelController.WordGroup GetNextWord()
        {
            if (loadedWords.Count == 0)
            {
                return CreateFallbackWord();
            }

            if (drawBag.Count == 0)
            {
                RefillAndShuffleBag();
            }

            if (drawBag.Count == 0)
            {
                return CreateFallbackWord();
            }

            int selectedIndex = drawBag[0];
            drawBag.RemoveAt(0);

            if (loadedWords.Count > 1 && selectedIndex == lastWordIndex && drawBag.Count > 0)
            {
                int replacementIndex = drawBag[0];
                drawBag[0] = selectedIndex;
                selectedIndex = replacementIndex;
            }

            lastWordIndex = selectedIndex;
            return loadedWords[selectedIndex];
        }

        private void LoadWordsForCategory(string categoryId)
        {
            loadedWords.Clear();
            drawBag.Clear();
            lastWordIndex = -1;

            string resourcePath = $"{wordsFolderResourcesPath}/{categoryId}";
            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);

            if (jsonAsset == null)
            {
                Debug.LogWarning(
                    $"{nameof(WordProvider)}: Kelime JSON bulunamadı. " +
                    $"Path: Resources/{resourcePath}.json"
                );
                return;
            }

            WordDatabaseJson database = ParseDatabase(jsonAsset.text);

            if (database == null || database.words == null || database.words.Count == 0)
            {
                Debug.LogWarning(
                    $"{nameof(WordProvider)}: JSON okundu ama kelime bulunamadı. " +
                    $"CategoryId: {categoryId}"
                );
                return;
            }

            for (int i = 0; i < database.words.Count; i++)
            {
                WordJson jsonWord = database.words[i];

                if (!TryConvertWord(jsonWord, out GamePanelController.WordGroup wordGroup))
                    continue;

                loadedWords.Add(wordGroup);
            }

            if (logLoadedWords)
            {
                Debug.Log(
                    $"{nameof(WordProvider)}: {loadedWords.Count} kelime yüklendi. " +
                    $"CategoryId: {categoryId}"
                );
            }
        }

        private WordDatabaseJson ParseDatabase(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            string trimmedJson = json.Trim();

            // JsonUtility normalde en dışta direkt array desteklemez.
            // Bu yüzden football.json şu formatta bile olsa destekliyoruz:
            // [
            //   { "word": "...", "forbidden": [...] }
            // ]
            if (trimmedJson.StartsWith("["))
            {
                trimmedJson = "{\"words\":" + trimmedJson + "}";
            }

            try
            {
                return JsonUtility.FromJson<WordDatabaseJson>(trimmedJson);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"{nameof(WordProvider)}: JSON parse edilirken hata oluştu.\n" +
                    exception.Message
                );

                return null;
            }
        }

        private bool TryConvertWord(WordJson jsonWord, out GamePanelController.WordGroup wordGroup)
        {
            wordGroup = null;

            if (jsonWord == null)
                return false;

            string mainWord = !string.IsNullOrWhiteSpace(jsonWord.word)
                ? jsonWord.word
                : jsonWord.mainWord;

            string[] forbiddenSource = jsonWord.forbidden != null && jsonWord.forbidden.Length > 0
                ? jsonWord.forbidden
                : jsonWord.forbiddenWords;

            if (string.IsNullOrWhiteSpace(mainWord))
                return false;

            string[] forbiddenWords = new string[5];

            for (int i = 0; i < forbiddenWords.Length; i++)
            {
                if (forbiddenSource != null && i < forbiddenSource.Length && !string.IsNullOrWhiteSpace(forbiddenSource[i]))
                    forbiddenWords[i] = forbiddenSource[i];
                else
                    forbiddenWords[i] = "-";
            }

            wordGroup = new GamePanelController.WordGroup
            {
                mainWord = mainWord,
                forbiddenWords = forbiddenWords
            };

            return true;
        }

        private void RefillAndShuffleBag()
        {
            drawBag.Clear();

            for (int i = 0; i < loadedWords.Count; i++)
            {
                drawBag.Add(i);
            }

            Shuffle(drawBag);

            if (drawBag.Count > 1 && drawBag[0] == lastWordIndex)
            {
                int temp = drawBag[0];
                drawBag[0] = drawBag[1];
                drawBag[1] = temp;
            }
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, list.Count);

                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        private GamePanelController.WordGroup CreateFallbackWord()
        {
            return new GamePanelController.WordGroup
            {
                mainWord = "KELİME",
                forbiddenWords = new[]
                {
                    "Yasak 1",
                    "Yasak 2",
                    "Yasak 3",
                    "Yasak 4",
                    "Yasak 5"
                }
            };
        }
    }
}