using DefaultNamespace;
using UnityEngine;

public class GoBackToSampleScene : MonoBehaviour
{
    [SerializeField] private string _sceneName = "SampleScene";

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.SceneTransition.TransitionFromActivity(_sceneName);
        }
    }
}
