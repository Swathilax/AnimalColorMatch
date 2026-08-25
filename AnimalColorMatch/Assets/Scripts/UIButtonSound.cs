using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    [Tooltip("Optional custom sound clip. If empty, AudioManager default button click sound is played.")]
    public AudioClip customClickSound;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    private void PlaySound()
    {
        if (AudioManager.Instance != null)
        {
            if (customClickSound != null)
            {
                AudioManager.Instance.PlaySFX(customClickSound);
            }
            else
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}
