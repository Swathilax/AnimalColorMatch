using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    [Header("Loading Panel (Optional)")]
    public GameObject loadingPanel;
    public loadingbar loadingBarController;

    private void Awake()
    {
        AutoWireLoading();
    }

    private void Start()
    {
        AutoWireLoading();
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    public void AutoWireLoading()
    {
        if (loadingBarController == null)
        {
            loadingBarController = FindFirstObjectByType<loadingbar>(FindObjectsInactive.Include);
        }

        if (loadingPanel == null)
        {
            if (loadingBarController != null && loadingBarController.transform.parent != null)
            {
                loadingPanel = loadingBarController.transform.parent.gameObject;
            }

            if (loadingPanel == null)
            {
                GameObject found = GameObject.Find("loading3");
                if (found == null) found = GameObject.Find("LoadingPanel");
                if (found == null) found = GameObject.Find("Loading Panel");
                loadingPanel = found;
            }
        }
    }

    public void PlayGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;

        AutoWireLoading();

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        if (loadingBarController != null)
        {
            loadingBarController.StartLoading("Gameplay");
        }
        else
        {
            loadingbar.Load("Gameplay");
        }
    }

    public void GoHome()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        SceneManager.LoadScene("HomeScreen");
    }

    public void RestartLevel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Retry()
    {
        RestartLevel();
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        Application.Quit();
    }
}