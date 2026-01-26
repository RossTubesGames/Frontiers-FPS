using UnityEngine;
using TMPro;
using System.Collections;
public class Subtitlebox : MonoBehaviour
{
    [SerializeField] private TMP_Text healText;
    [SerializeField] private GameObject textHolder;

    private Coroutine healCoroutine;

    public void ShowHealText(float amount, float time)
    {
        healText.text = $"Healed {amount}";
        textHolder.SetActive(true);

        if (healCoroutine != null)
            StopCoroutine(healCoroutine);

        healCoroutine = StartCoroutine(HideText());
    }
    public void ShowText(string text, float time)
{
    healText.text = text;
    textHolder.SetActive(true);

    if (healCoroutine != null)
        StopCoroutine(healCoroutine);

    healCoroutine = StartCoroutine(HideText());
}

    public IEnumerator HideText()
    {
        yield return new WaitForSeconds(3);
        textHolder.SetActive(false);
    }
}
