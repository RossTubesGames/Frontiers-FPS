using UnityEngine;

public class ShotGun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject hitImpactPrefab;

    [Header("Shotgun")]
    [SerializeField] private int pellets = 10;
    [SerializeField] private float damagePerPellet = 5f;
    [SerializeField] private float range = 20f;
    [SerializeField] private float spreadAngle = 7f;
    [SerializeField] private float fireCooldown = 0.8f;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 2;         // shells in gun
    [SerializeField] private int startingReserveAmmo = 8;  // extra shells carried
    [SerializeField] private float reloadTime = 1.2f;
    [SerializeField] private bool autoReloadWhenEmpty = false;

    [Header("Hit Filtering")]
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Impact Cleanup")]
    [SerializeField] private float hitImpactLifetime = 30f;

    private int ammoInMag;
    private int reserveAmmo;

    private float nextFireTime;
    private bool reloading;

    private void Awake()
    {
        ammoInMag = magazineSize;
        reserveAmmo = startingReserveAmmo;
    }

    private void OnEnable()
    {
        reloading = false;
        CancelInvoke(nameof(FinishReload));
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
            if (ammoInMag <= 0)
            {
                if (autoReloadWhenEmpty && reserveAmmo > 0)
                    StartReload();

                return;
            }

            if (Time.time >= nextFireTime)
                Fire();
        }
    }

    private void Fire()
    {
        nextFireTime = Time.time + fireCooldown;
        ammoInMag--;

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play(true);
            FMODUnity.RuntimeManager.PlayOneShot("event:/Shotgun fire");
        }

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
                    GameObject impact = Instantiate(hitImpactPrefab, hit.point + hit.normal * 0.001f, rot);

                    if (hitImpactLifetime > 0f)
                        Destroy(impact, hitImpactLifetime);
                }
            }
        }

        if (ammoInMag <= 0 && autoReloadWhenEmpty && reserveAmmo > 0)
            StartReload();
    }

    private Vector3 GetSpreadDirection(Vector3 forward, float angleDeg)
    {
        Vector3 axis = Vector3.Cross(forward, Vector3.up);
        if (axis.sqrMagnitude < 0.0001f) axis = Vector3.right;

        Quaternion rot = Quaternion.AngleAxis(Random.Range(-angleDeg, angleDeg), Vector3.up) *
                         Quaternion.AngleAxis(Random.Range(-angleDeg, angleDeg), axis);

        return (rot * forward).normalized;
    }

    private void StartReload()
    {
        if (reloading) return;

        if (ammoInMag >= magazineSize) return;
        if (reserveAmmo <= 0) return;

        reloading = true;
        Invoke(nameof(FinishReload), reloadTime);
        FMODUnity.RuntimeManager.PlayOneShot("event:/Shotgun reload");
    }

    private void FinishReload()
    {
        int needed = magazineSize - ammoInMag;
        int take = Mathf.Min(needed, reserveAmmo);

        ammoInMag += take;
        reserveAmmo -= take;

        reloading = false;
    }

    public string GetAmmoText()
    {
        return ammoInMag + "/" + reserveAmmo;
    }

    public void AddReserveAmmo(int amount)
    {
        if (amount <= 0) return;
        reserveAmmo += amount;
    }


    public int GetAmmoInMag() => ammoInMag;
    public int GetReserveAmmo() => reserveAmmo;
    public int GetMagazineSize() => magazineSize;
    public bool IsReloading() => reloading;
}
