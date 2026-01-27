using UnityEngine;

public class EnemyHealth : MonoBehaviour, Arrows.IDamageable
{

    [SerializeField] private float health = 30f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
            Destroy(gameObject);
    }
}
