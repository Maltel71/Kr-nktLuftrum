using UnityEngine;

public class Test : MonoBehaviour
{

    private SmokeEffectScript smokeEffect;

    void Start()
    {
        smokeEffect = GetComponent<SmokeEffectScript>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            smokeEffect.TakeDamage(10); // Damage by 10
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            smokeEffect.HealHealth(10); // Heal by 10
        }
    }
}