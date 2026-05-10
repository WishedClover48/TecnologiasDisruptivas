using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    [SerializeField] private List<Transform> _objectsToReset;

    List<Transform> _originalPosition;
    private void Awake()
    {
        CachePositions();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_objectsToReset.Contains(other.transform))
        {
            ResetObjectPosition(other.transform);
        }
    }
    private void ResetObjectPosition(Transform obj)
    {
        int index = _objectsToReset.IndexOf(obj);
        if (index != -1)
        {
            _objectsToReset[index].position = _originalPosition[index].position;
        }
    }
    private void CachePositions() 
    { 
        _originalPosition = new List<Transform>();
        foreach (Transform obj in _objectsToReset)
        {
            Transform original = new GameObject().transform;
            original.position = obj.position;
            _originalPosition.Add(original);
        }
    }
}