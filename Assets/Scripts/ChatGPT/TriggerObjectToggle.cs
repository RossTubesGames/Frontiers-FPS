//made with ChatGPT
using UnityEngine;

public class TriggerIndividualToggle : MonoBehaviour
{
    [System.Serializable]
    public class ToggleObject
    {
        public GameObject target;
        public bool setActive;
    }

    [Header("Objects & Their States")]
    [SerializeField] private ToggleObject[] objectsToToggle;

    [Header("Trigger Settings")]
    [SerializeField] private string triggeringTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(triggeringTag)) return;

        foreach (ToggleObject toggle in objectsToToggle)
        {
            if (toggle.target != null)
            {
                toggle.target.SetActive(toggle.setActive);
            }
        }
    }
}
