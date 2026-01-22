using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

[SerializeField] private GameObject pauseMenuUI;

 private bool isPaused = false;
     void Start()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }
    public void ShowGameOverScreen()
    {
     SceneManager.LoadScene("GameOver");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }
    void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuUI.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

    }

    public void StartGame()
    {
        SceneManager.LoadScene("DemoBuild");
        FMODUnity.RuntimeManager.PlayOneShot("event:/Button click");
    }
    public void QuitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
        FMODUnity.RuntimeManager.PlayOneShot("event:/Button click");
    }
    public void QuitGame()
   {
        Application.Quit();
    }

}
