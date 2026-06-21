using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    [SerializeField] private Transform cameraRigRoot;
    // Assign the CenterEyeAnchor (the actual Main Camera inside the rig)
    [SerializeField] private Transform centerEyeCamera;
    [SerializeField] private Oculus.Interaction.Locomotion.CharacterController _characterController;
    [SerializeField] private Vector3 _initialPosition;
    [SerializeField] private List<Transform> _objectsToReset;
    List<Transform> _originalPosition;

    private void Awake()
    {
        CachePositions();
        _characterController.enabled = false;
        TeleportTo(_initialPosition);
        _characterController.enabled = true;
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


    public void TeleportTo(Vector3 targetWorldPosition)
    {
        if (cameraRigRoot == null || centerEyeCamera == null) return;

        // 1. Calculate how far the player's physical head is from the room center
        Vector3 cameraOffset = centerEyeCamera.localPosition;

        // 2. We only care about X and Z offsets. Keep the Y height tied to the ground.
        cameraOffset.y = 0f;

        // 3. Subtract that local offset from your destination coordinate
        Vector3 finalRigPosition = targetWorldPosition - cameraOffset;

        // 4. Move the root rig. The camera will now land precisely on target!
        cameraRigRoot.position = finalRigPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_initialPosition, 0.5f);
    }

}