using UnityEngine;

public class EnemyColliderDamage : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private BossEnemy _bossRef;

    void Start()
    {
        try
        {
            
            _bossRef = GetComponentInParent<BossEnemy>();
        }
        catch
        {
            Debug.LogError("Couldn't find a BossEnemy script on the parents of this object! Object name: " + transform.name);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        // Debug.Log(other.transform.name);
        if (other.gameObject.CompareTag(playerTag))
        {
            Transform player = other.transform;
            PlayerHealth hp = player.GetComponentInParent<PlayerHealth>();
                if (hp != null)
                    hp.TakeDamage(_bossRef.Damage);
                else
                    player.SendMessageUpwards("TakeDamage", _bossRef.Damage, SendMessageOptions.DontRequireReceiver);
            
                
            }
    }

}
