using UnityEngine;
using UnityEngine.Video;

public class EndSequenceController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private CreditsCrawl credits;

    [Header("Options")]
    [SerializeField] private bool loopVideo = true;

    private void Start()
    {
        if (videoPlayer)
        {
            videoPlayer.isLooping = loopVideo;
            videoPlayer.Play();
        }
    }

    private void Update()
    {
        // Optional: skip
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndNow();
        }

        // Optional: end when credits finished
        // if (credits != null && credits.IsFinished()) EndNow();
    }

    private void EndNow()
    {
        if (videoPlayer) videoPlayer.Stop();
        // Load menu scene, show "Thanks for playing", etc.
        // Example:
        // SceneManager.LoadScene("MainMenu");
    }
}