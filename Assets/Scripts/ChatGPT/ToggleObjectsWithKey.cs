using System.Collections.Generic;
using UnityEngine;

public class ToggleObjectsWithKey : MonoBehaviour
{
    [Header("Input")]
    public KeyCode toggleKey = KeyCode.E;

    [Header("Objects")]
    public List<ToggleEntry> objectsToToggle = new List<ToggleEntry>();

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            foreach (ToggleEntry entry in objectsToToggle)
            {
                if (!entry.enabled || entry.target == null)
                    continue;

                entry.target.SetActive(!entry.target.activeSelf);
            }
        }
    }
}

[System.Serializable]
public class ToggleEntry
{
    public bool enabled = true;      // Checkbox per object
    public GameObject target;        // Object reference
}
