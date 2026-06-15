using UnityEngine;
using UnityEngine.EventSystems;

namespace TabuOyunu.UI.Common
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class DraggablePinchReturnUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("ON / OFF")]
        [SerializeField] private bool interactionEnabled = false;

        [Tooltip("True ise interactionEnabled false -> true olduğunda o anki pozisyon ve scale başlangıç olarak kaydedilir.")]
        [SerializeField] private bool captureHomeWhenEnabled = true;

        [Header("Drag")]
        [SerializeField] private bool allowDrag = true;
        [SerializeField] private bool bringToFrontOnTouch = true;

        [Header("Pinch Zoom")]
        [SerializeField] private bool allowPinchZoom = true;

        [Tooltip("Başlangıç scale'inin en az kaç katına küçülebilir.")]
        [SerializeField] private float minScaleMultiplier = 0.75f;

        [Tooltip("Başlangıç scale'inin en fazla kaç katına büyüyebilir.")]
        [SerializeField] private float maxScaleMultiplier = 1.35f;

        [Tooltip("İki parmakla büyütürken parmakların merkezi hareket ederse panel de hareket etsin mi?")]
        [SerializeField] private bool moveWithTwoFingerCenter = true;

        [Header("Return Motion")]
        [SerializeField] private bool returnWhenReleased = true;

        [Tooltip("Pozisyonun geri dönüş yumuşaklığı. Küçük değer daha hızlı döner.")]
        [SerializeField] private float positionReturnSmoothTime = 0.22f;

        [Tooltip("Scale'in geri dönüş yumuşaklığı. Küçük değer daha hızlı döner.")]
        [SerializeField] private float scaleReturnSmoothTime = 0.22f;

        [SerializeField] private float maxPositionReturnSpeed = 8000f;
        [SerializeField] private float maxScaleReturnSpeed = 20f;

        [Header("Snap")]
        [SerializeField] private float snapPositionDistance = 0.25f;
        [SerializeField] private float snapScaleDistance = 0.001f;

        private RectTransform rectTransform;
        private RectTransform parentRectTransform;
        private Canvas parentCanvas;

        private Vector2 homeAnchoredPosition;
        private Vector3 homeScale;
        private bool homeCaptured;

        private bool lastInteractionEnabled;

        private bool isDragging;
        private bool isPinching;

        private int activePointerId = int.MinValue;
        private Vector2 dragOffsetInParentSpace;

        private Vector2 positionVelocity;
        private Vector3 scaleVelocity;

        private float pinchStartDistance;
        private float pinchStartScaleMultiplier;
        private Vector2 pinchStartCenterLocal;
        private Vector2 pinchStartAnchoredPosition;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentRectTransform = rectTransform.parent as RectTransform;
            parentCanvas = GetComponentInParent<Canvas>();

            lastInteractionEnabled = interactionEnabled;

            if (interactionEnabled || !captureHomeWhenEnabled)
            {
                CaptureHome();
            }
        }

        private void Update()
        {
            CheckManualToggleChange();

            if (!interactionEnabled)
                return;

            if (!homeCaptured)
                CaptureHome();

            HandlePinchZoom();

            if (!IsUserInteracting() && returnWhenReleased)
            {
                ReturnToHome();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactionEnabled || !allowDrag)
                return;

            if (isPinching)
                return;

            if (Input.touchCount >= 2)
                return;

            if (!homeCaptured)
                CaptureHome();

            if (bringToFrontOnTouch)
                rectTransform.SetAsLastSibling();

            isDragging = true;
            activePointerId = eventData.pointerId;

            positionVelocity = Vector2.zero;
            scaleVelocity = Vector3.zero;

            Vector2 pointerLocalPosition;
            if (ScreenPointToParentLocalPoint(eventData.position, eventData.pressEventCamera, out pointerLocalPosition))
            {
                dragOffsetInParentSpace = rectTransform.anchoredPosition - pointerLocalPosition;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!interactionEnabled || !allowDrag)
                return;

            if (!isDragging || isPinching)
                return;

            if (eventData.pointerId != activePointerId)
                return;

            Vector2 pointerLocalPosition;
            if (ScreenPointToParentLocalPoint(eventData.position, eventData.pressEventCamera, out pointerLocalPosition))
            {
                rectTransform.anchoredPosition = pointerLocalPosition + dragOffsetInParentSpace;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
                return;

            isDragging = false;
            activePointerId = int.MinValue;
        }

        private void HandlePinchZoom()
        {
            if (!allowPinchZoom)
                return;

            if (Input.touchCount < 2)
            {
                isPinching = false;
                return;
            }

            Touch touchA = Input.GetTouch(0);
            Touch touchB = Input.GetTouch(1);

            if (touchA.phase == TouchPhase.Ended ||
                touchA.phase == TouchPhase.Canceled ||
                touchB.phase == TouchPhase.Ended ||
                touchB.phase == TouchPhase.Canceled)
            {
                isPinching = false;
                return;
            }

            Vector2 positionA = touchA.position;
            Vector2 positionB = touchB.position;

            float currentDistance = Vector2.Distance(positionA, positionB);

            if (currentDistance <= 1f)
                return;

            Vector2 currentCenterScreen = (positionA + positionB) * 0.5f;

            Vector2 currentCenterLocal;
            if (!ScreenPointToParentLocalPoint(currentCenterScreen, GetCanvasCamera(), out currentCenterLocal))
                return;

            if (!isPinching)
            {
                StartPinch(currentDistance, currentCenterLocal);
            }

            UpdatePinch(currentDistance, currentCenterLocal);
        }

        private void StartPinch(float startDistance, Vector2 startCenterLocal)
        {
            isPinching = true;
            isDragging = false;
            activePointerId = int.MinValue;

            if (bringToFrontOnTouch)
                rectTransform.SetAsLastSibling();

            positionVelocity = Vector2.zero;
            scaleVelocity = Vector3.zero;

            pinchStartDistance = startDistance;
            pinchStartScaleMultiplier = GetCurrentScaleMultiplier();
            pinchStartCenterLocal = startCenterLocal;
            pinchStartAnchoredPosition = rectTransform.anchoredPosition;
        }

        private void UpdatePinch(float currentDistance, Vector2 currentCenterLocal)
        {
            float pinchRatio = currentDistance / pinchStartDistance;

            float targetMultiplier = pinchStartScaleMultiplier * pinchRatio;
            targetMultiplier = Mathf.Clamp(targetMultiplier, minScaleMultiplier, maxScaleMultiplier);

            rectTransform.localScale = GetScaleFromMultiplier(targetMultiplier);

            if (moveWithTwoFingerCenter)
            {
                Vector2 centerDelta = currentCenterLocal - pinchStartCenterLocal;
                rectTransform.anchoredPosition = pinchStartAnchoredPosition + centerDelta;
            }
        }

        private void ReturnToHome()
        {
            rectTransform.anchoredPosition = Vector2.SmoothDamp(
                rectTransform.anchoredPosition,
                homeAnchoredPosition,
                ref positionVelocity,
                positionReturnSmoothTime,
                maxPositionReturnSpeed,
                Time.unscaledDeltaTime
            );

            rectTransform.localScale = Vector3.SmoothDamp(
                rectTransform.localScale,
                homeScale,
                ref scaleVelocity,
                scaleReturnSmoothTime,
                maxScaleReturnSpeed,
                Time.unscaledDeltaTime
            );

            bool positionCloseEnough =
                Vector2.Distance(rectTransform.anchoredPosition, homeAnchoredPosition) <= snapPositionDistance;

            bool scaleCloseEnough =
                Vector3.Distance(rectTransform.localScale, homeScale) <= snapScaleDistance;

            if (positionCloseEnough && scaleCloseEnough)
            {
                rectTransform.anchoredPosition = homeAnchoredPosition;
                rectTransform.localScale = homeScale;

                positionVelocity = Vector2.zero;
                scaleVelocity = Vector3.zero;
            }
        }

        private bool IsUserInteracting()
        {
            return isDragging || isPinching;
        }

        private void CheckManualToggleChange()
        {
            if (interactionEnabled == lastInteractionEnabled)
                return;

            if (interactionEnabled)
            {
                CancelInteraction();

                if (captureHomeWhenEnabled || !homeCaptured)
                    CaptureHome();
            }
            else
            {
                CancelInteraction();
            }

            lastInteractionEnabled = interactionEnabled;
        }

        public void CaptureHome()
        {
            homeAnchoredPosition = rectTransform.anchoredPosition;
            homeScale = rectTransform.localScale;
            homeCaptured = true;

            positionVelocity = Vector2.zero;
            scaleVelocity = Vector3.zero;
        }

        public void ResetToHomeInstant()
        {
            if (!homeCaptured)
                CaptureHome();

            rectTransform.anchoredPosition = homeAnchoredPosition;
            rectTransform.localScale = homeScale;

            positionVelocity = Vector2.zero;
            scaleVelocity = Vector3.zero;
        }

        public void SetInteractionEnabled(bool value)
        {
            interactionEnabled = value;

            if (interactionEnabled)
            {
                CancelInteraction();

                if (captureHomeWhenEnabled || !homeCaptured)
                    CaptureHome();
            }
            else
            {
                CancelInteraction();
            }

            lastInteractionEnabled = interactionEnabled;
        }

        public void EnableInteraction()
        {
            SetInteractionEnabled(true);
        }

        public void DisableInteraction()
        {
            SetInteractionEnabled(false);
        }

        private void CancelInteraction()
        {
            isDragging = false;
            isPinching = false;
            activePointerId = int.MinValue;
        }

        private float GetCurrentScaleMultiplier()
        {
            if (!homeCaptured)
                CaptureHome();

            if (Mathf.Approximately(homeScale.x, 0f))
                return 1f;

            return rectTransform.localScale.x / homeScale.x;
        }

        private Vector3 GetScaleFromMultiplier(float multiplier)
        {
            return new Vector3(
                homeScale.x * multiplier,
                homeScale.y * multiplier,
                homeScale.z
            );
        }

        private bool ScreenPointToParentLocalPoint(Vector2 screenPoint, Camera eventCamera, out Vector2 localPoint)
        {
            localPoint = Vector2.zero;

            if (parentRectTransform == null)
                return false;

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRectTransform,
                screenPoint,
                eventCamera,
                out localPoint
            );
        }

        private Camera GetCanvasCamera()
        {
            if (parentCanvas == null)
                return null;

            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;

            return parentCanvas.worldCamera;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            minScaleMultiplier = Mathf.Max(0.01f, minScaleMultiplier);
            maxScaleMultiplier = Mathf.Max(minScaleMultiplier, maxScaleMultiplier);

            positionReturnSmoothTime = Mathf.Max(0.01f, positionReturnSmoothTime);
            scaleReturnSmoothTime = Mathf.Max(0.01f, scaleReturnSmoothTime);

            maxPositionReturnSpeed = Mathf.Max(1f, maxPositionReturnSpeed);
            maxScaleReturnSpeed = Mathf.Max(0.1f, maxScaleReturnSpeed);

            snapPositionDistance = Mathf.Max(0f, snapPositionDistance);
            snapScaleDistance = Mathf.Max(0f, snapScaleDistance);
        }
#endif
    }
}