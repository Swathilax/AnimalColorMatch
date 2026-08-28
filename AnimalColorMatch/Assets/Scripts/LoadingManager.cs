using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public static string TargetScene = "Gameplay";

    [Header("UI Elements")]
    public Slider progressBar;
    public TMP_Text progressText;
    public TMP_Text loadingStatusText;

    [Header("Settings")]
    public float minLoadingDuration = 1.8f;
    public float progressSmoothingSpeed = 3.5f;

    private void Awake()
    {
        AutoWireElements();
    }

    private void Start()
    {
        AutoWireElements();
        StartCoroutine(LoadTargetSceneAsync());
    }

    public void AutoWireElements()
    {
        if (progressBar == null)
        {
            progressBar = FindFirstObjectByType<Slider>();
            if (progressBar == null)
            {
                GameObject barObj = GameObject.Find("ProgressBar");
                if (barObj != null)
                {
                    progressBar = barObj.GetComponent<Slider>();
                }
            }
        }

        if (progressText == null)
        {
            TMP_Text[] allTmp = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMP_Text t in allTmp)
            {
                if (t == null) continue;
                string tName = t.gameObject.name.ToLower();
                if (tName.Contains("percent") || tName.Contains("progress") || tName.Contains("count"))
                {
                    progressText = t;
                    break;
                }
            }
        }

        if (loadingStatusText == null)
        {
            TMP_Text[] allTmp = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMP_Text t in allTmp)
            {
                if (t == null || t == progressText) continue;
                string tName = t.gameObject.name.ToLower();
                if (tName.Contains("loading") || tName.Contains("status") || tName.Contains("tip"))
                {
                    loadingStatusText = t;
                    break;
                }
            }
        }
    }

    public static void LoadScene(string sceneName)
    {
        TargetScene = string.IsNullOrEmpty(sceneName) ? "Gameplay" : sceneName;

        if (Application.CanStreamedLevelBeLoaded("Loading Scene"))
        {
            SceneManager.LoadScene("Loading Scene");
        }
        else
        {
            SceneManager.LoadScene(TargetScene);
        }
    }

    private IEnumerator LoadTargetSceneAsync()
    {
        string sceneToLoad = string.IsNullOrEmpty(TargetScene) ? "Gameplay" : TargetScene;

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError("[LoadingManager] Scene '" + sceneToLoad + "' cannot be loaded! Falling back to Gameplay.");
            sceneToLoad = "Gameplay";
        }

        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;
        }

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneToLoad);
        if (asyncOp == null)
        {
            yield break;
        }

        asyncOp.allowSceneActivation = false;

        float visualProgress = 0f;
        float elapsedTime = 0f;

        while (!asyncOp.isDone)
        {
            elapsedTime += Time.deltaTime;

            float rawProgress = Mathf.Clamp01(asyncOp.progress / 0.9f);
            float timeRatio = Mathf.Clamp01(elapsedTime / minLoadingDuration);
            float targetProgress = Mathf.Min(rawProgress, timeRatio);

            if (asyncOp.progress >= 0.9f && elapsedTime >= minLoadingDuration)
            {
                targetProgress = 1f;
            }

            visualProgress = Mathf.MoveTowards(visualProgress, targetProgress, Time.deltaTime * progressSmoothingSpeed);

            if (progressBar != null)
            {
                progressBar.value = visualProgress;
            }

            if (progressText != null)
            {
                progressText.text = Mathf.RoundToInt(visualProgress * 100f) + "%";
            }

            if (visualProgress >= 0.999f && asyncOp.progress >= 0.9f && elapsedTime >= minLoadingDuration)
            {
                if (progressBar != null) progressBar.value = 1f;
                if (progressText != null) progressText.text = "100%";

                yield return new WaitForSeconds(0.2f);
                asyncOp.allowSceneActivation = true;
                yield break;
            }

            yield return null;
        }
    }
}
