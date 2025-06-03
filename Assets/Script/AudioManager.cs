using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
            InitializeAudioPool();
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
    [SerializeField] private AudioSource engineSource;
    [SerializeField] private AudioSource radioSource;

    // Snabba ljud källor för låg latency
    [SerializeField] private AudioSource bulletsSource;
    [SerializeField] private AudioSource missilesSource;
    [SerializeField] private AudioSource bombsSource;
    [SerializeField] private AudioSource explosionsSource;
    [SerializeField] private AudioSource specialSource;
    #endregion

    #region Master Volume Settings
    [Header("Master Volume Settings")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float engineVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float radioVolume = 0.7f;
    #endregion

    #region Music Control
    [Header("Music Control")]
    [SerializeField] private bool isMusicEnabled = true;
    [SerializeField] private bool isRadioEnabled = true;
    #endregion

    #region Audio Clips - Grundljud
    [Header("Grundljud")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip engineSound;
    #endregion

    #region Audio Clips - Bullets & Vapen
    [Header("Bullets & Vapen Audio")]
    [SerializeField] private AudioClip playerShootSound;
    [SerializeField] private AudioClip enemyShootSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip flareSound;
    [SerializeField, Range(0f, 1f)] private float playerShootVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float enemyShootVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float hitVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float flareVolume = 0.7f;
    #endregion

    #region Audio Clips - Missiles
    [Header("Missile Audio")]
    [SerializeField] private AudioClip missileLaunchSound;
    [SerializeField] private AudioClip missileHitSound;
    [SerializeField, Range(0f, 1f)] private float missileLaunchVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float missileHitVolume = 0.8f;
    #endregion

    #region Audio Clips - Bombs
    [Header("Bomb Audio")]
    [SerializeField] private AudioClip bombDropSound;
    [SerializeField] private AudioClip bombFallingSound;
    [SerializeField] private AudioClip bombExplosionSound;
    [SerializeField, Range(0f, 1f)] private float bombDropVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float bombFallingVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float bombExplosionVolume = 0.9f;
    #endregion

    #region Audio Clips - Special & Alerts
    [Header("Special & Alert Audio")]
    [SerializeField] private AudioClip boostSound;
    [SerializeField] private AudioClip airRaidSiren;
    [SerializeField] private AudioClip bossAlertSound;
    [SerializeField, Range(0f, 1f)] private float boostVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float sirenVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float bossAlertVolume = 0.8f;
    #endregion

    #region Radio System
    [Header("Radio System")]
    [SerializeField] private AudioClip radioStart;
    [SerializeField] private AudioClip radioEnd;
    [SerializeField] private AudioClip[] radioMessages;
    [SerializeField] private float radioDelay = 30f;
    #endregion

    #region Miljöljud
    [Header("Fordons & Miljöljud")]
    [SerializeField] private AudioClip helicopterSound;
    [SerializeField] private AudioClip boatEngineSound;
    [SerializeField] private AudioClip tankMovementSound;
    [SerializeField] private AudioClip waterSplashSound;
    [SerializeField] private AudioClip sandImpactSound;
    [SerializeField] private AudioClip metalHitSound;
    [SerializeField, Range(0f, 1f)] private float vehicleVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float environmentVolume = 0.5f;
    #endregion

    #region Audio Pooling
    [Header("Audio Pooling")]
    [SerializeField] private int audioPoolSize = 15;
    private Queue<AudioSource> audioSourcePool;
    #endregion

    #region Prewarming - PRELOAD FÖR ZERO LATENCY
    [Header("Prewarming")]
    [SerializeField] private bool prewarmSoundsOnStart = true;
    [SerializeField] private bool usePreloadedSounds = true;
    #endregion

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

    private void SetupAudioSources()
    {
        // Konfigurera alla ljudkällor för optimal prestanda
        SetupAudioSource(ref musicSource, true, false);
        SetupAudioSource(ref engineSource, true, false);
        SetupAudioSource(ref radioSource, false, false);

        // Snabba källor för action-ljud
        SetupAudioSource(ref bulletsSource, false, true);
        SetupAudioSource(ref missilesSource, false, true);
        SetupAudioSource(ref bombsSource, false, true);
        SetupAudioSource(ref explosionsSource, false, true);
        SetupAudioSource(ref specialSource, false, true);
    }

    private void SetupAudioSource(ref AudioSource source, bool loop, bool lowLatency)
    {
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }

        source.loop = loop;
        source.playOnAwake = false;

        if (lowLatency)
        {
            // KRITISKA inställningar för låg latency
            source.priority = 0; // HÖGSTA prioritet (0 = högst)
            source.bypassEffects = true; // Ingen processing
            source.bypassListenerEffects = true; // Ingen listener processing
            source.bypassReverbZones = true; // Ingen reverb
            source.rolloffMode = AudioRolloffMode.Linear; // Enklare beräkning
            source.dopplerLevel = 0f; // Ingen doppler effect
            source.spread = 0f; // Mono ljud
            source.spatialBlend = 0f; // 2D ljud för snabbhet
        }
    }

    private void InitializeAudioPool()
    {
        audioSourcePool = new Queue<AudioSource>();
        for (int i = 0; i < audioPoolSize; i++)
        {
            GameObject pooledSource = new GameObject($"PooledAudioSource_{i}");
            pooledSource.transform.SetParent(transform);
            AudioSource source = pooledSource.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.priority = 200;
            audioSourcePool.Enqueue(source);
        }
    }

    private void LoadAudioSettings()
    {
        isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        isRadioEnabled = PlayerPrefs.GetInt("RadioEnabled", 1) == 1;
        Debug.Log($"[AudioManager] Ljudinställningar laddade - Musik: {isMusicEnabled}, Radio: {isRadioEnabled}");
    }

    private void StartGameAudio()
    {
        if (isMusicEnabled)
        {
            PlayBackgroundMusic();
        }

        // StartEngine(); // <-- KOMMENTERA BORT ELLER TA BORT

        if (isRadioEnabled)
        {
            StartCoroutine(PlayDelayedRadio());
        }
    }

    private IEnumerator PrewarmAllSounds()
    {
        yield return null;
        Debug.Log("[AudioManager] Starting ULTRA sound prewarming...");

        // Preload de mest använda ljuden i sina källor
        if (usePreloadedSounds)
        {
            PreloadCriticalSounds();
        }

        GameObject tempObj = new GameObject("AudioPrewarmer");
        AudioSource prewarmer = tempObj.AddComponent<AudioSource>();
        prewarmer.volume = 0f;
        prewarmer.playOnAwake = false;
        prewarmer.priority = 256;

        AudioClip[] clipsToPrewarm = new AudioClip[]
        {
            // KRITISKA ljud först (mest latency-känsliga)
            playerShootSound,
            enemyShootSound,
            hitSound,
            
            // Explosioner
            bombExplosionSound,
            missileHitSound,
            
            // Vapen
            missileLaunchSound,
            flareSound,
            
            // Bombs
            bombDropSound,
            bombFallingSound,
            
            // Special
            boostSound,
            airRaidSiren,
            bossAlertSound
        };

        foreach (var clip in clipsToPrewarm)
        {
            if (clip != null)
            {
                prewarmer.clip = clip;
                prewarmer.Play();
                yield return new WaitForSeconds(0.02f);
                prewarmer.Stop();
            }
        }

        Destroy(tempObj);
        Debug.Log("[AudioManager] ULTRA sound prewarming complete");
    }

    private void PreloadCriticalSounds()
    {
        // Preload de mest kritiska ljuden i sina dedikerade källor
        if (bulletsSource != null && playerShootSound != null)
        {
            bulletsSource.clip = playerShootSound;
        }

        if (explosionsSource != null && bombExplosionSound != null)
        {
            explosionsSource.clip = bombExplosionSound;
        }

        if (missilesSource != null && missileLaunchSound != null)
        {
            missilesSource.clip = missileLaunchSound;
        }

        Debug.Log("[AudioManager] Critical sounds preloaded for zero-latency playback");
    }
    #endregion

    #region Music Control Methods
    public void ToggleMusic()
    {
        isMusicEnabled = !isMusicEnabled;

        if (isMusicEnabled)
        {
            if (musicSource != null && backgroundMusic != null)
            {
                musicSource.Play();
                Debug.Log("[AudioManager] Musik påslagen");
            }
        }
        else
        {
            if (musicSource != null)
            {
                musicSource.Pause();
                Debug.Log("[AudioManager] Musik avstängd");
            }
        }

        PlayerPrefs.SetInt("MusicEnabled", isMusicEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;

        if (enabled)
        {
            if (musicSource != null && backgroundMusic != null)
            {
                musicSource.Play();
            }
        }
        else
        {
            if (musicSource != null)
            {
                musicSource.Pause();
            }
        }

        PlayerPrefs.SetInt("MusicEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleRadio()
    {
        isRadioEnabled = !isRadioEnabled;

        if (!isRadioEnabled)
        {
            if (radioSource != null)
            {
                radioSource.Stop();
                Debug.Log("[AudioManager] Radio avstängd");
            }
        }
        else
        {
            Debug.Log("[AudioManager] Radio påslagen");
        }

        PlayerPrefs.SetInt("RadioEnabled", isRadioEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    // Lägg till EFTER ToggleRadio() metoden:

    public void PlaySpecificBackgroundMusic(AudioClip musicClip)
    {
        if (!isMusicEnabled || musicClip == null || musicSource == null)
            return;

        if (musicSource.clip != musicClip)
        {
            musicSource.Stop();
            musicSource.clip = musicClip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.loop = true;
            musicSource.Play();

            Debug.Log($"[AudioManager] Spelar ny bakgrundsmusik: {musicClip.name}");
        }
        else if (!musicSource.isPlaying)
        {
            musicSource.Play();
            Debug.Log($"[AudioManager] Återupptar bakgrundsmusik: {musicClip.name}");
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            Debug.Log("[AudioManager] Bakgrundsmusik stoppad");
        }
    }

    public void StartEngineSound()
    {
        Debug.Log($"[AudioManager] StartEngineSound() anropad - engineSound: {engineSound != null}, engineSource: {engineSource != null}, isEnginePlaying: {isEnginePlaying}");

        if (engineSound != null && engineSource != null && !isEnginePlaying)
        {
            engineSource.clip = engineSound;
            engineSource.volume = engineVolume * masterVolume;
            engineSource.Play();
            isEnginePlaying = true;
            Debug.Log("[AudioManager] ✈️ Motor startad");
        }
        else
        {
            Debug.Log("[AudioManager] ❌ Kunde inte starta motor - kontrollera villkoren ovan");
        }
    }

    public void StopEngineSound()
    {
        Debug.Log($"[AudioManager] StopEngineSound() anropad - engineSource: {engineSource != null}, isEnginePlaying: {isEnginePlaying}");

        if (engineSource != null && isEnginePlaying)
        {
            engineSource.Stop();
            isEnginePlaying = false;
            Debug.Log("[AudioManager] ⏹️ Motor stoppad");
        }
        else
        {
            Debug.Log("[AudioManager] ❌ Kunde inte stoppa motor eller redan stoppad");
        }
    }

    public bool IsMusicEnabled() => isMusicEnabled;
    public bool IsRadioEnabled() => isRadioEnabled;
    #endregion

    #region Background Audio
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null && isMusicEnabled)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }
    }

    private void StartEngine()
    {
        if (engineSound != null && engineSource != null && !isEnginePlaying)
        {
            engineSource.clip = engineSound;
            engineSource.volume = engineVolume * masterVolume;
            engineSource.Play();
            isEnginePlaying = true;
        }
    }
    #endregion

    #region Bullet & Weapon Sounds - ULTRA LOW LATENCY
    public void PlayPlayerShootSound()
    {
        if (usePreloadedSounds && bulletsSource != null && bulletsSource.clip == playerShootSound)
        {
            PlayPreloadedSound(playerShootSound, bulletsSource, playerShootVolume);
        }
        else
        {
            PlayFastSound(playerShootSound, bulletsSource, playerShootVolume);
        }
    }

    public void PlayEnemyShootSound()
    {
        PlayFastSound(enemyShootSound, bulletsSource, enemyShootVolume);
    }

    public void PlayHitSound()
    {
        // Använd slumpmässig pitch för variation men snabbare implementation
        if (explosionsSource != null)
        {
            float randomPitch = Random.Range(0.8f, 1.2f);
            explosionsSource.pitch = randomPitch;
            PlayFastSound(hitSound, explosionsSource, hitVolume);
            // Återställ pitch direkt efter
            explosionsSource.pitch = 1f;
        }
    }

    public void PlayFlareSound()
    {
        PlayFastSound(flareSound, specialSource, flareVolume);
    }
    #endregion

    #region Missile Sounds
    public void PlayMissileLaunchSound()
    {
        PlayFastSound(missileLaunchSound, missilesSource, missileLaunchVolume);
    }

    public void PlayMissileHitSound()
    {
        PlayFastSound(missileHitSound, missilesSource, missileHitVolume);
        // Spela även explosionsljud
        PlayBombSound(BombSoundType.Explosion);
    }
    #endregion

    #region Bomb Sounds
    public void PlayBombSound(BombSoundType type)
    {
        AudioClip clip = null;
        float volume = 0f;

        switch (type)
        {
            case BombSoundType.Drop:
                clip = bombDropSound;
                volume = bombDropVolume;
                break;
            case BombSoundType.Falling:
                clip = bombFallingSound;
                volume = bombFallingVolume;
                break;
            case BombSoundType.Explosion:
                clip = bombExplosionSound;
                volume = bombExplosionVolume;
                break;
        }

        if (clip != null)
        {
            if (type == BombSoundType.Explosion)
            {
                PlayFastSound(clip, explosionsSource, volume);
            }
            else
            {
                PlayFastSound(clip, bombsSource, volume);
            }
        }
    }

    // Bakåtkompatibilitetsmetoder
    public void PlayBombDropSound()
    {
        PlayBombSound(BombSoundType.Drop);
    }

    public void PlayBombFallingSound()
    {
        PlayBombSound(BombSoundType.Falling);
    }

    public void PlayBombExplosionSound()
    {
        PlayBombSound(BombSoundType.Explosion);
    }
    #endregion

    #region Combat Sounds
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
        }
    }

    // Bakåtkompatibilitet
    public void PlayShootSound()
    {
        PlayPlayerShootSound();
    }

    public void PlayDeathSound()
    {
        // Tom metod för bakåtkompatibilitet - ingen deathsound som du ville
    }
    #endregion

    #region Special Sounds & Alerts
    public void PlayBoostSound()
    {
        PlayFastSound(boostSound, specialSource, boostVolume);
    }

    public void PlayAirRaidSiren()
    {
        PlayFastSound(airRaidSiren, specialSource, sirenVolume);
    }

    public void PlayBossAlert()
    {
        PlayFastSound(bossAlertSound, specialSource, bossAlertVolume);
    }
    #endregion

    #region Radio System
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
            radioSource.PlayOneShot(radioStart, radioVolume * masterVolume);
            yield return new WaitForSeconds(radioStart.length);
        }

        foreach (AudioClip message in radioMessages)
        {
            if (!isRadioEnabled) yield break;

            if (message != null)
            {
                radioSource.PlayOneShot(message, radioVolume * masterVolume);
                yield return new WaitForSeconds(message.length);
            }
        }

        if (isRadioEnabled && radioEnd != null)
        {
            radioSource.PlayOneShot(radioEnd, radioVolume * masterVolume);
        }
    }
    #endregion

    #region Fordons- och Miljöljud
    public void PlayVehicleEngineSound(VehicleType vehicleType, Vector3 position)
    {
        AudioClip clip = vehicleType switch
        {
            VehicleType.Helicopter => helicopterSound,
            VehicleType.Boat => boatEngineSound,
            VehicleType.Tank => tankMovementSound,
            _ => null
        };

        if (clip != null)
        {
            PlaySoundAtPosition(clip, position, vehicleVolume);
        }
    }

    public void PlayEnvironmentalSound(EnvironmentType environment, Vector3 position)
    {
        AudioClip clip = environment switch
        {
            EnvironmentType.Water => waterSplashSound,
            EnvironmentType.Desert => sandImpactSound,
            EnvironmentType.Metal => metalHitSound,
            _ => null
        };

        if (clip != null)
        {
            PlaySoundAtPosition(clip, position, environmentVolume);
        }
    }

    public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetPooledAudioSource();
        if (source != null)
        {
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume * masterVolume;
            source.spatialBlend = 1f; // 3D ljud
            source.Play();

            StartCoroutine(ReturnSourceAfterPlay(source, clip.length));
        }
    }
    #endregion

    #region Audio Pool Management
    private AudioSource GetPooledAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            return audioSourcePool.Dequeue();
        }

        GameObject newSource = new GameObject("TempAudioSource");
        newSource.transform.SetParent(transform);
        return newSource.AddComponent<AudioSource>();
    }

    private void ReturnToPool(AudioSource source)
    {
        if (source != null)
        {
            source.Stop();
            source.clip = null;
            if (audioSourcePool.Count < audioPoolSize)
            {
                audioSourcePool.Enqueue(source);
            }
            else
            {
                Destroy(source.gameObject);
            }
        }
    }

    private IEnumerator ReturnSourceAfterPlay(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        ReturnToPool(source);
    }
    #endregion

    #region Optimerad ljudavspelning - ULTRA LOW LATENCY
    private void PlayFastSound(AudioClip clip, AudioSource source, float volume)
    {
        if (clip == null || source == null) return;

        // DIREKT avspelning utan att stoppa - låter flera ljud spelas samtidigt
        source.volume = volume * masterVolume;
        source.pitch = 1f;
        source.PlayOneShot(clip); // PlayOneShot är faktiskt snabbare för korta ljud
    }

    // ALTERNATIV metod för ännu lägre latency - kräver förladdat ljud
    private void PlayPreloadedSound(AudioClip clip, AudioSource source, float volume)
    {
        if (clip == null || source == null) return;

        // Om samma ljud redan är laddat, spela direkt
        if (source.clip == clip)
        {
            source.volume = volume * masterVolume;
            source.Play();
        }
        else
        {
            // Preload och spela
            source.clip = clip;
            source.volume = volume * masterVolume;
            source.Play();
        }
    }
    #endregion

    #region Volume Control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
    }

    public void SetEngineVolume(float volume)
    {
        engineVolume = Mathf.Clamp01(volume);
        if (engineSource != null)
            engineSource.volume = engineVolume * masterVolume;
    }

    public void SetRadioVolume(float volume)
    {
        radioVolume = Mathf.Clamp01(volume);
        if (radioSource != null)
            radioSource.volume = radioVolume * masterVolume;
    }

    private void UpdateAllVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        if (engineSource != null)
            engineSource.volume = engineVolume * masterVolume;
        // AudioSources för vapen uppdateras vid nästa avspelning
    }

    public void StopAllSounds()
    {
        if (musicSource != null) musicSource.Stop();
        if (engineSource != null) engineSource.Stop();
        if (radioSource != null) radioSource.Stop();
        if (bulletsSource != null) bulletsSource.Stop();
        if (missilesSource != null) missilesSource.Stop();
        if (bombsSource != null) bombsSource.Stop();
        if (explosionsSource != null) explosionsSource.Stop();
        if (specialSource != null) specialSource.Stop();
        StopAllCoroutines();
    }
    #endregion
}

#region Enums
public enum CombatSoundType
{
    PlayerShoot,
    EnemyShoot,
    Hit
}

public enum BombSoundType
{
    Drop,
    Falling,
    Explosion
}

public enum VehicleType
{
    Helicopter,
    Boat,
    Tank,
    Plane
}

public enum EnvironmentType
{
    Water,
    Desert,
    Metal,
    Grass
}
#endregion