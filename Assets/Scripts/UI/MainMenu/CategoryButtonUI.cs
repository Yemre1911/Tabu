using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabuOyunu.UI.MainMenu
{
    public class CategoryButtonUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text categoryNameText;
        [SerializeField] private Image selectedImage;

        private string categoryId;
        private string categoryDisplayName;
        private CategoryUIController owner;

        public string CategoryId => categoryId;

        public void Initialize(
            string id,
            string displayName,
            CategoryUIController controller)
        {
            categoryId = id;
            categoryDisplayName = displayName;
            owner = controller;

            if (categoryNameText != null)
                categoryNameText.text = categoryDisplayName;

            SetSelected(false);

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            if (owner == null)
            {
                Debug.LogError($"{nameof(CategoryButtonUI)}: Owner CategoryUIController bulunamadı.");
                return;
            }

            owner.SelectCategory(categoryId, categoryDisplayName);
        }

        public void SetSelected(bool isSelected)
        {
            if (selectedImage != null)
                selectedImage.gameObject.SetActive(isSelected);
        }
    }
}