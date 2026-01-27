using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class EventSceneLoader : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnBeforeLoad;

    // Reload the active scene
    public void ReloadCurrentScene()
    {
        OnBeforeLoad?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
