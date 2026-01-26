using UnityEngine;
using TMPro;
public class GrappleGunUI : MonoBehaviour
{
     [SerializeField] private GrapplerGun grapplerGun;
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject grappleIcon;
    

    private void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (grapplerGun == null || text == null) return;
    }
    private void OnEnable()
    {
        grappleIcon.SetActive(true);
    }
    private void OnDisable()
    {
        grappleIcon.SetActive(false);
    }
}
