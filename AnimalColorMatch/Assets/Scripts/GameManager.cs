using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Question Sprites")]
    public Sprite redBear;
    public Sprite blueBear;
    public Sprite yellowBear;
    public Sprite greenBear;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Question Image")]
    public Image questionImage;

    [Header("Game Settings")]
    public float levelDuration = 30f;
    public float questionDuration = 5f;
    public int totalQuestions = 10;
    public int startingLives = 3;

    [Header("Timer UI")]
    public TMP_Text timerText;

    [Header("Lives UI")]
    public TMP_Text livesText;

    [Header("Coins")]
    public int coinsPerCorrectAnswer = 10;
    public TMP_Text coinsText;

    [Header("Panels")]
    public GameObject levelCompletedPanel;
    public GameObject levelFailedPanel;
    public GameObject pausePanel;

    [Header("Answer Buttons")]
    public Button[] answerButtons;

    [Header("Pause & UI Controls")]
    public Button pauseButton;
    public Button resumeButton;
    public List<Button> restartButtons = new List<Button>();
    public List<Button> homeButtons = new List<Button>();
    public List<Button> nextLevelButtons = new List<Button>();

    private Sprite currentSprite;
    private string currentCorrectColor;

    private int correctAnswers;
    private int lives;
    private int currentQuestion;

    private int coins;

    private float levelTimer;
    private float questionTimer;

    private bool gameRunning;
    private bool isPaused;
    private bool questionAnswered;
    private Coroutine popCoroutine;

    private const string COINS_KEY = "PlayerCoins";


    private void Awake()
    {
        SetupButtonListeners();
        SetupPauseAndMenuButtons();
    }

    private void Start()
    {
        coins = PlayerPrefs.GetInt(COINS_KEY, 0);

        UpdateCoinsUI();

        StartLevel();
    }

    private void SetupPauseAndMenuButtons()
    {
        if (pausePanel == null)
        {
            GameObject pauseObj = GameObject.Find("Pause");
            if (pauseObj != null)
            {
                pausePanel = pauseObj;
            }
            else
            {
                Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
                foreach (Transform t in allTransforms)
                {
                    if (t.gameObject.name == "Pause" && t.GetComponent<RectTransform>() != null)
                    {
                        pausePanel = t.gameObject;
                        break;
                    }
                }
            }
        }

        if (levelCompletedPanel == null)
        {
            GameObject compObj = GameObject.Find("Level Completed");
            if (compObj != null) levelCompletedPanel = compObj;
        }

        if (levelFailedPanel == null)
        {
            GameObject failObj = GameObject.Find("LevelFailed");
            if (failObj != null) levelFailedPanel = failObj;
        }

        if (pauseButton == null)
        {
            GameObject pBtn = GameObject.Find("Pause Btn");
            if (pBtn != null)
                pauseButton = pBtn.GetComponent<Button>();
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(PauseGame);
            pauseButton.onClick.AddListener(PauseGame);
        }

        if (resumeButton == null && pausePanel != null)
        {
            Button[] panelButtons = pausePanel.GetComponentsInChildren<Button>(true);
            foreach (Button b in panelButtons)
            {
                if (b.gameObject.name.ToLower().Contains("resume"))
                {
                    resumeButton = b;
                    break;
                }
            }
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
            resumeButton.onClick.AddListener(ResumeGame);
        }

        AutoFindPanelButtons();
        RegisterButtonList(restartButtons, RetryLevel);
        RegisterButtonList(homeButtons, GoHome);
        RegisterButtonList(nextLevelButtons, NextLevel);
    }

    private void AutoFindPanelButtons()
    {
        GameObject[] panels = { pausePanel, levelCompletedPanel, levelFailedPanel };
        foreach (GameObject panel in panels)
        {
            if (panel == null) continue;
            Button[] btns = panel.GetComponentsInChildren<Button>(true);
            foreach (Button b in btns)
            {
                if (b == null) continue;
                string bName = b.gameObject.name.ToLower();
                if ((bName.Contains("restart") || bName.Contains("retry")) && !restartButtons.Contains(b))
                {
                    restartButtons.Add(b);
                }
                else if (bName.Contains("home") && !homeButtons.Contains(b))
                {
                    homeButtons.Add(b);
                }
                else if (bName.Contains("next") && !nextLevelButtons.Contains(b))
                {
                    nextLevelButtons.Add(b);
                }
            }
        }
    }

    private void RegisterButtonList(List<Button> buttons, UnityEngine.Events.UnityAction action)
    {
        if (buttons == null) return;
        foreach (Button btn in buttons)
        {
            if (btn != null)
            {
                btn.onClick.RemoveListener(action);
                btn.onClick.AddListener(action);
            }
        }
    }

    private void SetupButtonListeners()
    {
        if (answerButtons == null || answerButtons.Length == 0)
        {
            Button[] foundButtons = GetComponentsInChildren<Button>(true);
            if (foundButtons != null && foundButtons.Length > 0)
            {
                answerButtons = foundButtons;
            }
        }

        if (answerButtons != null)
        {
            foreach (Button btn in answerButtons)
            {
                if (btn == null) continue;

                string col = GetColorForButton(btn);
                if (!string.IsNullOrEmpty(col))
                {
                    btn.onClick.AddListener(() => CheckAnswer(col));
                }
            }
        }
    }

    private string GetColorForButton(Button btn)
    {
        ColorAnswerButton colorBtn = btn.GetComponent<ColorAnswerButton>();
        if (colorBtn != null && !string.IsNullOrEmpty(colorBtn.buttonColor))
            return NormalizeColor(colorBtn.buttonColor);

        string name = btn.gameObject.name.ToLower();
        if (name.Contains("red")) return "Red";
        if (name.Contains("blue")) return "Blue";
        if (name.Contains("yellow")) return "Yellow";
        if (name.Contains("green")) return "Green";

        return "";
    }

    public string NormalizeColor(string color)
    {
        if (string.IsNullOrEmpty(color)) return "";
        string c = color.Trim().ToLower();
        if (c.Contains("red")) return "Red";
        if (c.Contains("blue")) return "Blue";
        if (c.Contains("yellow")) return "Yellow";
        if (c.Contains("green")) return "Green";
        return color.Trim();
    }

    private void Update()
    {
        if (!gameRunning)
            return;

        levelTimer -= Time.deltaTime;

        // Update timer UI
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(levelTimer).ToString();
        }

        // Time is over
        if (levelTimer <= 0f)
        {
            levelTimer = 0f;

            if (timerText != null)
                timerText.text = "0";

            // If all questions are completed
            if (currentQuestion >= totalQuestions)
                LevelComplete();
            else
                LevelFailed();
        }
    }

    public void StartLevel()
    {
        StopAllCoroutines();
        popCoroutine = null;

        Time.timeScale = 1f;
        isPaused = false;

        correctAnswers = 0;
        lives = startingLives;

        correctAnswers = 0;
        lives = startingLives;
        currentQuestion = 0;

        UpdateCoinsUI();

        levelTimer = levelDuration;
        questionTimer = questionDuration;

        gameRunning = true;
        questionAnswered = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (levelCompletedPanel != null)
            levelCompletedPanel.SetActive(false);

        if (levelFailedPanel != null)
            levelFailedPanel.SetActive(false);

        if (questionImage != null)
        {
            questionImage.gameObject.SetActive(true);
            questionImage.transform.localScale = Vector3.zero;
        }

        SetAnswerButtons(true);
        UpdateLivesUI();

        StartCoroutine(QuestionLoop());
    }

    public void PauseGame()
    {
        if (!gameRunning || isPaused)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        SetAnswerButtons(false);

        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        if (!gameRunning || !isPaused)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        SetAnswerButtons(true);

        Debug.Log("Game Resumed");
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private IEnumerator QuestionLoop()
    {
        while (gameRunning)
        {
            questionAnswered = false;

            SpawnRandomQuestion();

            questionTimer = questionDuration;

            while (
                questionTimer > 0f &&
                !questionAnswered &&
                gameRunning
            )
            {
                if (!isPaused)
                    questionTimer -= Time.deltaTime;

                yield return null;
            }

            if (!gameRunning)
                yield break;

            if (!questionAnswered)
            {
                questionAnswered = true;

                Debug.Log("TIME UP");

                LoseLife();

                if (!gameRunning)
                    yield break;

                AnimatePopOut();
            }

            // Check if all questions are completed
            if (currentQuestion >= totalQuestions)
            {
                LevelComplete();
                yield break;
            }

            yield return new WaitForSeconds(1f);

            if (!gameRunning)
                yield break;
        }
    }


    private void SpawnRandomQuestion()
    {
        currentQuestion++;
        Sprite[] sprites =
        {
            redBear,
            blueBear,
            yellowBear,
            greenBear
        };

        int randomSpriteIndex = Random.Range(0, sprites.Length);
        currentSprite = sprites[randomSpriteIndex];

        switch (randomSpriteIndex)
        {
            case 0:
                currentCorrectColor = "Red";
                break;
            case 1:
                currentCorrectColor = "Blue";
                break;
            case 2:
                currentCorrectColor = "Yellow";
                break;
            case 3:
                currentCorrectColor = "Green";
                break;
        }

        if (spawnPoints == null || spawnPoints.Length == 0 || questionImage == null)
            return;

        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);

        questionImage.transform.position = spawnPoints[randomSpawnIndex].position;
        questionImage.sprite = currentSprite;
        questionImage.transform.localScale = Vector3.zero;

        AnimatePopIn();

        Debug.Log(
            "Spawned at: " +
            spawnPoints[randomSpawnIndex].name +
            " | Answer: " +
            currentCorrectColor
        );
    }

    private void AnimatePopIn()
    {
        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine = StartCoroutine(PopInQuestion());
    }

    private void AnimatePopOut()
    {
        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine = StartCoroutine(PopOutQuestion());
    }

    private IEnumerator PopInQuestion()
    {
        if (questionImage == null)
            yield break;

        questionImage.gameObject.SetActive(true);

        float duration = 0.3f;
        float timer = 0f;
        Vector3 targetScale = new Vector3(2f, 2f, 2f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            questionImage.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        questionImage.transform.localScale = targetScale;
    }

    private IEnumerator PopOutQuestion()
    {
        if (questionImage == null)
            yield break;

        float duration = 0.25f;
        float timer = 0f;
        Vector3 startScale = questionImage.transform.localScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            questionImage.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        questionImage.transform.localScale = Vector3.zero;
        questionImage.gameObject.SetActive(false);
    }

    public void CheckAnswer(string selectedColor)
    {
        if (!gameRunning)
            return;

        if (questionAnswered)
            return;

        questionAnswered = true;

        string normalizedSelected = NormalizeColor(selectedColor);
        string normalizedCorrect = NormalizeColor(currentCorrectColor);

        if (normalizedSelected == normalizedCorrect)
        {
            correctAnswers++;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrectAnswer();

            Debug.Log(
                "CORRECT! Selected: " +
                selectedColor +
                " | Correct Answers: " +
                correctAnswers
            );

            AnimatePopOut();

            if (selectedColor == currentCorrectColor)
            {
                correctAnswers++;

                coins += coinsPerCorrectAnswer;

                PlayerPrefs.SetInt(COINS_KEY, coins);
                PlayerPrefs.Save();

                UpdateCoinsUI();

                Debug.Log(
                    "CORRECT! " +
                    selectedColor +
                    " | Correct Answers: " +
                    correctAnswers +
                    " | Coins: " +
                    coins
                );
            }
        }
        else
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrongAnswer();

            Debug.Log(
                "WRONG! Selected: " +
                selectedColor +
                " | Correct: " +
                currentCorrectColor
            );

            LoseLife();
            AnimatePopOut();
        }
    }

    private void UpdateCoinsUI()
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    private void LoseLife()
    {
        if (!gameRunning)
            return;

        lives--;

        if (lives < 0)
            lives = 0;

        UpdateLivesUI();

        Debug.Log("Life Lost! Remaining Lives: " + lives);

        if (lives <= 0)
            LevelFailed();
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
            livesText.text = lives.ToString();
    }

    private void LevelComplete()
    {
        if (!gameRunning)
            return;

        gameRunning = false;

        StopAllCoroutines();

        SetAnswerButtons(false);

        if (questionImage != null)
            questionImage.gameObject.SetActive(false);

        if (levelCompletedPanel != null)
            levelCompletedPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLevelCompleted();

        Debug.Log(
            "LEVEL COMPLETE! Correct Answers: " +
            correctAnswers
        );
    }

    private void LevelFailed()
    {
        if (!gameRunning)
            return;

        gameRunning = false;

        StopAllCoroutines();

        SetAnswerButtons(false);

        if (questionImage != null)
            questionImage.gameObject.SetActive(false);

        if (levelFailedPanel != null)
            levelFailedPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLevelFailed();

        Debug.Log(
            "LEVEL FAILED! Correct Answers: " +
            correctAnswers
        );
    }

    private void SetAnswerButtons(bool state)
    {
        if (answerButtons == null)
            return;

        foreach (Button button in answerButtons)
        {
            if (button != null)
                button.interactable = state;
        }
    }

    public void RetryLevel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        StartLevel();
    }

    public void RestartScene()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        StartLevel();
    }

    public string GetCurrentCorrectColor()
    {
        return currentCorrectColor;
    }

    public void GoHome()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("HomeScreen");
    }

    public void RedButton()
    {
        CheckAnswer("Red");
    }

    public void BlueButton()
    {
        CheckAnswer("Blue");
    }

    public void YellowButton()
    {
        CheckAnswer("Yellow");
    }

    public void GreenButton()
    {
        CheckAnswer("Green");
    }
}