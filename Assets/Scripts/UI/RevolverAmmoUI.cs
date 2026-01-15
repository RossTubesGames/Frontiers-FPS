using UnityEngine;
using TMPro;

public class RevolverAmmoUI : MonoBehaviour
{
    [SerializeField] private Revolver revolver;
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (revolver == null || text == null) return;
        text.text = revolver.GetAmmoText();
    }
}
