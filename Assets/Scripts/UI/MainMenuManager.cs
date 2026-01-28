using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{

    public void StartGame()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Button click");
        SceneManager.LoadScene("Demo 1");
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
