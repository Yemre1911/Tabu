using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace TabuOyunu.Gameplay
{
    public sealed class TimerController : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;

        public event Action OnTimeExpired;

        public int RemainingSeconds { get; private set; }
        public bool IsRunning { get; private set; }

        private Coroutine timerRoutine;

        public void StartTimer(int durationSeconds)
        {
            StopTimer();

            RemainingSeconds = Mathf.Max(0, durationSeconds);
            IsRunning = true;

            RefreshText();

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
        }

        private IEnumerator TimerRoutine()
        {
            while (RemainingSeconds > 0)
            {
                yield return new WaitForSeconds(1f);

                RemainingSeconds--;
                RefreshText();
            }

            IsRunning = false;
            timerRoutine = null;

            OnTimeExpired?.Invoke();
        }

        private void RefreshText()
        {
            if (timerText != null)
                timerText.text = RemainingSeconds.ToString();
        }
    }
}