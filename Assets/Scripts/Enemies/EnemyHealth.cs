using UnityEngine;
<<<<<<< HEAD
public class EnemyHealth : MonoBehaviour,Arrows.IDamageable
=======

public class EnemyHealth : MonoBehaviour, Arrows.IDamageable
>>>>>>> c7af2187104436ba9dc1de60ea18bda460beeed2
{

    [SerializeField] private float health = 30f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
            Destroy(gameObject);
    }
}
