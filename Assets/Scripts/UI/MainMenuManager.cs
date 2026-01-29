using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    [SerializeField]private SettingsMenu settingsMenu;
    void Start()
    {
        settingsMenu.SetStartVolume();
    }
    public void StartGame()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Button click");
        SceneManager.LoadScene("Scene 1 DEMO 2");
        PlayerHealth.isDead=false;
    }
    public void QuitToMenu()
    {
        SceneManager.LoadScene("MainMenuTodor");
        FMODUnity.RuntimeManager.PlayOneShot("event:/Button click");

    }
    public void QuitGame()
   {
        Application.Quit();
   }
      public void RollCredits()
    {
        SceneManager.LoadScene("EndSceneWin");
    }
}
