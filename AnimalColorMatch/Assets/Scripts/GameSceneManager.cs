using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public void PlayGame()
    {
        Time.timeScale = 1f;
        Debug.Log("[GameSceneManager] PlayGame() called. Loading 'Gameplay' scene...");
        SceneManager.LoadScene("Gameplay");
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("HomeScreen");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}