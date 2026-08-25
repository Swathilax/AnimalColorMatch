using UnityEngine;
using UnityEngine.UI;

public class ColorAnswerButton : MonoBehaviour
{
    [Header("Button Colour")]
    public string buttonColor;

    [Header("Game Manager")]
    public GameManager gameManager;

    private void Awake()
    {
        InitializeButton();
    }

    private void Start()
    {
        InitializeButton();
    }

    private void InitializeButton()
    {
        if (gameManager == null)
        {
#if UNITY_2023_1_OR_NEWER
            gameManager = Object.FindFirstObjectByType<GameManager>();
#else
            gameManager = Object.FindObjectOfType<GameManager>();
#endif
        }

        if (string.IsNullOrEmpty(buttonColor))
        {
            string objName = gameObject.name.ToLower();
            if (objName.Contains("red")) buttonColor = "Red";
            else if (objName.Contains("blue")) buttonColor = "Blue";
            else if (objName.Contains("yellow")) buttonColor = "Yellow";
            else if (objName.Contains("green")) buttonColor = "Green";
        }

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveListener(OnColorButtonClicked);
            btn.onClick.AddListener(OnColorButtonClicked);
        }
    }

    public void OnColorButtonClicked()
    {
        if (gameManager == null)
        {
#if UNITY_2023_1_OR_NEWER
            gameManager = Object.FindFirstObjectByType<GameManager>();
#else
            gameManager = Object.FindObjectOfType<GameManager>();
#endif
        }

        if (gameManager == null)
        {
            Debug.LogError(
                gameObject.name +
                " is missing GameManager reference!"
            );
            return;
        }

        Debug.Log("Color Button Clicked: " + buttonColor);
        gameManager.CheckAnswer(buttonColor);
    }
}