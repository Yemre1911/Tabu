using System.Collections;
using UnityEngine;

public class UIScreenSlider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform sliderRoot;
    [SerializeField] private RectTransform mainMenuPanel;
    [SerializeField] private RectTransform categoriesPanel;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.35f;

    private float screenWidth;
    private Coroutine slideRoutine;

    private void Awake()
    {
        if (sliderRoot == null)
            sliderRoot = transform as RectTransform;
    }

    private void Start()
    {
        SetupPanels();
        ShowMainMenuInstant();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!Application.isPlaying)
            return;

        SetupPanels();
    }

    private void SetupPanels()
    {
        RectTransform parentRect = sliderRoot.parent as RectTransform;

        if (parentRect == null)
        {
            Debug.LogError("UIScreenSlider parent must be a RectTransform.");
            return;
        }

        screenWidth = parentRect.rect.width;

        if (mainMenuPanel != null)
            mainMenuPanel.anchoredPosition = Vector2.zero;

        if (categoriesPanel != null)
            categoriesPanel.anchoredPosition = new Vector2(screenWidth, 0f);
    }

    public void ShowMainMenu()
    {
        SlideTo(Vector2.zero);
    }

    public void ShowCategories()
    {
        SlideTo(new Vector2(-screenWidth, 0f));
    }

    private void ShowMainMenuInstant()
    {
        sliderRoot.anchoredPosition = Vector2.zero;
    }

    private void SlideTo(Vector2 targetPosition)
    {
        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        slideRoutine = StartCoroutine(SlideRoutine(targetPosition));
    }

    private IEnumerator SlideRoutine(Vector2 targetPosition)
    {
        Vector2 startPosition = sliderRoot.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / slideDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            sliderRoot.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        sliderRoot.anchoredPosition = targetPosition;
        slideRoutine = null;
    }
}