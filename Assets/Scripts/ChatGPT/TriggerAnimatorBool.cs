using UnityEngine;

public class TriggerAnimatorBool : MonoBehaviour
{
    [Header("Animators to control")]
    public Animator[] targetAnimators;   // Array of Animators
    public string boolName = "KeyPickedUp";  // Name of the bool to trigger

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (Animator animator in targetAnimators)
            {
                if (animator != null)
                {
                    animator.SetBool(boolName, true);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (Animator animator in targetAnimators)
            {
                if (animator != null)
                {
                    animator.SetBool(boolName, false);
                }
            }
        }
    }
}
