using UnityEngine; 
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    public float Health => health;
    [SerializeField] private HealthBarUI healthBarUI;


    public void TakeDamage(float amount)
    {
        healthBarUI.UpdateHealthBar();

        health -= amount;

        if (health <= 0f)
        Debug.Log("Player died");
    
    }
}
