using System.Collections;
using UnityEngine;

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
    [Tooltip("Si la cámara VR no tiene OVRScreenFade, se agrega automáticamente.")]
    [SerializeField] private bool autoAddScreenFade = true;

    private OVRScreenFade fade;
    private Coroutine     routine;

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
        if (!EnsureFade()) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SleepCycle());
    }

    private bool EnsureFade()
    {
        if (fade != null) return true;

        fade = OVRScreenFade.instance;

        if (fade == null && autoAddScreenFade)
        {
            var camGO = ResolveCameraGO();
            if (camGO != null)
            {
                fade = camGO.GetComponent<OVRScreenFade>();
                if (fade == null)
                {
                    fade = camGO.AddComponent<OVRScreenFade>();
                    fade.fadeOnStart = false;
                }
            }
        }

        if (fade != null)
        {
            fade.fadeColor = fadeColor;
            fade.SetExplicitFade(0f);
            return true;
        }

        Debug.LogWarning("[DayTransitionFade] No se encontró OVRScreenFade ni una cámara VR.");
        return false;
    }

    private GameObject ResolveCameraGO()
    {
        var anchor = GameObject.Find("CenterEyeAnchor");
        if (anchor != null) return anchor;
        if (Camera.main != null) return Camera.main.gameObject;
        var cam = FindAnyObjectByType<Camera>();
        return cam != null ? cam.gameObject : null;
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
        if (fade == null) yield break;

        float start = fade.currentAlpha;

        if (duration <= 0f)
        {
            fade.SetExplicitFade(target);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fade.SetExplicitFade(Mathf.Lerp(start, target, t / duration));
            yield return null;
        }
        fade.SetExplicitFade(target);
    }
}
