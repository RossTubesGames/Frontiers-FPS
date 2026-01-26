using FMODUnity;
using UnityEngine;

public class MusicStarter : MonoBehaviour
{
    [SerializeField] private EventReference musicEvent;
    private FMOD.Studio.EventInstance musicInstance;

    void Start()
    {
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }

    void OnDestroy()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
    }
}

