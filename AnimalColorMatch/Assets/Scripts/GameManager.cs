using System.Collections;
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
    public float levelDuration = 60f;
    public float questionDuration = 5f;
    public int startingLives = 3;
    public int requiredCorrectAnswers = 7;

    [Header("Lives UI")]
    public TMP_Text livesText;

    [Header("Panels")]
    public GameObject levelCompletedPanel;
    public GameObject levelFailedPanel;

    [Header("Answer Buttons")]
    public Button[] answerButtons;

    private Sprite currentSprite;
    private string currentCorrectColor;

    private int correctAnswers;
    private int lives;

    private float levelTimer;
    private float questionTimer;

    private bool gameRunning;
    private bool questionAnswered;

    private void Start()
    {
        StartLevel();
    }

    private void Update()
    {
        if (!gameRunning)
            return;

        levelTimer -= Time.deltaTime;

        if (levelTimer <= 0f)
        {
            levelTimer = 0f;

            if (correctAnswers >= requiredCorrectAnswers)
                LevelComplete();
            else
                LevelFailed();
        }
    }

    public void StartLevel()
    {
        StopAllCoroutines();

        correctAnswers = 0;
        lives = startingLives;

        levelTimer = levelDuration;
        questionTimer = questionDuration;

        gameRunning = true;
        questionAnswered = false;

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

    private IEnumerator QuestionLoop()
    {
        while (gameRunning)
        {
            SpawnRandomQuestion();

            questionAnswered = false;
            questionTimer = questionDuration;

            while (
                questionTimer > 0f &&
                !questionAnswered &&
                gameRunning
            )
            {
                questionTimer -= Time.deltaTime;
                yield return null;
            }

            if (!gameRunning)
                yield break;

            if (!questionAnswered)
            {
                LoseLife();

                if (!gameRunning)
                    yield break;
            }

            yield return null;
        }
    }

    private void SpawnRandomQuestion()
{
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

    if (spawnPoints == null || spawnPoints.Length == 0)
        return;

    int randomSpawnIndex = Random.Range(0, spawnPoints.Length);

    questionImage.transform.position =
        spawnPoints[randomSpawnIndex].position;

    questionImage.sprite = currentSprite;

    questionImage.transform.localScale = Vector3.zero;

    StartCoroutine(PopInQuestion());

    Debug.Log(
        "Spawned at: " +
        spawnPoints[randomSpawnIndex].name +
        " | Answer: " +
        currentCorrectColor
    );
}

    private IEnumerator PopInQuestion()
    {
        if (questionImage == null)
            yield break;

        float duration = 0.3f;
        float timer = 0f;

        Vector3 targetScale = Vector3.one;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            t = Mathf.SmoothStep(0f, 1f, t);

            questionImage.transform.localScale =
                Vector3.Lerp(
                    Vector3.zero,
                    targetScale,
                    t
                );

            yield return null;
        }

        questionImage.transform.localScale = targetScale;
    }

    public void CheckAnswer(string selectedColor)
    {
        if (!gameRunning)
            return;

        if (questionAnswered)
            return;

        questionAnswered = true;

        if (selectedColor == currentCorrectColor)
        {
            correctAnswers++;

            Debug.Log(
                "CORRECT! " +
                selectedColor +
                " | Correct Answers: " +
                correctAnswers
            );

            if (correctAnswers >= requiredCorrectAnswers)
                LevelComplete();
        }
        else
        {
            Debug.Log(
                "WRONG! Selected: " +
                selectedColor +
                " | Correct: " +
                currentCorrectColor
            );

            LoseLife();
        }
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
        StartLevel();
    }

    public void NextLevel()
    {
        StartLevel();
    }

    public string GetCurrentCorrectColor()
    {
        return currentCorrectColor;
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