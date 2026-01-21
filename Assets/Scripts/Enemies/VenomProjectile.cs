using UnityEngine;

public class VenomProjectile : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private GameObject venomAreaPrefab;
    [SerializeField] private float areaLifetime = 15f;

    [Header("Optional")]
    [SerializeField] private float spawnOffsetUp = 0.02f;

    private void OnCollisionEnter(Collision collision)
    {
        int otherLayerMask = 1 << collision.gameObject.layer;
        bool hitGround = (groundMask.value & otherLayerMask) != 0;

        if (!hitGround) return;

        Vector3 hitPoint = collision.GetContact(0).point;

        Vector3 spawnPos = hitPoint + Vector3.up * spawnOffsetUp;
        Quaternion spawnRot = Quaternion.identity;

        GameObject areaObj = Instantiate(venomAreaPrefab, spawnPos, spawnRot);

        // If the area script exists, set lifetime.
        VenomArea area = areaObj.GetComponent<VenomArea>();
        if (area != null)
        {
            area.SetLifetime(areaLifetime);
        }
        else
        {
            // Fallback: still destroy the spawned object after lifetime.
            Destroy(areaObj, areaLifetime);
        }

        Destroy(gameObject);
    }
}
