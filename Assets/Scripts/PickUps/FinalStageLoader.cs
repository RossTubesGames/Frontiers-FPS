using UnityEngine;
using UnityEngine.SceneManagement;
public class FinalStageLoader : MonoBehaviour
{

    [SerializeField]private Subtitlebox subtitlebox;
    void OnTriggerEnter(Collider other)
    {
        if(KeyPickUp.hasFirstKey&&KeyPickUp.hasSecondKey)
        {
            SceneManager.LoadScene("Scene 2 DEMO 2");
        }
        else
        {
            subtitlebox.ShowText("This door appears to be locked",3f);
        }
    }
}
