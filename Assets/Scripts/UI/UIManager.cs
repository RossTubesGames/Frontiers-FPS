using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Collections;

public class UIManager : MonoBehaviour
{

[SerializeField] private GameObject pauseMenuUI;
[SerializeField]private GameObject tutorialBox;
[SerializeField] private GameObject settingsMenu;
[SerializeField] private GameObject otherMenu;
[SerializeField] private GameObject pauseFirstButton;
[SerializeField] private GameObject tutorialFirstButton;
[SerializeField] private GameObject settingsFirstButton;


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
    tutorialBox.SetActive(false);

    Time.timeScale = isPaused ? 0f : 1f;

    if (isPaused)
        SelectButton(pauseFirstButton);
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
        Debug.Log("quitting");
   }
public void ShowTutorials()
{
    tutorialBox.SetActive(true);
    pauseMenuUI.SetActive(false);
    Time.timeScale = 0f;
    isPaused = true;
    SelectButton(tutorialFirstButton);
}
public void ShowSettingsMenu()
    {
        settingsMenu.SetActive(true);
        
        SelectButton(settingsFirstButton);
        otherMenu.SetActive(false);
    }
public void HideSettingsMenu()
    {
        
    }
public void ExitTutorial()
{
    tutorialBox.SetActive(false);
    pauseMenuUI.SetActive(true);

    Time.timeScale = 0f;
    isPaused = true;

    SelectButton(pauseFirstButton);
}
private void SelectButton(GameObject button)
{
    EventSystem.current.SetSelectedGameObject(null);
    EventSystem.current.SetSelectedGameObject(button);
}
}
