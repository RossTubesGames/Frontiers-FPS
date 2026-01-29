using UnityEngine;

public class EnemyWithKey : MonoBehaviour
{
    [SerializeField] private GameObject keyPrefab;

    private bool keyDropped;
    public void DropKeyOnce()
    {
        if (keyDropped) return;
        keyDropped = true;

        if (keyPrefab == null)
        {
            Debug.LogWarning($"{name}: No key prefab assigned.");
            return;
        }

        Instantiate(keyPrefab, transform.position, Quaternion.identity);
    }
    public void OnDestroy()
    {
        Debug.Log("Drop");
        if(PlayerHealth.isDead==true) return;
        Debug.Log("Drop");
        DropKeyOnce();
    }

}
