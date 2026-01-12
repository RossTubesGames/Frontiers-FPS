using UnityEngine;

public class ShotGun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shootOrigin;      // barrel or camera
    [SerializeField] private ParticleSystem muzzleFlash; // optional
    [SerializeField] private GameObject hitImpactPrefab; // optional

    [Header("Shotgun")]
    [SerializeField] private int pellets = 10;                 // how many rays per shot
    [SerializeField] private float damagePerPellet = 5f;       // each pellet damage
    [SerializeField] private float range = 20f;
    [SerializeField] private float spreadAngle = 7f;           // degrees
    [SerializeField] private float fireCooldown = 0.8f;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 2;
    [SerializeField] private float reloadTime = 1.2f;
    [SerializeField] private bool autoReloadWhenEmpty = false;

    [Header("Hit Filtering")]
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private string enemyTag = "Enemy";

    private int ammo;
    private float nextFireTime;
    private bool reloading;

    private void OnEnable()
    {
        if (ammo <= 0) ammo = magazineSize;
        reloading = false;
    }

    private void Update()
    {
        if (reloading) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (ammo <= 0)
            {
                if (autoReloadWhenEmpty) StartReload();
                return;
            }

            if (Time.time >= nextFireTime)
                Fire();
        }
    }

    private void Fire()
    {
        nextFireTime = Time.time + fireCooldown;
        ammo--;

        if (muzzleFlash != null) muzzleFlash.Play();

        Transform o = shootOrigin != null ? shootOrigin : transform;

        for (int i = 0; i < pellets; i++)
        {
            Vector3 dir = GetSpreadDirection(o.forward, spreadAngle);

            Ray ray = new Ray(o.position, dir);
            if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag(enemyTag))
                {
                    EnemyHealth hp = hit.collider.GetComponentInParent<EnemyHealth>();
                    if (hp != null)
                        hp.TakeDamage(damagePerPellet);
                    else
                        hit.collider.SendMessageUpwards("TakeDamage", damagePerPellet, SendMessageOptions.DontRequireReceiver);
                }

                if (hitImpactPrefab != null)
                {
                    Quaternion rot = Quaternion.LookRotation(hit.normal);
                    Instantiate(hitImpactPrefab, hit.point + hit.normal * 0.001f, rot);
                }
            }
        }

        if (ammo <= 0 && autoReloadWhenEmpty)
            StartReload();
    }

    private Vector3 GetSpreadDirection(Vector3 forward, float angleDeg)
    {
        // Random direction inside a cone around forward
        float angleRad = angleDeg * Mathf.Deg2Rad;
        Vector3 rand = Random.insideUnitSphere;
        rand.z = Mathf.Abs(rand.z); // bias forward-ish

        // Small rotation around forward using random offsets
        Vector3 axis = Vector3.Cross(forward, Vector3.up);
        if (axis.sqrMagnitude < 0.0001f) axis = Vector3.right;

        Quaternion rot = Quaternion.AngleAxis(Random.Range(-angleDeg, angleDeg), Vector3.up) *
                         Quaternion.AngleAxis(Random.Range(-angleDeg, angleDeg), axis);

        return (rot * forward).normalized;
    }

    private void StartReload()
    {
        if (reloading) return;
        if (ammo == magazineSize) return;

        reloading = true;
        Invoke(nameof(FinishReload), reloadTime);
    }

    private void FinishReload()
    {
        ammo = magazineSize;
        reloading = false;
    }
}
