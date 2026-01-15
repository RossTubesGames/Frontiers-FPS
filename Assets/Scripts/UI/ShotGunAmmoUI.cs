using UnityEngine;
using TMPro;


public class ShotGunAmmoUI : MonoBehaviour
{
    [SerializeField] private ShotGun shotgun;
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (shotgun == null || text == null) return;

        text.text = shotgun.GetAmmoText();
    }
}