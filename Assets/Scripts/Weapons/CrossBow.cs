using System.Collections;
using UnityEngine;

public class CrossBow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shootOrigin;

    [Header("Aiming")]
    [SerializeField] private Transform aimSource;          // Main Camera
    [SerializeField] private float aimMaxDistance = 100f;
    [SerializeField] private LayerMask aimMask = ~0;       // what the camera can aim at (world + enemies)

    [Header("Projectile")]
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private float muzzleSpeed = 55f;

    [Header("CrossBow")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private int pierceCount = 3;
    [SerializeField] private float fireCooldown = 0.25f;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 6;
    [SerializeField] private int startingReserveAmmo = 36;
    [SerializeField] private float reloadTime = 1.0f;
    [SerializeField] private bool autoReloadWhenEmpty = true;

    [Header("Hit Filtering")]
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private string enemyTag = "Enemy";

    private int ammoInMag;
    private int reserveAmmo;

    private float nextFireTime;
    private bool reloading;

    private void Start()
    {
        ammoInMag = magazineSize;
        reserveAmmo = startingReserveAmmo;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryStartReload();
        }

        if (Input.GetButtonDown("Fire1"))
        {
            TryFire();
        }

        if (autoReloadWhenEmpty && !reloading && ammoInMag <= 0 && reserveAmmo > 0)
        {
            TryStartReload();
        }
    }

    private void TryFire()
    {
        if (reloading) return;
        if (Time.time < nextFireTime) return;

        if (ammoInMag <= 0)
        {
            if (autoReloadWhenEmpty) TryStartReload();
            return;
        }

        nextFireTime = Time.time + fireCooldown;
        ammoInMag--;

        // 1) Find what the camera is aiming at
        Vector3 targetPoint = (aimSource != null)
            ? (aimSource.position + aimSource.forward * aimMaxDistance)
            : (shootOrigin.position + shootOrigin.forward * aimMaxDistance);

        if (aimSource != null)
        {
            Ray ray = new Ray(aimSource.position, aimSource.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, aimMaxDistance, aimMask, QueryTriggerInteraction.Ignore))
            {
                targetPoint = hit.point;
            }
        }

        // 2) Shoot from the muzzle toward that target point
        Vector3 dir = (targetPoint - shootOrigin.position).normalized;

        Arrow arrowInstance = Instantiate(arrowPrefab, shootOrigin.position, Quaternion.LookRotation(dir));
        arrowInstance.transform.SetParent(null, true);

        arrowInstance.Launch(dir * muzzleSpeed, damage, pierceCount, hitMask, enemyTag);
    }


    private void TryStartReload()
    {
        if (reloading) return;
        if (ammoInMag >= magazineSize) return;
        if (reserveAmmo <= 0) return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        reloading = true;
        yield return new WaitForSeconds(reloadTime);

        int need = magazineSize - ammoInMag;
        int take = Mathf.Min(need, reserveAmmo);

        ammoInMag += take;
        reserveAmmo -= take;

        reloading = false;
    }

    public void AddReserveAmmo(int amount)
    {
        reserveAmmo += Mathf.Max(0, amount);
    }
}
