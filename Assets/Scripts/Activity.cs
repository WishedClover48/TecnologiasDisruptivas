using UnityEngine;
using DefaultNamespace;

public class Activity : MonoBehaviour
{
    [SerializeField] private ActivitySO activity;

    
    public ActivitySO GetActivity() => activity;
    

    [ContextMenu("Do Activity")]
    public void DoActivity()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("PlayerManager instance not found!");
            return;
        }

        if (activity == null)
        {
            Debug.LogError("No ActivitySO assigned to this Activity!");
            return;
        }
        if (PlayerManager.Instance.CanAffordActivity(activity))
        {
            PlayerManager.Instance.ApplyActivity(activity);
        }
        else
        {
            Debug.LogWarning($"Cannot afford activity: {activity.ActivityName}");
        }
    }


}