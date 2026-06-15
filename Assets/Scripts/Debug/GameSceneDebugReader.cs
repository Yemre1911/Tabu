using TabuOyunu.Managers;
using UnityEngine;

namespace TabuOyunu.Debugging
{
    public class GameSceneDebugReader : MonoBehaviour
    {
        private void Start()
        {
            if (SessionManager.Instance == null)
            {
                Debug.LogError($"{nameof(GameSceneDebugReader)}: SessionManager was not found.");
                return;
            }

            if (!SessionManager.Instance.HasSettings())
            {
                Debug.LogError($"{nameof(GameSceneDebugReader)}: No GameSettings found.");
                return;
            }

            var settings = SessionManager.Instance.CurrentSettings;

            Debug.Log("===== GAME SETTINGS DEBUG =====");

            Debug.Log($"Category Id: {settings.CategoryId}");
            Debug.Log($"Category Name: {settings.CategoryDisplayName}");

            Debug.Log($"Game Mode Id: {settings.GameModeId}");
            Debug.Log($"Game Mode Name: {settings.GameModeDisplayName}");

            Debug.Log($"Round Time: {settings.RoundTimeSeconds}");
            Debug.Log($"Taboo Penalty: {settings.TabooPenalty}");
            Debug.Log($"Pass Count: {settings.PassCount}");
            Debug.Log($"Target Score: {settings.TargetScore}");

            Debug.Log($"Red Team Name: {settings.RedTeamName}");
            Debug.Log($"Blue Team Name: {settings.BlueTeamName}");

            Debug.Log("================================");
        }
    }
}