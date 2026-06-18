using System.Collections;
using UnityEngine;

namespace TabuOyunu.Gameplay
{
    public sealed class CurtainController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform curtainTop;
        [SerializeField] private RectTransform curtainBottom;

        [Header("Animation")]
        [SerializeField] private float duration = 0.45f;

        [Tooltip("Kapakların ekran dışına ne kadar gideceği. Yetmezse artır.")]
        [SerializeField] private float openDistance = 1600f;

        private Vector3 topClosedLocalPosition;
        private Vector3 bottomClosedLocalPosition;

        private void Awake()
        {
            SaveClosedPositions();
        }

        [ContextMenu("Save Closed Positions")]
        public void SaveClosedPositions()
        {
            if (curtainTop != null)
                topClosedLocalPosition = curtainTop.localPosition;

            if (curtainBottom != null)
                bottomClosedLocalPosition = curtainBottom.localPosition;
        }

        public void SetClosedInstant()
        {
            if (curtainTop != null)
                curtainTop.localPosition = topClosedLocalPosition;

            if (curtainBottom != null)
                curtainBottom.localPosition = bottomClosedLocalPosition;
        }

        public IEnumerator OpenRoutine()
        {
            yield return MoveRoutine(true);
        }

        public IEnumerator CloseRoutine()
        {
            yield return MoveRoutine(false);
        }

        private IEnumerator MoveRoutine(bool open)
        {
            if (curtainTop == null || curtainBottom == null)
                yield break;

            Vector3 topStart = curtainTop.localPosition;
            Vector3 bottomStart = curtainBottom.localPosition;

            Vector3 topTarget = open
                ? topClosedLocalPosition + Vector3.up * openDistance
                : topClosedLocalPosition;

            Vector3 bottomTarget = open
                ? bottomClosedLocalPosition + Vector3.down * openDistance
                : bottomClosedLocalPosition;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);

                curtainTop.localPosition = Vector3.LerpUnclamped(topStart, topTarget, t);
                curtainBottom.localPosition = Vector3.LerpUnclamped(bottomStart, bottomTarget, t);

                yield return null;
            }

            curtainTop.localPosition = topTarget;
            curtainBottom.localPosition = bottomTarget;
        }
    }
}