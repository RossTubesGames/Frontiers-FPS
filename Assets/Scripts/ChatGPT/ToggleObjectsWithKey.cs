using System.Collections.Generic;
using UnityEngine;

public class ToggleObjectsWithKey : MonoBehaviour
{
    [Header("Input")]
    public KeyCode toggleKey = KeyCode.E;

    [Header("Objects")]
    public List<ToggleEntry> objects = new List<ToggleEntry>();

    void Start()
    {
        // Apply initial A/B state
        foreach (ToggleEntry entry in objects)
        {
            if (entry.target != null)
            {
                entry.target.SetActive(entry.startsOn);
            }
        }
    }

    void Update()
    {
        if (!Input.GetKeyDown(toggleKey))
            return;

        foreach (ToggleEntry entry in objects)
        {
            if (entry.target != null)
            {
                entry.target.SetActive(!entry.target.activeSelf);
            }
        }
    }
}

[System.Serializable]
public class ToggleEntry
{
    public bool startsOn;      // A/B flag
    public GameObject target;
}
