using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Collections;

public class KeyPickUp : MonoBehaviour
{
[SerializeField]private Subtitlebox subtitlebox;
public static bool hasFirstKey=false;
public static bool hasSecondKey=false;
private static int triggerCount=0;
    void OnTriggerEnter(Collider other)
    {
           if (other.CompareTag("Player"))
    {
        triggerCount++;

        if (triggerCount == 1)
        {
        hasFirstKey=true;
        Destroy(gameObject);
        subtitlebox.ShowText("First key obtained",3f);

        }
        else if (triggerCount ==2 )
        {
         hasSecondKey=true;
         subtitlebox.ShowText("Second key obtained",3f);
         Destroy(gameObject);
        }
    }

}
}
