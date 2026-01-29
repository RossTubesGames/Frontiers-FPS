using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;
    [SerializeField] private HealthBarUI healthBarUI;
    [SerializeField]private GameObject damageFeedback;
    public static bool isDead=false;

    public float Health => health;

    public void TakeDamage(float amount)
    {

        health -= amount;
        health = Mathf.Max(health, 0f);
        StartCoroutine(DamageFeedback());
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar();
        }

        if (health <= 0f)
        {
            Debug.Log("Player died");
            isDead=true;
            SceneManager.LoadScene("GameOverTodor");

        }
    }
    private IEnumerator DamageFeedback()
    {
        Debug.Log("damagefeedback");
        damageFeedback.SetActive(true);
        yield return new WaitForSeconds(2f);
        damageFeedback.SetActive(false);
    }
}
