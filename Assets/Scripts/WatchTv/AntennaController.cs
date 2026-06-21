using UnityEngine;

public class AntennaController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Rotación objetivo en Euler local")]
    [SerializeField] private Vector3 targetEulerAngles;
    [SerializeField] private float toleranceDegrees = 8f;

    [Header("Visual feedback")]
    [SerializeField] private Renderer indicatorRenderer; // material del Tip
    [SerializeField] private Color colorFar = Color.red;
    [SerializeField] private Color colorMid = Color.yellow;
    [SerializeField] private Color colorClose = Color.green;

    public bool IsAligned { get; private set; }

    private Material _indicatorMaterialInstance;

    private void Awake()
    {
        if (indicatorRenderer != null)
            _indicatorMaterialInstance = indicatorRenderer.material; // instancia, no afecta a otras antenas
    }

    private void Update()
    {
        float angleDiff = Quaternion.Angle(
            transform.localRotation,
            Quaternion.Euler(targetEulerAngles)
        );

        IsAligned = angleDiff <= toleranceDegrees;
        UpdateIndicator(angleDiff);
    }

    private void UpdateIndicator(float angleDiff)
    {
        if (_indicatorMaterialInstance == null) return;

        // Normalizamos: 0° = alineado perfecto, 90°+ = lejos
        float t = Mathf.Clamp01(angleDiff / 90f);

        Color color = t < 0.15f ? colorClose
                    : t < 0.5f  ? colorMid
                    : colorFar;

        _indicatorMaterialInstance.color = color;
    }

    public void SetRandomTarget()
    {
        targetEulerAngles = new Vector3(
            0f,
            Random.Range(0f, 360f),
            Random.Range(-40f, 40f)
        );
    }
}