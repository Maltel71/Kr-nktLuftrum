using UnityEngine;

public class Test : MonoBehaviour
{
    // In another script:
    private SmokeEffectScript smokeEffect;

    void Start()
    {
        smokeEffect = GetComponent<SmokeEffectScript>();
    }

    void Update()
    {
        // Test with keyboard
        if (Input.GetKeyDown(KeyCode.Space))
        {
            smokeEffect.TakeDamage(10); // Damage by 10 each time space is pressed
        }
    }
}