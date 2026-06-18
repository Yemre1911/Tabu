using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TabuOyunu.Gameplay
{
    public sealed class LowerToggleAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Button toggleButton;

        [Header("Objects")]
        [Tooltip("Açılırken aktif edilecek, kapanınca deaktif edilecek UI objeleri.")]
        [SerializeField] private GameObject[] toggleObjects;

        [Header("Animator Triggers")]
        [SerializeField] private string onTriggerName = "on";
        [SerializeField] private string offTriggerName = "off";

        [Header("Timing")]
        [SerializeField] private float openWait = 0.25f;
        [SerializeField] private float closeWait = 0.25f;

        public bool IsOpen { get; private set; }

        private bool isAnimating;
        private Coroutine currentRoutine;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(Toggle);
                toggleButton.onClick.AddListener(Toggle);
            }
        }

        private void OnDisable()
        {
            if (toggleButton != null)
                toggleButton.onClick.RemoveListener(Toggle);
        }

        public void Toggle()
        {
            if (isAnimating)
                return;

            if (IsOpen)
                Close();
            else
                Open();
        }

        public void Open()
        {
            StartSafeRoutine(OpenRoutine());
        }

        public void Close()
        {
            StartSafeRoutine(CloseRoutine());
        }

        public IEnumerator OpenRoutine()
        {
            if (IsOpen)
                yield break;

            isAnimating = true;

            SetToggleObjectsActive(true);

            if (animator != null)
            {
                animator.ResetTrigger(offTriggerName);
                animator.SetTrigger(onTriggerName);
            }

            yield return new WaitForSeconds(openWait);

            IsOpen = true;
            isAnimating = false;
        }

        public IEnumerator CloseRoutine()
        {
            if (!IsOpen && !isAnimating)
            {
                SetToggleObjectsActive(false);
                yield break;
            }

            isAnimating = true;

            SetToggleObjectsActive(true);

            if (animator != null)
            {
                animator.ResetTrigger(onTriggerName);
                animator.SetTrigger(offTriggerName);
            }

            yield return new WaitForSeconds(closeWait);

            SetToggleObjectsActive(false);

            IsOpen = false;
            isAnimating = false;
        }

        public void CloseInstant()
        {
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
                currentRoutine = null;
            }

            SetToggleObjectsActive(false);

            IsOpen = false;
            isAnimating = false;
        }

        private void StartSafeRoutine(IEnumerator routine)
        {
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);

            currentRoutine = StartCoroutine(RunRoutine(routine));
        }

        private IEnumerator RunRoutine(IEnumerator routine)
        {
            yield return routine;
            currentRoutine = null;
        }

        private void SetToggleObjectsActive(bool active)
        {
            if (toggleObjects == null)
                return;

            for (int i = 0; i < toggleObjects.Length; i++)
            {
                if (toggleObjects[i] != null)
                    toggleObjects[i].SetActive(active);
            }
        }
    }
}