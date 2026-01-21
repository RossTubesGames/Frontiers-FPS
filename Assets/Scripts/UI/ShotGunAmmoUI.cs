using UnityEngine;
using TMPro;


public class ShotGunAmmoUI : MonoBehaviour
{
    [SerializeField] private ShotGun shotgun;
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject shotgunIcon;

    private void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (shotgun == null || text == null) return;

        text.text = shotgun.GetAmmoText();
    }
      private void OnEnable()
    {
        shotgunIcon.SetActive(true);
    }
    private void OnDisable()
    {
        shotgunIcon.SetActive(false);
    }

}