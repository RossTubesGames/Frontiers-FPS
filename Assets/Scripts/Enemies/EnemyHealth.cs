using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount);
}

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float health = 30f;

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
