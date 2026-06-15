using UnityEngine;

public class FrameRateController : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60;

    private static FrameRateController instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyFrameRate();
    }

    private void ApplyFrameRate()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}