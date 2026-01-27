using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyWithKey : MonoBehaviour
{
    [SerializeField] private GameObject keyPrefab;
    //assign this script if you want a specific enemy(not a boss) to drop a key
    void OnDestroy()
    {
        DropKey();
    }
    void DropKey()
    {
        // Only drop if a prefab is assigned
        if (keyPrefab == null)
        {
            Debug.LogWarning("KeyBossHealth: No key prefab assigned.");
            return;
        }

        // Spawn the key at the boss position (you can add an offset if needed)
        Instantiate(keyPrefab, transform.position, Quaternion.identity);
    }
}
