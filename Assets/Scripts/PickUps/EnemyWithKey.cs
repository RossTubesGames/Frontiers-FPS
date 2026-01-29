using UnityEngine;

public class EnemyWithKey : MonoBehaviour
{
    [SerializeField] private GameObject keyPrefab;

    private bool keyDropped;

    // Call this from your enemy health script when it actually dies
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
}
