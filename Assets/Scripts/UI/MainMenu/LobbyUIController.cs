using System.Collections.Generic;
using TabuOyunu.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabuOyunu.UI.MainMenu
{
    public class LobbyUIController : MonoBehaviour
    {
        [Header("Team Name Inputs")]
        [SerializeField] private TMP_InputField redTeamInput;
        [SerializeField] private TMP_InputField blueTeamInput;

        [Header("Game Mode UI")]
        [SerializeField] private Button previousModeButton;
        [SerializeField] private Button nextModeButton;
        [SerializeField] private TMP_Text selectedModeText;

        private readonly List<GameModeDefinition> gameModes = new()
        {
            new GameModeDefinition("classic", "Klasik"),
            new GameModeDefinition("fast", "Hızlı"),
            new GameModeDefinition("hard", "Zor")
        };

        private int selectedModeIndex;

        public string RedTeamName => GetInputOrDefault(redTeamInput, "A takımı");
        public string BlueTeamName => GetInputOrDefault(blueTeamInput, "B takımı");

        public GameModeDefinition SelectedGameMode => gameModes[selectedModeIndex];

        private void Awake()
        {
            SetupDefaultTeamNames();

            if (previousModeButton != null)
                previousModeButton.onClick.AddListener(SelectPreviousMode);

            if (nextModeButton != null)
                nextModeButton.onClick.AddListener(SelectNextMode);

            selectedModeIndex = 0;
            UpdateSelectedModeText();
        }

        private void SetupDefaultTeamNames()
        {
            if (redTeamInput != null)
                redTeamInput.text = "A takımı";

            if (blueTeamInput != null)
                blueTeamInput.text = "B takımı";
        }

        private string GetInputOrDefault(TMP_InputField input, string defaultValue)
        {
            if (input == null)
                return defaultValue;

            string value = input.text.Trim();

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return value;
        }

        private void SelectPreviousMode()
        {
            selectedModeIndex--;

            if (selectedModeIndex < 0)
                selectedModeIndex = gameModes.Count - 1;

            UpdateSelectedModeText();
        }

        private void SelectNextMode()
        {
            selectedModeIndex++;

            if (selectedModeIndex >= gameModes.Count)
                selectedModeIndex = 0;

            UpdateSelectedModeText();
        }

        private void UpdateSelectedModeText()
        {
            if (selectedModeText != null)
                selectedModeText.text = SelectedGameMode.DisplayName;
        }
    }
}