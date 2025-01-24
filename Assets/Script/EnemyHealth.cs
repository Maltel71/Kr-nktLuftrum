using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Vector3 sliderOffset = new Vector3(0, 2f, 0);
    private Camera mainCamera;

    [Header("Destruction Effects")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float destroyDelay = 0.5f;
    [SerializeField] private AudioClip explosionSound;

    private AudioSource audioSource;

    private void Start()
    {
        currentHealth = maxHealth;
        mainCamera = Camera.main;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && explosionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (healthSlider != null && mainCamera != null)
        {
            healthSlider.transform.position = mainCamera.WorldToScreenPoint(transform.position + sliderOffset);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        if (healthSlider != null)
        {
            Destroy(healthSlider.gameObject);
        }

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        var colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        Destroy(gameObject, destroyDelay);
    }
}