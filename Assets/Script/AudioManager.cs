using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip boostSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip shootSound;    // Nytt ljud för skott
    [SerializeField] private AudioClip bombSound;     // Nytt ljud för bomber
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 0.7f;

    private static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    // Nytt ljud för skjutande
    public void PlayShootSound()
    {
        if (shootSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(shootSound, sfxVolume);
        }
    }

    // Nytt ljud för bombsläpp
    public void PlayBombSound()
    {
        if (bombSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(bombSound, sfxVolume);
        }
    }

    public void PlayDeathSound()
    {
        if (deathSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(deathSound, sfxVolume);
        }
    }

    public void PlayBoostSound()
    {
        if (boostSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(boostSound, sfxVolume);
        }
    }

    public void PlayHitSound()
    {
        if (hitSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(hitSound, sfxVolume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void ToggleMusic(bool enabled)
    {
        if (musicSource != null)
        {
            if (enabled)
                musicSource.Play();
            else
                musicSource.Pause();
        }
    }

    public void ToggleSFX(bool enabled)
    {
        sfxVolume = enabled ? 0.7f : 0f;
    }

    public static AudioManager Instance => instance;
}