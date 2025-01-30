using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    private static AudioManager instance;
    public static AudioManager Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Audio Sources
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource engineSource;
    [SerializeField] private AudioSource radioSource;
    #endregion

    #region Volume Settings
    [Header("Volume Settings")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float engineVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float radioVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float sirenVolume = 0.7f;
    #endregion

    #region Audio Clips
    [Header("Game Audio")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip engineSound;

    [Header("Combat Audio")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    [Header("Bomb Audio")]
    [SerializeField] private AudioClip bombDropSound;
    [SerializeField] private AudioClip bombFallingSound;
    [SerializeField] private AudioClip bombExplosionSound;

    [Header("Power-up Audio")]
    [SerializeField] private AudioClip boostSound;

    [Header("Alert Audio")]
    [SerializeField] private AudioClip airRaidSiren;

    [Header("Radio Audio")]
    [SerializeField] private AudioClip radioStart;
    [SerializeField] private AudioClip radioEnd;
    [SerializeField] private AudioClip[] radioMessages;
    [SerializeField] private float radioDelay = 30f;
    #endregion

    private bool isEnginePlaying = false;

    #region Initialization
    private void Start()
    {
        StartGameAudio();
    }

    private void SetupAudioSources()
    {
        SetupAudioSource(ref musicSource, true);
        SetupAudioSource(ref sfxSource, false);
        SetupAudioSource(ref engineSource, true);
        SetupAudioSource(ref radioSource, false);
    }

    private void SetupAudioSource(ref AudioSource source, bool loop)
    {
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.loop = loop;
        }
    }

    private void StartGameAudio()
    {
        PlayBackgroundMusic();
        StartEngine();
        StartCoroutine(PlayDelayedRadio());
    }
    #endregion

    #region Sound Effects
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    // Bakåtkompatibla metoder
    public void PlayShootSound() => PlayCombatSound(CombatSoundType.Shoot);
    public void PlayHitSound() => PlayCombatSound(CombatSoundType.Hit);
    public void PlayDeathSound() => PlayCombatSound(CombatSoundType.Death);

    public void PlayBombSound() => PlayBombSound(BombSoundType.Drop);
    public void PlayBombFallingSound() => PlayBombSound(BombSoundType.Falling);
    public void PlayBombExplosionSound() => PlayBombSound(BombSoundType.Explosion);

    public void PlayCombatSound(CombatSoundType type)
    {
        AudioClip clip = type switch
        {
            CombatSoundType.Shoot => shootSound,
            CombatSoundType.Hit => hitSound,
            CombatSoundType.Death => deathSound,
            _ => null
        };
        PlaySound(clip);
    }

    public void PlayBombSound(BombSoundType type)
    {
        AudioClip clip = type switch
        {
            BombSoundType.Drop => bombDropSound,
            BombSoundType.Falling => bombFallingSound,
            BombSoundType.Explosion => bombExplosionSound,
            _ => null
        };
        PlaySound(clip);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }
    #endregion

    #region Engine and Radio
    private void StartEngine()
    {
        if (engineSound != null && engineSource != null && !isEnginePlaying)
        {
            engineSource.clip = engineSound;
            engineSource.volume = engineVolume;
            engineSource.Play();
            isEnginePlaying = true;
        }
    }

    private IEnumerator PlayDelayedRadio()
    {
        yield return new WaitForSeconds(radioDelay);
        StartRadioSequence();
    }

    public void StartRadioSequence()
    {
        StartCoroutine(RadioSequence());
    }

    private IEnumerator RadioSequence()
    {
        if (radioStart != null)
        {
            radioSource.PlayOneShot(radioStart, radioVolume);
            yield return new WaitForSeconds(radioStart.length);
        }

        foreach (AudioClip message in radioMessages)
        {
            if (message != null)
            {
                radioSource.PlayOneShot(message, radioVolume);
                yield return new WaitForSeconds(message.length);
            }
        }

        if (radioEnd != null)
        {
            radioSource.PlayOneShot(radioEnd, radioVolume);
        }
    }
    #endregion

    #region Alert Sounds
    public void PlayAirRaidSiren()
    {
        if (airRaidSiren != null)
        {
            sfxSource.PlayOneShot(airRaidSiren, sirenVolume);
        }
    }

    public void PlayBoostSound()
    {
        if (boostSound != null)
        {
            sfxSource.PlayOneShot(boostSound, sfxVolume);
        }
    }
    #endregion

    #region Volume Control
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetEngineVolume(float volume)
    {
        engineVolume = Mathf.Clamp01(volume);
        if (engineSource != null)
            engineSource.volume = engineVolume;
    }

    public void SetRadioVolume(float volume)
    {
        radioVolume = Mathf.Clamp01(volume);
        if (radioSource != null)
            radioSource.volume = radioVolume;
    }

    public void SetSirenVolume(float volume)
    {
        sirenVolume = Mathf.Clamp01(volume);
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

    public void StopAllSounds()
    {
        if (musicSource != null) musicSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
        if (engineSource != null) engineSource.Stop();
        if (radioSource != null) radioSource.Stop();
        StopAllCoroutines();
    }
    #endregion
}

#region Enums
public enum CombatSoundType
{
    Shoot,
    Hit,
    Death
}

public enum BombSoundType
{
    Drop,
    Falling,
    Explosion
}
#endregion


//using UnityEngine;
//using System.Collections;

//public class AudioManager : MonoBehaviour
//{
//    [Header("Audio Sources")]
//    [SerializeField] private AudioSource musicSource;
//    [SerializeField] private AudioSource sfxSource;
//    [SerializeField] private AudioSource engineSource;
//    [SerializeField] private AudioSource radioSource;

//    [Header("Volume Settings")]
//    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.5f;
//    [SerializeField][Range(0f, 1f)] private float sfxVolume = 0.7f;
//    [SerializeField][Range(0f, 1f)] private float engineVolume = 0.7f;
//    [SerializeField][Range(0f, 1f)] private float radioVolume = 0.7f;
//    [SerializeField][Range(0f, 1f)] private float sirenVolume = 0.7f;

//    [Header("Audio Clips")]
//    [SerializeField] private AudioClip backgroundMusic;
//    [SerializeField] private AudioClip deathSound;
//    [SerializeField] private AudioClip boostSound;
//    [SerializeField] private AudioClip hitSound;
//    [SerializeField] private AudioClip shootSound;
//    [SerializeField] private AudioClip bombSound;
//    [SerializeField] private AudioClip bombFallingSound;
//    [SerializeField] private AudioClip bombExplosionSound;
//    [SerializeField] private AudioClip engineSound;
//    [SerializeField] private AudioClip airRaidSiren;

//    [Header("Radio Settings")]
//    [SerializeField] private AudioClip radioStart;
//    [SerializeField] private AudioClip radioEnd;
//    [SerializeField] private AudioClip[] radioMessages;
//    [SerializeField] private float radioDelay = 30f;

//    private static AudioManager instance;
//    private bool isEnginePlaying = false;

//    private void Awake()
//    {
//        if (instance == null)
//        {
//            instance = this;
//            DontDestroyOnLoad(gameObject);
//            SetupAudioSources();
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    private void SetupAudioSources()
//    {
//        if (musicSource == null)
//        {
//            musicSource = gameObject.AddComponent<AudioSource>();
//            musicSource.loop = true;
//        }
//        if (sfxSource == null)
//        {
//            sfxSource = gameObject.AddComponent<AudioSource>();
//            sfxSource.loop = false;
//        }
//        if (engineSource == null)
//        {
//            engineSource = gameObject.AddComponent<AudioSource>();
//            engineSource.loop = true;
//        }
//        if (radioSource == null)
//        {
//            radioSource = gameObject.AddComponent<AudioSource>();
//            radioSource.loop = false;
//        }
//    }

//    private void Start()
//    {
//        PlayBackgroundMusic();
//        StartEngine();
//        StartCoroutine(PlayDelayedRadio());
//    }

//    private void StartEngine()
//    {
//        if (engineSound != null && engineSource != null && !isEnginePlaying)
//        {
//            engineSource.clip = engineSound;
//            engineSource.volume = engineVolume;
//            engineSource.Play();
//            isEnginePlaying = true;
//        }
//    }

//    public void PlayAirRaidSiren()
//    {
//        if (airRaidSiren != null)
//        {
//            sfxSource.PlayOneShot(airRaidSiren, sirenVolume);
//        }
//    }

//    private IEnumerator PlayDelayedRadio()
//    {
//        yield return new WaitForSeconds(radioDelay);
//        StartRadioSequence();
//    }

//    public void StartRadioSequence()
//    {
//        StartCoroutine(RadioSequence());
//    }

//    private IEnumerator RadioSequence()
//    {
//        if (radioStart != null)
//        {
//            radioSource.PlayOneShot(radioStart, radioVolume);
//            yield return new WaitForSeconds(radioStart.length);
//        }

//        foreach (AudioClip message in radioMessages)
//        {
//            radioSource.PlayOneShot(message, radioVolume);
//            yield return new WaitForSeconds(message.length);
//        }

//        if (radioEnd != null)
//        {
//            radioSource.PlayOneShot(radioEnd, radioVolume);
//        }
//    }

//    public void PlayBackgroundMusic()
//    {
//        if (backgroundMusic != null)
//        {
//            musicSource.clip = backgroundMusic;
//            musicSource.volume = musicVolume;
//            musicSource.Play();
//        }
//    }

//    public void PlayShootSound() => PlaySound(shootSound);
//    public void PlayBombSound() => PlaySound(bombSound);
//    public void PlayBombFallingSound() => PlaySound(bombFallingSound);
//    public void PlayBombExplosionSound() => PlaySound(bombExplosionSound);
//    public void PlayDeathSound() => PlaySound(deathSound);
//    public void PlayBoostSound() => PlaySound(boostSound);
//    public void PlayHitSound() => PlaySound(hitSound);

//    private void PlaySound(AudioClip clip)
//    {
//        if (clip != null)
//        {
//            sfxSource.PlayOneShot(clip, sfxVolume);
//        }
//    }

//    public void SetMusicVolume(float volume) => musicVolume = Mathf.Clamp01(volume);
//    public void SetSFXVolume(float volume) => sfxVolume = Mathf.Clamp01(volume);
//    public void SetEngineVolume(float volume) => engineVolume = Mathf.Clamp01(volume);
//    public void SetRadioVolume(float volume) => radioVolume = Mathf.Clamp01(volume);
//    public void SetSirenVolume(float volume) => sirenVolume = Mathf.Clamp01(volume);

//    public void ToggleMusic(bool enabled)
//    {
//        if (musicSource != null)
//        {
//            if (enabled) musicSource.Play();
//            else musicSource.Pause();
//        }
//    }

//    public void ToggleSFX(bool enabled)
//    {
//        sfxVolume = enabled ? 0.7f : 0f;
//    }

//    public static AudioManager Instance => instance;
//}