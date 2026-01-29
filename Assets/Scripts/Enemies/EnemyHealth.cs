using UnityEngine;
<<<<<<< HEAD

public class EnemyHealth : MonoBehaviour, Arrows.IDamageable
=======
public class EnemyHealth : MonoBehaviour,Arrows.IDamageable
>>>>>>> 7ef5aaee30d1fe9049575437171d978c2f706b06
{

    [SerializeField] private float health = 30f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
            Destroy(gameObject);
    }
}
