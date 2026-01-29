using UnityEngine;
using FMOD;
public class EnemyHealth : MonoBehaviour,Arrows.IDamageable
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
