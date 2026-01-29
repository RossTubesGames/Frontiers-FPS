using UnityEngine;

using FMOD;
public class EnemyHealth : MonoBehaviour,Arrows.IDamageable
{
    [SerializeField] private float health = 30f;

    private HitMarker hitMarker;

    private void Awake()
    {
        hitMarker = FindFirstObjectByType<HitMarker>();
    }

    public void TakeDamage(float damage)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Cultist");
        health -= damage;

        if (hitMarker != null)
            hitMarker.Show();

        if (health <= 0f)
            Destroy(gameObject);
    }
}