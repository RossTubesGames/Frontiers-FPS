using UnityEngine;
using UnityEngine.SceneManagement;
public class CreditsLoad : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
       SceneManager.LoadScene("Credits"); 
    } 
}
