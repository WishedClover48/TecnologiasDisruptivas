using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class UIShaderRect : MonoBehaviour
{
    private static readonly int ID_RectSize = Shader.PropertyToID("_RectSize");

    private Image         image;
    private RectTransform rectTf;
    private Material      mat;

    private void Awake()
    {
        image  = GetComponent<Image>();
        rectTf = GetComponent<RectTransform>();

        if (image.material != null)
        {
            mat            = new Material(image.material);
            image.material = mat;
        }
    }

    private void Start() => SyncSize();

    private void OnRectTransformDimensionsChange() => SyncSize();

    private void OnDestroy()
    {
        if (mat != null) Destroy(mat);
    }

    private void SyncSize()
    {
        if (mat == null || rectTf == null) return;
        Rect r = rectTf.rect;
        mat.SetVector(ID_RectSize, new Vector4(r.width, r.height, 0f, 0f));
    }
}
