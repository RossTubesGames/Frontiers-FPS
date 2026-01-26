using UnityEngine;

public class KeyBossHealth : MonoBehaviour
{
    [Header("Key drop on death")]
    [SerializeField] private GameObject key2Prefab; // assign the Key2 prefab in Inspector
    [SerializeField] private float health = 30f;

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0f)
        {
            DropKey();
            Destroy(gameObject);
        }
    }

    private void DropKey()
    {
        // Only drop if a prefab is assigned
        if (key2Prefab == null)
        {
            Debug.LogWarning("KeyBossHealth: No key prefab assigned.");
            return;
        }

        // Spawn the key at the boss position (you can add an offset if needed)
        Instantiate(key2Prefab, transform.position, Quaternion.identity);
    }
}
