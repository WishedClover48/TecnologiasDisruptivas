using System;
using UnityEngine;

public class FindMG_Item : MonoBehaviour
{
    [SerializeField] private GameObject visualIndicator;
    private Transform modelRoot;
    private BoxCollider boxCollider;

    public bool ToDo { get; private set;}
    private GameObject currentModel;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        visualIndicator.SetActive(false);
    }

    public void Init(FindMG_ItemDefinition definition, bool isToDo)
    {
        gameObject.name = definition.itemName;
        ToDo = isToDo;

        if (currentModel != null) Destroy(currentModel);
        
        currentModel = Instantiate(definition.modelPrefab, transform);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;

        SetLayerRecursively(currentModel, gameObject.layer);
        FitColliderToModel();
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    private void FitColliderToModel()
    {
        var renderers = currentModel.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var lossy = transform.lossyScale;
        boxCollider.center = transform.InverseTransformPoint(bounds.center);
        boxCollider.size = new Vector3(
            bounds.size.x / Mathf.Max(Mathf.Abs(lossy.x), 1e-5f),
            bounds.size.y / Mathf.Max(Mathf.Abs(lossy.y), 1e-5f),
            bounds.size.z / Mathf.Max(Mathf.Abs(lossy.z), 1e-5f));
    }
}
