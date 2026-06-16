using System.Text;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_GameEndFeedback : MonoBehaviour
{
    [Header("Canvas root (world space, starts hidden)")]
    [SerializeField] private GameObject panelRoot;

    [Header("Text labels")]
    [SerializeField] private TMP_Text tierLabel;
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private TMP_Text headlineLabel;
    [SerializeField] private TMP_Text detailLabel;
    [SerializeField] private TMP_Text summaryLabel;

    [Header("Placement")]
    [SerializeField] private float spawnDistance = 1.5f;
    [SerializeField] private float heightOffset  = -0.1f;

    [Header("Global Events")]
    [SerializeField] private GlobalEventSO_Void onGameEnded;

    [Header("Scene control")]
    [SerializeField] private string mainSceneName = "";
    [SerializeField] private GameObject[] hideWhileShown;

    private bool shown;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (onGameEnded != null) onGameEnded.OnEventRaised += Show;
    }

    private void OnDisable()
    {
        if (onGameEnded != null) onGameEnded.OnEventRaised -= Show;
    }

    public void Show()
    {
        if (shown) return;
        shown = true;

        var pm = PlayerManager.Instance;
        if (pm == null)
        {
            Debug.LogError("[UI_GameEndFeedback] No PlayerManager.Instance found.");
            return;
        }

        GameResultEvaluator.Result result = GameResultEvaluator.Evaluate(
            pm.Health, pm.Stress, pm.Finance);

        Populate(result);
        PlaceInFrontOfPlayer();

        if (panelRoot != null) panelRoot.SetActive(true);

        if (hideWhileShown != null)
            foreach (var go in hideWhileShown)
                if (go != null) go.SetActive(false);

        Time.timeScale = 0f;
    }

    private void Populate(GameResultEvaluator.Result r)
    {
        if (tierLabel != null)
        {
            tierLabel.text  = r.TierLabel;
            tierLabel.color = r.TierColor;
        }
        if (scoreLabel    != null) scoreLabel.text    = $"{r.FinalScore} / 100";
        if (headlineLabel != null) headlineLabel.text = r.Headline;
        if (summaryLabel  != null) summaryLabel.text  = r.Summary;

        if (detailLabel != null)
        {
            var sb = new StringBuilder();
            foreach (var d in r.Dimensions)
                sb.AppendLine($"<b>{d.Label}</b> — {d.Score}\n<size=80%>{d.Comment}</size>\n");
            detailLabel.text = sb.ToString().TrimEnd();
        }
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

        Transform t = panelRoot.transform;
        t.position = pos;
        t.rotation = Quaternion.LookRotation(fwd, Vector3.up);
    }

    private Transform ResolveCameraTransform()
    {
        if (Camera.main != null) return Camera.main.transform;

        var anchor = GameObject.Find("CenterEyeAnchor");
        if (anchor != null) return anchor.transform;

        var anyCam = FindObjectOfType<Camera>();
        return anyCam != null ? anyCam.transform : null;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        shown = false;
        if (panelRoot != null) panelRoot.SetActive(false);

        if (PlayerManager.Instance  != null) PlayerManager.Instance.ResetState();
        if (DayTimeManager.Instance != null) DayTimeManager.Instance.ResetState();

        string scene = string.IsNullOrEmpty(mainSceneName)
            ? SceneManager.GetActiveScene().name
            : mainSceneName;
        SceneManager.LoadScene(scene);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
