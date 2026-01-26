using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ButtonTextColor : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Transform textTransform;
    [SerializeField] private float scaleAmount = 1.1f;
    [SerializeField] private float bounceDuration = 1f;

    private Coroutine bounceRoutine;
    private Vector3 originalScale;

    private TMP_Text text;

    void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
        originalScale = textTransform.localScale;
    }

    public void OnSelect(BaseEventData eventData)
    {
        text.color = selectedColor;
              if (bounceRoutine != null)
            StopCoroutine(bounceRoutine);

        bounceRoutine = StartCoroutine(Bounce());
    }

    public void OnDeselect(BaseEventData eventData)
    {
        text.color = normalColor;
    }
      private IEnumerator Bounce()
    {
        float timer = 0f;

        while (timer < bounceDuration)
        {
            float t = Mathf.PingPong(timer * 4f, 1f);
            textTransform.localScale = Vector3.Lerp(
                originalScale,
                originalScale * scaleAmount,
                t
            );

            timer += Time.unscaledDeltaTime; 
            yield return null;
        }

        textTransform.localScale = originalScale;
    }
}
