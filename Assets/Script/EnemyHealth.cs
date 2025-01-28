using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Vector3 healthBarScale = new Vector3(0.05f, 0.05f, 0.05f);

    // Referens till själva Slider-komponenten
    private Slider healthSlider;
    private GameObject healthBarInstance;

    private void Start()
    {
        currentHealth = maxHealth;
        CreateHealthBar();
    }

    private void CreateHealthBar()
    {
        // Skapa health bar instansen
        healthBarInstance = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity, transform);

        // Hitta Slider-komponenten i hierarkin
        healthSlider = healthBarInstance.GetComponentInChildren<Slider>();

        if (healthSlider != null)
        {
            // Konfigurera Slider
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            Debug.Log($"Health bar created. Slider configured with max: {maxHealth}, current: {currentHealth}");
        }
        else
        {
            Debug.LogError("Kunde inte hitta Slider-komponenten i health bar prefaben!");
        }

        // Konfigurera Canvas
        Canvas canvas = healthBarInstance.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;
        }

        // Sätt scale
        healthBarInstance.transform.localScale = healthBarScale;
    }

    private void Update()
    {
        if (healthBarInstance != null)
        {
            // Vänd health bar mot kameran
            healthBarInstance.transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"Enemy taking damage: {damage}, Current health: {currentHealth}");

        // Uppdatera UI
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
            Debug.Log($"Slider value updated to: {healthSlider.value}");
        }
        else
        {
            Debug.LogError("Health slider reference is missing!");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
        Destroy(gameObject);
    }
}