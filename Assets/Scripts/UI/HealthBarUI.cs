using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text percentageText;



    private float maxHealth;

    void Start()
    {
        maxHealth = 100f;
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
    float percentage = (playerHealth.Health / maxHealth) * 100f;
    percentage = Mathf.Clamp(percentage, 0f, 100f);
    fillImage.fillAmount = percentage / 100f;
    percentageText.text = Mathf.RoundToInt(percentage) + "%";
    }


}
