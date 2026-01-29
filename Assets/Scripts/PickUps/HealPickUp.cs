using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Collections;
public class HealPickUp : MonoBehaviour
{
    [SerializeField]private Subtitlebox subtitlebox;
    [SerializeField] private PlayerHealth player;
    [SerializeField]private HealthBarUI healthBarUI;
    [SerializeField] float healAmmount=20;
     [SerializeField] private float maxHealth = 100f;
    [SerializeField] private TMP_Text display;
    [SerializeField] private float displayTime;
    [SerializeField]private GameObject textHolder;
    [SerializeField]private GameObject healFeedback;
    
 private void OnTriggerEnter(Collider other)
{
    StartCoroutine(HealFeedback());
    Debug.Log("enter");
    if (!other.CompareTag("Player"))
        return;

    float healedAmount = Mathf.Min(healAmmount, maxHealth - player.health);
    player.health += healedAmount;

    healthBarUI.UpdateHealthBar();

    subtitlebox.ShowHealText(healedAmount, displayTime);

    GetComponent<MeshRenderer>().enabled = false;
    GetComponent<Collider>().enabled = false;

    Destroy(gameObject);
}
    public IEnumerator HealFeedback()
    {
        healFeedback.SetActive(true);
        yield return new WaitForSeconds(2f);
        healFeedback.SetActive(false);
    }
}
