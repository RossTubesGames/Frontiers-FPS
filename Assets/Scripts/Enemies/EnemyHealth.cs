using UnityEngine;
<<<<<<< HEAD
<<<<<<< HEAD

public class EnemyHealth : MonoBehaviour, Arrows.IDamageable
=======
=======
using FMOD;
>>>>>>> 689838c2751bf5a0b45c716d1a17054e651edacf
public class EnemyHealth : MonoBehaviour,Arrows.IDamageable
>>>>>>> 7ef5aaee30d1fe9049575437171d978c2f706b06
{

    [SerializeField] private float health = 30f;

    public void TakeDamage(float damage)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Cultist");
        health -= damage;

        if (health <= 0f)
            Destroy(gameObject);
    }
}
