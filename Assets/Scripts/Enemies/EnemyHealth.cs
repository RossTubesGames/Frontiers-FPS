using UnityEngine;

<<<<<<< HEAD
public class EnemyHealth : MonoBehaviour
=======
public class EnemyHealth : MonoBehaviour,Arrows.IDamageable
>>>>>>> 5ac2df7a4d13fde6e827e0ef1f4f7d5b97ade46f
{

    [SerializeField] private float health = 30f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
            Destroy(gameObject);
    }
}