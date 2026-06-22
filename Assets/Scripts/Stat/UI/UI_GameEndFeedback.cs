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

    [Header("Spot fijo de fin de juego (opcional)")]
    [Tooltip("Si se asigna, al terminar el juego el panel aparece SIEMPRE acá (no frente a la mirada) " +
             "y el jugador es teletransportado a este punto. El panel sale a 'spawnDistance' adelante.")]
    [SerializeField] private Transform endGameAnchor;
    [Tooltip("El TPPlayer (objeto 'SetPlayerPosition') que teletransporta el rig. Reusa su lógica " +
             "(offset de cámara + desactivar CharacterController + mirar al panel). Si queda vacío, no teletransporta.")]
    [SerializeField] private TPPlayer tpPlayer;

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
        PlaceEndScreen();

        if (panelRoot != null) panelRoot.SetActive(true);

        if (hideWhileShown != null)
            foreach (var go in hideWhileShown)
                if (go != null) go.SetActive(false);

        // sin Time.timeScale = 0: en VR congela el ray del control y rompe los botones del panel.
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

    private void PlaceEndScreen()
    {
        if (panelRoot == null) return;

        if (endGameAnchor == null)
        {
            PlaceInFrontOfPlayer();
            return;
        }

        Vector3 fwd = endGameAnchor.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        fwd.Normalize();

        if (tpPlayer != null) tpPlayer.TeleportSafely(endGameAnchor.position, fwd);

        Transform cam = ResolveCameraTransform();
        float eyeY = cam != null ? cam.position.y : endGameAnchor.position.y;

        Vector3 pos = endGameAnchor.position + fwd * spawnDistance;
        pos.y = eyeY + heightOffset;

        Transform t = panelRoot.transform;
        t.position = pos;
        t.rotation = Quaternion.LookRotation(fwd, Vector3.up);
    }

    private Transform ResolveCameraTransform()
    {
        var anchor = GameObject.Find("CenterEyeAnchor");
        if (anchor != null) return anchor.transform;

        if (Camera.main != null) return Camera.main.transform;

        var anyCam = FindAnyObjectByType<Camera>();
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
