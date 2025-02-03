using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Boss Settings")]
    [SerializeField] private bool isBoss = false;

    [Header("UI")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Vector3 healthBarScale = new Vector3(0.05f, 0.05f, 0.05f);

    [Header("Crash Settings")]
    [SerializeField] private float crashSpeed = 500f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private bool rotateClockwise = true;

    [Header("Effects")]
    [SerializeField] private ParticleSystem smokeEffect;    // Rök när planet störtar

    private Slider healthSlider;
    private GameObject healthBarInstance;
    private CameraShake cameraShake;
    private ScoreManager scoreManager;
    private bool isDying = false;

    private void Start()
    {
        currentHealth = maxHealth;
        CreateHealthBar();
        cameraShake = CameraShake.Instance;
        scoreManager = ScoreManager.Instance;

        // Stäng av rökeffekten vid start
        if (smokeEffect != null) smokeEffect.Stop();
    }

    private void Update()
    {
        if (healthBarInstance != null)
        {
            healthBarInstance.transform.rotation = Camera.main.transform.rotation;
        }

        if (isDying)
        {
            HandleCrashing();
        }
    }

    private void HandleCrashing()
    {
        // Uppdatera position
        Vector3 pos = transform.position;
        pos.y -= crashSpeed * Time.deltaTime;
        transform.position = pos;

        // Rotera planet baserat på inställning
        float rotationAmount = rotateClockwise ? rotationSpeed : -rotationSpeed;
        transform.Rotate(0, 0, rotationAmount * Time.deltaTime);

        // Förstör när det når marken
        if (pos.y <= 0)
        {
            AudioManager.Instance?.PlayBombSound(BombSoundType.Explosion);
            Destroy(gameObject);
        }
    }

    private void StartDying()
    {
        isDying = true;

        // Deaktivera alla scripts som kan påverka position
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // Inaktivera alla colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Starta rökeffekten
        if (smokeEffect != null)
        {
            smokeEffect.Play();
            var main = smokeEffect.main;
            main.simulationSpeed = 2f; // Snabbare rökeffekt
        }

        // Lägg till poäng
        if (scoreManager != null)
        {
            if (isBoss)
            {
                scoreManager.AddBossPoints();
                if (cameraShake != null)
                {
                    cameraShake.ShakaCameraVidBossDöd();
                }
            }
            else
            {
                scoreManager.AddEnemyShipPoints();
            }
        }

        // Ta bort health bar
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }

        // Spela dödsljud
        AudioManager.Instance?.PlayCombatSound(CombatSoundType.Death);
    }

    public void TakeDamage(float damage)
    {
        if (isDying) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            StartDying();
        }
    }

    private void CreateHealthBar()
    {
        healthBarInstance = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity, transform);
        healthSlider = healthBarInstance.GetComponentInChildren<Slider>();

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        Canvas canvas = healthBarInstance.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;
        }

        healthBarInstance.transform.localScale = healthBarScale;
    }
}