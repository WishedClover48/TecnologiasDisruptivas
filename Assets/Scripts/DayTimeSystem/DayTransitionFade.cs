using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DayTransitionFade : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private GlobalEventSO_Int onDayEnded;

    [Header("Timing (segundos, tiempo real)")]
    [SerializeField] private float fadeOutDuration = 1.2f;
    [SerializeField] private float holdDuration    = 0.6f;
    [SerializeField] private float fadeInDuration   = 1.4f;

    [Header("Fade")]
    [SerializeField] private Color fadeColor = Color.black;

    [Header("Overlay (pegado a la cámara)")]
    [Tooltip("Distancia del overlay frente a la cámara (m). Suficientemente cerca para tapar todo.")]
    [SerializeField] private float overlayDistance = 0.2f;
    [Tooltip("Lado del overlay en metros (debe cubrir todo el FOV).")]
    [SerializeField] private float overlaySize = 3f;

    private Image     overlay;
    private Coroutine routine;

    private void OnEnable()
    {
        if (onDayEnded != null) onDayEnded.OnEventRaised += HandleDayEnded;
    }

    private void OnDisable()
    {
        if (onDayEnded != null) onDayEnded.OnEventRaised -= HandleDayEnded;
    }

    private void HandleDayEnded(int dayThatEnded)
    {
        var dtm = DayTimeManager.Instance;
        bool hasNextDay = dtm == null || dtm.CurrentDay < dtm.TotalDays;
        if (!hasNextDay) return;

        PlaySleepCycle();
    }

    public void PlaySleepCycle()
    {
        if (!EnsureOverlay()) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SleepCycle());
    }

    private bool EnsureOverlay()
    {
        if (overlay != null) return true;

        Transform cam = ResolveCamera();
        if (cam == null)
        {
            Debug.LogWarning("[DayTransitionFade] No se encontró la cámara VR (CenterEyeAnchor).");
            return false;
        }

        var go = new GameObject("DayFadeOverlay");
        go.transform.SetParent(cam, false);
        go.transform.localPosition = new Vector3(0f, 0f, overlayDistance);
        go.transform.localRotation = Quaternion.identity;

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.sortingOrder = 32760;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(overlaySize, overlaySize) * 1000f;
        rt.localScale = Vector3.one * 0.001f;

        overlay = go.AddComponent<Image>();
        overlay.raycastTarget = false;
        SetAlpha(0f);
        return true;
    }

    private Transform ResolveCamera()
    {
        var anchor = GameObject.Find("CenterEyeAnchor");
        if (anchor != null) return anchor.transform;
        if (Camera.main != null) return Camera.main.transform;
        var cam = FindAnyObjectByType<Camera>();
        return cam != null ? cam.transform : null;
    }

    private void SetAlpha(float a)
    {
        overlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, Mathf.Clamp01(a));
    }

    private IEnumerator SleepCycle()
    {
        yield return FadeTo(1f, fadeOutDuration);

        float t = 0f;
        while (t < holdDuration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return FadeTo(0f, fadeInDuration);
        routine = null;
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = overlay.color.a;

        if (duration <= 0f)
        {
            SetAlpha(target);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(start, target, t / duration));
            yield return null;
        }
        SetAlpha(target);
    }
}
