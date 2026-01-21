using UnityEngine;

public class Revolver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject hitImpactPrefab;

    [Header("Gun")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 60f;
    [SerializeField] private float fireCooldown = 0.25f;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 6;
    [SerializeField] private int startingReserveAmmo = 36;
    [SerializeField] private float reloadTime = 1.0f;
    [SerializeField] private bool autoReloadWhenEmpty = true;

    [Header("Hit Filtering")]
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Impact Cleanup")]
    [SerializeField] private float hitImpactLifetime = 30f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private static readonly int AnimShoot = Animator.StringToHash("RevolverShoot");
    private static readonly int AnimReload = Animator.StringToHash("RevolverReload");
    private static readonly int AnimEmpty = Animator.StringToHash("RevolverEmpty");

    private int ammoInMag;
    private int reserveAmmo;

    private float nextFireTime;
    private bool reloading;

    private void Awake()
    {
        ammoInMag = magazineSize;
        reserveAmmo = startingReserveAmmo;

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
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
                Trigger(AnimEmpty);

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

        Trigger(AnimShoot);

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play(true);
            FMODUnity.RuntimeManager.PlayOneShot("event:/Revolver");
        }

        Transform o = shootOrigin != null ? shootOrigin : transform;
        Ray ray = new Ray(o.position, o.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag(enemyTag))
            {
                EnemyHealth hp = hit.collider.GetComponentInParent<EnemyHealth>();
                if (hp != null) hp.TakeDamage(damage);
                else hit.collider.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }

            if (hitImpactPrefab != null)
            {
                Quaternion rot = Quaternion.LookRotation(hit.normal);
                GameObject impact = Instantiate(hitImpactPrefab, hit.point + hit.normal * 0.001f, rot);

                if (hitImpactLifetime > 0f)
                    Destroy(impact, hitImpactLifetime);
            }
        }

        if (ammoInMag <= 0 && autoReloadWhenEmpty && reserveAmmo > 0)
            StartReload();
    }

    private void StartReload()
    {
        if (reloading) return;
        if (ammoInMag >= magazineSize) return;
        if (reserveAmmo <= 0) return;

        FMODUnity.RuntimeManager.PlayOneShot("event:/Revolver reload");

        Trigger(AnimReload);

        reloading = true;
        Invoke(nameof(FinishReload), reloadTime);
    }

    private void FinishReload()
    {
        int needed = magazineSize - ammoInMag;
        int take = Mathf.Min(needed, reserveAmmo);

        ammoInMag += take;
        reserveAmmo -= take;

        reloading = false;
    }

    private void Trigger(int hash)
    {
        if (animator == null) return;
        animator.ResetTrigger(hash);
        animator.SetTrigger(hash);
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
    public bool IsReloading() => reloading;
}
