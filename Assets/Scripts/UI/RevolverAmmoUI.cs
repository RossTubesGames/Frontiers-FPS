using UnityEngine;
using TMPro;

public class RevolverAmmoUI : MonoBehaviour
{
    [SerializeField] private Revolver revolver;
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject revolverIcon;
    

    private void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (revolver == null || text == null) return;
        text.text = revolver.GetAmmoText();
    }
    private void OnEnable()
    {
        revolverIcon.SetActive(true);
    }
    private void OnDisable()
    {
        revolverIcon.SetActive(false);
    }
}
