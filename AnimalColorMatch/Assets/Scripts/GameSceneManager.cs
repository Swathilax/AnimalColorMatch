using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public void PlayGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        Debug.Log("[GameSceneManager] PlayGame() called. Loading 'Gameplay' scene...");
        SceneManager.LoadScene("Gameplay");
    }

    public void GoHome()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        SceneManager.LoadScene("HomeScreen");
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        Application.Quit();
    }
}