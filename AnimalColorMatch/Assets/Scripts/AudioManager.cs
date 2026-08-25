using System;
using UnityEngine;

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
        // Default to enabled (1) if not previously set
        isMusicEnabled = PlayerPrefs.GetInt(PREFS_MUSIC_KEY, 1) == 1;
        isSoundEnabled = PlayerPrefs.GetInt(PREFS_SOUND_KEY, 1) == 1;

        ApplyMusicSettings();
        ApplySoundSettings();
    }

    private void ApplyMusicSettings()
    {
        if (bgmSource != null)
        {
            bgmSource.mute = !isMusicEnabled;
            bgmSource.volume = bgmVolume;

            if (isMusicEnabled)
            {
                if (!bgmSource.isPlaying && bgmClip != null)
                {
                    bgmSource.clip = bgmClip;
                    bgmSource.Play();
                }
            }
        }
    }

    private void ApplySoundSettings()
    {
        if (sfxSource != null)
        {
            sfxSource.mute = !isSoundEnabled;
            sfxSource.volume = sfxVolume;
        }
    }

    // --- Music Controls ---
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

    // --- SFX Controls ---
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

    // --- Toggle & State Management ---
    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;
        PlayerPrefs.SetInt(PREFS_MUSIC_KEY, isMusicEnabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMusicSettings();
        OnMusicStateChanged?.Invoke(isMusicEnabled);
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
    }

    public void ToggleSound()
    {
        SetSoundEnabled(!isSoundEnabled);
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
