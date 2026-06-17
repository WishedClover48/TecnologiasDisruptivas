using System;
using UnityEngine;

public class FindMG_Item : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshCollider meshCollider;

    public bool ToDo;
    public void Init(FindMG_ItemDefinition definition, bool toDo)
    {
        gameObject.name = definition.itemName;
        ToDo = toDo;

        meshFilter.sharedMesh = definition.itemMesh;
        meshRenderer.sharedMaterials = definition.itemMaterial;
        meshCollider.sharedMesh = definition.itemMesh;
    }
}
