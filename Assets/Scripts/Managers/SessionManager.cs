using TabuOyunu.Data;
using UnityEngine;

namespace TabuOyunu.Managers
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        public GameSettings CurrentSettings { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetSettings(GameSettings settings)
        {
            CurrentSettings = settings;
        }

        public bool HasSettings()
        {
            return CurrentSettings != null;
        }
    }
}