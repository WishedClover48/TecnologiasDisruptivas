using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsLabel;

    [Tooltip("Cada cuántos segundos se actualiza el contador")]
    [SerializeField] private float updateInterval = 0.5f;
    
    
    [Header("Placement")]
    [SerializeField] private bool placeInFrontOfPlayer = true;
    [SerializeField] private float spawnDistance = 1.6f;
    [SerializeField] private float heightOffset  = 0f;
    [SerializeField] private GameObject panelRoot;

    private float _timer;
    private int _frameCount;

    private void Update()
    {
        _frameCount++;
        _timer += Time.deltaTime;

        if (_timer >= updateInterval)
        {
            float fps = _frameCount / _timer;
            fpsLabel.text = $"FPS: {Mathf.RoundToInt(fps)}";
            SetColor(fps);

            _frameCount = 0;
            _timer = 0f;
            
            PlaceInFrontOfPlayer();
        }
    }

    private void SetColor(float fps)
    {
        // Verde arriba de 60, amarillo entre 30-60, rojo abajo de 30
        fpsLabel.color = fps >= 60f ? Color.green
                       : fps >= 30f ? Color.yellow
                       : Color.red;
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