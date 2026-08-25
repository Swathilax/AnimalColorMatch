using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Self-contained Music/Sound settings toggle UI controller.
/// 
/// HOW TO USE IN UNITY INSPECTOR:
///   1. Assign musicOnObject, musicOffObject, soundOnObject, soundOffObject in Inspector.
///   2. On the Music button OnClick:    call SettingsUIController.OnMusicOnClicked()
///   3. On the MusicOff button OnClick: call SettingsUIController.OnMusicOffClicked()
///   4. On the Sound button OnClick:    call SettingsUIController.OnSoundOnClicked()
///   5. On the SoundOff button OnClick: call SettingsUIController.OnSoundOffClicked()
///   6. REMOVE any old GameObject.SetActive calls from button OnClick Inspector events.
///
/// The script handles ALL SetActive logic itself.
/// Uses Update() to always keep UI in sync — works regardless of scene history.
/// </summary>
public class SettingsUIController : MonoBehaviour
{
    [Header("Music UI Elements")]
    [Tooltip("GameObject shown when Music is ON (clicking this turns music OFF)")]
    public GameObject musicOnObject;
    [Tooltip("GameObject shown when Music is OFF (clicking this turns music ON)")]
    public GameObject musicOffObject;

    [Header("Sound UI Elements")]
    [Tooltip("GameObject shown when Sound is ON (clicking this turns sound OFF)")]
    public GameObject soundOnObject;
    [Tooltip("GameObject shown when Sound is OFF (clicking this turns sound ON)")]
    public GameObject soundOffObject;

    private bool _lastMusicState = true;
    private bool _lastSoundState = true;

    // ---------------------------------------------------------------
    // Lifecycle
    // ---------------------------------------------------------------

    private void Awake()
    {
        WireButtonListeners();
    }

    private void OnEnable()
    {
        // Wire listeners in case Awake was skipped (parent was inactive at scene load)
        WireButtonListeners();
        ForceRefreshUI();
        StartCoroutine(RefreshNextFrame());
    }

    private void Start()
    {
        WireButtonListeners();
        ForceRefreshUI();
    }

    /// <summary>
    /// Lightweight Update: detects any state change and refreshes UI.
    /// Only runs while this component is active (settings panel is open).
    /// </summary>
    private void Update()
    {
        bool musicNow = GetMusicEnabled();
        bool soundNow = GetSoundEnabled();

        if (musicNow != _lastMusicState || soundNow != _lastSoundState)
        {
            _lastMusicState = musicNow;
            _lastSoundState = soundNow;
            ApplyUIState(musicNow, soundNow);
        }
    }

    // ---------------------------------------------------------------
    // Button Listener Setup (idempotent — safe to call multiple times)
    // ---------------------------------------------------------------

    private void WireButtonListeners()
    {
        AddListener(musicOnObject,  OnMusicOnClicked);
        AddListener(musicOffObject, OnMusicOffClicked);
        AddListener(soundOnObject,  OnSoundOnClicked);
        AddListener(soundOffObject, OnSoundOffClicked);
    }

    private static void AddListener(GameObject go, UnityEngine.Events.UnityAction action)
    {
        if (go == null) return;
        Button btn = go.GetComponent<Button>();
        if (btn == null) return;
        btn.onClick.RemoveListener(action);
        btn.onClick.AddListener(action);
    }

    // ---------------------------------------------------------------
    // UI Refresh
    // ---------------------------------------------------------------

    private IEnumerator RefreshNextFrame()
    {
        yield return null;   // wait one frame
        ForceRefreshUI();
    }

    public void ForceRefreshUI()
    {
        bool music = GetMusicEnabled();
        bool sound = GetSoundEnabled();
        _lastMusicState = music;
        _lastSoundState = sound;
        ApplyUIState(music, sound);
    }

    private void ApplyUIState(bool musicEnabled, bool soundEnabled)
    {
        SetActive(musicOnObject,  musicEnabled);
        SetActive(musicOffObject, !musicEnabled);
        SetActive(soundOnObject,  soundEnabled);
        SetActive(soundOffObject, !soundEnabled);
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }

    // ---------------------------------------------------------------
    // State Helpers
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
    // Button Click Handlers (call from Inspector OR via code)
    // ---------------------------------------------------------------

    /// <summary>Music is ON → turn it OFF.</summary>
    public void OnMusicOnClicked()
    {
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
        ForceRefreshUI();
    }

    /// <summary>Music is OFF → turn it ON.</summary>
    public void OnMusicOffClicked()
    {
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
        ForceRefreshUI();
    }

    /// <summary>Sound is ON → turn it OFF.</summary>
    public void OnSoundOnClicked()
    {
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
        ForceRefreshUI();
    }

    /// <summary>Sound is OFF → turn it ON.</summary>
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
            PlayerPrefs.Save();
        }
        ForceRefreshUI();
    }

    // Convenience single-button toggles
    public void ToggleMusic()
    {
        if (GetMusicEnabled()) OnMusicOnClicked();
        else                   OnMusicOffClicked();
    }

    public void ToggleSound()
    {
        if (GetSoundEnabled()) OnSoundOnClicked();
        else                   OnSoundOffClicked();
    }
}
