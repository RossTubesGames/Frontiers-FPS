using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("Name of your main menu scene")]
    [SerializeField] private string menuSceneName = "MainMenu";

    [Header("Events (optional)")]
    public UnityEngine.Events.UnityEvent OnBeforeSceneLoad;

    private void Update()
    {
        // Restart current scene
        if (Input.GetKeyDown(KeyCode.P))
        {
            RestartScene();
        }

        // Go back to menu
        if (Input.GetKeyDown(KeyCode.M))
        {
            LoadMenuScene();
        }
    }

    // Reloads the current active scene
    public void RestartScene()
    {
        OnBeforeSceneLoad?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Loads the menu scene
    public void LoadMenuScene()
    {
        if (string.IsNullOrEmpty(menuSceneName))
        {
            Debug.LogWarning("Menu scene name not set!");
            return;
        }

        OnBeforeSceneLoad?.Invoke();
        SceneManager.LoadScene(menuSceneName);
    }
}
