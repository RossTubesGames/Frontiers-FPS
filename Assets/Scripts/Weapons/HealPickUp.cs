using UnityEngine;

public class HealPickUp : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;
    [SerializeField]private HealthBarUI healthBarUI;
    [SerializeField] float healAmmount=20;
    private void OnTriggerEnter(Collider other)
    {
        if(player.health<=100-healAmmount)
        {
          healthBarUI.UpdateHealthBar();
          player.health=player.health+healAmmount;
          Destroy(gameObject);
        }
    }
}
