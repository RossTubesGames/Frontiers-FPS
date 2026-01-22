using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class GrappleGunUI : MonoBehaviour
{
    [SerializeField] private GameObject grappleIcon;

    private void OnEnable()
    {
        grappleIcon.SetActive(true);
    }
    private void OnDisable()
    {
        grappleIcon.SetActive(false);
    }
}
