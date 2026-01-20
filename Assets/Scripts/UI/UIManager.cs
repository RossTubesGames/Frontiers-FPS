using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
[SerializeField] private GameObject gameOverUI;
public void ShowGameOverScreen()
    {
        gameOverUI.SetActive(true);
    }
public void RestartGame()
    {
        Scene currentScene=SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
