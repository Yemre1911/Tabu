using UnityEngine;

public class UIPulseEffect : MonoBehaviour
{
    [Header("Pulse Amount")]
    [Tooltip("Objenin kendi orijinal boyutuna göre yüzde kaç büyüyüp küçüleceği. 0.2 = %20")]
    [SerializeField, Range(0f, 1f)] private float pulsePercent = 0.051f;

    [Header("Pulse Speed")]
    [SerializeField] private float speed = 6f;

    [Header("Options")]
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void Update()
    {
        float time = useUnscaledTime ? Time.unscaledTime : Time.time;

        float t = (Mathf.Sin(time * speed) + 1f) * 0.5f;

        // pulsePercent = 0.2 ise:
        // min = %80, max = %120
        float minMultiplier = 1f - pulsePercent;
        float maxMultiplier = 1f + pulsePercent;

        float scaleMultiplier = Mathf.Lerp(minMultiplier, maxMultiplier, t);

        transform.localScale = _originalScale * scaleMultiplier;
    }

    private void OnDisable()
    {
        transform.localScale = _originalScale;
    }
}