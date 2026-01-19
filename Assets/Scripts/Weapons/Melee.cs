using UnityEngine;

public class Melee : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Punch Shape")]
    [SerializeField] private float range = 1.2f;
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float maxAngle = 55f; // 3D cone in front

    [Header("Timing")]
    [SerializeField] private float cooldown = 0.25f;

    private float nextTime;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextTime)
        {
            nextTime = Time.time + cooldown;
            Punch();
        }
    }

    private void Punch()
    {
        //Play the sword swinging sound
        FMODUnity.RuntimeManager.PlayOneShot("event:/Axe swings");

        //defines where the sphere is casted
        Vector3 origin = transform.position; //origin is punchers position so the player
        Vector3 forward = transform.forward.normalized; //forward direction the punchers is facing
        Vector3 center = origin + forward * range; //center is the point infront of the player at distance range

        Collider[] hits = Physics.OverlapSphere(center, radius, hitMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i];
            if (!c.CompareTag(enemyTag))
                continue;

            Vector3 toTarget = (c.bounds.center - origin);
            if (toTarget.sqrMagnitude < 0.0001f)
                continue;

            float angle = Vector3.Angle(forward, toTarget.normalized);
            if (angle > maxAngle)
                continue;

            EnemyHealth hp = c.GetComponentInParent<EnemyHealth>();
            if (hp != null)
                hp.TakeDamage(damage);
            else
                c.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward.normalized;
        Gizmos.DrawWireSphere(origin + forward * range, radius);
    }
}
