using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LobbyVerticalSlider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform sliderRoot;
    [SerializeField] private RectTransform mainArea;
    [SerializeField] private RectTransform lobbyPanel;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.35f;

    [Header("Input Lock")]
    [SerializeField] private Button[] buttonsToLock;

    [Header("Optional Canvas Groups")]
    [SerializeField] private CanvasGroup mainAreaCanvasGroup;
    [SerializeField] private CanvasGroup lobbyCanvasGroup;

    private float screenHeight;
    private bool isLobbyOpen;
    private bool isAnimating;
    private Coroutine slideRoutine;

    private void Awake()
    {
        if (sliderRoot == null)
            sliderRoot = transform as RectTransform;
    }

    private void Start()
    {
        SetupPanels();
        ShowMainInstant();
        ApplyCanvasGroupState(false);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!Application.isPlaying)
            return;

        SetupPanels();

        if (isLobbyOpen)
            sliderRoot.anchoredPosition = new Vector2(0f, -screenHeight);
        else
            sliderRoot.anchoredPosition = Vector2.zero;
    }

    private void SetupPanels()
    {
        if (sliderRoot == null)
        {
            Debug.LogError("Slider Root is missing.", this);
            return;
        }

        RectTransform parentRect = sliderRoot.parent as RectTransform;

        if (parentRect == null)
        {
            Debug.LogError("LobbyVerticalSlider parent must be a RectTransform.", this);
            return;
        }

        screenHeight = parentRect.rect.height;

        PrepareFullScreenPanel(mainArea);
        PrepareFullScreenPanel(lobbyPanel);

        if (mainArea != null)
            mainArea.anchoredPosition = Vector2.zero;

        if (lobbyPanel != null)
            lobbyPanel.anchoredPosition = new Vector2(0f, screenHeight);
    }

    private void PrepareFullScreenPanel(RectTransform panel)
    {
        if (panel == null)
            return;

        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
        panel.localScale = Vector3.one;
    }

    public void ShowLobby()
    {
        if (isAnimating || isLobbyOpen)
            return;

        SlideToLobby();
    }

    public void ShowMain()
    {
        if (isAnimating || !isLobbyOpen)
            return;

        SlideToMain();
    }

    private void SlideToLobby()
    {
        Vector2 targetPosition = new Vector2(0f, -screenHeight);
        StartSlide(targetPosition, true);
    }

    private void SlideToMain()
    {
        Vector2 targetPosition = Vector2.zero;
        StartSlide(targetPosition, false);
    }

    private void StartSlide(Vector2 targetPosition, bool targetLobbyOpen)
    {
        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        slideRoutine = StartCoroutine(SlideRoutine(targetPosition, targetLobbyOpen));
    }

    private IEnumerator SlideRoutine(Vector2 targetPosition, bool targetLobbyOpen)
    {
        isAnimating = true;
        SetButtonsInteractable(false);

        // Lobby açılırken lobby inputunu animasyon başında hazırla.
        if (targetLobbyOpen)
            SetCanvasGroupInteraction(lobbyCanvasGroup, true);

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
        isLobbyOpen = targetLobbyOpen;

        ApplyCanvasGroupState(isLobbyOpen);

        SetButtonsInteractable(true);
        isAnimating = false;
        slideRoutine = null;
    }

    private void ShowMainInstant()
    {
        if (sliderRoot != null)
            sliderRoot.anchoredPosition = Vector2.zero;

        isLobbyOpen = false;
    }

    private void SetButtonsInteractable(bool value)
    {
        if (buttonsToLock == null)
            return;

        for (int i = 0; i < buttonsToLock.Length; i++)
        {
            if (buttonsToLock[i] != null)
                buttonsToLock[i].interactable = value;
        }
    }

    private void ApplyCanvasGroupState(bool lobbyOpen)
    {
        SetCanvasGroupInteraction(mainAreaCanvasGroup, !lobbyOpen);
        SetCanvasGroupInteraction(lobbyCanvasGroup, lobbyOpen);
    }

    private void SetCanvasGroupInteraction(CanvasGroup canvasGroup, bool value)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }
}