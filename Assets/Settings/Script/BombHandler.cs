using UnityEngine;

public class BombHandler : MonoBehaviour
{
    private AudioManager audioManager;
    private bool hasPlayedFallingSound = false;

    private void Start()
    {
        audioManager = AudioManager.Instance;
    }

    private void Update()
    {
        if (!hasPlayedFallingSound && GetComponent<Rigidbody>().linearVelocity.y < 0)
        {
            audioManager?.PlayHitSound();  // Använd den befintliga HitSound för fallande ljud
            hasPlayedFallingSound = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        audioManager?.PlayBombSound();  // Använd den befintliga BombSound
        Destroy(gameObject);
    }
}