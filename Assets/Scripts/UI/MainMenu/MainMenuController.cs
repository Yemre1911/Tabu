using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private UIPanel gameSettingsPanel;
    [SerializeField] private UIPanel generalSettingsPanel;

    private void Start()
    {
        CloseAllPopups();
    }

    public void OpenGameSettings()
    {
        CloseAllPopups();

        if (gameSettingsPanel != null)
            gameSettingsPanel.Show();
    }

    public void OpenGeneralSettings()
    {
        CloseAllPopups();

        if (generalSettingsPanel != null)
            generalSettingsPanel.Show();
    }

    public void CloseAllPopups()
    {
        if (gameSettingsPanel != null)
            gameSettingsPanel.Hide();

        if (generalSettingsPanel != null)
            generalSettingsPanel.Hide();
    }
}