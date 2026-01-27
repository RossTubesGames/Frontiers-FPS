using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class EndSequenceController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private CreditsCrawl credits;

    [Header("Video")]
    [SerializeField] private bool loopVideo = true;

    [Header("Optional Fallback Overlay (WebGL)")]
    [SerializeField] private GameObject clickToStartOverlay;

    private bool started;

    private void Awake()
    {
        if (credits) credits.enabled = false;

        if (videoPlayer)
        {
            videoPlayer.errorReceived += OnVideoError;
            videoPlayer.prepareCompleted += OnPrepared;
        }
    }

    private void Start()
    {
        TryStart();
    }

    private void OnDestroy()
    {
        if (videoPlayer)
        {
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.prepareCompleted -= OnPrepared;
        }
    }

    private void TryStart()
    {
        if (started) return;
        started = true;

        if (!videoPlayer)
        {
            Debug.LogError("EndSequenceController: VideoPlayer reference missing.");
            return;
        }

        if (clickToStartOverlay) clickToStartOverlay.SetActive(false);

        videoPlayer.isLooping = loopVideo;
        videoPlayer.source = VideoSource.Url;

        string chosenFile =
#if UNITY_WEBGL && !UNITY_EDITOR
            "end.webm";
#else
            "end_preview.mp4";
#endif

        string path = Path.Combine(Application.streamingAssetsPath, chosenFile);

#if UNITY_WEBGL && !UNITY_EDITOR
        string url = path; // already a URL in WebGL
#else
        string url = "file:///" + path.Replace("\\", "/");
#endif

        Debug.Log("EndSequenceController: Video URL = " + url);

        videoPlayer.url = url;
        videoPlayer.Prepare();

        if (clickToStartOverlay)
            Invoke(nameof(ShowOverlayIfNotPlaying), 1.0f);
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.EnableAudioTrack(0, true);
        vp.SetTargetAudioSource(0, videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource
            ? videoPlayer.GetTargetAudioSource(0)
            : null);

        vp.Play();
        if (credits) credits.enabled = true;
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("VideoPlayer error: " + message);
        if (clickToStartOverlay) clickToStartOverlay.SetActive(true);
    }

    private void ShowOverlayIfNotPlaying()
    {
        if (!videoPlayer) return;
        if (!videoPlayer.isPlaying && clickToStartOverlay)
            clickToStartOverlay.SetActive(true);
    }

    // Hook this to your Click me button
    public void OnUserClickStart()
    {
        if (clickToStartOverlay) clickToStartOverlay.SetActive(false);

        if (!videoPlayer) return;

        if (!videoPlayer.isPrepared)
            videoPlayer.Prepare();
        else
            videoPlayer.Play();

        if (credits) credits.enabled = true;
    }
}
