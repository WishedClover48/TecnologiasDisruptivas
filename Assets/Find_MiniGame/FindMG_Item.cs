using System;
using UnityEngine;

public class FindMG_Item : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    
    public bool ToDo { get; private set; }
    public void Init(FindMG_ItemDefinition definition, bool toDo)
    {
        gameObject.name = definition.itemName;
        ToDo = toDo;

        meshFilter.sharedMesh = definition.itemMesh;
        meshRenderer.sharedMaterials = definition.itemMaterial;
    }
}
