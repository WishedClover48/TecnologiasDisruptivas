using Oculus.Interaction;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public sealed class ControllerDistanceGrabRange : MonoBehaviour
{
    [SerializeField, Min(5f)] private float _maxDistance = 10f;

    private void Awake()
    {
        DistanceGrabInteractor[] interactors = FindObjectsByType<DistanceGrabInteractor>(
            FindObjectsInactive.Include);

        foreach (DistanceGrabInteractor interactor in interactors)
        {
            ConicalFrustum[] frustums = interactor.GetComponentsInChildren<ConicalFrustum>(true);
            foreach (ConicalFrustum frustum in frustums)
            {
                frustum.MaxLength = Mathf.Max(frustum.MaxLength, _maxDistance);
            }
        }
    }
}
