using UnityEngine;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    [Header("Music UI Elements")]
    [Tooltip("The GameObject / Button shown when Music is ON")]
    public GameObject musicOnObject;
    [Tooltip("The GameObject / Button shown when Music is OFF")]
    public GameObject musicOffObject;

    [Header("Sound UI Elements")]
    [Tooltip("The GameObject / Button shown when Sound is ON")]
    public GameObject soundOnObject;
    [Tooltip("The GameObject / Button shown when Sound is OFF")]
    public GameObject soundOffObject;

    private void Awake()
    {
        SetupButtonListeners();
    }

    private void OnEnable()
    {
        UpdateUI();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnMusicStateChanged += HandleMusicStateChanged;
            AudioManager.Instance.OnSoundStateChanged += HandleSoundStateChanged;
        }
    }

    private void OnDisable()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnMusicStateChanged -= HandleMusicStateChanged;
            AudioManager.Instance.OnSoundStateChanged -= HandleSoundStateChanged;
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    private void SetupButtonListeners()
    {
        // Auto-wire buttons if attached
        if (musicOnObject != null)
        {
            Button btn = musicOnObject.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(OnMusicOnClicked);
                btn.onClick.AddListener(OnMusicOnClicked);
            }
        }

        if (musicOffObject != null)
        {
            Button btn = musicOffObject.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(OnMusicOffClicked);
                btn.onClick.AddListener(OnMusicOffClicked);
            }
        }

        if (soundOnObject != null)
        {
            Button btn = soundOnObject.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(OnSoundOnClicked);
                btn.onClick.AddListener(OnSoundOnClicked);
            }
        }

        if (soundOffObject != null)
        {
            Button btn = soundOffObject.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(OnSoundOffClicked);
                btn.onClick.AddListener(OnSoundOffClicked);
            }
        }
    }

    public void UpdateUI()
    {
        bool musicEnabled = AudioManager.Instance != null ? AudioManager.Instance.IsMusicEnabled() : (PlayerPrefs.GetInt("MusicEnabled", 1) == 1);
        bool soundEnabled = AudioManager.Instance != null ? AudioManager.Instance.IsSoundEnabled() : (PlayerPrefs.GetInt("SoundEnabled", 1) == 1);

        UpdateMusicUI(musicEnabled);
        UpdateSoundUI(soundEnabled);
    }

    private void UpdateMusicUI(bool isEnabled)
    {
        if (musicOnObject != null)
            musicOnObject.SetActive(isEnabled);

        if (musicOffObject != null)
            musicOffObject.SetActive(!isEnabled);
    }

    private void UpdateSoundUI(bool isEnabled)
    {
        if (soundOnObject != null)
            soundOnObject.SetActive(isEnabled);

        if (soundOffObject != null)
            soundOffObject.SetActive(!isEnabled);
    }

    private void HandleMusicStateChanged(bool isEnabled)
    {
        UpdateMusicUI(isEnabled);
    }

    private void HandleSoundStateChanged(bool isEnabled)
    {
        UpdateSoundUI(isEnabled);
    }

    // --- Button Click Handlers ---
    public void OnMusicOnClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.SetMusicEnabled(false);
        }
        else
        {
            PlayerPrefs.SetInt("MusicEnabled", 0);
            UpdateUI();
        }
    }

    public void OnMusicOffClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.SetMusicEnabled(true);
        }
        else
        {
            PlayerPrefs.SetInt("MusicEnabled", 1);
            UpdateUI();
        }
    }

    public void OnSoundOnClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.SetSoundEnabled(false);
        }
        else
        {
            PlayerPrefs.SetInt("SoundEnabled", 0);
            UpdateUI();
        }
    }

    public void OnSoundOffClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSoundEnabled(true);
            AudioManager.Instance.PlayButtonClick();
        }
        else
        {
            PlayerPrefs.SetInt("SoundEnabled", 1);
            UpdateUI();
        }
    }

    public void ToggleMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.ToggleMusic();
        }
    }

    public void ToggleSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.ToggleSound();
        }
    }
}
