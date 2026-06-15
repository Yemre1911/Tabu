using UnityEngine;

public class MainMenuPanelButton : MonoBehaviour
{
    [Header("Target Panel")]
    [SerializeField] private UIPanel targetPanel;

    [Header("Behaviour")]
    [SerializeField] private bool toggleInsteadOfShow = false;

    public void Execute()
    {
        if (targetPanel == null)
        {
            Debug.LogError("Target panel reference is missing.");
            return;
        }

        if (toggleInsteadOfShow)
            targetPanel.Toggle();
        else
            targetPanel.Show();
    }
}