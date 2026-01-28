using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TriggerSceneLoader : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("Scene Settings")]
    [SerializeField] private string sceneName;

    [Header("Events")]
    public UnityEvent OnTriggerEnterEvent;
    public UnityEvent OnBeforeLoad;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;

        OnTriggerEnterEvent?.Invoke();
        OnBeforeLoad?.Invoke();

        Destroy(gameObject); // destroy this pickup/trigger object
        SceneManager.LoadScene(sceneName);
    }
}
