using UnityEngine;

public class DeathBoarder : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private bool instantKill = true;

    [Header("Damage (if not instant kill)")]
    [SerializeField] private float damageAmount = 9999f;

    [Header("Filter")]
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        // Helps avoid forgetting trigger.
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null)
        {
            // If your collider is on a child object, try parent:
            health = other.GetComponentInParent<PlayerHealth>();
        }

        if (health == null)
            return;

        if (instantKill)
        {
            // Guaranteed kill regardless of current health.
            health.TakeDamage(health.Health);
        }
        else
        {
            health.TakeDamage(damageAmount);
        }
    }
}