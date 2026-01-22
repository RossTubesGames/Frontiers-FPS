using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;
    [SerializeField] private HealthBarUI healthBarUI;

    public float Health => health;

    public void TakeDamage(float amount)
    {
        health -= amount;
        health = Mathf.Max(health, 0f);

        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar();
        }

        if (health <= 0f)
        {
            Debug.Log("Player died");
            SceneManager.LoadScene("GameOver");
        }
    }
}
