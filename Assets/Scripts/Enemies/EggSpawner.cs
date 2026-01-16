using UnityEngine;

public class EggSpawner : MonoBehaviour
{
    // The prefab that will be spawned (your small spider enemy prefab).
    [SerializeField] private GameObject smallSpiderPrefab;

    // Where the spider should spawn from.
    // If you do not assign this in the Inspector, we will default to the egg's own transform.
    [SerializeField] private Transform spawnPoint;

    // Time (in seconds) between spawns.
    [SerializeField] private float spawnInterval = 20f;

    private void Awake()
    {
        // Safety fallback: if no spawnPoint was assigned,
        // spawn from the egg object's position/rotation.
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    private void OnEnable()
    {
        // Starts a repeating call to SpawnSmallSpider().
        // First parameter: which method to call (by name).
        // Second parameter: delay before the first spawn happens.
        // Third parameter: how often to repeat after that.
        //
        // With (spawnInterval, spawnInterval):
        // - first spawn happens after spawnInterval seconds
        // - then it repeats every spawnInterval seconds
        InvokeRepeating(nameof(SpawnSmallSpider), spawnInterval, spawnInterval);
    }

    private void OnDisable()
    {
        // Stops the repeating invoke when the object is disabled or destroyed.
        // This prevents callbacks firing on disabled objects and avoids warnings.
        CancelInvoke(nameof(SpawnSmallSpider));
    }

    private void SpawnSmallSpider()
    {
        // If the prefab is not assigned, do nothing safely.
        if (smallSpiderPrefab == null)
            return;

        // Create a new small spider at the spawnPoint position and rotation.
        Instantiate(smallSpiderPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
