using UnityEngine;
using TMPro;


public class CrossbowUI : MonoBehaviour
{
    [SerializeField] private CrossBow crossBow;
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject crossbowIcon;

    private void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (crossBow == null || text == null) return;

        text.text = crossBow.GetAmmoText();
        crossbowIcon.SetActive(true);

    }
}
