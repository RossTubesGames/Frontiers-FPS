using UnityEngine;

public class Revolver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shootOrigin;           // barrel or camera
    [SerializeField] private ParticleSystem muzzleFlash;      // optional
    [SerializeField] private GameObject hitImpactPrefab;      // optional

    [Header("Gun")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 60f;
    [SerializeField] private float fireCooldown = 0.25f;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 6;
    [SerializeField] private float reloadTime = 5.1f;
    [SerializeField] private bool autoReloadWhenEmpty = false;

    [Header("Hit Filtering")]
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private string enemyTag = "Enemy";

    private int ammo;
    private float nextFireTime;
    private bool reloading;

    private void OnEnable()
    {
        // When switching back to the weapon, ensure it has ammo initialized
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
        Ray ray = new Ray(o.position, o.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            // Damage enemies
            if (hit.collider.CompareTag(enemyTag))
            {
                EnemyHealth hp = hit.collider.GetComponentInParent<EnemyHealth>();
                if (hp != null)
                    hp.TakeDamage(damage);
                else
                    hit.collider.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }

            // Optional impact visual
            if (hitImpactPrefab != null)
            {
                Quaternion rot = Quaternion.LookRotation(hit.normal);
                Instantiate(hitImpactPrefab, hit.point + hit.normal * 0.001f, rot);
            }
        }

        // Optional: auto reload after last shot
        if (ammo <= 0 && autoReloadWhenEmpty)
            StartReload();
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

    // Optional helper if you want to display ammo in UI later
    public int GetAmmo() => ammo;
    public int GetMagazineSize() => magazineSize;
    public bool IsReloading() => reloading;
}
