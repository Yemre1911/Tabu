using System.Collections;
using TabuOyunu.UI.Common;
using UnityEngine;

namespace TabuOyunu.UI.Common
{
    public class LoadingFadeAutoCloser : MonoBehaviour
    {
        [SerializeField] private LoadingFadeController loadingFade;
        [SerializeField] private float delayBeforeClose = 0.1f;

        private IEnumerator Start()
        {
            if (loadingFade == null)
                loadingFade = GetComponent<LoadingFadeController>();

            if (loadingFade == null)
            {
                Debug.LogError($"{nameof(LoadingFadeAutoCloser)}: LoadingFadeController reference is missing.");
                yield break;
            }

            if (delayBeforeClose > 0f)
                yield return new WaitForSecondsRealtime(delayBeforeClose);

            yield return loadingFade.CloseRoutine();
        }
    }
}