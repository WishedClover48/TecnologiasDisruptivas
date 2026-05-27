using UnityEngine;

public class TpToMap : MonoBehaviour
{
    [SerializeField] Vector3 position;
    private void OnTriggerEnter(Collider other)
    {
        ResetObjectPosition(other.transform);
    }
    private void ResetObjectPosition(Transform obj)
    {
        obj.position = position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(position, .1f);
    }

}
