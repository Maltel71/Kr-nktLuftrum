using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SimpleMusicManager : MonoBehaviour
{
    [Header("Music per Level - Dra AudioClips hit")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip tutorialMusic; // LÄMNA TOM för ingen musik på Level0
    [SerializeField] private AudioClip level1Music;
    [SerializeField] private AudioClip level2Music;
    [SerializeField] private AudioClip level3Music;
    [SerializeField] private AudioClip level4Music;

    [Header("Test Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool forceStopCurrentMusic = true;
    [SerializeField] private float delayBeforeMusic = 0.5f;

    // FÖRBÄTTRAD Singleton med mer robust hantering
    private static SimpleMusicManager instance;
    public static SimpleMusicManager Instance => instance;

    // Håll koll på vilken scen vi senast spelade musik för
    private string lastScenePlayed = "";
    private bool isInitialized = false;

    private void Awake()
    {
        // Robust singleton-hantering
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (showDebugLogs)
                Debug.Log("[SimpleMusicManager] 🎵 FÖRSTA instansen skapad - blir persistent");
        }
        else if (instance != this)
        {
            if (showDebugLogs)
                Debug.Log("[SimpleMusicManager] ❌ DUPLIKAT hittad - förstör hela objektet");

            Destroy(gameObject);
            return;
        }

        // Prenumerera på scene events (viktigt att göra här!)
        SceneManager.sceneLoaded -= OnSceneLoaded; // Ta bort först för säkerhets skull
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (showDebugLogs)
            Debug.Log("[SimpleMusicManager] 🔧 Scene events registrerade");
    }

    private void Start()
    {
        // Säkerhetskontroll
        if (instance != this) return;

        if (showDebugLogs)
            Debug.Log("[SimpleMusicManager] ▶️ Start() - börjar konfigurera musik");

        StartCoroutine(InitializeMusicSystem());
    }

    private IEnumerator InitializeMusicSystem()
    {
        if (showDebugLogs)
            Debug.Log("[SimpleMusicManager] 🔄 Initialiserar musiksystem...");

        // Vänta lite så att allt annat hinner initialiseras
        yield return new WaitForSeconds(0.2f);

        // Stoppa befintlig musik om inställt
        if (forceStopCurrentMusic)
        {
            yield return StartCoroutine(WaitForAudioManagerAndStop());
        }

        // Sätt musik för nuvarande scen
        yield return StartCoroutine(SetMusicForCurrentSceneCoroutine());

        isInitialized = true;
        if (showDebugLogs)
            Debug.Log("[SimpleMusicManager] ✅ Musiksystem initialiserat");
    }

    private IEnumerator WaitForAudioManagerAndStop()
    {
        int attempts = 0;
        while (AudioManager.Instance == null && attempts < 20)
        {
            if (showDebugLogs)
                Debug.Log($"[SimpleMusicManager] ⏳ Väntar på AudioManager... Försök {attempts + 1}");

            yield return new WaitForSeconds(0.2f);
            attempts++;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBackgroundMusic();
            if (showDebugLogs)
                Debug.Log("[SimpleMusicManager] ⏹️ Stoppade AudioManager's automatiska musik");
        }
        else
        {
            Debug.LogWarning("[SimpleMusicManager] ⚠️ AudioManager hittades inte efter 4 sekunder!");
        }
    }

    private IEnumerator SetMusicForCurrentSceneCoroutine()
    {
        yield return new WaitForSeconds(delayBeforeMusic);

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[SimpleMusicManager] ⚠️ AudioManager fortfarande inte tillgänglig!");
            yield break;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        // Kontrollera om vi redan spelar rätt musik för denna scen
        if (currentScene == lastScenePlayed && isInitialized)
        {
            if (showDebugLogs)
                Debug.Log($"[SimpleMusicManager] ↩️ Musik redan satt för '{currentScene}' - hoppar över");
            yield break;
        }

        AudioClip musicToPlay = GetMusicForScene(currentScene);

        if (showDebugLogs)
            Debug.Log($"[SimpleMusicManager] 🎵 Scen: '{currentScene}' → Musik: {(musicToPlay != null ? musicToPlay.name : "INGEN")}");

        if (musicToPlay != null)
        {
            AudioManager.Instance.PlaySpecificBackgroundMusic(musicToPlay);
            if (showDebugLogs)
                Debug.Log($"[SimpleMusicManager] ✅ Spelar musik: {musicToPlay.name}");
        }
        else
        {
            AudioManager.Instance.StopBackgroundMusic();
            if (showDebugLogs)
                Debug.Log($"[SimpleMusicManager] ⏹️ Stoppar musik för scen: {currentScene}");
        }

        // Hantera motorljud baserat på scen
        HandleEngineSound(currentScene);

        // Uppdatera senast spelade scen
        lastScenePlayed = currentScene;
    }

    private AudioClip GetMusicForScene(string sceneName)
    {
        string lowerSceneName = sceneName.ToLower();

        if (showDebugLogs)
            Debug.Log($"[SimpleMusicManager] 🔍 Analyserar scen: '{sceneName}' (lower: '{lowerSceneName}')");

        // Main Menu / Start Scene
        if (lowerSceneName.Contains("main") || lowerSceneName.Contains("start") ||
            lowerSceneName.Contains("menu") || lowerSceneName.Contains("hub"))
        {
            if (showDebugLogs) Debug.Log("[SimpleMusicManager] → 🏠 Main menu musik");
            return mainMenuMusic;
        }

        // Tutorial / Level0
        if (lowerSceneName.Contains("level0") || lowerSceneName.Contains("tutorial"))
        {
            if (showDebugLogs) Debug.Log("[SimpleMusicManager] → 📚 Tutorial musik");
            return tutorialMusic;
        }

        // Level 1
        if (lowerSceneName.Contains("level1") || lowerSceneName.Contains("level 1"))
        {
            if (showDebugLogs) Debug.Log("[SimpleMusicManager] → 🎮 Level1 musik");
            return level1Music;
        }

        // Level 2
        if (lowerSceneName.Contains("level2") || lowerSceneName.Contains("level 2"))
        {
            if (showDebugLogs) Debug.Log("[SimpleMusicManager] → 🎮 Level2 musik");
            return level2Music;
        }

        // Level 3
        if (lowerSceneName.Contains("level3") || lowerSceneName.Contains("level 3"))
        {
            if (showDebugLogs) Debug.Log("[SimpleMusicManager] → 🎮 Level3 musik");
            return level3Music;
        }

        // Level 4
        if (lowerSceneName.Contains("level4") || lowerSceneName.Contains("level 4"))
        {
            if (showDebugLogs) Debug.Log("[SimpleMusicManager] → 🎮 Level4 musik");
            return level4Music;
        }

        // Loading screen
        if (lowerSceneName.Contains("loading") || lowerSceneName.Contains("load"))
        {
            if (showDebugLogs) Debug.Log("[SimpleMusicManager] → ⏳ Loading (ingen musik)");
            return null;
        }

        // Fallback
        if (showDebugLogs) Debug.Log("[SimpleMusicManager] → 🔄 Fallback till Level1 musik");
        return level1Music;
    }

    private void HandleEngineSound(string sceneName)
    {
        string lowerSceneName = sceneName.ToLower();

        // Bara starta motor på level-scener, inte på main/menu
        bool shouldPlayEngine = lowerSceneName.Contains("level") &&
                               !lowerSceneName.Contains("main") &&
                               !lowerSceneName.Contains("menu") &&
                               !lowerSceneName.Contains("loading");

        if (shouldPlayEngine)
        {
            // Starta motor om den inte redan spelar
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StartEngineSound();
                if (showDebugLogs)
                    Debug.Log($"[SimpleMusicManager] 🚁 Motor startad för: {sceneName}");
            }
        }
        else
        {
            // Stoppa motor för main menu etc.
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopEngineSound();
                if (showDebugLogs)
                    Debug.Log($"[SimpleMusicManager] ⏹️ Motor stoppad för: {sceneName}");
            }
        }
    }

    // DETTA ÄR DEN VIKTIGA METODEN SOM MÅSTE FUNGERA!
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // EXTRA kontroll att detta är rätt instans
        if (instance != this)
        {
            if (showDebugLogs)
                Debug.Log($"[SimpleMusicManager] ❌ OnSceneLoaded kallad på fel instans - ignorerar");
            return;
        }

        if (showDebugLogs)
            Debug.Log($"[SimpleMusicManager] 🚀 SCENE CHANGE DETECTED: '{scene.name}' (Mode: {mode})");

        // Reset för att tvinga musik-uppdatering
        lastScenePlayed = "";

        // Hantera scenbytet
        StartCoroutine(HandleSceneChange(scene.name));
    }

    private IEnumerator HandleSceneChange(string sceneName)
    {
        if (showDebugLogs)
            Debug.Log($"[SimpleMusicManager] 🔄 Hanterar scenändring till: {sceneName}");

        // Vänta lite så scenen hinner initialiseras
        yield return new WaitForSeconds(0.3f);

        // Dubbelkolla att vi fortfarande är den aktiva instansen
        if (instance != this)
        {
            if (showDebugLogs)
                Debug.Log("[SimpleMusicManager] ❌ Instance ändrad under scenändring - avbryter");
            yield break;
        }

        yield return StartCoroutine(SetMusicForCurrentSceneCoroutine());
    }

    private void OnDestroy()
    {
        if (showDebugLogs)
            Debug.Log($"[SimpleMusicManager] 💀 OnDestroy kallad - är detta instance? {instance == this}");

        // Bara städa upp om detta är den aktiva instansen
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;

            if (showDebugLogs)
                Debug.Log("[SimpleMusicManager] 🧹 Instance förstörd - städat upp events");
        }
    }

    // Debug-metoder för testing
    [ContextMenu("Show Current Status")]
    public void ShowCurrentStatus()
    {
        Debug.Log($"[SimpleMusicManager] === STATUS RAPPORT ===");
        Debug.Log($"  Instance aktiv: {instance == this}");
        Debug.Log($"  Är initialiserad: {isInitialized}");
        Debug.Log($"  Current Scene: {SceneManager.GetActiveScene().name}");
        Debug.Log($"  Senast spelade: {lastScenePlayed}");
        Debug.Log($"  AudioManager finns: {AudioManager.Instance != null}");
        Debug.Log($"  GameObject finns: {gameObject != null}");

        if (AudioManager.Instance != null)
        {
            Debug.Log($"  Musik enabled: {AudioManager.Instance.IsMusicEnabled()}");
        }
    }

    [ContextMenu("Test Scene Change Detection")]
    public void TestSceneChangeDetection()
    {
        Debug.Log("[SimpleMusicManager] 🧪 TESTAR SCENE CHANGE DETECTION");
        lastScenePlayed = ""; // Reset
        StartCoroutine(SetMusicForCurrentSceneCoroutine());
    }

    [ContextMenu("Force Music Reload")]
    public void ForceMusicReload()
    {
        lastScenePlayed = "";
        StartCoroutine(InitializeMusicSystem());
    }

    // Publik metod för manuell kontroll
    public void SetMusicForScene(string sceneName)
    {
        if (instance != this) return;

        AudioClip musicToPlay = GetMusicForScene(sceneName);

        if (AudioManager.Instance != null)
        {
            if (musicToPlay != null)
            {
                AudioManager.Instance.PlaySpecificBackgroundMusic(musicToPlay);
            }
            else
            {
                AudioManager.Instance.StopBackgroundMusic();
            }
            lastScenePlayed = sceneName;
        }
    }
}