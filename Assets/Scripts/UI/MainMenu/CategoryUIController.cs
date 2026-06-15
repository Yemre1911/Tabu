using System.Collections.Generic;
using TabuOyunu.Data;
using TMPro;
using UnityEngine;

namespace TabuOyunu.UI.MainMenu
{
    public class CategoryUIController : MonoBehaviour
    {
        [Header("JSON")]
        [SerializeField] private TextAsset categoriesJson;

        [Header("Scroll Content")]
        [SerializeField] private Transform contentParent;

        [Header("Template Button")]
        [SerializeField] private CategoryButtonUI categoryButtonTemplate;

        [Header("Main Menu Selected Category Text")]
        [SerializeField] private TMP_Text selectedCategorySmallText;

        private readonly List<CategoryButtonUI> spawnedButtons = new();

        private CategorySelection selectedCategory;

        public CategorySelection SelectedCategory => selectedCategory;

        private void Awake()
        {
            LoadAndCreateCategoryButtons();
        }

        private void LoadAndCreateCategoryButtons()
        {
            CategoryDatabase database = LoadCategoryDatabase();

            if (database == null || database.categories == null || database.categories.Count == 0)
            {
                Debug.LogError($"{nameof(CategoryUIController)}: Hiç kategori bulunamadı.");
                CreateFallbackCategory();
                return;
            }

            ClearOldButtons();

            CategoryData defaultCategory = FindDefaultCategory(database.categories);

            foreach (CategoryData category in database.categories)
            {
                CreateCategoryButton(category);
            }

            SelectCategory(defaultCategory.id, defaultCategory.displayName);
        }

        private CategoryDatabase LoadCategoryDatabase()
        {
            if (categoriesJson == null)
            {
                Debug.LogError($"{nameof(CategoryUIController)}: categoriesJson Inspector'dan atanmadı.");
                return null;
            }

            return JsonUtility.FromJson<CategoryDatabase>(categoriesJson.text);
        }

        private CategoryData FindDefaultCategory(List<CategoryData> categories)
        {
            foreach (CategoryData category in categories)
            {
                if (category.isDefault)
                    return category;
            }

            return categories[0];
        }

        private void CreateCategoryButton(CategoryData category)
        {
            if (contentParent == null)
            {
                Debug.LogError($"{nameof(CategoryUIController)}: Content parent atanmadı.");
                return;
            }

            if (categoryButtonTemplate == null)
            {
                Debug.LogError($"{nameof(CategoryUIController)}: Category button template atanmadı.");
                return;
            }

            CategoryButtonUI newButton = Instantiate(categoryButtonTemplate, contentParent);
            newButton.gameObject.SetActive(true);

            newButton.Initialize(
                category.id,
                category.displayName,
                this
            );

            spawnedButtons.Add(newButton);
        }

        private void ClearOldButtons()
        {
            spawnedButtons.Clear();

            if (categoryButtonTemplate != null)
                categoryButtonTemplate.gameObject.SetActive(false);

            if (contentParent == null)
                return;

            for (int i = contentParent.childCount - 1; i >= 0; i--)
            {
                Transform child = contentParent.GetChild(i);

                if (categoryButtonTemplate != null && child == categoryButtonTemplate.transform)
                    continue;

                Destroy(child.gameObject);
            }
        }

        public void SelectCategory(string categoryId, string categoryDisplayName)
        {
            selectedCategory = new CategorySelection(categoryId, categoryDisplayName);

            UpdateSelectedButtonVisuals();
            UpdateSelectedCategoryText();

            Debug.Log($"Kategori seçildi: {selectedCategory.DisplayName} ({selectedCategory.Id})");
        }

        private void UpdateSelectedButtonVisuals()
        {
            foreach (CategoryButtonUI button in spawnedButtons)
            {
                bool isSelected = button.CategoryId == selectedCategory.Id;
                button.SetSelected(isSelected);
            }
        }

        private void UpdateSelectedCategoryText()
        {
            if (selectedCategorySmallText != null)
                selectedCategorySmallText.text = selectedCategory.DisplayName;
        }

        private void CreateFallbackCategory()
        {
            selectedCategory = new CategorySelection("classic", "Klasik");

            if (selectedCategorySmallText != null)
                selectedCategorySmallText.text = selectedCategory.DisplayName;
        }
    }
}