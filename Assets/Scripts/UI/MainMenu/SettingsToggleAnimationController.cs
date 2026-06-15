using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingsAnimatorToggleController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Trigger Names")]
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string closeTriggerName = "Close";

    [Header("Timing")]
    [SerializeField] private float animationLockDuration = 0.30f;

    [Header("Buttons To Lock")]
    [SerializeField] private Button[] buttonsToLock;

    [Header("Optional Panel Interaction")]
    [SerializeField] private CanvasGroup settingsPanelCanvasGroup;

    [Header("Start State")]
    [SerializeField] private bool startOpen = false;

    private bool isOpen;
    private bool isAnimating;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        isOpen = startOpen;

        SetSettingsPanelInteraction(isOpen);
    }

    public void Toggle()
    {
        if (isAnimating)
            return;

        if (isOpen)
            StartCoroutine(CloseRoutine());
        else
            StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        isAnimating = true;
        SetButtonsInteractable(false);

        SetSettingsPanelInteraction(true);

        animator.ResetTrigger(closeTriggerName);
        animator.SetTrigger(openTriggerName);

        yield return new WaitForSecondsRealtime(animationLockDuration);

        isOpen = true;
        SetSettingsPanelInteraction(true);

        SetButtonsInteractable(true);
        isAnimating = false;
    }

    private IEnumerator CloseRoutine()
    {
        isAnimating = true;
        SetButtonsInteractable(false);

        animator.ResetTrigger(openTriggerName);
        animator.SetTrigger(closeTriggerName);

        yield return new WaitForSecondsRealtime(animationLockDuration);

        isOpen = false;
        SetSettingsPanelInteraction(false);

        SetButtonsInteractable(true);
        isAnimating = false;
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

    private void SetSettingsPanelInteraction(bool value)
    {
        if (settingsPanelCanvasGroup == null)
            return;

        settingsPanelCanvasGroup.interactable = value;
        settingsPanelCanvasGroup.blocksRaycasts = value;
    }
}