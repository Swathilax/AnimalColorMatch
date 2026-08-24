using UnityEngine;

public class ColorAnswerButton : MonoBehaviour
{
    [Header("Button Colour")]
    public string buttonColor;

    [Header("Game Manager")]
    public GameManager gameManager;

    public void OnColorButtonClicked()
    {
        if (gameManager == null)
        {
            Debug.LogError(
                gameObject.name +
                " is missing ColorMatchGameManager reference!"
            );

            return;
        }

        Debug.Log("Clicked: " + buttonColor);

        gameManager.CheckAnswer(buttonColor);
    }
}