using UnityEngine;

[DisallowMultipleComponent]
public class VRControllerLasers : MonoBehaviour
{
    [SerializeField] private float _maxLength = 8f;
    [SerializeField] private float _width = 0.006f;
    [SerializeField] private Color _color = new Color(0.36f, 0.71f, 1f, 0.9f);

    [Header("Only show the laser while one of these panels is active")]
    [Tooltip("Assign the tutorial / end-feedback panel roots. Leave empty to always show.")]
    [SerializeField] private GameObject[] _showWhileActive;

    private Transform    _right, _left;
    private LineRenderer _rightLine, _leftLine;

    private void Start()
    {
        _right = FindAnchor("RightControllerAnchor", "RightHandAnchor");
        _left  = FindAnchor("LeftControllerAnchor",  "LeftHandAnchor");

        if (_right != null) _rightLine = CreateLine(_right);
        if (_left  != null) _leftLine  = CreateLine(_left);
    }

    private void Update()
    {
        bool visible = AnyPanelActive();

        SetVisible(_rightLine, visible);
        SetVisible(_leftLine,  visible);

        if (!visible) return;

        UpdateLine(_right, _rightLine);
        UpdateLine(_left,  _leftLine);
    }

    private bool AnyPanelActive()
    {
        if (_showWhileActive == null || _showWhileActive.Length == 0)
            return true; // no panels assigned -> behave as before (always on)

        foreach (var go in _showWhileActive)
            if (go != null && go.activeInHierarchy)
                return true;

        return false;
    }

    private static void SetVisible(LineRenderer line, bool visible)
    {
        if (line != null && line.enabled != visible)
            line.enabled = visible;
    }

    private static Transform FindAnchor(string primary, string fallback)
    {
        var go = GameObject.Find(primary) ?? GameObject.Find(fallback);
        return go != null ? go.transform : null;
    }

    private LineRenderer CreateLine(Transform parent)
    {
        var go = new GameObject("UILaser");
        go.transform.SetParent(parent, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace   = true;
        lr.positionCount   = 2;
        lr.widthMultiplier = _width;
        lr.numCapVertices  = 4;
        lr.material        = new Material(Shader.Find("Sprites/Default"));
        lr.startColor      = _color;
        lr.endColor        = new Color(_color.r, _color.g, _color.b, 0f);
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows  = false;
        return lr;
    }

    private void UpdateLine(Transform anchor, LineRenderer line)
    {
        if (anchor == null || line == null) return;

        Vector3 origin = anchor.position;
        Vector3 dir    = anchor.forward;
        float   len    = _maxLength;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, _maxLength))
            len = hit.distance;

        line.SetPosition(0, origin);
        line.SetPosition(1, origin + dir * len);
    }
}
