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
    [Header("Main Volume Settings")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float engineVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float radioVolume = 0.7f;

    [Header("Effect Volumes")]
    [SerializeField, Range(0f, 1f)] private float deathVolume = 0.7f;

    #endregion

    #region Music Control
    [Header("Music Control")]
    [SerializeField] private bool isMusicEnabled = true;
    [SerializeField] private bool isRadioEnabled = true;
    #endregion

    #region Audio Clips
    [Header("Game Audio")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip engineSound;

    [Header("Gun Audio")]
    [SerializeField] private AudioClip playerShootSound;
    [SerializeField] private AudioClip enemyShootSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField, Range(0f, 1f)] private float playerShootVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float enemyShootVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float hitVolume = 0.7f;

    [Header("Bomb Audio")]
    [SerializeField] private AudioClip bombDropSound;
    [SerializeField] private AudioClip bombFallingSound;
    [SerializeField] private AudioClip bombExplosionSound;
    [SerializeField, Range(0f, 1f)] private float bombDropVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float bombFallingVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float bombExplosionVolume = 0.7f;

    [Header("Missile Sounds")]
    [SerializeField] private AudioClip missileHitSound;
    [SerializeField] private AudioClip missileLaunchSound;
    [SerializeField, Range(0f, 1f)] private float missileHitVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float missileLaunchVolume = 0.7f;

    [Header("Flare Sound")]
    [SerializeField] private AudioClip flareSound;
    [SerializeField, Range(0f, 1f)] private float flareVolume = 0.7f;

    [Header("Power-up Audio")]
    [SerializeField] private AudioClip boostSound;
    [SerializeField, Range(0f, 1f)] private float boostVolume = 0.7f;

    [Header("Alert Audio")]
    [SerializeField] private AudioClip airRaidSiren;
    [SerializeField, Range(0f, 1f)] private float sirenVolume = 0.7f;
    [SerializeField] private AudioClip bossAlertSound;
    [SerializeField, Range(0f, 1f)] private float bossAlertVolume = 0.7f;

    [Header("Radio Audio")]
    [SerializeField] private AudioClip radioStart;
    [SerializeField] private AudioClip radioEnd;
    [SerializeField] private AudioClip[] radioMessages;
    [SerializeField] private float radioDelay = 30f;
    #endregion

    [Header("Prewarming")]
    [SerializeField] private bool prewarmSoundsOnStart = true;

    private bool isEnginePlaying = false;

    #region Initialization
    private void Start()
    {
        LoadAudioSettings();
        StartGameAudio();

        if (prewarmSoundsOnStart)
        {
            StartCoroutine(PrewarmAllSounds());
        }
    }

    private void LoadAudioSettings()
    {
        // Ladda sparade inst�llningar (standard �r p�)
        isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        isRadioEnabled = PlayerPrefs.GetInt("RadioEnabled", 1) == 1;

        Debug.Log($"[AudioManager] Laddade ljudinst�llningar - Musik: {isMusicEnabled}, Radio: {isRadioEnabled}");
    }

    private IEnumerator PrewarmAllSounds()
    {
        // V�nta ett frame f�r att l�ta andra system initialisera
        yield return null;

        Debug.Log("[AudioManager] Starting sound prewarming...");

        // Skapa en temporary AudioSource f�r prewarming
        GameObject tempObj = new GameObject("AudioPrewarmer");
        AudioSource prewarmer = tempObj.AddComponent<AudioSource>();
        prewarmer.volume = 0f; // Noll volym
        prewarmer.playOnAwake = false;

        // Lista alla ljudklipp som anv�nds
        AudioClip[] clipsToPrewarm = new AudioClip[]
        {
            // Explosioner
            bombExplosionSound,
            missileHitSound,
            deathSound,
            bombFallingSound,
            bombDropSound,
            
            // Skottsystem
            playerShootSound,
            enemyShootSound,
            hitSound,
            
            // Power-ups och �vriga
            boostSound,
            flareSound,
            missileLaunchSound,
            airRaidSiren,
            bossAlertSound
        };

        // Spela upp varje ljud i noll-volym f�r att ladda dem
        foreach (var clip in clipsToPrewarm)
        {
            if (clip != null)
            {
                prewarmer.clip = clip;
                prewarmer.Play();

                // V�nta lite f�r att l�ta ljudet ladda
                yield return new WaitForSeconds(0.05f);

                prewarmer.Stop();
            }
        }

        // F�rst�r temporary object
        Destroy(tempObj);
        Debug.Log("[AudioManager] Sound prewarming complete");
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
        if (isMusicEnabled)
        {
            PlayBackgroundMusic();
        }

        StartEngine();

        if (isRadioEnabled)
        {
            StartCoroutine(PlayDelayedRadio());
        }
    }
    #endregion

    #region Music Control Methods
    // Metod f�r att v�xla musik p�/av
    public void ToggleMusic()
    {
        isMusicEnabled = !isMusicEnabled;

        if (isMusicEnabled)
        {
            // S�tt p� musiken
            if (musicSource != null && backgroundMusic != null)
            {
                musicSource.Play();
                Debug.Log("[AudioManager] Musik p�slagen");
            }
        }
        else
        {
            // St�ng av musiken
            if (musicSource != null)
            {
                musicSource.Pause();
                Debug.Log("[AudioManager] Musik avst�ngd");
            }
        }

        // Spara inst�llningen
        PlayerPrefs.SetInt("MusicEnabled", isMusicEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Metod f�r att s�tta musik p�/av direkt
    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;

        if (enabled)
        {
            if (musicSource != null && backgroundMusic != null)
            {
                musicSource.Play();
                Debug.Log("[AudioManager] Musik aktiverad");
            }
        }
        else
        {
            if (musicSource != null)
            {
                musicSource.Pause();
                Debug.Log("[AudioManager] Musik inaktiverad");
            }
        }

        // Spara inst�llningen
        PlayerPrefs.SetInt("MusicEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Metod f�r att v�xla radio p�/av
    public void ToggleRadio()
    {
        isRadioEnabled = !isRadioEnabled;

        if (!isRadioEnabled)
        {
            // St�ng av radion
            if (radioSource != null)
            {
                radioSource.Stop();
                Debug.Log("[AudioManager] Radio avst�ngd");
            }
        }
        else
        {
            Debug.Log("[AudioManager] Radio p�slagen");
            // Radio kommer spela n�sta g�ng den �r schemalagd
        }

        // Spara inst�llningen
        PlayerPrefs.SetInt("RadioEnabled", isRadioEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Metod f�r att f� status p� musiken
    public bool IsMusicEnabled()
    {
        return isMusicEnabled;
    }

    // Metod f�r att f� status p� radion
    public bool IsRadioEnabled()
    {
        return isRadioEnabled;
    }
    #endregion

    #region Sound Effects
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null && isMusicEnabled)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public void PlayMissileLaunchSound()
    {
        if (missileLaunchSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(missileLaunchSound, missileLaunchVolume);
        }
    }

    public void PlayMissileHitSound()
    {
        if (missileHitSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(missileHitSound, missileHitVolume);
            PlayBombSound(BombSoundType.Explosion);
        }
    }

    // Nya direkta metoder f�r skott
    public void PlayPlayerShootSound()
    {
        if (playerShootSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(playerShootSound, playerShootVolume);
        }
    }

    public void PlayFlareSound()
    {
        if (flareSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(flareSound, flareVolume);
        }
    }

    public void PlayEnemyShootSound()
    {
        if (enemyShootSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(enemyShootSound, enemyShootVolume);
        }
    }

    public void PlayHitSound()
    {
        if (hitSound != null && sfxSource != null)
        {
            // Bredare pitch-variation
            float randomPitch = Random.Range(0.6f, 1.4f);

            // Spara ursprunglig pitch
            float originalPitch = sfxSource.pitch;

            // S�tt ny pitch
            sfxSource.pitch = randomPitch;

            // Spela ljudet
            sfxSource.PlayOneShot(hitSound, hitVolume);

            // �terst�ll pitch
            sfxSource.pitch = originalPitch;
        }
    }

    public void PlayDeathSound()
    {
        if (deathSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(deathSound, deathVolume);
        }
    }

    public void PlayCombatSound(CombatSoundType type)
    {
        switch (type)
        {
            case CombatSoundType.PlayerShoot:
                PlayPlayerShootSound();
                break;
            case CombatSoundType.EnemyShoot:
                PlayEnemyShootSound();
                break;
            case CombatSoundType.Hit:
                PlayHitSound();
                break;
            case CombatSoundType.Death:
                PlayDeathSound();
                break;
        }
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

        float volume = type switch
        {
            BombSoundType.Drop => bombDropVolume,
            BombSoundType.Falling => bombFallingVolume,
            BombSoundType.Explosion => bombExplosionVolume,
            _ => 0f
        };

        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
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
        if (isRadioEnabled)
        {
            StartRadioSequence();
        }
    }

    public void StartRadioSequence()
    {
        if (isRadioEnabled)
        {
            StartCoroutine(RadioSequence());
        }
    }

    private IEnumerator RadioSequence()
    {
        if (!isRadioEnabled) yield break;

        if (radioStart != null)
        {
            radioSource.PlayOneShot(radioStart, radioVolume);
            yield return new WaitForSeconds(radioStart.length);
        }

        foreach (AudioClip message in radioMessages)
        {
            if (!isRadioEnabled) yield break; // Avbryt om radio st�ngs av under sekvensen

            if (message != null)
            {
                radioSource.PlayOneShot(message, radioVolume);
                yield return new WaitForSeconds(message.length);
            }
        }

        if (isRadioEnabled && radioEnd != null)
        {
            radioSource.PlayOneShot(radioEnd, radioVolume);
        }
    }
    #endregion

    #region Alert Sounds
    public void PlayAirRaidSiren()
    {
        if (airRaidSiren != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(airRaidSiren, sirenVolume);
        }
    }

    public void PlayBossAlert()
    {
        if (bossAlertSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(bossAlertSound, bossAlertVolume);
        }
    }

    public void PlayBoostSound()
    {
        if (boostSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(boostSound, boostVolume);
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
    PlayerShoot,
    EnemyShoot,
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