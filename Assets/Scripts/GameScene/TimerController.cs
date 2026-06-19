using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabuOyunu.Gameplay
{
    public sealed class TimerController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private Image timerFillImage;

        [Header("Timer Colors")]
        [SerializeField] private Color fullColor = new Color(0.35f, 1f, 0.35f);
        [SerializeField] private Color middleColor = new Color(1f, 0.9f, 0.25f);
        [SerializeField] private Color lowColor = new Color(1f, 0.55f, 0.15f);
        [SerializeField] private Color criticalColor = new Color(1f, 0.15f, 0.15f);

        public event Action OnTimeExpired;

        public int RemainingSeconds { get; private set; }
        public float RemainingTime { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }

        public float Progress01
        {
            get
            {
                if (totalDuration <= 0f)
                    return 0f;

                return Mathf.Clamp01(RemainingTime / totalDuration);
            }
        }

        private float totalDuration;
        private Coroutine timerRoutine;

        public void StartTimer(int durationSeconds)
        {
            StopTimer();

            totalDuration = Mathf.Max(0, durationSeconds);
            RemainingTime = totalDuration;
            RemainingSeconds = Mathf.CeilToInt(RemainingTime);

            IsRunning = true;
            IsPaused = false;

            RefreshVisuals();

            timerRoutine = StartCoroutine(TimerRoutine());
        }

        public void StopTimer()
        {
            if (timerRoutine != null)
            {
                StopCoroutine(timerRoutine);
                timerRoutine = null;
            }

            IsRunning = false;
            IsPaused = false;
        }

        public void PauseTimer()
        {
            if (!IsRunning)
                return;

            IsPaused = true;
        }

        public void ResumeTimer()
        {
            if (!IsRunning)
                return;

            IsPaused = false;
        }
        public void AddTime(float seconds)
        {
            if (seconds <= 0f)
                return;

            RemainingTime += seconds;
            totalDuration += seconds;

            RefreshVisuals();
        }

        private IEnumerator TimerRoutine()
        {
            while (RemainingTime > 0f)
            {
                if (!IsPaused)
                {
                    RemainingTime -= Time.deltaTime;

                    if (RemainingTime < 0f)
                        RemainingTime = 0f;

                    RefreshVisuals();
                }

                yield return null;
            }

            IsRunning = false;
            IsPaused = false;
            timerRoutine = null;

            RefreshVisuals();

            OnTimeExpired?.Invoke();
        }

        private void RefreshVisuals()
        {
            RemainingSeconds = Mathf.CeilToInt(RemainingTime);

            if (timerText != null)
                timerText.text = RemainingSeconds.ToString();

            if (timerFillImage != null)
            {
                float progress = Progress01;
                timerFillImage.fillAmount = progress;
                timerFillImage.color = EvaluateTimerColor(progress);
            }
        }

        private Color EvaluateTimerColor(float progress)
        {
            if (progress >= 0.5f)
            {
                float t = Mathf.InverseLerp(0.5f, 1f, progress);
                return Color.Lerp(middleColor, fullColor, t);
            }

            if (progress >= 0.2f)
            {
                float t = Mathf.InverseLerp(0.2f, 0.5f, progress);
                return Color.Lerp(lowColor, middleColor, t);
            }

            {
                float t = Mathf.InverseLerp(0f, 0.2f, progress);
                return Color.Lerp(criticalColor, lowColor, t);
            }
        }
    }
}