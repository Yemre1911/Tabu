using System.Collections;
using UnityEngine;

namespace TabuOyunu.UI.Common
{
    public class LoadingFadeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Animasyonun bulunduğu obje. Sende bu obje: Background")]
        [SerializeField] private GameObject animatedRoot;

        [Tooltip("Animator component'i Background objesinde olmalı.")]
        [SerializeField] private Animator animator;

        [Header("Animator Triggers")]
        [SerializeField] private string openTriggerName = "Open";
        [SerializeField] private string closeTriggerName = "Close";

        [Header("Animator States")]
        [SerializeField] private string openStateName = "Open";
        [SerializeField] private string closeStateName = "Close";

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.35f;

        [Header("Initial State")]
        [SerializeField] private bool startOpened;

        private Coroutine currentRoutine;

        private int openTriggerHash;
        private int closeTriggerHash;
        private int openStateHash;
        private int closeStateHash;

        public bool IsOpen { get; private set; }
        public bool IsAnimating => currentRoutine != null;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (animatedRoot != null && animator == null)
                animator = animatedRoot.GetComponent<Animator>();

            if (animator != null)
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            openTriggerHash = Animator.StringToHash(openTriggerName);
            closeTriggerHash = Animator.StringToHash(closeTriggerName);
            openStateHash = Animator.StringToHash(openStateName);
            closeStateHash = Animator.StringToHash(closeStateName);

            ApplyInitialState();
        }

        private void ApplyInitialState()
        {
            if (animatedRoot == null)
                return;

            IsOpen = startOpened;

            if (startOpened)
            {
                animatedRoot.SetActive(true);
                SetCanvasInteraction(true);

                if (animator != null)
                {
                    animator.Play(openStateHash, 0, 1f);
                    animator.Update(0f);
                }
            }
            else
            {
                SetCanvasInteraction(false);
                animatedRoot.SetActive(false);
            }
        }

        public void Open()
        {
            PlayTransition(true);
        }

        public void Close()
        {
            PlayTransition(false);
        }

        public IEnumerator OpenRoutine()
        {
            yield return PlayRoutine(true);
        }

        public IEnumerator CloseRoutine()
        {
            yield return PlayRoutine(false);
        }

        private void PlayTransition(bool open)
        {
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);

            currentRoutine = StartCoroutine(PlayRoutine(open));
        }

        private IEnumerator PlayRoutine(bool open)
        {
            if (animatedRoot == null)
            {
                Debug.LogError($"{nameof(LoadingFadeController)}: Animated Root reference is missing.");
                yield break;
            }

            animatedRoot.SetActive(true);

            if (animator == null)
                animator = animatedRoot.GetComponent<Animator>();

            if (animator == null)
            {
                Debug.LogError($"{nameof(LoadingFadeController)}: Animator reference is missing.");
                yield break;
            }

            SetCanvasInteraction(true);

            animator.ResetTrigger(openTriggerHash);
            animator.ResetTrigger(closeTriggerHash);

            if (open)
            {
                animator.SetTrigger(openTriggerHash);
            }
            else
            {
                animator.SetTrigger(closeTriggerHash);
            }

            float timer = 0f;

            while (timer < animationDuration)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            IsOpen = open;
            currentRoutine = null;

            if (!open)
            {
                SetCanvasInteraction(false);
                animatedRoot.SetActive(false);
            }
        }

        private void SetCanvasInteraction(bool value)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.interactable = value;
            canvasGroup.blocksRaycasts = value;
        }
    }
}