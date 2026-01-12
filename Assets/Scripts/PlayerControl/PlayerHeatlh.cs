using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float health = 100f;

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0f)
            Debug.Log("Player died");
    }
}
