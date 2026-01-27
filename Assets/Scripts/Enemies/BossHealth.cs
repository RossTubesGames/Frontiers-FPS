using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [SerializeField] private BossHealthbar bossHealthbar;
    public float bosshealth = 100f;

    public void TakeDamage(float damage)
    {
        bosshealth -= damage;
        bossHealthbar.UpdateBossHealthBar();

        if (bosshealth <= 0f)
        {
            Destroy(gameObject);
            bossHealthbar.DestroyHealthBar(); 
        }
           

    }
}