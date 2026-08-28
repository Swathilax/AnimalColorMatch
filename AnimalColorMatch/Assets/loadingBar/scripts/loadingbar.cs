using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class loadingbar : MonoBehaviour
{
    public static loadingbar Instance { get; private set; }

    public string targetScene = "Gameplay";
    public bool autoStartOnStart = false;

    public Image imageComp;
    public Image secondaryImageComp;
    public Image[] extraFillImages;

    public TMP_Text progressText;
    public TMP_Text loadingStatusText;

    public float fillSpeed = 0.8f;
    public float minLoadingDuration = 1.5f;

    private RectTransform rectComponent;
    private AsyncOperation asyncOperation;
    private float currentProgress = 0.0f;
    private bool isCurrentlyLoading = false;

    private void Awake()
    {
        Instance = this;
        rectComponent = GetComponent<RectTransform>();

        AutoWireElements();
    }

    private void Start()
    {
        AutoWireElements();
        UpdateAllFills(0.0f);
        UpdateProgressText(0.0f);
        UpdateLoadingStatusText("Loading...");

        if (autoStartOnStart)
        {
            StartLoading(targetScene);
        }
    }

    public void AutoWireElements()
    {
        if (imageComp == null && rectComponent != null)
        {
            Image[] allImages = GetComponentsInChildren<Image>(true);
            foreach (Image img in allImages)
            {
                if (img != null && (img.type == Image.Type.Filled || img.gameObject != gameObject))
                {
                    if (imageComp == null)
                    {
                        imageComp = img;
                    }
                    else if (secondaryImageComp == null && img != imageComp)
                    {
                        secondaryImageComp = img;
                    }
                }
            }

            if (imageComp == null)
            {
                imageComp = GetComponent<Image>();
            }
        }

        if (progressText == null)
        {
            TMP_Text[] allTmp = GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text t in allTmp)
            {
                if (t != null && t != loadingStatusText)
                {
                    progressText = t;
                    break;
                }
            }
        }
    }

    public void StartLoading(string sceneName = "Gameplay")
    {
        if (isCurrentlyLoading) return;

        targetScene = string.IsNullOrEmpty(sceneName) ? "Gameplay" : sceneName;

        UpdateAllFills(0.0f);
        UpdateProgressText(0.0f);
        UpdateLoadingStatusText("Loading...");

        gameObject.SetActive(true);
        if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(true);
        }

        StartCoroutine(LoadSceneAsyncRoutine());
    }

    public static void Load(string sceneName)
    {
        if (Instance != null)
        {
            Instance.StartLoading(sceneName);
        }
        else
        {
            if (Application.CanStreamedLevelBeLoaded("Loading Scene"))
            {
                SceneManager.LoadScene("Loading Scene");
            }
            else
            {
                SceneManager.LoadScene(string.IsNullOrEmpty(sceneName) ? "Gameplay" : sceneName);
            }
        }
    }

    private void UpdateAllFills(float fillValue)
    {
        if (imageComp != null) imageComp.fillAmount = fillValue;
        if (secondaryImageComp != null) secondaryImageComp.fillAmount = fillValue;

        if (extraFillImages != null)
        {
            foreach (Image img in extraFillImages)
            {
                if (img != null) img.fillAmount = fillValue;
            }
        }
    }

    private void UpdateProgressText(float fillValue)
    {
        if (progressText != null)
        {
            int percent = Mathf.RoundToInt(Mathf.Clamp01(fillValue) * 100f);
            progressText.text = percent + "%";
        }
    }

    private void UpdateLoadingStatusText(string statusStr)
    {
        if (loadingStatusText != null)
        {
            loadingStatusText.text = statusStr;
        }
    }

    private IEnumerator LoadSceneAsyncRoutine()
    {
        isCurrentlyLoading = true;
        currentProgress = 0.0f;

        string sceneToLoad = string.IsNullOrEmpty(targetScene) ? "Gameplay" : targetScene;

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            sceneToLoad = "Gameplay";
        }

        asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        if (asyncOperation == null)
        {
            isCurrentlyLoading = false;
            yield break;
        }

        asyncOperation.allowSceneActivation = false;

        float elapsedTime = 0f;
        float dotTimer = 0f;
        int dotCount = 0;

        while (!asyncOperation.isDone)
        {
            elapsedTime += Time.deltaTime;
            dotTimer += Time.deltaTime;

            if (dotTimer >= 0.3f)
            {
                dotTimer = 0f;
                dotCount = (dotCount + 1) % 4;
                string dots = new string('.', dotCount);
                UpdateLoadingStatusText("Loading" + dots);
            }

            float rawProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            float timeRatio = Mathf.Clamp01(elapsedTime / minLoadingDuration);
            float targetProgress = Mathf.Min(rawProgress, timeRatio);

            if (asyncOperation.progress >= 0.9f && elapsedTime >= minLoadingDuration)
            {
                targetProgress = 1.0f;
            }

            float currentSpeed = fillSpeed > 0 ? fillSpeed : 0.8f;
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * currentSpeed);

            UpdateAllFills(currentProgress);
            UpdateProgressText(currentProgress);

            if (currentProgress >= 0.999f && asyncOperation.progress >= 0.9f && elapsedTime >= minLoadingDuration)
            {
                UpdateAllFills(1.0f);
                UpdateProgressText(1.0f);

                yield return new WaitForSeconds(0.15f);
                asyncOperation.allowSceneActivation = true;
                yield break;
            }

            yield return null;
        }

        isCurrentlyLoading = false;
    }
}
