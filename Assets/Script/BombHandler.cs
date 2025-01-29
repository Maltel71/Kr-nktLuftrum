using UnityEngine;

public class BombHandler : MonoBehaviour
{
    private AudioManager audioManager;
    private CameraShake cameraShake;
    private bool hasPlayedFallingSound = false;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        cameraShake = CameraShake.Instance;

        if (cameraShake == null)
        {
            Debug.LogWarning("Kunde inte hitta CameraShake!");
        }
    }

    private void Update()
    {
        if (!hasPlayedFallingSound && GetComponent<Rigidbody>().linearVelocity.y < 0)
        {
            audioManager?.PlayHitSound();
            hasPlayedFallingSound = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Bomb kolliderade med: {collision.gameObject.name}");

        if (cameraShake != null)
        {
            Debug.Log("Aktiverar kameraskakning för bomb");
            cameraShake.ShakaCameraVidBomb();
        }

        audioManager?.PlayBombSound();
        Destroy(gameObject);
    }
}