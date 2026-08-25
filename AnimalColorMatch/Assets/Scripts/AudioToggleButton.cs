using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to Music, Music Off, Sound, or Sound Off buttons.
/// Each button ONLY controls its own visibility based on AudioManager state.
/// No cross-references needed — no Missing Reference errors across scenes.
///
/// SETUP (per button):
///   1. Add this component to the button GameObject.
///   2. Set the correct Toggle Type in the Inspector.
///   3. Clear ALL entries from the button's OnClick() list in the Inspector.
///
/// Toggle Types:
///   MusicOn  = visible when Music IS ON  (clicking it turns music OFF)
///   MusicOff = visible when Music IS OFF (clicking it turns music ON)
///   SoundOn  = visible when Sound IS ON  (clicking it turns sound OFF)
///   SoundOff = visible when Sound IS OFF (clicking it turns sound ON)
/// </summary>
public class AudioToggleButton : MonoBehaviour
{
    public enum ToggleType
    {
        MusicOn,
        MusicOff,
        SoundOn,
        SoundOff
    }

    [Header("Button Type")]
    public ToggleType toggleType;

    private Button _button;

    // ---------------------------------------------------------------
    // Lifecycle
    // ---------------------------------------------------------------

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClicked);
            _button.onClick.AddListener(OnClicked);
        }
    }

    private void OnEnable()
    {
        SyncVisibility();
    }

    // ---------------------------------------------------------------
    // Visibility — each button only controls itself
    // ---------------------------------------------------------------

    /// <summary>
    /// Show this button if the current audio state matches what it represents.
    /// Each button independently decides whether it should be visible.
    /// </summary>
    public void SyncVisibility()
    {
        bool shouldBeVisible = ShouldBeVisible();

        if (gameObject.activeSelf != shouldBeVisible)
        {
            gameObject.SetActive(shouldBeVisible);
        }
    }

    private bool ShouldBeVisible()
    {
        bool musicOn = GetMusicEnabled();
        bool soundOn = GetSoundEnabled();

        switch (toggleType)
        {
            case ToggleType.MusicOn:  return musicOn;        // show when music IS on
            case ToggleType.MusicOff: return !musicOn;       // show when music IS off
            case ToggleType.SoundOn:  return soundOn;        // show when sound IS on
            case ToggleType.SoundOff: return !soundOn;       // show when sound IS off
            default:                  return true;
        }
    }

    // ---------------------------------------------------------------
    // State helpers — uses AudioManager singleton, PlayerPrefs fallback
    // ---------------------------------------------------------------

    private static bool GetMusicEnabled()
    {
        if (AudioManager.Instance != null)
            return AudioManager.Instance.IsMusicEnabled();
        return PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
    }

    private static bool GetSoundEnabled()
    {
        if (AudioManager.Instance != null)
            return AudioManager.Instance.IsSoundEnabled();
        return PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
    }

    // ---------------------------------------------------------------
    // Click handler
    // ---------------------------------------------------------------

    public void OnClicked()
    {
        switch (toggleType)
        {
            case ToggleType.MusicOn:
                // Music is on → turn it off
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SetMusicEnabled(false);
                    AudioManager.Instance.PlayButtonClick();
                }
                else
                {
                    PlayerPrefs.SetInt("MusicEnabled", 0);
                    PlayerPrefs.Save();
                }
                break;

            case ToggleType.MusicOff:
                // Music is off → turn it on
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SetMusicEnabled(true);
                    AudioManager.Instance.PlayButtonClick();
                }
                else
                {
                    PlayerPrefs.SetInt("MusicEnabled", 1);
                    PlayerPrefs.Save();
                }
                break;

            case ToggleType.SoundOn:
                // Sound is on → turn it off
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SetSoundEnabled(false);
                    AudioManager.Instance.PlayButtonClick();
                }
                else
                {
                    PlayerPrefs.SetInt("SoundEnabled", 0);
                    PlayerPrefs.Save();
                }
                break;

            case ToggleType.SoundOff:
                // Sound is off → turn it on
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SetSoundEnabled(true);
                    AudioManager.Instance.PlayButtonClick();
                }
                else
                {
                    PlayerPrefs.SetInt("SoundEnabled", 1);
                    PlayerPrefs.Save();
                }
                break;
        }

        // After changing state, re-evaluate our own visibility.
        // (Our partner button will re-evaluate on its own OnEnable
        // when it becomes active again via AudioManager.RefreshSettingsUINextFrame)
        SyncVisibility();
    }
}
