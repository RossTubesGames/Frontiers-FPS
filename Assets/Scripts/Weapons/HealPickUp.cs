using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Collections;
public class HealPickUp : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;
    [SerializeField]private HealthBarUI healthBarUI;
    [SerializeField] float healAmmount=20;
    [SerializeField] private TMP_Text display;
    [SerializeField] private float displayTime;
    [SerializeField]private GameObject textHolder;
    
    private void OnTriggerEnter(Collider other)
    {
        if(player.health<=100-healAmmount && other.CompareTag("Player"))
        {
          player.health=player.health+healAmmount;
          healthBarUI.UpdateHealthBar();
          display.text=$"Healed {healAmmount}"; 
          textHolder.SetActive(true);
          Destroy(gameObject);
          HideText();
        }
    }
    public IEnumerator HideText()
    {
        yield return new WaitForSeconds(displayTime);
        display.text="";
    }
}
