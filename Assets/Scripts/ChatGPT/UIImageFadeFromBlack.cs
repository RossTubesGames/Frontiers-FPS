using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIImageFadeFromBlack : MonoBehaviour
{
    public float fadeDuration = 1.5f;

    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void Start()
    {
        StartCoroutine(FadeFromBlack());
    }

    IEnumerator FadeFromBlack()
    {
        float t = 0f;
        image.color = Color.black;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            image.color = Color.Lerp(Color.black, Color.white, t / fadeDuration);
            yield return null;
        }

        image.color = Color.white;
    }
}
