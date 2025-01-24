using UnityEngine;

public class RaidTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by: {other.gameObject.name} with tag: {other.tag}");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected, playing siren");
            AudioManager.Instance?.PlayAirRaidSiren();
        }
    }
}