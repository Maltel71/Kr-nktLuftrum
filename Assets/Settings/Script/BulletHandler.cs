using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject smokeTrailPrefab;
    private GameObject currentSmokeTrail;

    private void Start()
    {
        // Create muzzle flash at bullet spawn
        if (muzzleFlashPrefab != null)
        {
            GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, transform.position, transform.rotation);
            Destroy(muzzleFlash, 0.1f);
        }

        // Create smoke trail following bullet
        if (smokeTrailPrefab != null)
        {
            currentSmokeTrail = Instantiate(smokeTrailPrefab, transform.position, transform.rotation);
            currentSmokeTrail.transform.SetParent(transform);
        }
    }

    private void OnDestroy()
    {
        // Keep smoke trail briefly after bullet is destroyed
        if (currentSmokeTrail != null)
        {
            currentSmokeTrail.transform.SetParent(null);
            Destroy(currentSmokeTrail, 1f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Om kulan träffar något som inte är en annan kula
        if (!collision.gameObject.CompareTag("Bullet"))
        {
            AudioManager.Instance?.PlayHitSound();
            Destroy(gameObject);
        }
    }
}