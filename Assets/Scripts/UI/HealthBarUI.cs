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
      fillImage.fillAmount = playerHealth.Health / maxHealth;
      percentageText.text = Mathf.RoundToInt((playerHealth.Health / maxHealth) * 100f) + "%";

    }


}
