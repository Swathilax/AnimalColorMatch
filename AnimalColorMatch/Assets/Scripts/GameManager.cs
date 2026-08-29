using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Question Sprites - Bear")]
    public Sprite redBear;
    public Sprite blueBear;
    public Sprite yellowBear;
    public Sprite greenBear;

    [Header("Question Sprites - Fox")]
    public Sprite redFox;
    public Sprite blueFox;
    public Sprite yellowFox;
    public Sprite greenFox;

    [Header("Question Sprites - Elephant")]
    public Sprite redElephant;
    public Sprite blueElephant;
    public Sprite yellowElephant;
    public Sprite greenElephant;

    [Header("Question Sprites - Tiger")]
    public Sprite redTiger;
    public Sprite blueTiger;
    public Sprite yellowTiger;
    public Sprite greenTiger;

    [Header("Question Sprites - Lion")]
    public Sprite redLion;
    public Sprite blueLion;
    public Sprite yellowLion;
    public Sprite greenLion;

    [Header("Danger Spirit Sprites")]
    public Sprite dangerBear;
    public Sprite dangerFox;
    public Sprite dangerElephant;
    public Sprite dangerTiger;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Question Image")]
    public Image questionImage;

    [Header("Game Settings")]
    public float levelDuration = 30f;
    public float questionDuration = 5f;
    public int totalQuestions = 10;
    public int startingLives = 3;

    [Header("Animal Spawning Balance")]
    [Tooltip("Minimum number of previous (danger) animal questions per level when previous animals exist")]
    public int minPreviousAnimalSpawns = 2;
    [Tooltip("Maximum number of previous (danger) animal questions per level when previous animals exist")]
    public int maxPreviousAnimalSpawns = 3;
    [Tooltip("Duration a previous/danger animal is displayed before disappearing automatically")]
    public float dangerAnimalDuration = 3f;
    [Tooltip("Transition delay between questions")]
    public float transitionDelay = 0.6f;

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

    [Header("Confetti Effect")]
    public ParticleSystem confettiParticleSystem;
    public ParticleSystem confettiParticleSystem2;
    public GameObject confettiPrefab;

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
    private string currentAnimalName = AnimalShopManager.ANIMAL_BEAR;
    private bool isCurrentAnimalTarget = true;

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
    private Dictionary<string, Sprite> _cachedSprites;
    private List<bool> _questionIsTargetPlan = new List<bool>();
    private Vector3 _currentSpawnPointPosition;

    private const string COINS_KEY = "PlayerCoins";


    private void Awake()
    {
        EnsureCanvasAndCameraSetup();
        SetupButtonListeners();
        SetupPauseAndMenuButtons();
        AutoFindConfetti();
        StopConfetti();
    }

    private void Start()
    {
        if (CoinManager.Instance != null)
        {
            coins = CoinManager.Instance.Coins;
            if (coinsText != null)
                CoinManager.Instance.RegisterCoinText(coinsText);
        }
        else
        {
            coins = PlayerPrefs.GetInt(COINS_KEY, 0);
        }

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

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(levelTimer).ToString();
        }

        if (levelTimer <= 0f)
        {
            levelTimer = 0f;

            if (timerText != null)
                timerText.text = "0";

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
        StopConfetti();

        Time.timeScale = 1f;
        isPaused = false;

        correctAnswers = 0;
        lives = startingLives;
        currentQuestion = 0;

        if (CoinManager.Instance != null)
            coins = CoinManager.Instance.Coins;
        else
            coins = PlayerPrefs.GetInt(COINS_KEY, 0);

        UpdateCoinsUI();

        if (levelDuration <= 0f)
            levelDuration = 30f;

        levelTimer = levelDuration;
        questionTimer = questionDuration;

        gameRunning = true;
        questionAnswered = false;

        GenerateQuestionPlan();

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

    private void GenerateQuestionPlan()
    {
        _questionIsTargetPlan.Clear();
        for (int i = 0; i < totalQuestions; i++)
        {
            _questionIsTargetPlan.Add(true);
        }

        List<string> previousAnimals = AnimalShopManager.GetPreviousUnlockedAnimals();
        if (previousAnimals == null || previousAnimals.Count == 0 || totalQuestions <= 2)
        {
            Debug.Log("[GameManager] Generated Question Plan: Active Target Animal only (" + AnimalShopManager.GetLatestUnlockedAnimal() + "), no previous danger spirits.");
            return;
        }

        int prevCount = previousAnimals.Count;
        int targetPreviousCount;
        if (prevCount == 1)
        {
            targetPreviousCount = 2;
        }
        else if (prevCount == 2)
        {
            targetPreviousCount = Random.Range(2, 4);
        }
        else
        {
            targetPreviousCount = Random.Range(3, 5);
        }

        targetPreviousCount = Mathf.Clamp(targetPreviousCount, 1, (totalQuestions - 2) / 2);

        // Candidates exclude first question (index 0) and last question (index totalQuestions - 1)
        List<int> candidateIndices = new List<int>();
        for (int i = 1; i < totalQuestions - 1; i++)
        {
            candidateIndices.Add(i);
        }

        // Shuffle candidate indices with Fisher-Yates
        for (int i = candidateIndices.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            int temp = candidateIndices[i];
            candidateIndices[i] = candidateIndices[r];
            candidateIndices[r] = temp;
        }

        // Pick slots ensuring no two danger spirit questions are consecutive
        List<int> selectedIndices = new List<int>();
        foreach (int candidate in candidateIndices)
        {
            if (selectedIndices.Count >= targetPreviousCount)
                break;

            bool adjacent = false;
            foreach (int sel in selectedIndices)
            {
                if (Mathf.Abs(sel - candidate) <= 1)
                {
                    adjacent = true;
                    break;
                }
            }

            if (!adjacent)
            {
                selectedIndices.Add(candidate);
            }
        }

        foreach (int idx in selectedIndices)
        {
            if (idx >= 0 && idx < _questionIsTargetPlan.Count)
            {
                _questionIsTargetPlan[idx] = false;
            }
        }

        Debug.Log("[GameManager] Generated Question Plan for " + totalQuestions + " questions. Active Target: " + AnimalShopManager.GetLatestUnlockedAnimal() + " | Danger Spirit slots: " + selectedIndices.Count + " (Indices: " + string.Join(", ", selectedIndices) + ")");
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

            questionTimer = isCurrentAnimalTarget ? questionDuration : dangerAnimalDuration;

            while (
                questionTimer > 0f &&
                !questionAnswered &&
                gameRunning
            )
            {
                if (!isPaused)
                {
                    questionTimer -= Time.deltaTime;
                }

                yield return null;
            }

            if (!gameRunning)
                yield break;

            if (!questionAnswered)
            {
                questionAnswered = true;

                if (isCurrentAnimalTarget)
                {
                    Debug.Log("TIME UP on Target Animal (" + currentAnimalName + ")! Life lost.");
                    LoseLife();
                }
                else
                {
                    Debug.Log("TIME UP on Danger Spirit (" + currentAnimalName + ")! Correctly avoided, no life lost.");
                }

                if (!gameRunning)
                    yield break;

                AnimatePopOut();
            }

            if (currentQuestion >= totalQuestions)
            {
                LevelComplete();
                yield break;
            }

            yield return new WaitForSeconds(transitionDelay);

            if (!gameRunning)
                yield break;
        }
    }


    private void SpawnRandomQuestion()
    {
        currentQuestion++;

        string latestAnimal = AnimalShopManager.GetLatestUnlockedAnimal();
        List<string> previousAnimals = AnimalShopManager.GetPreviousUnlockedAnimals();

        int planIndex = currentQuestion - 1;
        bool isTarget = true;

        if (planIndex >= 0 && planIndex < _questionIsTargetPlan.Count)
        {
            isTarget = _questionIsTargetPlan[planIndex];
        }
        else
        {
            isTarget = (previousAnimals == null || previousAnimals.Count == 0);
        }

        if (!isTarget && previousAnimals != null && previousAnimals.Count > 0)
        {
            // Danger Spirit Spawn (Traps)
            currentAnimalName = previousAnimals[Random.Range(0, previousAnimals.Count)];
            isCurrentAnimalTarget = false;
            currentCorrectColor = ""; // No matching answer for Danger Spirit!
            currentSprite = GetDangerSpiritSprite(currentAnimalName);
        }
        else
        {
            // Active Target Animal Spawn (Color Matching)
            currentAnimalName = latestAnimal;
            isCurrentAnimalTarget = true;
            string[] colors = { "Red", "Blue", "Yellow", "Green" };
            currentCorrectColor = colors[Random.Range(0, colors.Length)];
            currentSprite = GetAnimalSprite(currentAnimalName, currentCorrectColor);
        }

        if (spawnPoints == null || spawnPoints.Length == 0 || questionImage == null)
            return;

        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        _currentSpawnPointPosition = spawnPoints[randomSpawnIndex].position;
        questionImage.transform.position = _currentSpawnPointPosition;

        if (currentSprite != null)
        {
            questionImage.sprite = currentSprite;
        }
        questionImage.transform.localScale = Vector3.zero;

        AnimatePopIn();

        Debug.Log(
            "Spawned Q" + currentQuestion + "/" + totalQuestions +
            ": " + currentAnimalName +
            (isCurrentAnimalTarget ? (" | Color: " + currentCorrectColor) : " | DANGER SPIRIT (Do NOT tap!)") +
            " | Spawn: " + spawnPoints[randomSpawnIndex].name
        );
    }

    private Sprite GetAnimalSprite(string animalName, string color)
    {
        string animal = AnimalShopManager.NormalizeAnimalName(animalName);
        string c = NormalizeColor(color);

        if (animal == AnimalShopManager.ANIMAL_BEAR)
        {
            if (c == "Red") return redBear != null ? redBear : FindSpriteByName("Red Bear");
            if (c == "Blue") return blueBear != null ? blueBear : FindSpriteByName("Blue Bear");
            if (c == "Yellow") return yellowBear != null ? yellowBear : FindSpriteByName("Yellow Bear");
            if (c == "Green") return greenBear != null ? greenBear : (FindSpriteByName("Green Bear 1") ?? FindSpriteByName("Green Bear"));
        }
        else if (animal == AnimalShopManager.ANIMAL_FOX)
        {
            if (c == "Red") return redFox != null ? redFox : FindSpriteByName("Red Fox");
            if (c == "Blue") return blueFox != null ? blueFox : FindSpriteByName("Blue Fox");
            if (c == "Yellow") return yellowFox != null ? yellowFox : FindSpriteByName("Yellow Fox");
            if (c == "Green") return greenFox != null ? greenFox : FindSpriteByName("Green Fox");
        }
        else if (animal == AnimalShopManager.ANIMAL_ELEPHANT)
        {
            if (c == "Red") return redElephant != null ? redElephant : FindSpriteByName("Red Elephant");
            if (c == "Blue") return blueElephant != null ? blueElephant : FindSpriteByName("Blue Elephant");
            if (c == "Yellow") return yellowElephant != null ? yellowElephant : FindSpriteByName("Yellow Elephant");
            if (c == "Green") return greenElephant != null ? greenElephant : FindSpriteByName("Green Elephant");
        }
        else if (animal == AnimalShopManager.ANIMAL_TIGER)
        {
            if (c == "Red") return redTiger != null ? redTiger : FindSpriteByName("Red Tiger");
            if (c == "Blue") return blueTiger != null ? blueTiger : FindSpriteByName("Blue Tiger");
            if (c == "Yellow") return yellowTiger != null ? yellowTiger : FindSpriteByName("Yellow Tiger");
            if (c == "Green") return greenTiger != null ? greenTiger : FindSpriteByName("Green Tiger");
        }
        else if (animal == AnimalShopManager.ANIMAL_LION)
        {
            if (c == "Red") return redLion != null ? redLion : FindSpriteByName("Red Lion");
            if (c == "Blue") return blueLion != null ? blueLion : FindSpriteByName("Blue Lion");
            if (c == "Yellow") return yellowLion != null ? yellowLion : FindSpriteByName("Yellow Lion");
            if (c == "Green") return greenLion != null ? greenLion : FindSpriteByName("Green Lion");
        }

        return null;
    }

    private Sprite GetDangerSpiritSprite(string animalName)
    {
        string animal = AnimalShopManager.NormalizeAnimalName(animalName);
        if (animal == AnimalShopManager.ANIMAL_BEAR)
        {
            return dangerBear != null ? dangerBear : (FindSpriteByName("BrownBear") ?? FindSpriteByName("Brown Bear") ?? FindSpriteByName("Bear Profile"));
        }
        else if (animal == AnimalShopManager.ANIMAL_FOX)
        {
            return dangerFox != null ? dangerFox : (FindSpriteByName("Orange Danger Fox") ?? FindSpriteByName("Fox"));
        }
        else if (animal == AnimalShopManager.ANIMAL_ELEPHANT)
        {
            return dangerElephant != null ? dangerElephant : (FindSpriteByName("Grey Danger Elephant") ?? FindSpriteByName("Elephant"));
        }
        else if (animal == AnimalShopManager.ANIMAL_TIGER)
        {
            return dangerTiger != null ? dangerTiger : (FindSpriteByName("Orange Danger Tiger") ?? FindSpriteByName("Tiger"));
        }

        return null;
    }

    private Sprite FindSpriteByName(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
            return null;

        if (_cachedSprites == null)
        {
            _cachedSprites = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);
            Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (Sprite s in allSprites)
            {
                if (s != null && !_cachedSprites.ContainsKey(s.name))
                {
                    _cachedSprites[s.name] = s;
                }
            }
        }

        if (_cachedSprites.TryGetValue(spriteName, out Sprite found))
        {
            return found;
        }

        string cleanTarget = spriteName.Replace(" ", "").ToLower();
        foreach (var kvp in _cachedSprites)
        {
            if (kvp.Key.Replace(" ", "").ToLower().Equals(cleanTarget))
            {
                return kvp.Value;
            }
        }

        return null;
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
        {
            popCoroutine = null;
            yield break;
        }

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
        popCoroutine = null;
    }

    private IEnumerator PopOutQuestion()
    {
        if (questionImage == null)
        {
            popCoroutine = null;
            yield break;
        }

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
        popCoroutine = null;
    }

    public void CheckAnswer(string selectedColor)
    {
        if (!gameRunning)
            return;

        if (questionAnswered)
            return;

        questionAnswered = true;

        if (!isCurrentAnimalTarget)
        {
            Debug.Log("WRONG! Clicked button (" + selectedColor + ") on Danger Spirit (" + currentAnimalName + ")! Life lost.");

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrongAnswer();

            LoseLife();
            AnimatePopOut();
            return;
        }

        string normalizedSelected = NormalizeColor(selectedColor);
        string normalizedCorrect = NormalizeColor(currentCorrectColor);

        if (normalizedSelected == normalizedCorrect)
        {
            correctAnswers++;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrectAnswer();

            PlayConfetti(questionImage != null ? questionImage.transform.position : (Vector3?)null);

            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.AddCoins(coinsPerCorrectAnswer);
                coins = CoinManager.Instance.Coins;
            }
            else
            {
                coins += coinsPerCorrectAnswer;
                PlayerPrefs.SetInt(COINS_KEY, coins);
                PlayerPrefs.Save();
            }

            UpdateCoinsUI();

            Debug.Log(
                "CORRECT! Selected: " +
                selectedColor +
                " on " + currentAnimalName +
                " | Correct Answers: " +
                correctAnswers +
                " | Coins: " +
                coins
            );

            AnimatePopOut();
        }
        else
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrongAnswer();

            Debug.Log(
                "WRONG COLOR! Selected: " +
                selectedColor +
                " on " + currentAnimalName +
                " | Correct: " +
                currentCorrectColor
            );

            LoseLife();
            AnimatePopOut();
        }
    }

    private void UpdateCoinsUI()
    {
        if (CoinManager.Instance != null)
            coins = CoinManager.Instance.Coins;

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

        PlayConfetti(null);

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

    public string GetCurrentAnimalName()
    {
        return currentAnimalName;
    }

    public bool IsCurrentAnimalTarget()
    {
        return isCurrentAnimalTarget;
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

    private void EnsureCanvasAndCameraSetup()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            Camera cam = Camera.main;
            if (cam == null) cam = FindFirstObjectByType<Camera>();
            if (cam != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 50f;
            }
        }
    }

    private void AutoFindConfetti()
    {
        GameObject confettiObj = GameObject.Find("Confetti");
        if (confettiObj != null)
        {
            ParticleSystem[] allPS = confettiObj.GetComponentsInChildren<ParticleSystem>(true);
            if (allPS != null && allPS.Length > 0)
            {
                if (confettiParticleSystem == null)
                    confettiParticleSystem = allPS[0];
                if (confettiParticleSystem2 == null && allPS.Length > 1)
                    confettiParticleSystem2 = allPS[1];
            }

            ParticleSystemRenderer[] allRenderers = confettiObj.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (ParticleSystemRenderer psr in allRenderers)
            {
                if (psr != null)
                {
                    psr.sortingLayerName = "Default";
                    psr.sortingOrder = 500;
                    psr.maskInteraction = SpriteMaskInteraction.None;
                }
            }
        }

        if (confettiParticleSystem != null)
        {
            ParticleSystemRenderer psr = confettiParticleSystem.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.sortingLayerName = "Default";
                psr.sortingOrder = 500;
                psr.maskInteraction = SpriteMaskInteraction.None;
            }
        }

        if (confettiParticleSystem2 != null)
        {
            ParticleSystemRenderer psr2 = confettiParticleSystem2.GetComponent<ParticleSystemRenderer>();
            if (psr2 != null)
            {
                psr2.sortingLayerName = "Default";
                psr2.sortingOrder = 500;
                psr2.maskInteraction = SpriteMaskInteraction.None;
            }
        }
    }

    private void StopConfetti()
    {
        if (confettiParticleSystem != null)
        {
            confettiParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (confettiParticleSystem2 != null)
        {
            confettiParticleSystem2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        GameObject confettiObj = GameObject.Find("Confetti");
        if (confettiObj != null)
        {
            ParticleSystem[] allPS = confettiObj.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in allPS)
            {
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
    }

    public void PlayConfetti(Vector3? spawnPosition = null)
    {
        AutoFindConfetti();

        bool played = false;

        if (confettiParticleSystem != null)
        {
            confettiParticleSystem.gameObject.SetActive(true);
            confettiParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            confettiParticleSystem.Play(true);
            played = true;
        }

        if (confettiParticleSystem2 != null)
        {
            confettiParticleSystem2.gameObject.SetActive(true);
            confettiParticleSystem2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            confettiParticleSystem2.Play(true);
            played = true;
        }

        GameObject confettiObj = GameObject.Find("Confetti");
        if (confettiObj != null)
        {
            confettiObj.SetActive(true);
            ParticleSystem[] allPS = confettiObj.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in allPS)
            {
                if (ps != null && ps != confettiParticleSystem && ps != confettiParticleSystem2)
                {
                    ps.gameObject.SetActive(true);
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.Play(true);
                    played = true;
                }
            }
        }

        if (!played && confettiPrefab != null)
        {
            Vector3 pos = spawnPosition ?? (questionImage != null ? questionImage.transform.position : Vector3.zero);
            GameObject instance = Instantiate(confettiPrefab, pos, Quaternion.identity);
            ParticleSystem[] psList = instance.GetComponentsInChildren<ParticleSystem>(true);
            ParticleSystemRenderer[] psrList = instance.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (ParticleSystemRenderer psr in psrList)
            {
                if (psr != null)
                {
                    psr.sortingLayerName = "Default";
                    psr.sortingOrder = 500;
                    psr.maskInteraction = SpriteMaskInteraction.None;
                }
            }
            foreach (ParticleSystem ps in psList)
            {
                if (ps != null)
                {
                    ps.Play(true);
                }
            }
            Destroy(instance, 5f);
        }
    }
}