using UnityEngine;

public class AmmoCrate : MonoBehaviour
{
    [Header("Pickup Amounts")]
    [SerializeField] private int revolverBullets = 10;
    [SerializeField] private int shotgunShells = 4;

    [Header("Pickup Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool destroyOnPickup = true;

    [Header("Lookup")]
    [SerializeField] private bool searchFromPlayerRoot = true;

    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;

        // Make sure this trigger is the player (or a child collider of the player)
        if (!other.CompareTag(playerTag) && other.GetComponentInParent<Transform>() == null)
            return;

        Transform playerRoot = other.CompareTag(playerTag)
            ? other.transform
            : other.transform.root;

        // Some setups put the Player tag on a parent. If so, climb up to the tagged object.
        Transform taggedPlayer = other.CompareTag(playerTag) ? other.transform : other.GetComponentInParent<Transform>();
        if (taggedPlayer != null)
        {
            Transform t = other.transform;
            while (t != null)
            {
                if (t.CompareTag(playerTag))
                {
                    playerRoot = t;
                    break;
                }
                t = t.parent;
            }
        }

        Transform searchRoot = searchFromPlayerRoot ? playerRoot : other.transform;

        Revolver revolver = searchRoot.GetComponentInChildren<Revolver>(true);
        ShotGun shotGun = searchRoot.GetComponentInChildren<ShotGun>(true);

        // Fallback: if weapons are only under the camera, search Camera.main too
        if (revolver == null || shotGun == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                if (revolver == null) revolver = cam.GetComponentInChildren<Revolver>(true);
                if (shotGun == null) shotGun = cam.GetComponentInChildren<ShotGun>(true);
            }
        }

        bool gaveAny = false;

        if (revolver != null && revolverBullets > 0)
        {
            revolver.AddReserveAmmo(revolverBullets);
            gaveAny = true;
        }

        if (shotGun != null && shotgunShells > 0)
        {
            shotGun.AddReserveAmmo(shotgunShells);
            gaveAny = true;
        }

        if (!gaveAny) return;

        used = true;

        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
