using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;
    private GameObject previousMenu;

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
   public void ShowSettings(GameObject currentMenu)
{
    previousMenu = currentMenu; 
    currentMenu.SetActive(false); 
    settingsMenu.SetActive(true);   
    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
}


}
