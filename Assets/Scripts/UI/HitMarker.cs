using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HitMarker : MonoBehaviour
{
    [SerializeField] private Image hitMarkerImage;
    [SerializeField] private float visibleTime = 0.08f;

    private Coroutine routine;

    private void Awake()
    {
        if (hitMarkerImage != null)
            hitMarkerImage.enabled = false;
    }

    public void Show()
    {
        if (hitMarkerImage == null)
            return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        hitMarkerImage.enabled = true;
        yield return new WaitForSeconds(visibleTime);
        hitMarkerImage.enabled = false;
        routine = null;
    }
}