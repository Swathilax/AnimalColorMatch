using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("[GameSceneManager] PlayGame() called. Loading 'Gameplay' scene...");
        SceneManager.LoadScene("Gameplay");
    }

    public void GoHome()
    {
        SceneManager.LoadScene("HomeScreen");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}