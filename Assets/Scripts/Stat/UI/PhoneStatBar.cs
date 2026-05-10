using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class PhoneStatBar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Base StatBar material — will be instanced automatically.")]
    [SerializeField] private Material statBarMaterial;
    [Tooltip("Optional label showing the numeric/text value.")]
    [SerializeField] private TMP_Text valueLabel;

    [Header("Color Thresholds (normalized 0-1)")]
    [SerializeField] private Color highColor  = new Color(0.22f, 0.82f, 0.49f); // green
    [SerializeField] private Color midColor   = new Color(0.95f, 0.76f, 0.06f); // amber
    [SerializeField] private Color lowColor   = new Color(0.92f, 0.25f, 0.20f); // red
    [SerializeField] [Range(0f, 1f)] private float lowThreshold  = 0.30f;
    [SerializeField] [Range(0f, 1f)] private float midThreshold  = 0.60f;

    [Header("Animation")]
    [SerializeField] private float lerpSpeed = 5f;

    private static readonly int ID_Fill     = Shader.PropertyToID("_FillAmount");
    private static readonly int ID_Color    = Shader.PropertyToID("_FillColor");
    private static readonly int ID_RectSize = Shader.PropertyToID("_RectSize");

    private Image         image;
    private RectTransform rectTf;
    private Material      mat;          // per-instance material
    private float         currentFill;
    private float         targetFill;
    private Coroutine     anim;

    private void Awake()
    {
        image  = GetComponent<Image>();
        rectTf = GetComponent<RectTransform>();

        if (statBarMaterial != null)
        {
            mat         = new Material(statBarMaterial);
            image.material = mat;
        }
        else
        {
            Debug.LogWarning($"[PhoneStatBar] No StatBar material assigned on {name}.", this);
        }
    }

    private void Start()
    {
        SyncRectSize();
        ApplyImmediate(targetFill);
    }

    private void OnRectTransformDimensionsChange() => SyncRectSize();

    private void OnDestroy()
    {
        if (mat != null) Destroy(mat);
    }

    public void SetValue(float normalizedValue, string label = null)
    {
        targetFill = Mathf.Clamp01(normalizedValue);

        if (label != null && valueLabel != null)
            valueLabel.text = label;

        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(AnimateFill());
    }

    private IEnumerator AnimateFill()
    {
        while (!Mathf.Approximately(currentFill, targetFill))
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, lerpSpeed * Time.deltaTime);
            if (Mathf.Abs(currentFill - targetFill) < 0.001f)
                currentFill = targetFill;

            ApplyImmediate(currentFill);
            yield return null;
        }
        anim = null;
    }

    private void ApplyImmediate(float value)
    {
        if (mat == null) return;
        mat.SetFloat(ID_Fill,  value);
        mat.SetColor(ID_Color, EvaluateColor(value));
    }

    private void SyncRectSize()
    {
        if (mat == null || rectTf == null) return;
        Rect r = rectTf.rect;
        mat.SetVector(ID_RectSize, new Vector4(r.width, r.height, 0f, 0f));
    }

    private Color EvaluateColor(float t)
    {
        if (t <= lowThreshold)
        {
            float n = lowThreshold > 0f ? t / lowThreshold : 0f;
            return Color.Lerp(lowColor, midColor, n);
        }
        if (t >= midThreshold)
        {
            float n = midThreshold < 1f ? (t - midThreshold) / (1f - midThreshold) : 1f;
            return Color.Lerp(midColor, highColor, n);
        }
        {
            float n = (t - lowThreshold) / Mathf.Max(midThreshold - lowThreshold, 0.001f);
            return Color.Lerp(midColor, midColor, n);
        }
    }
}
