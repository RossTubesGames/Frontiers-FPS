using UnityEngine;

public class Projectileenemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform player;

    [Header("Shooting")]
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float detectionRange = 15f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 5f;

    private float nextFireTime;



private void RotateTowardsPlayerFull3D()
{
    Vector3 direction = (player.position - transform.position).normalized;
    if (direction == Vector3.zero) return;

    Quaternion targetRotation = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
}


    private void Update()
{
    if (player == null) return;

    float distance = Vector3.Distance(transform.position, player.position);

    if (distance <= detectionRange)
    {
        RotateTowardsPlayerFull3D();

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }
}
private void AimFirePointAtPlayer()
{
    if (firePoint == null) return;

    Vector3 dir = (player.position - firePoint.position).normalized;
    firePoint.rotation = Quaternion.LookRotation(dir);
}


    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = proj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * projectileSpeed;
        }
    }
}
