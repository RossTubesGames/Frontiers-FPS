using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject settingsFirstSelected; 
    private GameObject previousMenu;


    public void SetMasterVolume(float value)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(
            "MasterVolume",
            value
        );
    }
       public void SetMusicVolume(float value)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(
            "Music",
            value
        );
    }
    public void SetSFXVolume(float value)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(
            "SFXVolume",
            value
        );
    }
 public void ShowSettings(GameObject currentMenu)
{
    previousMenu = currentMenu;
    currentMenu.SetActive(false); 
    settingsMenu.SetActive(true);   
    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    

}
public void HideSettings()
{
    settingsMenu.SetActive(false);

    if (previousMenu != null)
        previousMenu.SetActive(true);

    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
}

}
