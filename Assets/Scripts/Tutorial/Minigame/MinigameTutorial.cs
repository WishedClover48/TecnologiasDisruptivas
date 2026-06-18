using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class MinigameTutorial : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private string titleText = "Cómo jugar";
    [SerializeField] [TextArea(3, 8)] private string descriptionText = "";
    [SerializeField] private VideoClip videoClip;

    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Placement")]
    [SerializeField] private bool placeInFrontOfPlayer = true;
    [SerializeField] private float spawnDistance = 1.6f;
    [SerializeField] private float heightOffset  = 0f;

    [Header("Flow")]
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private bool pauseWhileShown = false;
    [SerializeField] private GameObject[] enableOnAcknowledge;
    [SerializeField] private UnityEvent onAcknowledged;

    private bool acknowledged;
    private bool videoReady;

    private void Awake()
    {
        if (enableOnAcknowledge != null)
            foreach (var go in enableOnAcknowledge)
                if (go != null) go.SetActive(false);

        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void Start()
    {
        if (showOnStart) Show();
    }

    public void Show()
    {
        acknowledged = false;

        if (titleLabel != null       && !string.IsNullOrEmpty(titleText))       titleLabel.text       = titleText;
        if (descriptionLabel != null && !string.IsNullOrEmpty(descriptionText)) descriptionLabel.text = descriptionText;

        if (panelRoot != null) panelRoot.SetActive(true);

        SetupVideo();

        if (pauseWhileShown) Time.timeScale = 0f;

        if (placeInFrontOfPlayer)
        {
            PlaceInFrontOfPlayer();
            StartCoroutine(SettlePlacement());
        }
    }

    private IEnumerator SettlePlacement()
    {
        float t = 0f;
        while (t < 0.5f)
        {
            PlaceInFrontOfPlayer();
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void SetupVideo()
    {
        if (videoPlayer == null) return;

        if (videoReady)
        {
            videoPlayer.Play();
            return;
        }

        videoPlayer.playOnAwake     = false;
        videoPlayer.isLooping       = true;
        videoPlayer.source          = VideoSource.VideoClip;
        videoPlayer.renderMode      = VideoRenderMode.APIOnly;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        if (videoClip != null) videoPlayer.clip = videoClip;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
        videoReady = true;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        if (videoImage != null)
        {
            videoImage.texture = vp.texture;
            videoImage.color   = Color.white;
        }
        vp.Play();
    }

    public void Acknowledge()
    {
        if (acknowledged) return;
        acknowledged = true;

        if (videoPlayer != null) videoPlayer.Stop();
        if (pauseWhileShown) Time.timeScale = 1f;
        if (panelRoot != null) panelRoot.SetActive(false);

        if (enableOnAcknowledge != null)
            foreach (var go in enableOnAcknowledge)
                if (go != null) go.SetActive(true);

        onAcknowledged?.Invoke();
    }

    private void PlaceInFrontOfPlayer()
    {
        if (panelRoot == null) return;

        Transform cam = ResolveCameraTransform();
        if (cam == null) return;

        Vector3 fwd = cam.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        fwd.Normalize();

        Vector3 pos = cam.position + fwd * spawnDistance;
        pos.y = cam.position.y + heightOffset;

        panelRoot.transform.position = pos;
        panelRoot.transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
    }

    private Transform ResolveCameraTransform()
    {
        var anchor = GameObject.Find("CenterEyeAnchor");
        if (anchor != null) return anchor.transform;

        if (Camera.main != null) return Camera.main.transform;

        var anyCam = FindFirstObjectByType<Camera>();
        return anyCam != null ? anyCam.transform : null;
    }
}
