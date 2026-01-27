using UnityEngine;

public class CreditsCrawl : MonoBehaviour
{
    [SerializeField] private RectTransform creditsText;
    [SerializeField] private RectTransform maskArea;

    [Header("Scroll")]
    [SerializeField] private float pixelsPerSecond = 60f;
    [SerializeField] private float startPadding = 50f;
    [SerializeField] private float endPadding = 50f;

    private Vector2 startPos;
    private float endY;

    private void OnEnable()
    {
        if (!creditsText) creditsText = (RectTransform)transform;

        // Start just below the mask
        float startY = -maskArea.rect.height * 0.5f - startPadding;
        startPos = new Vector2(creditsText.anchoredPosition.x, startY);
        creditsText.anchoredPosition = startPos;

        // End when text has fully moved past the top
        endY = maskArea.rect.height * 0.5f + creditsText.rect.height + endPadding;
    }

    private void Update()
    {
        Vector2 p = creditsText.anchoredPosition;
        p.y += pixelsPerSecond * Time.deltaTime;
        creditsText.anchoredPosition = p;
    }

    public bool IsFinished()
    {
        return creditsText.anchoredPosition.y >= endY;
    }
}