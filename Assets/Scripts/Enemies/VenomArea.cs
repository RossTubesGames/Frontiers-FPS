using UnityEngine;

public class VenomArea : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private string playerTag = "Player";

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 15f;

    private float lifeTimer;
    private PlayerHealth playerHealth;

    public void SetLifetime(float seconds)
    {
        lifetime = Mathf.Max(0.1f, seconds);
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (playerHealth == null) return;

        playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerHealth = other.GetComponent<PlayerHealth>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (playerHealth != null && other.gameObject == playerHealth.gameObject)
        {
            playerHealth = null;
        }
    }
}
