using UnityEngine;

public class MainMenuBackButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIScreenSlider screenSlider;

    public void GoBackToMainMenu()
    {
        if (screenSlider == null)
        {
            Debug.LogError("UIScreenSlider reference is missing.");
            return;
        }

        screenSlider.ShowMainMenu();
    }
}