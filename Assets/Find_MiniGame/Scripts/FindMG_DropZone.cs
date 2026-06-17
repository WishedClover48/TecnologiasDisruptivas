using System;
using UnityEngine;

public class FindMG_DropZone : MonoBehaviour
{
    [SerializeField] private LayerMask detectionMask;
    public event Action<FindMG_Item> OnItemDropped;

    private void OnTriggerEnter(Collider other)
    {
        if ((detectionMask & (1 << other.gameObject.layer)) == 0) return;

        if (other.TryGetComponent<FindMG_Item>(out var item))
        {
            OnItemDropped?.Invoke(item);
        }
    }
}
