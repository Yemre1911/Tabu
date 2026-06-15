using UnityEngine;

public class MainMenuCategoryButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIScreenSlider screenSlider;

    public void OpenCategories()
    {
        if (screenSlider == null)
        {
            Debug.LogError("UIScreenSlider reference is missing.");
            return;
        }

        screenSlider.ShowCategories();
    }
}