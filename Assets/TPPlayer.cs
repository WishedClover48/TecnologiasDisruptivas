using UnityEngine;

public class TPPlayer : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;
    // Assign the CenterEyeAnchor (the actual Main Camera inside the rig)
    [SerializeField] private Transform centerEyeCamera;
    [SerializeField] private Vector3 _newPosition;
    [SerializeField] private Oculus.Interaction.Locomotion.CharacterController _characterController;
    private void Start()
    {
        _characterController.enabled = false;
        TeleportTo(_newPosition);
        _characterController.enabled = true;
    }


    public void TeleportTo(Vector3 targetWorldPosition)
    {
        if (_targetTransform == null || centerEyeCamera == null) return;

        // 1. Calculate how far the player's physical head is from the room center
        Vector3 cameraOffset = centerEyeCamera.localPosition;

        // 2. We only care about X and Z offsets. Keep the Y height tied to the ground.
        cameraOffset.y = 0f;

        // 3. Subtract that local offset from your destination coordinate
        Vector3 finalRigPosition = targetWorldPosition - cameraOffset;

        // 4. Move the root rig. The camera will now land precisely on target!
        _targetTransform.position = finalRigPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_newPosition, 0.5f);
    }
}
