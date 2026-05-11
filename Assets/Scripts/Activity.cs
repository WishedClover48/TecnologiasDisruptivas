using System;
using UnityEngine;
using DefaultNamespace;
using UnityEngine.Serialization;

public class Activity : MonoBehaviour
{
    [FormerlySerializedAs("activity")] [SerializeField] private ActionData_SO actionData;

    public Action OnActivity;
    
    public ActionData_SO GetActivity() => actionData;

    [ContextMenu("Do Activity")]
    public void DoActivity()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("PlayerManager instance not found!");
            return;
        }

        if (actionData == null)
        {
            Debug.LogError("No ActivitySO assigned to this " + gameObject.name + "!");
            return;
        }
        if (PlayerManager.Instance.CanAffordActivity(actionData))
        {
            //PlayerManager.Instance.ApplyActivity(actionData);
            OnActivity?.Invoke();
            PlayerManager.Instance.SceneTransition.TransitionToActivity(actionData);
        }
        else
        {
            Debug.LogWarning($"Cannot afford activity: {actionData.ActivityName}");
        }
    }


}