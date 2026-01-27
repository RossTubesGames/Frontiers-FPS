using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BossHealthbar : MonoBehaviour
{
[SerializeField]private BossHealth health;
[SerializeField] private Image fillImage;
[SerializeField] private float maxhealth;
   public void UpdateBossHealthBar()
    {
        fillImage.fillAmount = health.bosshealth /maxhealth;
    }
    public void DestroyHealthBar()
    {
     Destroy(gameObject);
    }
}
