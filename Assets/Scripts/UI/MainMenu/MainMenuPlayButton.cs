using UnityEngine;

public class MainMenuPlayButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SceneLoader sceneLoader;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    public void Play()
    {
        if (sceneLoader == null)
        {
            Debug.LogError("SceneLoader reference is missing.");
            return;
        }

        sceneLoader.LoadScene(gameSceneName);
    }
}