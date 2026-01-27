using UnityEngine;

public class ActivateOnTrigger : MonoBehaviour
{
    [Header("Object to activate")]
    public GameObject objectToActivate;

    [Header("Optional")]
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }
            else
            {
                Debug.LogWarning("No object assigned to activate!", this);
            }
        }
    }
}
