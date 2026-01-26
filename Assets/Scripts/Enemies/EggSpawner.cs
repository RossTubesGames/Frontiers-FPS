using UnityEngine;
using System.Collections.Generic;
public class EggSpawner : MonoBehaviour
{
    public class SpiderGlobalTracker : MonoBehaviour
{
    private int id;

    public void Init(int instanceID)
    {
        id = instanceID;
    }

    private void OnDestroy()
    {
        EggSpawner.UnregisterSpider(id);
    }
}

    // The prefab that will be spawned (your small spider enemy prefab).
[SerializeField] private GameObject smallSpiderPrefab;
private static HashSet<int> aliveSpiderIDs = new HashSet<int>();
[SerializeField] private int maxGlobalSpiders = 1;
 [SerializeField]private float activationRange = 25f;


private Transform player;


    // Where the spider should spawn from.
    // If you do not assign this in the Inspector, we will default to the egg's own transform.
    [SerializeField] private Transform spawnPoint;

    // Time (in seconds) between spawns.
    [SerializeField] private float spawnInterval = 20f;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
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
    if (smallSpiderPrefab == null)
        return;

    if (Vector3.Distance(transform.position, player.position) > activationRange)
        return;

    // Clean dead entries automatically (Unity destroys objects → instance ID becomes invalid)
    aliveSpiderIDs.RemoveWhere(id => id == 0);

    if (aliveSpiderIDs.Count >= maxGlobalSpiders)
        return;

    GameObject newSpider = Instantiate(smallSpiderPrefab, spawnPoint.position, spawnPoint.rotation);

    int id = newSpider.GetInstanceID();
    aliveSpiderIDs.Add(id);

    // Add auto-unregister component
    var tracker = newSpider.AddComponent<SpiderGlobalTracker>();
    tracker.Init(id);
}
public static void UnregisterSpider(int id)
{
    aliveSpiderIDs.Remove(id);
}



}
