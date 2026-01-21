using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonTextColor : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.red;

    private TMP_Text text;

    void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        text.color = selectedColor;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        text.color = normalColor;
    }
}
