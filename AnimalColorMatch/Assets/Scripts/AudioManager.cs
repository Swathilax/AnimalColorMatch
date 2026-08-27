using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string PREFS_MUSIC_KEY = "MusicEnabled";
    private const string PREFS_SOUND_KEY = "SoundEnabled";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip bgmClip;

    [Header("SFX Clips")]
    public AudioClip buttonClickClip;
    public AudioClip correctAnswerClip;
    public AudioClip wrongAnswerClip;
    public AudioClip levelCompletedClip;
    public AudioClip levelFailedClip;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Play On Awake")]
    public bool playBgmOnStart = true;

    private bool isMusicEnabled = true;
    private bool isSoundEnabled = true;

    public event Action<bool> OnMusicStateChanged;
    public event Action<bool> OnSoundStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        LoadSettings();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMusicSettings();
        ApplySoundSettings();

        OnMusicStateChanged?.Invoke(isMusicEnabled);
        OnSoundStateChanged?.Invoke(isSoundEnabled);

        StartCoroutine(RefreshSettingsUINextFrame());
    }

    private IEnumerator RefreshSettingsUINextFrame()
    {
        yield return null;

#if UNITY_2023_1_OR_NEWER
        SettingsUIController[] uiControllers = FindObjectsByType<SettingsUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        AudioToggleButton[]    toggleButtons  = FindObjectsByType<AudioToggleButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        SettingsUIController[] uiControllers = Resources.FindObjectsOfTypeAll<SettingsUIController>();
        AudioToggleButton[]    toggleButtons  = Resources.FindObjectsOfTypeAll<AudioToggleButton>();
#endif
        foreach (SettingsUIController ctrl in uiControllers)
        {
            if (ctrl != null) ctrl.ForceRefreshUI();
        }

        foreach (AudioToggleButton btn in toggleButtons)
        {
            if (btn != null) btn.SyncVisibility();
        }
    }

    private void Start()
    {
        if (playBgmOnStart && bgmClip != null && isMusicEnabled)
        {
            PlayBGM(bgmClip);
        }
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    private void LoadSettings()
    {
        isMusicEnabled = PlayerPrefs.GetInt(PREFS_MUSIC_KEY, 1) == 1;
        isSoundEnabled = PlayerPrefs.GetInt(PREFS_SOUND_KEY, 1) == 1;

        ApplyMusicSettings();
        ApplySoundSettings();
    }

    private void ApplyMusicSettings()
    {
        if (bgmSource == null) return;

        bgmSource.volume = bgmVolume;

        if (isMusicEnabled)
        {
            bgmSource.mute = false;
            if (!bgmSource.isPlaying && bgmClip != null)
            {
                bgmSource.clip = bgmClip;
                bgmSource.Play();
            }
        }
        else
        {
            bgmSource.Stop();
        }
    }

    private void ApplySoundSettings()
    {
        if (sfxSource != null)
        {
            sfxSource.mute   = !isSoundEnabled;
            sfxSource.volume = sfxVolume;
        }
    }

    public void PlayBGM(AudioClip clip = null)
    {
        if (clip != null)
        {
            bgmClip = clip;
        }

        if (bgmSource == null || bgmClip == null) return;

        bgmSource.clip = bgmClip;
        bgmSource.volume = bgmVolume;
        bgmSource.mute = !isMusicEnabled;

        if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource != null && isMusicEnabled && !bgmSource.isPlaying)
        {
            bgmSource.UnPause();
        }
    }
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || !isSoundEnabled || sfxSource == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    public void PlayButtonClick()
    {
        if (buttonClickClip != null)
        {
            PlaySFX(buttonClickClip);
        }
    }

    public void PlayCorrectAnswer()
    {
        if (correctAnswerClip != null)
        {
            PlaySFX(correctAnswerClip);
        }
    }

    public void PlayWrongAnswer()
    {
        if (wrongAnswerClip != null)
        {
            PlaySFX(wrongAnswerClip);
        }
    }

    public void PlayLevelCompleted()
    {
        if (levelCompletedClip != null)
        {
            PlaySFX(levelCompletedClip);
        }
    }

    public void PlayLevelFailed()
    {
        if (levelFailedClip != null)
        {
            PlaySFX(levelFailedClip);
        }
    }

    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;
        PlayerPrefs.SetInt(PREFS_MUSIC_KEY, isMusicEnabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMusicSettings();
        OnMusicStateChanged?.Invoke(isMusicEnabled);
        SyncAllToggleButtons();
    }

    public void ToggleMusic()
    {
        SetMusicEnabled(!isMusicEnabled);
    }

    public void SetSoundEnabled(bool enabled)
    {
        isSoundEnabled = enabled;
        PlayerPrefs.SetInt(PREFS_SOUND_KEY, isSoundEnabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplySoundSettings();
        OnSoundStateChanged?.Invoke(isSoundEnabled);
        SyncAllToggleButtons();
    }

    public void ToggleSound()
    {
        SetSoundEnabled(!isSoundEnabled);
    }

    private void SyncAllToggleButtons()
    {
#if UNITY_2023_1_OR_NEWER
        AudioToggleButton[] buttons = FindObjectsByType<AudioToggleButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        AudioToggleButton[] buttons = Resources.FindObjectsOfTypeAll<AudioToggleButton>();
#endif
        foreach (AudioToggleButton btn in buttons)
        {
            if (btn != null) btn.SyncVisibility();
        }
    }

    public bool IsMusicEnabled()
    {
        return isMusicEnabled;
    }

    public bool IsSoundEnabled()
    {
        return isSoundEnabled;
    }
}
